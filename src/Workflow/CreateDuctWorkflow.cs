using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
namespace CadLayerTools
{
    internal static class CreateDuctWorkflow
    {
        static bool Show(Window window,ExternalCommandData data){new WindowInteropHelper(window).Owner=data.Application.MainWindowHandle;return window.ShowDialog()==true;}
        public static Result Run(ExternalCommandData data,ref string message,ElementSet elements)
        {
            var ui=data.Application.ActiveUIDocument;if(ui==null)return Result.Cancelled;
            try
            {
                var doc=ui.Document;
                if(doc.IsFamilyDocument){TaskDialog.Show("place_element_from_cad","Hãy chạy tool trong project Revit.");return Result.Cancelled;}
                var sources=LoadingWindow.Load(data.Application.MainWindowHandle,()=>CadSourceService.Collect(doc));
                var selectedMode=PlacementMode.Duct;var sourceState=new SourceSettings();var equipmentState=new CreationSettings();var ductState=new CreationSettings();
                var pointCache=new Dictionary<string,List<EquipmentPoint>>();EquipmentHost equipmentHost=null;var exitResult=Result.Cancelled;
                while(true)
                {
                    var mode=new ModeWindow(selectedMode);if(!Show(mode,data))return exitResult;selectedMode=mode.Mode;
                    while(true)
                    {
                        var source=new SourceWindow(sources,selectedMode,sourceState);
                        if(!Show(source,data)){if(source.BackRequested)break;return exitResult;}
                        var sourceDocument=doc;var sourceToHost=Transform.Identity;
                        if(source.LinkId!=null)
                        {
                            var link=doc.GetElement((ElementId)source.LinkId) as RevitLinkInstance;sourceDocument=link?.GetLinkDocument();
                            if(sourceDocument==null){TaskDialog.Show("place_element_from_cad","Revit Link không còn khả dụng.");return exitResult;}sourceToHost=link.GetTransform();
                        }
                        var cad=sourceDocument.GetElement((ElementId)source.CadId) as ImportInstance;if(cad==null)return exitResult;
                        var options=new Options {IncludeNonVisibleObjects=true};if(cad.ViewSpecific)options.View=sourceDocument.GetElement(cad.OwnerViewId) as View;
                        var geometry=cad.get_Geometry(options);if(geometry==null){TaskDialog.Show("CAD","Không đọc được geometry.");continue;}
                        var levels=new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().OrderBy(l=>l.ProjectElevation).Select(l=>new UiChoice {Id=l.Id,Name=l.Name,ElevationMm=l.ProjectElevation*304.8}).ToList();
                        if(selectedMode==PlacementMode.Equipment)
                        {
                            // Modal windows and these callbacks stay on the Revit command's UI thread.
                            Func<object,List<EquipmentPoint>> readLayer=layerId=>{
                                string key=$"{source.LinkId??"host"}:{source.CadId}:{layerId}";
                                if(!pointCache.ContainsKey(key)){string diagnostic;pointCache[key]=CadBlockReader.Read(sourceDocument,geometry,sourceToHost,(ElementId)layerId,out diagnostic);}
                                return pointCache[key];
                            };
                            var symbols=new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>()
                                .Where(EquipmentCreationService.IsEquipmentSymbol).Select(s=>new UiChoice {Id=s.Id,Name=s.Name,CategoryName=s.Category.Name,FamilyId=s.Family.Id,FamilyName=s.Family.Name,HostKind=EquipmentCreationService.HostKind(s)})
                                .OrderBy(s=>s.CategoryName).ThenBy(s=>s.FamilyName).ThenBy(s=>s.Name).ToList();
                            var categoryChoices=doc.Settings.Categories.Cast<Category>().Select(c=>c.Name).Distinct().OrderBy(n=>n).ToList();
                            var previews=new Dictionary<object,System.Windows.Media.ImageSource>();
                            Func<object,System.Windows.Media.ImageSource> readPreview=id=>{if(!previews.ContainsKey(id))previews[id]=FamilyPreviewService.Read(doc,id);return previews[id];};
                            var worksets=doc.IsWorkshared?new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksets().OrderBy(w=>w.Name).Select(w=>new UiChoice {Id=w.Id.IntegerValue,Name=w.Name}).ToList():new List<UiChoice>();
                            object activeWorkset=doc.IsWorkshared?(object)doc.GetWorksetTable().GetActiveWorksetId().IntegerValue:null;
                            while(true)
                            {
                                var setup=new EquipmentWindow(source.Layers,readLayer,symbols,levels,doc.ActiveView.GenLevel?.Id,EquipmentCreationService.Snapshot(doc),equipmentState,worksets,activeWorkset,equipmentHost?.FamilyId,equipmentHost?.Description,categoryChoices,readPreview);
                                if(!Show(setup,data))
                                {
                                    if(setup.PickHostRequested)
                                    {
                                        try{equipmentHost=EquipmentHostService.Pick(ui,(FamilySymbol)doc.GetElement((ElementId)setup.TypeId));}
                                        catch(Autodesk.Revit.Exceptions.OperationCanceledException){}
                                        catch(Exception ex){TaskDialog.Show("Chọn host",ex.Message);}
                                        continue;
                                    }
                                    if(setup.BackRequested)break;return exitResult;
                                }
                                ElementCreationResult created;
                                try{created=EquipmentCreationService.Create(doc,setup.SelectedPoints,(FamilySymbol)doc.GetElement((ElementId)setup.TypeId),(Level)doc.GetElement((ElementId)setup.LevelId),setup.Offset,equipmentHost,setup.WorksetId,setup.Comments);}
                                catch(Exception ex){TaskDialog.Show("Không tạo được thiết bị",ex.Message+"\nHãy kiểm tra host, Family và cấu hình. Lần tạo bị lỗi đã được hoàn tác.");continue;}
                                exitResult=Result.Succeeded;if(!Complete(created,"thiết bị",data))return exitResult;
                            }
                        }
                        else
                        {
                            Func<object,object,double,double,double,DuctAnalysis> analyze=(layerId,markerId,lengthTolerance,endpointTolerance,minLength)=>{
                                var lines=new List<Boundary>();var markers=new List<Boundary>();
                                CadGeometryReader.ReadGeometry(sourceDocument,geometry,sourceToHost,null,(ElementId)layerId,lines,new List<string>());
                                CadGeometryReader.ReadGeometry(sourceDocument,geometry,sourceToHost,null,(ElementId)(markerId??layerId),markers,new List<string>(),0,true);
                                int ambiguous;var pairs=MarkerMatcher.Pair(lines,markers,lengthTolerance,endpointTolerance,minLength,out ambiguous);
                                return new DuctAnalysis {Lines=lines,Pairs=pairs,Rows=PairPreviewService.Create(lines,pairs),Diagnostic=$"{ambiguous} trường hợp mơ hồ. Chưa nối fitting."};
                            };
                            var types=new FilteredElementCollector(doc).OfClass(typeof(DuctType)).Cast<DuctType>().Where(t=>t.Shape==ConnectorProfileType.Round).Select(t=>new UiChoice {Id=t.Id,Name=t.Name}).OrderBy(t=>t.Name).ToList();
                            var systems=new FilteredElementCollector(doc).OfClass(typeof(MechanicalSystemType)).Cast<MechanicalSystemType>().Select(s=>new UiChoice {Id=s.Id,Name=s.Name}).OrderBy(s=>s.Name).ToList();
                            while(true)
                            {
                                var setup=new DuctWindow(types,systems,levels,doc.ActiveView.GenLevel?.Id,source.Layers,analyze,ductState);
                                if(!Show(setup,data)){if(setup.BackRequested)break;return exitResult;}
                                double z=((Level)doc.GetElement((ElementId)setup.LevelId)).ProjectElevation+setup.Offset/304.8;
                                var created=DuctCreationService.Create(doc,setup.Analysis.Lines,setup.Analysis.Pairs,(ElementId)setup.SystemId,(ElementId)setup.TypeId,(ElementId)setup.LevelId,z);
                                exitResult=Result.Succeeded;if(!Complete(created,"ống gió tròn",data))return exitResult;
                            }
                        }
                    }
                }
            }
            catch(Autodesk.Revit.Exceptions.OperationCanceledException){return Result.Cancelled;}
            catch(Exception ex){message=ex.ToString();return Result.Failed;}
        }
        static bool Complete(ElementCreationResult created,string kind,ExternalCommandData data)
        {
            if(created.CreatedIds.Count>0)data.Application.ActiveUIDocument.Selection.SetElementIds(created.CreatedIds);
            var complete=new CompletionWindow($"Đã tạo {created.CreatedIds.Count} {kind}, bỏ qua {created.DuplicateCount} phần tử trùng.",CreatedElementReport.Read(data.Application.ActiveUIDocument.Document,created.CreatedIds));Show(complete,data);return complete.BackRequested;
        }
    }
}

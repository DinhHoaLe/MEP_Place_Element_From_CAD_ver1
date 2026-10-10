using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
namespace CadLayerTools
{
    internal static class EquipmentWorkflow
    {
        static bool Show(Window window,ExternalCommandData data){new WindowInteropHelper(window).Owner=data.Application.MainWindowHandle;return window.ShowDialog()==true;}
        public static Result Run(ExternalCommandData data,ref string message,ElementSet elements)
        {
            var ui=data.Application.ActiveUIDocument;if(ui==null)return Result.Cancelled;
            try
            {
                var doc=ui.Document;if(doc.IsFamilyDocument){TaskDialog.Show("PLACE ELEMENT FROM CAD","Run this tool in a Revit project.");return Result.Cancelled;}
                var sources=LoadingWindow.Load(data.Application.MainWindowHandle,()=>CadSourceService.Collect(doc));
                var sourceState=new SourceSettings();var settings=new CreationSettings();EquipmentHost host=null;bool placed=false;
                while(true)
                {
                    var source=new SourceWindow(sources,PlacementMode.Equipment,sourceState);
                    if(!Show(source,data))return placed?Result.Succeeded:Result.Cancelled;
                    var sourceDocument=doc;var toHost=Transform.Identity;
                    if(source.LinkId!=null){var link=doc.GetElement((ElementId)source.LinkId) as RevitLinkInstance;sourceDocument=link?.GetLinkDocument();if(sourceDocument==null)throw new InvalidOperationException("The Revit link is unloaded.");toHost=link.GetTransform();}
                    var cad=sourceDocument.GetElement((ElementId)source.CadId) as ImportInstance;if(cad==null)throw new InvalidOperationException("The selected CAD file is no longer available.");
                    var options=new Options {IncludeNonVisibleObjects=true};if(cad.ViewSpecific)options.View=sourceDocument.GetElement(cad.OwnerViewId) as View;else options.DetailLevel=ViewDetailLevel.Fine;
                    var geometry=cad.get_Geometry(options);if(geometry==null)throw new InvalidOperationException("Unable to read CAD geometry.");
                    var annotations=CadAnnotationService.Load(sourceDocument,cad);
                    var cache=new Dictionary<object,List<EquipmentPoint>>();
                    Func<object,List<EquipmentPoint>> readLayer=id=>{if(!cache.ContainsKey(id)){string diagnostic;cache[id]=CadBlockReader.Read(sourceDocument,geometry,toHost,(ElementId)id,out diagnostic,(point,root)=>annotations?.Apply(point,root,source.Layers.FirstOrDefault(l=>Equals(l.Id,id))?.Name));}return cache[id];};
                    var levels=new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().OrderBy(l=>l.ProjectElevation).Select(l=>new UiChoice {Id=l.Id,Name=l.Name,ElevationMm=l.ProjectElevation*304.8}).ToList();
                    var symbols=new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().Where(EquipmentCreationService.IsEquipmentSymbol).Select(s=>new UiChoice {Id=s.Id,Name=s.Name,CategoryName=s.Category.Name,FamilyId=s.Family.Id,FamilyName=s.Family.Name,HostKind=EquipmentCreationService.HostKind(s)}).OrderBy(s=>s.CategoryName).ThenBy(s=>s.FamilyName).ThenBy(s=>s.Name).ToList();
                    var categories=doc.Settings.Categories.Cast<Category>().Select(c=>c.Name).Distinct().OrderBy(n=>n).ToList();
                    var worksets=doc.IsWorkshared?new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksets().OrderBy(w=>w.Name).Select(w=>new UiChoice {Id=w.Id.IntegerValue,Name=w.Name}).ToList():new List<UiChoice>();
                    object active=doc.IsWorkshared?(object)doc.GetWorksetTable().GetActiveWorksetId().IntegerValue:null;
                    while(true)
                    {
                        var setup=new EquipmentWindow(source.Layers,readLayer,symbols,levels,doc.ActiveView.GenLevel?.Id,EquipmentCreationService.Snapshot(doc),settings,worksets,active,host?.FamilyId,host?.Description,categories,id=>FamilyPreviewService.Read(doc,id),window=>{
                            var result=EquipmentCreationService.Create(doc,window.SelectedPoints,(FamilySymbol)doc.GetElement((ElementId)window.TypeId),(Level)doc.GetElement((ElementId)window.LevelId),window.Offset,host,window.WorksetId,window.Comments,window.UpdateProgress);
                            placed|=result.CreatedIds.Count>0;
                            window.RefreshExisting(EquipmentCreationService.Snapshot(doc));
                            var summary=$"Placed {result.CreatedIds.Count} devices; skipped {result.DuplicateCount} duplicates.";
                            if(MessageBox.Show(window,summary+"\n\nWould you like to view the report?","Placement complete",MessageBoxButton.YesNo,MessageBoxImage.Information)==MessageBoxResult.Yes)
                            {
                                var report=new CompletionWindow(summary,CreatedElementReport.Read(doc,result.CreatedIds));report.Owner=window;report.ShowDialog();
                            }
                            // Return to this exact setup window; layer and family selectors stay available.
                        });
                        Show(setup,data);
                        if(setup.PickHostRequested)
                        {
                            try{host=EquipmentHostService.Pick(ui,(FamilySymbol)doc.GetElement((ElementId)setup.TypeId));}
                            catch(Autodesk.Revit.Exceptions.OperationCanceledException){}
                            catch(Exception ex){TaskDialog.Show("Pick host",ex.Message);}
                            continue;
                        }
                        if(setup.BackRequested)break;
                        return placed?Result.Succeeded:Result.Cancelled;
                    }
                }
            }
            catch(Autodesk.Revit.Exceptions.OperationCanceledException){return Result.Cancelled;}
            catch(Exception ex){message=ex.ToString();return Result.Failed;}
        }
    }
}
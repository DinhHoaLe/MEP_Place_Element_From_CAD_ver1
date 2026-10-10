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
    internal static class DuctWorkflow
    {
        static bool Show(Window window,ExternalCommandData data){new WindowInteropHelper(window).Owner=data.Application.MainWindowHandle;return window.ShowDialog()==true;}
        public static Result Run(ExternalCommandData data,ref string message,ElementSet elements)
        {
            var ui=data.Application.ActiveUIDocument;if(ui==null)return Result.Cancelled;
            try
            {
                var doc=ui.Document;if(doc.IsFamilyDocument){TaskDialog.Show("CREATE DUCT FROM CAD","Run this tool in a Revit project.");return Result.Cancelled;}
                var sources=LoadingWindow.Load(data.Application.MainWindowHandle,()=>CadSourceService.Collect(doc));var sourceState=new SourceSettings();var settings=new CreationSettings();bool createdAny=false;
                while(true)
                {
                    var source=new SourceWindow(sources,PlacementMode.Duct,sourceState);if(!Show(source,data))return createdAny?Result.Succeeded:Result.Cancelled;
                    var sourceDocument=doc;var transform=Transform.Identity;
                    if(source.LinkId!=null){var link=doc.GetElement((ElementId)source.LinkId) as RevitLinkInstance;sourceDocument=link?.GetLinkDocument();if(sourceDocument==null)throw new InvalidOperationException("The Revit link is unloaded.");transform=link.GetTransform();}
                    var cad=sourceDocument.GetElement((ElementId)source.CadId) as ImportInstance;if(cad==null)throw new InvalidOperationException("The selected CAD file is no longer available.");
                    var options=new Options {IncludeNonVisibleObjects=true};if(cad.ViewSpecific)options.View=sourceDocument.GetElement(cad.OwnerViewId) as View;
                    var geometry=cad.get_Geometry(options);if(geometry==null)throw new InvalidOperationException("Unable to read CAD geometry.");
                    var levels=new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().OrderBy(l=>l.ProjectElevation).Select(l=>new UiChoice {Id=l.Id,Name=l.Name,ElevationMm=l.ProjectElevation*304.8}).ToList();
                    var types=new FilteredElementCollector(doc).OfClass(typeof(DuctType)).Cast<DuctType>().Where(t=>t.Shape==ConnectorProfileType.Round).Select(t=>new UiChoice {Id=t.Id,Name=t.Name}).OrderBy(t=>t.Name).ToList();
                    var systems=new FilteredElementCollector(doc).OfClass(typeof(MechanicalSystemType)).Cast<MechanicalSystemType>().Select(t=>new UiChoice {Id=t.Id,Name=t.Name}).OrderBy(t=>t.Name).ToList();
                    Func<object,object,double,double,double,DuctAnalysis> analyze=(layer,marker,length,end,min)=>{
                        var lines=new List<Boundary>();var markers=new List<Boundary>();CadGeometryReader.ReadGeometry(sourceDocument,geometry,transform,null,(ElementId)layer,lines,new List<string>());CadGeometryReader.ReadGeometry(sourceDocument,geometry,transform,null,(ElementId)(marker??layer),markers,new List<string>(),0,true);
                        int ambiguous;var pairs=MarkerMatcher.Pair(lines,markers,length,end,min,out ambiguous);return new DuctAnalysis {Lines=lines,Pairs=pairs,Rows=PairPreviewService.Create(lines,pairs),Diagnostic=$"{ambiguous} ambiguous cases. Fittings are not connected."};
                    };
                    while(true)
                    {
                        var setup=new DuctWindow(types,systems,levels,doc.ActiveView.GenLevel?.Id,source.Layers,analyze,settings,window=>{
                            double z=((Level)doc.GetElement((ElementId)window.LevelId)).ProjectElevation+window.Offset/304.8;
                            var result=DuctCreationService.Create(doc,window.Analysis.Lines,window.Analysis.Pairs,(ElementId)window.SystemId,(ElementId)window.TypeId,(ElementId)window.LevelId,z,window.UpdateProgress);createdAny|=result.CreatedIds.Count>0;
                            var summary=$"Created {result.CreatedIds.Count} ducts; skipped {result.DuplicateCount} duplicates.";
                            if(MessageBox.Show(window,summary+"\n\nWould you like to view the report?","Duct creation complete",MessageBoxButton.YesNo,MessageBoxImage.Information)==MessageBoxResult.Yes)
                            {
                                var report=new CompletionWindow(summary,CreatedElementReport.Read(doc,result.CreatedIds));report.Owner=window;report.ShowDialog();
                            }
                        });
                        Show(setup,data);if(setup.BackRequested)break;
                        return createdAny?Result.Succeeded:Result.Cancelled;
                        // Continue configuring another layer until the user closes the tool.
                    }
                }
            }
            catch(Autodesk.Revit.Exceptions.OperationCanceledException){return Result.Cancelled;}
            catch(Exception ex){message=ex.ToString();return Result.Failed;}
        }
    }
}
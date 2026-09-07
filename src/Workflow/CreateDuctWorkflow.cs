using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Windows.Interop;


namespace CadLayerTools
{
/// <summary>Điều phối chọn CAD → cấu hình nguồn → đọc hình học → ghép biên → xem trước → tạo duct.</summary>
internal static class CreateDuctWorkflow
{
public static Result Run(ExternalCommandData data,ref string message,ElementSet elements)
        {
            var ui=data.Application.ActiveUIDocument;
            if(ui==null)return Result.Cancelled;
            try
            {
                var doc=ui.Document;
                var reference=ui.Selection.PickObject(ObjectType.Element,new CadFilter(),"Chọn CAD để tạo ống gió tròn");
                var cad=doc.GetElement(reference) as ImportInstance;
                if(cad==null)return Result.Cancelled;
                var categories=new Dictionary<string,Category>();
                foreach(var parent in new[]{cad.Category,doc.GetElement(cad.GetTypeId())?.Category})
                    if(parent!=null)foreach(Category c in parent.SubCategories)categories[c.Id.ToString()]=c;
                if(categories.Count==0){TaskDialog.Show("CAD","Không có layer.");return Result.Cancelled;}
                var source=new SourceWindow(doc.GetElement(cad.GetTypeId())?.Name??cad.Name,
                    categories.Values.OrderBy(c=>c.Name).Select(c=>new UiChoice {Id=c.Id,Name=c.Name}).ToList());
                new WindowInteropHelper(source).Owner=data.Application.MainWindowHandle;
                if(source.ShowDialog()!=true)return Result.Cancelled;
                var options=new Options {IncludeNonVisibleObjects=true};
                if(cad.ViewSpecific)options.View=doc.GetElement(cad.OwnerViewId) as View;
                var geometry=cad.get_Geometry(options);
                if(geometry==null){TaskDialog.Show("CAD","Không đọc được geometry.");return Result.Cancelled;}
                var lines=new List<Boundary>();var markers=new List<Boundary>();
                var notes=new List<string>();var markerNotes=new List<string>();
                CadGeometryReader.ReadGeometry(doc,geometry,Transform.Identity,null,(ElementId)source.LayerId,lines,notes);
                CadGeometryReader.ReadGeometry(doc,geometry,Transform.Identity,null,(ElementId)(source.MarkerId??source.LayerId),markers,markerNotes,0,true);
                int ambiguous;
                var pairs=MarkerMatcher.Pair(lines,markers,source.LengthTolerance,source.EndpointTolerance,source.MinLength,out ambiguous);
                var rows=PairPreviewService.Create(lines,pairs);
                int eligible=lines.Count(b=>b.Length>=source.MinLength);
                var results=new ResultsWindow(rows,eligible-pairs.Count*2,lines.Count-eligible,ambiguous,
                    MarkerMatcher.Diagnostics+"\nBiên bỏ block; đường nối đọc cả trong block. Chưa nối fitting.\n"+
                    "Tim và Diameter dùng kích thước thực trong Revit, không nhân tỷ lệ. Đường nối phải ngắn hơn hai biên.");
                new WindowInteropHelper(results).Owner=data.Application.MainWindowHandle;
                if(results.ShowDialog()!=true)return Result.Cancelled;
                Func<IEnumerable<Element>,List<UiChoice>> choices=items=>items.Select(e=>new UiChoice {Id=e.Id,Name=e.Name}).ToList();
                var setup=new DuctWindow(
                    choices(new FilteredElementCollector(doc).OfClass(typeof(DuctType)).Cast<DuctType>().Where(t=>t.Shape==ConnectorProfileType.Round).Cast<Element>()),
                    choices(new FilteredElementCollector(doc).OfClass(typeof(MechanicalSystemType)).Cast<Element>()),
                    choices(new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().OrderBy(l=>l.ProjectElevation).Cast<Element>()),
                    doc.ActiveView.GenLevel?.Id,pairs.Count);
                new WindowInteropHelper(setup).Owner=data.Application.MainWindowHandle;
                if(setup.ShowDialog()!=true)return Result.Cancelled;
                // ProjectElevation dùng cùng hệ tọa độ nội bộ với geometry; Offset nhập bằng mm và được đổi sang feet.
double z=((Level)doc.GetElement((ElementId)setup.LevelId)).ProjectElevation+setup.Offset/304.8;
                var created=DuctCreationService.Create(doc,lines,pairs,(ElementId)setup.SystemId,(ElementId)setup.TypeId,(ElementId)setup.LevelId,z);
                if(created.CreatedIds.Count>0)ui.Selection.SetElementIds(created.CreatedIds);
                TaskDialog.Show("Create Duct From CAD",$"Đã tạo {created.CreatedIds.Count} ống gió tròn, bỏ qua {created.DuplicateCount} ống trùng.\nChưa nối fitting. Undo một lần để hoàn tác.");
                return Result.Succeeded;
            }
            catch(Autodesk.Revit.Exceptions.OperationCanceledException){return Result.Cancelled;}
            catch(Exception ex){message=ex.ToString();return Result.Failed;}
        }
}
}

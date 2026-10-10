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
/// <summary>Đọc hình học CAD theo layer. Tách lượt đọc biên ngoài block và lượt đọc đoạn nối trong block.</summary>
internal static class CadGeometryReader
{
public static void ReadGeometry(Document doc,GeometryElement geometry,Transform transform,
            ElementId inherited,ElementId target,List<Boundary> lines,List<string> notes,int instanceDepth=0,bool includeBlockMarkers=false,bool targetBlock=false)
        {
            foreach(GeometryObject obj in geometry)
            {
                // GraphicsStyleCategory là layer Revit gắn với geometry. Nếu không có style riêng thì dùng layer kế thừa.
var style=doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                ElementId layer=style?.GraphicsStyleCategory?.Id ?? inherited;
                if(obj is GeometryInstance instance)
                {
                    // The first instance is the CAD container. Nested instances are
                    // excluded from boundaries; marker pass may traverse their descendants.
                    // Exploded/flattened blocks no longer have this identity in Revit.
                    // Instance cấp đầu là vỏ CAD. Block lồng bị bỏ ở lượt đọc biên, nhưng được duyệt ở lượt đọc marker. Block đã explode không thể nhận biết bằng cách này.
if(instanceDepth>0 && !includeBlockMarkers)
                    {
                        notes.Add("Skipped nested block/GeometryInstance and its contents");
                        continue;
                    }
                    // SymbolGeometry dùng tọa độ cục bộ. Nhân transform cha/con để đưa điểm về đúng vị trí CAD trong model; không áp transform lần thứ hai.
var children=instance.GetSymbolGeometry();
                    if(children!=null) ReadGeometry(doc,children,transform.Multiply(instance.Transform),layer,target,lines,notes,instanceDepth+1,includeBlockMarkers,(targetBlock || (includeBlockMarkers && instanceDepth>0 && layer!=null && layer.Equals(target))));
                    continue;
                }
                if(!targetBlock && (layer==null || !layer.Equals(target))) continue;
                // Không coi cung/tròn là biên thẳng. Nếu CAD đã biến đường cong thành PolyLine thì không còn đủ thông tin để nhận biết nguồn cong.
if(obj is Arc || obj is Ellipse)
                {
                    notes.Add("Skipped circles, arcs and ellipses on layer");
                    continue;
                }
                if(obj is Line line)
                    AddLine(transform.OfPoint(line.GetEndPoint(0)),transform.OfPoint(line.GetEndPoint(1)),lines,notes);
                else if(obj is PolyLine polyline)
                {
                    var points=polyline.GetCoordinates();
                    for(int i=1;i<points.Count;i++) AddLine(transform.OfPoint(points[i-1]),transform.OfPoint(points[i]),lines,notes);
                }
                else notes.Add("Skipped "+obj.GetType().Name);
            }
        }
static void AddLine(XYZ a,XYZ b,List<Boundary> lines,List<string> notes)
        {
            // Revit dùng feet: 1 foot = 304,8 mm. Chỉ giữ đoạn nằm ngang trong dung sai 1 mm; Z tạo duct được chọn riêng theo Level + Offset.
if(Math.Abs(a.Z-b.Z)*304.8>1) { notes.Add("Skipped non-horizontal segment"); return; }
            lines.Add(new Boundary(new V2(a.X*304.8,a.Y*304.8),new V2(b.X*304.8,b.Y*304.8)));
        }
}
}

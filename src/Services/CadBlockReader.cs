using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
namespace CadLayerTools
{
    internal static class CadBlockReader
    {
        public static List<EquipmentPoint> Read(Document document, GeometryElement geometry, Transform toHost, ElementId layer, out string diagnostic)
        {
            var points = new List<EquipmentPoint>();
            int unsupported = 0;
            Walk(document, geometry, toHost, null, layer, 0, points, ref unsupported);
            diagnostic = $"Nhận diện {points.Count} block trên layer; bỏ qua {unsupported} block nghiêng hoặc phản chiếu. " +
                "Điểm đặt là gốc chèn block. Một block được chọn tương ứng một thiết bị; không đếm lại block con. " +
                "Không nhận diện ký hiệu đã explode thành nét. Cao độ lấy theo Level + Offset của file hiện tại.";
            return points;
        }
        private static void Walk(Document doc, GeometryElement geometry, Transform parent, ElementId inherited,
            ElementId target, int depth, List<EquipmentPoint> points, ref int unsupported)
        {
            foreach (GeometryObject obj in geometry)
            {
                var instance = obj as GeometryInstance;
                if (instance == null) continue;
                var style = doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                var layer = style?.GraphicsStyleCategory?.Id ?? inherited;
                var transform = parent.Multiply(instance.Transform);
                // Depth zero is the CAD container, never a device.
                if (depth > 0 && layer != null && layer.Equals(target))
                {
                    var x = transform.BasisX; var y = transform.BasisY; var z = transform.BasisZ;
                    if (Math.Abs(x.Z) > 1e-7*x.GetLength() || Math.Abs(y.Z) > 1e-7*y.GetLength() ||
                        z.Z <= 0 || transform.Determinant <= 0)
                    { unsupported++; continue; }
                    var point = new EquipmentPoint { Index = points.Count+1, X=transform.Origin.X*304.8,
                        Y=transform.Origin.Y*304.8, Angle=Math.Atan2(x.Y,x.X)*180/Math.PI };
                    var symbol = instance.GetSymbolGeometry();
                    if (symbol != null) ReadPreview(symbol, transform, transform.Origin, point);
                    points.Add(point);
                    continue;
                }
                var children = instance.GetSymbolGeometry();
                if (children != null) Walk(doc, children, transform, layer, target, depth+1, points, ref unsupported);
            }
        }
        static void ReadPreview(GeometryElement geometry,Transform transform,XYZ origin,EquipmentPoint point)
        {
            foreach(GeometryObject obj in geometry)
            {
                if(point.Preview.Count>=2000) {point.PreviewTruncated=true;return;}
                var child=obj as GeometryInstance;
                if(child!=null) {var content=child.GetSymbolGeometry();if(content!=null)ReadPreview(content,transform.Multiply(child.Transform),origin,point);continue;}
                IList<XYZ> vertices=null;
                if(obj is PolyLine polyline) vertices=polyline.GetCoordinates();
                else if(obj is Curve curve && curve.IsBound) vertices=curve.Tessellate();
                if(vertices==null)continue;
                for(int i=1;i<vertices.Count;i++)
                {
                    if(point.Preview.Count>=2000){point.PreviewTruncated=true;return;}
                    var a=transform.OfPoint(vertices[i-1])-origin;var b=transform.OfPoint(vertices[i])-origin;
                    point.Preview.Add(new CadStroke {X1=a.X*304.8,Y1=a.Y*304.8,X2=b.X*304.8,Y2=b.Y*304.8});
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
namespace CadLayerTools
{
    internal static class CadBlockReader
    {
        public static List<EquipmentPoint> Read(Document document, GeometryElement geometry, Transform toHost, ElementId layer, out string diagnostic,Action<EquipmentPoint,Transform> annotations=null)
        {
            var points = new List<EquipmentPoint>();
            int unsupported = 0;
            Walk(document, geometry, toHost, null, layer, 0, points, ref unsupported,annotations,null);
            diagnostic = $"Detected {points.Count} blocks on layer; skipped {unsupported} tilted blocks or degenerate transforms. " +
                "Placement uses the block insertion point. Each selected block places one device; nested blocks are not counted twice. " +
                "Exploded symbols are not detected. Elevation uses the current model level and offset.";
            return points;
        }
        private static void Walk(Document doc, GeometryElement geometry, Transform parent, ElementId inherited,
            ElementId target, int depth, List<EquipmentPoint> points, ref int unsupported,Action<EquipmentPoint,Transform> annotations,Transform cadToHost)
        {
            foreach (GeometryObject obj in geometry)
            {
                var instance = obj as GeometryInstance;
                if (instance == null) continue;
                var style = doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                var layer = style?.GraphicsStyleCategory?.Id ?? inherited;
                var transform = parent.Multiply(instance.Transform);
                if(depth==0)cadToHost=transform;
                // Depth zero is the CAD container, never a device.
                if (depth > 0 && layer != null && layer.Equals(target))
                {
                    // A negative determinant is a valid mirrored block; keep its full transform for preview.
                    var x = transform.BasisX; var y = transform.BasisY; var z = transform.BasisZ;
                    if (Math.Abs(x.Z) > 1e-7*x.GetLength() || Math.Abs(y.Z) > 1e-7*y.GetLength() ||
                        z.GetLength() <= 1e-12 || transform.Determinant == 0)
                    { unsupported++; continue; }
                    var point = new EquipmentPoint { Index = points.Count+1, X=transform.Origin.X*304.8,
                        Y=transform.Origin.Y*304.8, Angle=Math.Atan2(x.Y,x.X)*180/Math.PI };
                    var symbol = instance.GetSymbolGeometry();
                    if (symbol != null) ReadPreview(symbol, transform, transform.Origin, point);
                    int symbolCount=point.Preview.Count,instanceCount=0;
                    try
                    {
                        // GetInstanceGeometry already includes this instance's transform.
                        // Apply only the parent transform, not the instance transform a second time.
                        var actual=symbolCount<=4?instance.GetInstanceGeometry():null;
                        var candidate=new EquipmentPoint();
                        if(actual!=null)ReadPreview(actual,parent,transform.Origin,candidate);
                        instanceCount=candidate.Preview.Count;
                        if(instanceCount>symbolCount){point.Preview=candidate.Preview;point.PreviewTruncated=candidate.PreviewTruncated;}
                    }
                    catch(Autodesk.Revit.Exceptions.InvalidOperationException) { }
                    point.PreviewInfo=$"{point.Preview.Count} CAD strokes (symbol: {symbolCount}, instance: {instanceCount}).";
                    if(point.Preview.Count<=4)point.PreviewInfo+=" CAD text/attributes may not be exposed by Revit geometry.";
                    annotations?.Invoke(point,cadToHost);
                    points.Add(point);
                    continue;
                }
                var children = instance.GetSymbolGeometry();
                if (children != null) Walk(doc, children, transform, layer, target, depth+1, points, ref unsupported,annotations,cadToHost);
            }
        }
        internal static void ReadPreview(GeometryElement geometry,Transform transform,XYZ origin,EquipmentPoint point)
        {
            foreach(GeometryObject obj in geometry)
            {
                if(point.Preview.Count>=2000) {point.PreviewTruncated=true;return;}
                var child=obj as GeometryInstance;
                if(child!=null) {var content=child.GetSymbolGeometry();if(content!=null)ReadPreview(content,transform.Multiply(child.Transform),origin,point);continue;}
                if(obj is Face face)
                {
                    foreach(EdgeArray loop in face.EdgeLoops)foreach(Edge edge in loop)AddPreviewVertices(edge.Tessellate(),transform,origin,point);
                    continue;
                }
                if(obj is Solid solid)
                {
                    // Filled CAD regions can be imported as thin solids instead of curves.
                    foreach(Edge edge in solid.Edges)
                        AddPreviewVertices(edge.Tessellate(),transform,origin,point);
                    continue;
                }
                if(obj is Mesh mesh)
                {
                    // Preserve faceted CAD geometry that has no Curve/PolyLine representation.
                    for(int i=0;i<mesh.NumTriangles;i++)
                    {
                        if(point.Preview.Count>=2000){point.PreviewTruncated=true;break;}
                        var triangle=mesh.get_Triangle(i);
                        AddPreviewVertices(new[]{triangle.get_Vertex(0),triangle.get_Vertex(1),triangle.get_Vertex(2),triangle.get_Vertex(0)},transform,origin,point);
                    }
                    continue;
                }
                if(obj is PolyLine polyline) AddPreviewVertices(polyline.GetCoordinates(),transform,origin,point);
                else if(obj is Curve curve && curve.IsBound) AddPreviewVertices(curve.Tessellate(),transform,origin,point);
            }
        }
        static void AddPreviewVertices(IList<XYZ> vertices,Transform transform,XYZ origin,EquipmentPoint point)
        {
            for(int i=1;i<vertices.Count;i++)
            {
                if(point.Preview.Count>=2000){point.PreviewTruncated=true;return;}
                var a=transform.OfPoint(vertices[i-1])-origin;var b=transform.OfPoint(vertices[i])-origin;
                // Ignore edges perpendicular to the plan, which project to a single point.
                if(Math.Abs(a.X-b.X)<1e-10 && Math.Abs(a.Y-b.Y)<1e-10)continue;
                point.Preview.Add(new CadStroke {X1=a.X*304.8,Y1=a.Y*304.8,X2=b.X*304.8,Y2=b.Y*304.8});
            }
        }
    }
}

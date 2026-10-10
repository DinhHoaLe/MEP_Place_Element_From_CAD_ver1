using System.Collections.Generic;
using Autodesk.Revit.DB;
namespace CadLayerTools
{
    internal static class CreatedElementReport
    {
        public static List<CreatedElementRow> Read(Document doc,IEnumerable<ElementId> ids)
        {
            var rows=new List<CreatedElementRow>();
            foreach(var id in ids)
            {
                var element=doc.GetElement(id);if(element==null)continue;
                var type=doc.GetElement(element.GetTypeId()) as ElementType;
                XYZ point=(element.Location as LocationPoint)?.Point;
                // For linear elements report their start point; coordinates are internal model mm.
                if(point==null&&element.Location is LocationCurve curve)point=curve.Curve.GetEndPoint(0);
                rows.Add(new CreatedElementRow {
                    Id=id.ToString(),Category=element.Category?.Name,Family=type?.FamilyName,Type=type?.Name,
                    Level=doc.GetElement(element.LevelId)?.Name,
                    X=point==null?(double?)null:point.X*304.8,Y=point==null?(double?)null:point.Y*304.8,Z=point==null?(double?)null:point.Z*304.8,
                    Workset=doc.IsWorkshared?doc.GetWorksetTable().GetWorkset(element.WorksetId)?.Name:"No worksets",
                    Mark=element.get_Parameter(BuiltInParameter.ALL_MODEL_MARK)?.AsString(),
                    Comments=element.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString()
                });
            }
            return rows;
        }
    }
}

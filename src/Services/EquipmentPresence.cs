using System;
using System.Collections.Generic;
using System.Linq;
namespace CadLayerTools
{
    public static class EquipmentPresence
    {
        // Same position means Euclidean distance < 1 mm, including host Z.
        public static bool SamePosition(double x,double y,double z, ExistingEquipment item)
            => (x-item.X)*(x-item.X)+(y-item.Y)*(y-item.Y)+(z-item.Z)*(z-item.Z)<1;
        public static void Update(List<EquipmentPoint> points,List<ExistingEquipment> existing,double z,object typeId)
        {
            foreach(var point in points)
            {
                var matches=existing.Where(e=>SamePosition(point.X,point.Y,z,e)).ToList();
                point.HasExisting=matches.Count>0;
                point.ExistingStatus=matches.Count==0 ? "Chưa có" :
                    (matches.Any(e=>Equals(e.TypeId,typeId)) ? "Đã có · cùng Type" : "Đã có · khác Type");
                if(point.HasExisting) point.IsSelected=false;
            }
        }
    }
}

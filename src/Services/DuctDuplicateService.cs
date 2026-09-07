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
/// <summary>Chặn tạo thêm duct có cùng hai đầu tim ở cùng cao độ, kể cả khi Diameter khác.</summary>
internal static class DuctDuplicateService
{
public static bool SameDuct(Duct duct,Route r,double z)
        {
            var line=(duct.Location as LocationCurve)?.Curve as Line;
            if(line==null) return false;
            var diameter=duct.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM);
            // Chỉ xét duct tròn. Kiểm tra hai đầu theo cả hai chiều, dung sai 1 mm; chưa phát hiện mọi trường hợp chồng lấn một phần.
if(diameter==null) return false; // Same centerline already occupied: do not stack a second diameter.
            XYZ a=new XYZ(r.A.X/304.8,r.A.Y/304.8,z),b=new XYZ(r.B.X/304.8,r.B.Y/304.8,z);
            return (line.GetEndPoint(0).DistanceTo(a)<1/304.8 && line.GetEndPoint(1).DistanceTo(b)<1/304.8) ||
                (line.GetEndPoint(1).DistanceTo(a)<1/304.8 && line.GetEndPoint(0).DistanceTo(b)<1/304.8);
        }
}
}

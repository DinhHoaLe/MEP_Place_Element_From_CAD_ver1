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
/// <summary>Xác minh tim, chiều dài và Diameter của duct khớp dữ liệu yêu cầu sau regenerate.</summary>
internal static class DuctValidationService
{
public static void VerifyDuct(Duct duct,Route route,double z)
        {
            var axis=(duct.Location as LocationCurve)?.Curve as Line;
            var diameter=duct.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM);
            XYZ a=new XYZ(route.A.X/304.8,route.A.Y/304.8,z),b=new XYZ(route.B.X/304.8,route.B.Y/304.8,z);
            // Dung sai xác minh 0,1 mm đổi sang feet. Nếu geometry thực lệch yêu cầu thì hủy lô tạo.
double eps=0.1/304.8;
            bool endpoints=axis!=null &&
                ((axis.GetEndPoint(0).DistanceTo(a)<=eps && axis.GetEndPoint(1).DistanceTo(b)<=eps) ||
                 (axis.GetEndPoint(1).DistanceTo(a)<=eps && axis.GetEndPoint(0).DistanceTo(b)<=eps));
            if(!endpoints || Math.Abs(axis.Length-route.Length/304.8)>eps ||
                diameter==null || Math.Abs(diameter.AsDouble()-route.Width/304.8)>eps)
                throw new Exception("The duct does not match the requested centerline or diameter. Creation was rolled back.");
        }
}
}

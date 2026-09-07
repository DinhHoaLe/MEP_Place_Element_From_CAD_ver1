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
/// <summary>Gán kích thước qua parameter Revit và kiểm tra giá trị thực sau khi gán.</summary>
internal static class DuctDimensionService
{
public static void SetDimension(Duct duct,BuiltInParameter id,double value)
        {
            Parameter p=duct.get_Parameter(id);
            if(p==null || p.IsReadOnly || !p.Set(value) || Math.Abs(p.AsDouble()-value)>0.1/304.8)
                throw new InvalidOperationException("Không gán được kích thước duct đã chọn.");
        }
}
}

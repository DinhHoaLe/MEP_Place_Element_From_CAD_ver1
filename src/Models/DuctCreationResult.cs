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
/// <summary>Kết quả tạo: ID duct mới và số tuyến bị bỏ qua do đã tồn tại.</summary>
internal class DuctCreationResult
{
    public List<ElementId> CreatedIds { get; set; }
    public int DuplicateCount { get; set; }
}
}

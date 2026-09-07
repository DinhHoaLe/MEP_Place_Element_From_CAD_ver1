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
/// <summary>Entry point retained for existing .addin manifests and Add-In Manager.</summary>
[Transaction(TransactionMode.Manual)]
/// <summary>Điểm vào add-in Revit. Chỉ chuyển yêu cầu sang workflow; không chứa thuật toán hình học.</summary>
public class ExportCadLayers : IExternalCommand
{
    public Result Execute(ExternalCommandData data, ref string message, ElementSet elements)
    {
        return CreateDuctWorkflow.Run(data, ref message, elements);
    }
}
}

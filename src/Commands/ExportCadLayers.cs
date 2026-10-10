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
        var target = typeof(ExportCadLayers).Assembly.GetReferencedAssemblies().First(a=>a.Name=="RevitAPI").Version.Major;
        var running = typeof(Document).Assembly.GetName().Version.Major;
        if(target!=running)
        {
            message=$"This DLL targets Revit {2000+target}. Load bin/Release/Revit{2000+running}/net48/CadLayerTools.dll for the running Revit version.";
            TaskDialog.Show("Revit version mismatch",message);return Result.Cancelled;
        }
        return CreateDuctWorkflow.Run(data, ref message, elements);
    }
}
}

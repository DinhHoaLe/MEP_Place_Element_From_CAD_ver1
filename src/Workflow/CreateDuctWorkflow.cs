using System.Windows.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
namespace CadLayerTools
{
    // Compatibility entry for the original combined add-in.
    internal static class CreateDuctWorkflow
    {
        public static Result Run(ExternalCommandData data,ref string message,ElementSet elements)
        {
            var mode=new ModeWindow();new WindowInteropHelper(mode).Owner=data.Application.MainWindowHandle;
            if(mode.ShowDialog()!=true)return Result.Cancelled;
            return mode.Mode==PlacementMode.Equipment?EquipmentWorkflow.Run(data,ref message,elements):DuctWorkflow.Run(data,ref message,elements);
        }
    }
}
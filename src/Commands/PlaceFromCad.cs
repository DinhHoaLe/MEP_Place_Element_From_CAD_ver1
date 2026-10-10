using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
namespace CadLayerTools
{
    [Transaction(TransactionMode.Manual)]
    public class PlaceFromCad : IExternalCommand
    {
        public Result Execute(ExternalCommandData data,ref string message,ElementSet elements)
        {
            if(!CommandVersion.Check(ref message))return Result.Cancelled;
            return EquipmentWorkflow.Run(data,ref message,elements);
        }
    }
}
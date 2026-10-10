using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
namespace CadLayerTools
{
    [Transaction(TransactionMode.Manual)]
    public class CreateDuctFromCad : IExternalCommand
    {
        public Result Execute(ExternalCommandData data,ref string message,ElementSet elements)
        {
            if(!CommandVersion.Check(ref message))return Result.Cancelled;
            return DuctWorkflow.Run(data,ref message,elements);
        }
    }
}
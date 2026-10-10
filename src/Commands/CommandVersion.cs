using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
namespace CadLayerTools
{
    internal static class CommandVersion
    {
        public static bool Check(ref string message)
        {
            var target=typeof(CommandVersion).Assembly.GetReferencedAssemblies().First(a=>a.Name=="RevitAPI").Version.Major;
            var running=typeof(Document).Assembly.GetName().Version.Major;if(target==running)return true;
            message=$"This DLL targets Revit {2000+target}. Load the DLL from bin/Release/Revit{2000+running}/net48 for the running Revit version.";
            TaskDialog.Show("Revit version mismatch",message);return false;
        }
    }
}
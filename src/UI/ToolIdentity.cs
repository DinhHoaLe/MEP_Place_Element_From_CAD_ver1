using System.Reflection;
namespace CadLayerTools
{
    internal static class ToolIdentity
    {
        static string AssemblyName=>Assembly.GetExecutingAssembly().GetName().Name;
        public static bool Dedicated=>AssemblyName=="PlaceFromCad"||AssemblyName=="CreateDuctFromCad";
        public static string Title=>AssemblyName=="CreateDuctFromCad"?"CREATE DUCT FROM CAD":"PLACE ELEMENT FROM CAD";
    }
}
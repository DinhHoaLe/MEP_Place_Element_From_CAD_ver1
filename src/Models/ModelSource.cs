using System.Collections.Generic;
namespace CadLayerTools
{
    public class ModelSource
    {
        public object LinkId { get; set; }
        public string Name { get; set; }
        public string UnavailableReason { get; set; }
        public List<CadSource> Cads { get; set; } = new List<CadSource>();
        public string DisplayName => Name + " · " + Cads.Count + " CAD";
        public static int DefaultIndex(List<ModelSource> sources)
        {
            for (int i = 0; i < sources.Count; i++)
            {
                if (sources[i].LinkId == null) return i;
            }
            return -1;
        }
    }
}

using System.Collections.Generic;
namespace CadLayerTools
{
    public class CadSource
    {
        public object Id { get; set; }
        public string Name { get; set; }
        public List<UiChoice> Layers { get; set; } = new List<UiChoice>();
    }
}

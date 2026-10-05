using System.Collections.Generic;
namespace CadLayerTools
{
    public enum EquipmentHostKind { None, Element, Face, Unsupported }
    public class DuctAnalysis
    {
        public List<Boundary> Lines = new List<Boundary>();
        public List<LengthPair> Pairs = new List<LengthPair>();
        public List<PairRow> Rows = new List<PairRow>();
        public string Diagnostic;
    }
}

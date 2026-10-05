using System.Collections.Generic;
using System.ComponentModel;
namespace CadLayerTools
{
    public class CadStroke
    {
        public double X1, Y1, X2, Y2;
    }
    public class ExistingEquipment
    {
        public double X, Y, Z;
        public object TypeId;
        public string Description;
    }
    // Host coordinates in mm. Preview strokes are relative to the insertion point.
    public class EquipmentPoint : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Angle { get; set; }
        double placementZ;
        public double PlacementZ {get=>placementZ;set{placementZ=value;Changed("PlacementZ");}}
        public List<CadStroke> Preview { get; set; } = new List<CadStroke>();
        public bool PreviewTruncated { get; set; }
        bool selected = true, existing;
        string status = "Chưa kiểm tra";
        public bool IsSelected { get => selected; set { if(selected == value) return; selected=value; Changed("IsSelected"); } }
        public bool HasExisting { get => existing; set { existing=value; Changed("HasExisting"); Changed("CanSelect"); } }
        public bool CanSelect => !HasExisting;
        public string ExistingStatus { get => status; set { status=value; Changed("ExistingStatus"); } }
        public event PropertyChangedEventHandler PropertyChanged;
        void Changed(string name) => PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(name));
    }
}

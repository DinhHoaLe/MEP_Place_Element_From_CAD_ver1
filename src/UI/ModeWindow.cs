using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace CadLayerTools
{
    public class ModeWindow : ToolWindow
    {
        public PlacementMode Mode { get; private set; }
        readonly Button equipment, duct;
        public ModeWindow(PlacementMode initial = PlacementMode.Duct) : base("01 / 04", "Choose mode", "One CAD source · Two ways to create Revit elements",1180,760)
        {
            Section("What would you like to create?", "Choose an option to begin. You can return and change it later.");
            var cards = new Grid(); cards.ColumnDefinitions.Add(new ColumnDefinition()); cards.ColumnDefinitions.Add(new ColumnDefinition());
            equipment = Card("equipment", "Place equipment", "CAD blocks → Revit families", "XYZ coordinates · Family / Type\nUnhosted or hosted families", PlacementMode.Equipment);
            duct = Card("routes", "Create MEP routes", "CAD routes → MEP elements", "Round ducts supported · Fittings are not connected\nPipe / Conduit / Cable tray: planned", PlacementMode.Duct);
            equipment.Margin=new Thickness(0,8,12,8);duct.Margin=new Thickness(12,8,0,8);
            cards.Children.Add(equipment); Grid.SetColumn(duct,1);cards.Children.Add(duct);Body.Children.Add(cards);
            UseWorkspace(Body);
            Select(initial);
            Action("Cancel",false,()=>Close()).IsCancel=true;
            Action("Select CAD source  →",true,()=>DialogResult=true).IsDefault=true;
        }
        Button Card(string icon,string title,string subtitle,string detail,PlacementMode mode)
        {
            var panel=new StackPanel {Margin=new Thickness(12,16,12,16)};
            panel.Children.Add(new Image {Source=ModeArtwork.Load(icon),Height=155,Stretch=Stretch.Uniform,Margin=new Thickness(0,0,0,16)});
            panel.Children.Add(new TextBlock {Text=title,FontSize=25,FontWeight=FontWeights.SemiBold});
            panel.Children.Add(new TextBlock {Text=subtitle,FontSize=15,Margin=new Thickness(0,12,0,20)});
            panel.Children.Add(new TextBlock {Text=detail,TextWrapping=TextWrapping.Wrap,FontSize=14});
            var button=new Button {Content=panel,HorizontalContentAlignment=HorizontalAlignment.Stretch,MinHeight=270};
            button.Click+=(s,e)=>Select(mode); return button;
        }
        void Select(PlacementMode mode)
        {
            Mode=mode;
            foreach(var button in new[]{equipment,duct})
            {
                bool selected=button==(mode==PlacementMode.Equipment?equipment:duct);
                button.Background=selected ? new SolidColorBrush(Color.FromRgb(98,84,60)) : Brushes.White;
                button.Foreground=selected?Brushes.White:Ink;
                button.BorderBrush=selected?new SolidColorBrush(Color.FromRgb(111,96,68)):new SolidColorBrush(Color.FromRgb(221,214,206));
                button.BorderThickness=new Thickness(selected?3:1);
                button.ToolTip=selected?"Selected":"Click to select";
            }
        }
    }
}


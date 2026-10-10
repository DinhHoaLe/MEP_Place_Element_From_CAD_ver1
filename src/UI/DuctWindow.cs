using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace CadLayerTools
{
    public class DuctWindow : ToolWindow
    {
        bool creating;readonly DataGrid grid;readonly ProgressBar overall;readonly TextBlock progressText;
        readonly ComboBox types,systems,levels,layer,marker;
        readonly TextBox offset,length,end,min;
        public void UpdateProgress(int row,int percent,int completed,string status)
        {
            if(row>=0&&row<Analysis.Rows.Count){Analysis.Rows[row].Progress=percent;Analysis.Rows[row].ProgressStatus=status;grid.ScrollIntoView(Analysis.Rows[row]);}
            overall.Value=Analysis.Pairs.Count==0?0:Math.Min(status=="Created"||status=="Skipped"?100:99,100.0*completed/Analysis.Pairs.Count);
            progressText.Text=status+" · "+completed+" / "+Analysis.Pairs.Count;
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render,new Action(()=>{}));
        }
        public object TypeId=>((UiChoice)types.SelectedItem).Id;
        public object SystemId=>((UiChoice)systems.SelectedItem).Id;
        public object LevelId=>((UiChoice)levels.SelectedItem).Id;
        public double Offset;
        public DuctAnalysis Analysis {get;private set;}=new DuctAnalysis();
        public DuctWindow(List<UiChoice> t,List<UiChoice> s,List<UiChoice> l,object defaultLevel,List<UiChoice> layers,
            Func<object,object,double,double,double,DuctAnalysis> analyze,CreationSettings saved=null,Action<DuctWindow> create=null)
            :base("03 / 04","Configure and preview","Create MEP routes · Round ducts · Select layers and review routes",1180,1040)
        {
            saved=saved??new CreationSettings();
            layer=Choices(layers);marker=Choices(new[]{new UiChoice {Name="Same as boundary layer"}}.Concat(layers));
            if(saved.LayerId!=null)layer.SelectedItem=layers.FirstOrDefault(c=>Equals(c.Id,saved.LayerId))??layers.FirstOrDefault();
            marker.SelectedItem=marker.Items.Cast<UiChoice>().FirstOrDefault(c=>Equals(c.Id,saved.MarkerId))??marker.Items[0];
            FieldPair("CAD boundary layer",layer,"Connector layer",marker);
            grid=PreviewGrid.Create(Analysis.Rows,new[]{"A","B","Length","Diameter","Start","End"},new[]{"Boundary A","Boundary B","Length (mm)","Diameter (mm)","Start X,Y","End X,Y"});            var rowPanel=new FrameworkElementFactory(typeof(StackPanel));var rowBar=new FrameworkElementFactory(typeof(ProgressBar));rowBar.SetValue(ProgressBar.HeightProperty,6.0);rowBar.SetValue(ProgressBar.MaximumProperty,100.0);rowBar.SetBinding(ProgressBar.ValueProperty,new Binding("Progress"));rowPanel.AppendChild(rowBar);
            var rowLabel=new FrameworkElementFactory(typeof(TextBlock));rowLabel.SetValue(TextBlock.FontSizeProperty,10.0);rowLabel.SetBinding(TextBlock.TextProperty,new Binding("ProgressStatus"));rowPanel.AppendChild(rowLabel);
            grid.Columns.Add(new DataGridTemplateColumn {Header="Progress",Width=150,CellTemplate=new DataTemplate {VisualTree=rowPanel}});Body.Children.Add(grid);
            Action fit=()=>{double width=Math.Max(0,grid.ActualWidth-SystemParameters.VerticalScrollBarWidth-2);if(width>0)foreach(var column in grid.Columns)column.Width=new DataGridLength(width/grid.Columns.Count);};grid.SizeChanged+=(s,e)=>fit();grid.Loaded+=(s,e)=>fit();
            overall=new ProgressBar {Height=7,Maximum=100,Margin=new Thickness(0,0,0,4)};progressText=new TextBlock {Text="Ready to create",Foreground=Muted};Body.Children.Add(overall);Body.Children.Add(progressText);
            var summary=new TextBlock {Foreground=Muted,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,12)};Body.Children.Add(summary);
            types=Choices(t);systems=Choices(s);levels=Choices(l);
            types.SelectedItem=t.FirstOrDefault(c=>Equals(c.Id,saved.TypeId))??t.FirstOrDefault();systems.SelectedItem=s.FirstOrDefault(c=>Equals(c.Id,saved.SystemId))??s.FirstOrDefault();
            levels.SelectedItem=l.FirstOrDefault(c=>Equals(c.Id,saved.LevelId??defaultLevel))??l.FirstOrDefault();
            offset=new TextBox {Text=saved.Offset??"3000"};
            FieldPair("Duct Type",types,"System Type",systems);FieldPair("Level",levels,"Centerline offset · mm",offset);
            var advanced=new StackPanel();var original=Body;Body=advanced;
            length=new TextBox {Text=saved.Length};end=new TextBox {Text=saved.Endpoint};min=new TextBox {Text=saved.Minimum};
            FieldPair("Length tolerance · mm",length,"Endpoint tolerance · mm",end);Field("Minimum length · mm",min);Body=original;
            Body.Children.Add(new Expander {Header="Detection settings",Content=advanced});
            BackButton();Action("Cancel",false,()=>Close()).IsCancel=true;
            var next=Action("Create ducts",true,()=>{
                try{if(types.SelectedItem==null||systems.SelectedItem==null||levels.SelectedItem==null)throw new Exception("Select a type, system and level.");Offset=Number(offset,-100000,100000,"Offset");                    if(Analysis.Pairs.Count>0)
                    {
                        if(create==null){DialogResult=true;return;}
                        if(creating)return;creating=true;Body.IsEnabled=false;Actions.IsEnabled=false;
                        try{create(this);}finally{creating=false;Body.IsEnabled=true;Actions.IsEnabled=true;}
                    }}
                catch(Exception ex){Error.Text=ex.Message;}
            });next.IsDefault=true;
            Action refresh=()=>{
                try{
                    if(layer.SelectedItem==null)throw new Exception("No CAD layers are available.");
                    Analysis=analyze(((UiChoice)layer.SelectedItem).Id,(marker.SelectedItem as UiChoice)?.Id,
                        Number(length,0,100,"Length tolerance"),Number(end,.001,10,"Endpoint tolerance"),Number(min,1,100000,"Minimum length"));
                    grid.ItemsSource=Analysis.Rows;summary.Text=Analysis.Pairs.Count+" valid pairs · "+Analysis.Diagnostic;
                    next.Content="Create "+Analysis.Pairs.Count+" duct segments";next.IsEnabled=Analysis.Pairs.Count>0&&t.Count>0&&s.Count>0&&l.Count>0;
                    Error.Text=Analysis.Pairs.Count==0?"Adjust the layers or detection settings to identify boundary pairs.":"Review the settings before creating elements.";
                }catch(Exception ex){Analysis=new DuctAnalysis();grid.ItemsSource=Analysis.Rows;summary.Text="";next.IsEnabled=false;Error.Text=ex.Message;}
            };
            layer.SelectionChanged+=(sender,e)=>refresh();marker.SelectionChanged+=(sender,e)=>refresh();length.TextChanged+=(sender,e)=>refresh();end.TextChanged+=(sender,e)=>refresh();min.TextChanged+=(sender,e)=>refresh();
            Closing+=(sender,e)=>{
                if(creating){e.Cancel=true;return;}
                saved.TypeId=(types.SelectedItem as UiChoice)?.Id;saved.SystemId=(systems.SelectedItem as UiChoice)?.Id;saved.LevelId=(levels.SelectedItem as UiChoice)?.Id;
                saved.LayerId=(layer.SelectedItem as UiChoice)?.Id;saved.MarkerId=(marker.SelectedItem as UiChoice)?.Id;saved.Offset=offset.Text;
                saved.Length=length.Text;saved.Endpoint=end.Text;saved.Minimum=min.Text;
            };
            refresh();
        }
    }
}

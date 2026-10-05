using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
namespace CadLayerTools
{
    public class DuctWindow : ToolWindow
    {
        readonly ComboBox types,systems,levels,layer,marker;
        readonly TextBox offset,length,end,min;
        public object TypeId=>((UiChoice)types.SelectedItem).Id;
        public object SystemId=>((UiChoice)systems.SelectedItem).Id;
        public object LevelId=>((UiChoice)levels.SelectedItem).Id;
        public double Offset;
        public DuctAnalysis Analysis {get;private set;}=new DuctAnalysis();
        public DuctWindow(List<UiChoice> t,List<UiChoice> s,List<UiChoice> l,object defaultLevel,List<UiChoice> layers,
            Func<object,object,double,double,double,DuctAnalysis> analyze,CreationSettings saved=null)
            :base("03 / 04","Cấu hình và xem trước","Vẽ tuyến MEP · Duct tròn · Chọn layer và kiểm tra tuyến",1180,1040)
        {
            saved=saved??new CreationSettings();
            layer=Choices(layers);marker=Choices(new[]{new UiChoice {Name="Cùng layer biên"}}.Concat(layers));
            if(saved.LayerId!=null)layer.SelectedItem=layers.FirstOrDefault(c=>Equals(c.Id,saved.LayerId))??layers.FirstOrDefault();
            marker.SelectedItem=marker.Items.Cast<UiChoice>().FirstOrDefault(c=>Equals(c.Id,saved.MarkerId))??marker.Items[0];
            FieldPair("Layer biên CAD",layer,"Layer đường nối",marker);
            var grid=PreviewGrid.Create(Analysis.Rows,new[]{"A","B","Length","Diameter","Start","End"},new[]{"Biên A","Biên B","Dài tim (mm)","Diameter (mm)","Điểm đầu X,Y","Điểm cuối X,Y"});Body.Children.Add(grid);
            var summary=new TextBlock {Foreground=Muted,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,12)};Body.Children.Add(summary);
            types=Choices(t);systems=Choices(s);levels=Choices(l);
            types.SelectedItem=t.FirstOrDefault(c=>Equals(c.Id,saved.TypeId))??t.FirstOrDefault();systems.SelectedItem=s.FirstOrDefault(c=>Equals(c.Id,saved.SystemId))??s.FirstOrDefault();
            levels.SelectedItem=l.FirstOrDefault(c=>Equals(c.Id,saved.LevelId??defaultLevel))??l.FirstOrDefault();
            offset=new TextBox {Text=saved.Offset??"3000"};
            FieldPair("Duct Type",types,"System Type",systems);FieldPair("Level",levels,"Offset tim · mm",offset);
            var advanced=new StackPanel();var original=Body;Body=advanced;
            length=new TextBox {Text=saved.Length};end=new TextBox {Text=saved.Endpoint};min=new TextBox {Text=saved.Minimum};
            FieldPair("Dung sai dài · mm",length,"Dung sai đầu mút · mm",end);Field("Chiều dài tối thiểu · mm",min);Body=original;
            Body.Children.Add(new Expander {Header="Thông số nhận diện",Content=advanced});
            BackButton();Action("Hủy",false,()=>Close()).IsCancel=true;
            var next=Action("Tạo ống",true,()=>{
                try{if(types.SelectedItem==null||systems.SelectedItem==null||levels.SelectedItem==null)throw new Exception("Chọn Type, hệ thống và Level.");Offset=Number(offset,-100000,100000,"Offset");if(Analysis.Pairs.Count>0)DialogResult=true;}
                catch(Exception ex){Error.Text=ex.Message;}
            });next.IsDefault=true;
            Action refresh=()=>{
                try{
                    if(layer.SelectedItem==null)throw new Exception("CAD chưa có layer khả dụng.");
                    Analysis=analyze(((UiChoice)layer.SelectedItem).Id,(marker.SelectedItem as UiChoice)?.Id,
                        Number(length,0,100,"Dung sai dài"),Number(end,.001,10,"Dung sai đầu mút"),Number(min,1,100000,"Chiều dài tối thiểu"));
                    grid.ItemsSource=Analysis.Rows;summary.Text=Analysis.Pairs.Count+" cặp hợp lệ · "+Analysis.Diagnostic;
                    next.Content="Tạo "+Analysis.Pairs.Count+" đoạn ống";next.IsEnabled=Analysis.Pairs.Count>0&&t.Count>0&&s.Count>0&&l.Count>0;
                    Error.Text=Analysis.Pairs.Count==0?"Chọn lại layer hoặc thông số để nhận diện cặp biên.":"Kiểm tra cấu hình trước khi tạo.";
                }catch(Exception ex){Analysis=new DuctAnalysis();grid.ItemsSource=Analysis.Rows;summary.Text="";next.IsEnabled=false;Error.Text=ex.Message;}
            };
            layer.SelectionChanged+=(sender,e)=>refresh();marker.SelectionChanged+=(sender,e)=>refresh();length.TextChanged+=(sender,e)=>refresh();end.TextChanged+=(sender,e)=>refresh();min.TextChanged+=(sender,e)=>refresh();
            Closing+=(sender,e)=>{
                saved.TypeId=(types.SelectedItem as UiChoice)?.Id;saved.SystemId=(systems.SelectedItem as UiChoice)?.Id;saved.LevelId=(levels.SelectedItem as UiChoice)?.Id;
                saved.LayerId=(layer.SelectedItem as UiChoice)?.Id;saved.MarkerId=(marker.SelectedItem as UiChoice)?.Id;saved.Offset=offset.Text;
                saved.Length=length.Text;saved.Endpoint=end.Text;saved.Minimum=min.Text;
            };
            refresh();
        }
    }
}

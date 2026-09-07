using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Markup;


namespace CadLayerTools
{
/// <summary>Giao diện chọn CAD/layer và dung sai nhận diện; chỉ lưu lựa chọn của người dùng.</summary>
public class SourceWindow:ToolWindow
    {
        ComboBox layer,marker;TextBox length,end,min;
        public object LayerId=>((UiChoice)layer.SelectedItem).Id;
        public object MarkerId=>((UiChoice)marker.SelectedItem).Id;
        public double LengthTolerance,EndpointTolerance,MinLength;
        public SourceWindow(string cad,List<UiChoice> layers):base("01 / 03","Chọn nguồn CAD","Chọn layer biên và đường nối để nhận diện từng đoạn ống.",680,760)
        {
            Field("Tên link CAD",new TextBox {Text=cad,IsReadOnly=true,Background=new SolidColorBrush(Color.FromRgb(246,248,251))});
            layer=Choices(layers);Field("Layer biên ống",layer);
            marker=Choices(new[]{new UiChoice {Name="Cùng layer biên",Id=null}}.Concat(layers));Field("Layer đường nối / miệng fitting",marker);
            var advanced=new StackPanel();var old=Body;Body=advanced;
            length=new TextBox {Text="1"};end=new TextBox {Text="0.1"};min=new TextBox {Text="100"};
            Field("Dung sai chiều dài · mm",length);Field("Dung sai đầu mút · mm",end);Field("Chiều dài tối thiểu · mm",min);
            Body=old;Body.Children.Add(new Expander {Header="Thông số nhận diện",Content=advanced,IsExpanded=true});
            Action("Hủy",false,()=>Close()).IsCancel=true;
            Action("Phân tích layer  →",true,()=>{
                try{LengthTolerance=Number(length,0,100,"Dung sai dài");EndpointTolerance=Number(end,0.001,10,"Dung sai đầu mút");MinLength=Number(min,1,100000,"Chiều dài tối thiểu");DialogResult=true;}
                catch(Exception e){Error.Text=e.Message;}
            }).IsDefault=true;
        }
    }
}

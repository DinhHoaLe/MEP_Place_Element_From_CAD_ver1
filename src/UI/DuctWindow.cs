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
/// <summary>Giao diện chọn Duct Type tròn, System Type, Level và Offset tim; không nhân hệ số Diameter.</summary>
public class DuctWindow:ToolWindow
    {
        ComboBox types,systems,levels;TextBox offset;
        public object TypeId=>((UiChoice)types.SelectedItem).Id;
        public object SystemId=>((UiChoice)systems.SelectedItem).Id;
        public object LevelId=>((UiChoice)levels.SelectedItem).Id;
        public double Offset;
        public DuctWindow(List<UiChoice> t,List<UiChoice> s,List<UiChoice> l,object defaultLevel,int count):
            base("03 / 03","Tạo ống gió tròn",$"{count} đoạn đã nhận diện · Chọn hệ thống và cao độ.",640,610)
        {
            types=Choices(t);systems=Choices(s);levels=Choices(l);
            foreach(var item in l)if(Equals(item.Id,defaultLevel))levels.SelectedItem=item;
            Field("Duct Type",types);Field("System Type",systems);Field("Level",levels);
            offset=new TextBox {Text="3000"};Field("Offset tim so với Level · mm",offset);
            Action("Hủy",false,()=>Close()).IsCancel=true;
            var button=Action("Tạo "+count+" đoạn ống",true,()=>{
                try{Offset=Number(offset,-100000,100000,"Offset");DialogResult=true;}catch(Exception e){Error.Text=e.Message;}
            });
            button.IsDefault=true;button.IsEnabled=t.Count>0&&s.Count>0&&l.Count>0;
            if(!button.IsEnabled)Error.Text="Model cần có Duct Type tròn, System Type và Level.";
        }
    }
}

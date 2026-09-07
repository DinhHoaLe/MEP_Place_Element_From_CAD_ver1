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
/// <summary>Khung WPF dùng chung: màu, font, styles, nút và kiểm tra nhập số. Không chỉnh sửa model.</summary>
public class ToolWindow:Window
    {
        protected StackPanel Body;
        protected StackPanel Actions;
        protected TextBlock Error;
        public static Brush Ink=new SolidColorBrush(Color.FromRgb(27,42,65));
        public static Brush Muted=new SolidColorBrush(Color.FromRgb(101,117,139));
        public ToolWindow(string step,string title,string subtitle,double width=650,double height=660)
        {
            Title="Create Duct From CAD";Width=width;Height=height;
            MinWidth=550;MinHeight=450;WindowStartupLocation=WindowStartupLocation.CenterOwner;
            ShowInTaskbar=false;FontFamily=new FontFamily("Segoe UI");FontSize=14;
            Background=new SolidColorBrush(Color.FromRgb(243,246,250));Foreground=Ink;
            Resources=(ResourceDictionary)XamlReader.Parse(@"<ResourceDictionary xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
<Style TargetType='Button'>
 <Setter Property='Padding' Value='20,10'/><Setter Property='Margin' Value='8,0,0,0'/>
 <Setter Property='MinHeight' Value='40'/><Setter Property='Background' Value='#2463EB'/><Setter Property='Foreground' Value='White'/>
 <Setter Property='BorderBrush' Value='#2463EB'/><Setter Property='Cursor' Value='Hand'/>
 <Setter Property='Template'><Setter.Value><ControlTemplate TargetType='Button'>
 <Border x:Name='surface' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='1' CornerRadius='7' Padding='{TemplateBinding Padding}'>
 <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
 </Border><ControlTemplate.Triggers>
 <Trigger Property='IsMouseOver' Value='True'><Setter TargetName='surface' Property='Opacity' Value='0.85'/></Trigger>
 <Trigger Property='IsEnabled' Value='False'><Setter TargetName='surface' Property='Opacity' Value='0.4'/></Trigger>
 <Trigger Property='IsKeyboardFocused' Value='True'><Setter TargetName='surface' Property='BorderBrush' Value='#0F172A'/></Trigger>
 </ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter>
</Style>
<Style TargetType='TextBox'><Setter Property='Padding' Value='10,8'/><Setter Property='BorderBrush' Value='#D6DFEB'/><Setter Property='MinHeight' Value='38'/></Style>
<Style TargetType='ComboBox'><Setter Property='Padding' Value='10,7'/><Setter Property='MinHeight' Value='38'/><Setter Property='MaxDropDownHeight' Value='280'/></Style>
<Style TargetType='DataGridColumnHeader'><Setter Property='Background' Value='#EAF0F8'/><Setter Property='Foreground' Value='#334155'/><Setter Property='Padding' Value='12'/><Setter Property='FontWeight' Value='SemiBold'/><Setter Property='BorderThickness' Value='0'/></Style>
<Style TargetType='DataGridCell'><Setter Property='Padding' Value='10,8'/><Setter Property='BorderThickness' Value='0'/></Style>
</ResourceDictionary>");
            var root=new DockPanel {Background=Background};
            var header=new StackPanel {Margin=new Thickness(28,24,28,20)};
            header.Children.Add(new TextBlock {Text="CAD  /  ROUND DUCT     •     "+step,FontSize=12,FontWeight=FontWeights.SemiBold,Foreground=new SolidColorBrush(Color.FromRgb(37,99,235))});
            header.Children.Add(new TextBlock {Text=title,FontSize=27,FontWeight=FontWeights.Bold,Margin=new Thickness(0,9,0,5)});
            header.Children.Add(new TextBlock {Text=subtitle,Foreground=Muted,TextWrapping=TextWrapping.Wrap});
            DockPanel.SetDock(header,Dock.Top);root.Children.Add(header);
            var bottom=new StackPanel {Margin=new Thickness(28,12,28,22)};
            Error=new TextBlock {Foreground=Brushes.Firebrick,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,10)};
            Actions=new StackPanel {Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right};
            bottom.Children.Add(Error);bottom.Children.Add(Actions);DockPanel.SetDock(bottom,Dock.Bottom);root.Children.Add(bottom);
            Body=new StackPanel();
            var card=new Border {Background=Brushes.White,CornerRadius=new CornerRadius(12),Padding=new Thickness(22),Margin=new Thickness(28,0,28,0)};
            card.Child=new ScrollViewer {Content=Body,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};
            root.Children.Add(card);Content=root;
        }
        protected void Field(string label,FrameworkElement input)
        {
            Body.Children.Add(new TextBlock {Text=label,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,7)});
            input.Margin=new Thickness(0,0,0,17);Body.Children.Add(input);
        }
        protected Button Action(string label,bool primary,Action callback)
        {
            var button=new Button {Content=label};
            if(!primary){button.Background=Brushes.White;button.Foreground=Ink;button.BorderBrush=new SolidColorBrush(Color.FromRgb(210,220,233));}
            button.Click+=(s,e)=>callback();Actions.Children.Add(button);return button;
        }
        protected static ComboBox Choices(IEnumerable<UiChoice> items)
        {
            var c=new ComboBox {ItemsSource=items.ToList(),DisplayMemberPath="Name"};c.SelectedIndex=0;return c;
        }
        protected static double Number(TextBox box,double min,double max,string label)
        {
            double value;
            if(!double.TryParse(box.Text.Trim().Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out value)||
               double.IsNaN(value)||double.IsInfinity(value)||value<min||value>max)
                throw new Exception(label+": nhập giá trị từ "+min+" đến "+max+".");
            return value;
        }
    }
}

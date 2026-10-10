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
        public bool BackRequested { get; private set; }
        Border workspaceCard;
        Grid workspaceHeader, workspaceFooter;
        bool fullWorkspace;
        protected StackPanel Body;
        protected StackPanel Actions;
        protected TextBlock Error;
        public static Brush Ink=new SolidColorBrush(Color.FromRgb(55,50,43));
        public static Brush Muted=new SolidColorBrush(Color.FromRgb(113,107,99));
        public ToolWindow(string step,string title,string subtitle,double width=650,double height=660)
        {
            Title=ToolIdentity.Title;Width=Math.Min(Math.Max(width,1100),SystemParameters.WorkArea.Width);Height=Math.Min(Math.Max(height,760),SystemParameters.WorkArea.Height);
            MinWidth=Math.Min(960,SystemParameters.WorkArea.Width);MinHeight=Math.Min(640,SystemParameters.WorkArea.Height);WindowStartupLocation=WindowStartupLocation.CenterOwner;
            ShowInTaskbar=false;FontFamily=new FontFamily("Segoe UI");FontSize=13;
            Background=new SolidColorBrush(Color.FromRgb(245,242,238));Foreground=Ink;
            Resources=(ResourceDictionary)XamlReader.Parse(@"<ResourceDictionary xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
<Style TargetType='Button'>
 <Setter Property='Padding' Value='20,10'/><Setter Property='Margin' Value='8,0,0,0'/>
 <Setter Property='MinHeight' Value='46'/><Setter Property='HorizontalContentAlignment' Value='Center'/><Setter Property='Background' Value='#62543C'/><Setter Property='Foreground' Value='White'/>
 <Setter Property='BorderBrush' Value='#62543C'/><Setter Property='Cursor' Value='Hand'/>
 <Setter Property='Template'><Setter.Value><ControlTemplate TargetType='Button'>
 <Border x:Name='surface' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='{TemplateBinding BorderThickness}' CornerRadius='9' Padding='{TemplateBinding Padding}'>
 <ContentPresenter HorizontalAlignment='{TemplateBinding HorizontalContentAlignment}' VerticalAlignment='Center'/>
 </Border><ControlTemplate.Triggers>
 <Trigger Property='IsMouseOver' Value='True'><Setter TargetName='surface' Property='Opacity' Value='0.85'/></Trigger>
 <Trigger Property='IsEnabled' Value='False'><Setter TargetName='surface' Property='Opacity' Value='0.4'/></Trigger>
 <Trigger Property='IsKeyboardFocused' Value='True'><Setter TargetName='surface' Property='BorderBrush' Value='#6F6044'/></Trigger>
 </ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter>
</Style>
<Style TargetType='TextBox'><Setter Property='Padding' Value='10,8'/><Setter Property='BorderBrush' Value='#DDD6CE'/><Setter Property='MinHeight' Value='46'/></Style>
<Style TargetType='ComboBox'>
 <Setter Property='Background' Value='#FAF8F5'/><Setter Property='BorderBrush' Value='#DDD6CE'/><Setter Property='Foreground' Value='#37322B'/>
 <Setter Property='MinHeight' Value='46'/><Setter Property='Padding' Value='14,10'/><Setter Property='MaxDropDownHeight' Value='280'/>
 <Setter Property='ScrollViewer.CanContentScroll' Value='True'/>
 <Setter Property='Template'><Setter.Value><ControlTemplate TargetType='ComboBox'>
  <Grid>
   <ToggleButton Focusable='False' ClickMode='Press' IsChecked='{Binding IsDropDownOpen, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}'>
    <ToggleButton.Template><ControlTemplate TargetType='ToggleButton'>
     <Border x:Name='box' Background='#FAF8F5' BorderBrush='#DDD6CE' BorderThickness='1' CornerRadius='8'>
      <Path Data='M 0 0 L 4 4 L 8 0' Stroke='#90805C' StrokeThickness='1.5' HorizontalAlignment='Right' VerticalAlignment='Center' Margin='0,0,17,0'/>
     </Border>
     <ControlTemplate.Triggers><Trigger Property='IsMouseOver' Value='True'><Setter TargetName='box' Property='BorderBrush' Value='#62543C'/></Trigger></ControlTemplate.Triggers>
    </ControlTemplate></ToggleButton.Template>
   </ToggleButton>
   <ContentPresenter Margin='14,4,40,4' VerticalAlignment='Center' IsHitTestVisible='False' Content='{TemplateBinding SelectionBoxItem}' ContentTemplate='{TemplateBinding SelectionBoxItemTemplate}' ContentTemplateSelector='{TemplateBinding ItemTemplateSelector}'/>
   <Popup x:Name='PART_Popup' Placement='Bottom' IsOpen='{TemplateBinding IsDropDownOpen}' AllowsTransparency='True' Focusable='False' PopupAnimation='Fade'>
    <Border Background='White' BorderBrush='#DDD6CE' BorderThickness='1' CornerRadius='8' Padding='4' MinWidth='{Binding ActualWidth, RelativeSource={RelativeSource TemplatedParent}}' MaxHeight='{TemplateBinding MaxDropDownHeight}'>
     <ScrollViewer CanContentScroll='True'><ItemsPresenter KeyboardNavigation.DirectionalNavigation='Contained'/></ScrollViewer>
    </Border>
   </Popup>
  </Grid>
  <ControlTemplate.Triggers><Trigger Property='IsEnabled' Value='False'><Setter Property='Opacity' Value='0.5'/></Trigger></ControlTemplate.Triggers>
 </ControlTemplate></Setter.Value></Setter>
</Style>
<Style TargetType='ComboBoxItem'><Setter Property='Padding' Value='12,9'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='ComboBoxItem'>
 <Border x:Name='item' Background='Transparent' CornerRadius='5' Padding='{TemplateBinding Padding}'><ContentPresenter/></Border>
 <ControlTemplate.Triggers><Trigger Property='IsHighlighted' Value='True'><Setter TargetName='item' Property='Background' Value='#EEE8E1'/></Trigger><Trigger Property='IsSelected' Value='True'><Setter TargetName='item' Property='Background' Value='#E8E2DA'/></Trigger></ControlTemplate.Triggers>
</ControlTemplate></Setter.Value></Setter></Style>
<Style TargetType='DataGridColumnHeader'><Setter Property='Background' Value='#EEE8E1'/><Setter Property='Foreground' Value='#37322B'/><Setter Property='Padding' Value='12'/><Setter Property='FontWeight' Value='SemiBold'/><Setter Property='BorderThickness' Value='0'/></Style>
<Style TargetType='DataGridCell'><Setter Property='Padding' Value='10,8'/><Setter Property='BorderThickness' Value='0'/><Style.Triggers><Trigger Property='IsSelected' Value='True'><Setter Property='Background' Value='#E8E2DA'/><Setter Property='Foreground' Value='#37322B'/></Trigger></Style.Triggers></Style>
<Style TargetType='DataGridRow'><Setter Property='Background' Value='White'/><Style.Triggers><Trigger Property='IsSelected' Value='True'><Setter Property='Background' Value='#E8E2DA'/><Setter Property='Foreground' Value='#37322B'/></Trigger><Trigger Property='IsMouseOver' Value='True'><Setter Property='Background' Value='#F5F2EE'/></Trigger></Style.Triggers></Style>
<Style TargetType='CheckBox'><Setter Property='VerticalContentAlignment' Value='Center'/><Setter Property='Cursor' Value='Hand'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='CheckBox'><StackPanel Orientation='Horizontal'><Border x:Name='marker' Width='18' Height='18' CornerRadius='4' Background='White' BorderBrush='#B3A491' BorderThickness='1.4'><TextBlock x:Name='mark' Text='✓' Foreground='White' FontSize='13' FontWeight='Bold' HorizontalAlignment='Center' VerticalAlignment='Center' Visibility='Collapsed'/></Border><ContentPresenter VerticalAlignment='Center'/></StackPanel><ControlTemplate.Triggers><Trigger Property='IsChecked' Value='True'><Setter TargetName='marker' Property='Background' Value='#6F6044'/><Setter TargetName='mark' Property='Visibility' Value='Visible'/></Trigger><Trigger Property='IsEnabled' Value='False'><Setter Property='Opacity' Value='0.45'/></Trigger></ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>
</ResourceDictionary>");
            var root=new DockPanel {Background=Background};
            var header=new Grid {Margin=new Thickness(40,34,40,32)};
            header.ColumnDefinitions.Add(new ColumnDefinition {Width=new GridLength(88)});
            header.ColumnDefinitions.Add(new ColumnDefinition());
            header.ColumnDefinitions.Add(new ColumnDefinition {Width=new GridLength(420)});
            var badge=new Border {Width=70,Height=70,CornerRadius=new CornerRadius(10),Background=new SolidColorBrush(Color.FromRgb(209,200,191)),BorderBrush=new SolidColorBrush(Color.FromRgb(200,188,172)),BorderThickness=new Thickness(1),HorizontalAlignment=HorizontalAlignment.Left};
            badge.Child=new Image {Source=ModeArtwork.Load("project-symbol"),Stretch=Stretch.Uniform,Margin=new Thickness(3)};
            header.Children.Add(badge);
            var heading=new StackPanel {VerticalAlignment=VerticalAlignment.Center};
            heading.Children.Add(new TextBlock {Text="STEP "+(ToolIdentity.Dedicated?(int.Parse(step.Substring(0,2))-1).ToString("00")+" / 03":step),FontSize=11,FontWeight=FontWeights.Bold,Foreground=new SolidColorBrush(Color.FromRgb(111,96,68)),Margin=new Thickness(0,0,0,4)});
            heading.Children.Add(new TextBlock {Text=title,FontSize=27,FontWeight=FontWeights.SemiBold});
            heading.Children.Add(new TextBlock {Text=subtitle,FontSize=13,Foreground=Muted,Margin=new Thickness(0,9,0,0),TextWrapping=TextWrapping.Wrap});
            Grid.SetColumn(heading,1);header.Children.Add(heading);
            var steps=new Grid {VerticalAlignment=VerticalAlignment.Center};
            int current=int.Parse(step.Substring(0,2));
            string[] labels={"Choose\nmode","Select CAD\nsource","Configure\nand preview","Create\nand finish"};
            if(ToolIdentity.Dedicated){current--;labels=new[]{"Select CAD\nsource",ToolIdentity.Title=="CREATE DUCT FROM CAD"?"Configure\nand create":"Configure\nand place","Report"};}
            for(int i=0;i<labels.Length;i++) {
                steps.ColumnDefinitions.Add(new ColumnDefinition());
                var item=new StackPanel();
                var circle=new Border {Width=34,Height=34,CornerRadius=new CornerRadius(17),Background=i+1==current?new SolidColorBrush(Color.FromRgb(98,84,60)):new SolidColorBrush(Color.FromRgb(232,226,218)),BorderBrush=new SolidColorBrush(Color.FromRgb(221,214,206)),BorderThickness=new Thickness(1)};
                circle.Child=new TextBlock {Text=(i+1).ToString(),HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center,Foreground=i+1==current?Brushes.White:Muted};
                item.Children.Add(circle);
                item.Children.Add(new TextBlock {Text=labels[i],TextAlignment=TextAlignment.Center,FontSize=11,Foreground=Muted,Margin=new Thickness(0,8,0,0)});
                Grid.SetColumn(item,i);steps.Children.Add(item);
            }
            Grid.SetColumn(steps,2);header.Children.Add(steps);
            DockPanel.SetDock(header,Dock.Top);root.Children.Add(header);
            var bottom=new Grid {Margin=new Thickness(38,24,38,26)};
            bottom.ColumnDefinitions.Add(new ColumnDefinition());
            bottom.ColumnDefinitions.Add(new ColumnDefinition {Width=GridLength.Auto});
            Error=new TextBlock {Foreground=Muted,TextWrapping=TextWrapping.Wrap,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(0,0,24,0)};
            Actions=new StackPanel {Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right};
            bottom.Children.Add(Error);Grid.SetColumn(Actions,1);bottom.Children.Add(Actions);
            DockPanel.SetDock(bottom,Dock.Bottom);root.Children.Add(bottom);
            Body=new StackPanel();
            var card=new Border {Background=Brushes.White,CornerRadius=new CornerRadius(16),BorderBrush=new SolidColorBrush(Color.FromRgb(221,214,206)),BorderThickness=new Thickness(1),Padding=new Thickness(30),Margin=new Thickness(38,0,38,0),VerticalAlignment=VerticalAlignment.Top,MaxHeight=Height-250};
            card.Child=new ScrollViewer {Content=Body,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};
            workspaceCard=card;workspaceHeader=header;workspaceFooter=bottom;
            SizeChanged += (s,e) => {if(!fullWorkspace)card.MaxHeight = Math.Max(100,ActualHeight-250);};
            root.Children.Add(card);StudioChrome.Attach(this,root);
        }
        protected void UseWorkspace(FrameworkElement content)
        {
            if(workspaceCard.Child is ScrollViewer scroll)scroll.Content=null;
            fullWorkspace=true;
            workspaceHeader.Margin=new Thickness(28,16,28,16);
            workspaceFooter.Margin=new Thickness(24,12,24,14);
            workspaceCard.Margin=new Thickness(24,0,24,0);workspaceCard.Padding=new Thickness(22);
            workspaceCard.VerticalAlignment=VerticalAlignment.Stretch;workspaceCard.MaxHeight=double.PositiveInfinity;
            workspaceCard.Child=content;
        }
        protected void BackButton()
        {
            Action("←  Back", false, () => { BackRequested = true; DialogResult = false; });
        }
        protected void Section(string title,string description)
        {
            Body.Children.Add(new TextBlock {Text=title,FontSize=22,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,8)});
            Body.Children.Add(new TextBlock {Text=description,Foreground=Muted,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,24)});
        }
        protected void Field(string label,FrameworkElement input)
        {
            Body.Children.Add(new TextBlock {Text=label,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,7)});
            input.Margin=new Thickness(0,0,0,17);Body.Children.Add(input);
        }
        protected void FieldPair(string firstLabel, FrameworkElement first, string secondLabel, FrameworkElement second)
        {
            var original = Body;
            var row = new Grid();
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition());
            Body = new StackPanel { Margin = new Thickness(0,0,12,0) };
            Field(firstLabel, first); row.Children.Add(Body);
            Body = new StackPanel { Margin = new Thickness(12,0,0,0) };
            Field(secondLabel, second); Grid.SetColumn(Body,1); row.Children.Add(Body);
            Body = original; Body.Children.Add(row);
        }
        protected Button Action(string label,bool primary,Action callback)
        {
            var button=new Button {Content=label};
            if(!primary){button.Background=Brushes.White;button.Foreground=Ink;button.BorderBrush=new SolidColorBrush(Color.FromRgb(221,214,206));}
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
                throw new Exception(label+": enter a value between "+min+" and "+max+".");
            return value;
        }
    }
}

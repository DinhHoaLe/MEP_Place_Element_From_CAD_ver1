using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shell;
namespace CadLayerTools
{
    internal static class StudioChrome
    {
        public static void Attach(Window window,UIElement content)
        {
            window.WindowStyle=WindowStyle.None;
            WindowChrome.SetWindowChrome(window,new WindowChrome {CaptionHeight=42,ResizeBorderThickness=new Thickness(6),GlassFrameThickness=new Thickness(0),CornerRadius=new CornerRadius(0),UseAeroCaptionButtons=false});
            var root=new Grid();root.RowDefinitions.Add(new RowDefinition {Height=new GridLength(42)});root.RowDefinitions.Add(new RowDefinition());
            var bar=new Grid {Background=new SolidColorBrush(Color.FromRgb(209,200,191))};bar.ColumnDefinitions.Add(new ColumnDefinition());bar.ColumnDefinitions.Add(new ColumnDefinition {Width=GridLength.Auto});
            var brand=new StackPanel {Orientation=Orientation.Horizontal,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(18,0,0,0)};
            var projectSymbol=ModeArtwork.Load("project-symbol");
            window.Icon=projectSymbol;
            brand.Children.Add(new TextBlock {Text=ToolIdentity.Title,FontSize=12,FontWeight=FontWeights.SemiBold,Foreground=ToolWindow.Ink,VerticalAlignment=VerticalAlignment.Center});bar.Children.Add(brand);
            var buttons=new StackPanel {Orientation=Orientation.Horizontal};Grid.SetColumn(buttons,1);bar.Children.Add(buttons);
            var style=(Style)XamlReader.Parse(@"<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='Button'><Setter Property='Width' Value='46'/><Setter Property='Height' Value='42'/><Setter Property='MinHeight' Value='0'/><Setter Property='Margin' Value='0'/><Setter Property='Padding' Value='0'/><Setter Property='Background' Value='Transparent'/><Setter Property='Foreground' Value='#37322B'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='Button'><Border x:Name='surface' Background='{TemplateBinding Background}'><ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/></Border><ControlTemplate.Triggers><Trigger Property='IsMouseOver' Value='True'><Setter TargetName='surface' Property='Background' Value='#B3A491'/></Trigger><Trigger Property='IsKeyboardFocused' Value='True'><Setter TargetName='surface' Property='Background' Value='#B3A491'/></Trigger></ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>");
            Add(buttons,style,"─","Minimize",()=>SystemCommands.MinimizeWindow(window));
            var maximize=Add(buttons,style,"□","Maximize",()=>{if(window.WindowState==WindowState.Maximized)SystemCommands.RestoreWindow(window);else SystemCommands.MaximizeWindow(window);});
            Add(buttons,style,"×","Close",()=>window.Close());
            window.StateChanged+=(s,e)=>{maximize.Content=window.WindowState==WindowState.Maximized?"❐":"□";maximize.ToolTip=window.WindowState==WindowState.Maximized?"Restore":"Maximize";root.Margin=new Thickness(window.WindowState==WindowState.Maximized?8:0);};
            root.Children.Add(bar);Grid.SetRow(content,1);root.Children.Add(content);window.Content=root;
        }
        static Button Add(Panel panel,Style style,string glyph,string label,Action click)
        {
            var button=new Button {Style=style,Content=glyph,ToolTip=label,FontSize=18};WindowChrome.SetIsHitTestVisibleInChrome(button,true);
            System.Windows.Automation.AutomationProperties.SetName(button,label);button.Click+=(s,e)=>click();panel.Children.Add(button);return button;
        }
    }
}


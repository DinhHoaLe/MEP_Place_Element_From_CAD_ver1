using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
namespace CadLayerTools
{
    public class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            Title="place_element_from_cad";Width=460;Height=300;WindowStyle=WindowStyle.None;
            ResizeMode=ResizeMode.NoResize;WindowStartupLocation=WindowStartupLocation.CenterOwner;
            ShowInTaskbar=false;Background=new SolidColorBrush(Color.FromRgb(245,242,238));
            var panel=new StackPanel {Margin=new Thickness(36),VerticalAlignment=VerticalAlignment.Center};
            var logo=new Border {Width=82,Height=82,CornerRadius=new CornerRadius(20),Background=new SolidColorBrush(Color.FromRgb(98,84,60)),Margin=new Thickness(0,0,0,16)};
            logo.Child=new TextBlock {Text="▱\nCAD",FontFamily=new FontFamily("Segoe UI"),FontWeight=FontWeights.Bold,FontSize=23,Foreground=Brushes.White,TextAlignment=TextAlignment.Center,VerticalAlignment=VerticalAlignment.Center};
            panel.Children.Add(logo);
            panel.Children.Add(new TextBlock {Text="PLACE ELEMENT FROM CAD",FontFamily=new FontFamily("Segoe UI"),FontSize=18,FontWeight=FontWeights.SemiBold,Foreground=ToolWindow.Ink,TextAlignment=TextAlignment.Center});
            panel.Children.Add(new TextBlock {Text="Đang đọc model, Revit Link và layer CAD…",FontFamily=new FontFamily("Segoe UI"),Foreground=ToolWindow.Muted,TextAlignment=TextAlignment.Center,Margin=new Thickness(0,12,0,16)});
            panel.Children.Add(new ProgressBar {IsIndeterminate=true,Height=4,Foreground=new SolidColorBrush(Color.FromRgb(98,84,60))});
            Content=new Border {BorderBrush=new SolidColorBrush(Color.FromRgb(221,214,206)),BorderThickness=new Thickness(1),Child=panel};
        }
        public static T Load<T>(IntPtr owner,Func<T> read)
        {
            var window=new LoadingWindow(); if(owner!=IntPtr.Zero)new WindowInteropHelper(window).Owner=owner;
            T value=default(T); Exception error=null; bool finished=false, queued=false;
            // Keep all Revit API reads on the command's UI thread. Render the logo before reading.
            window.Closing+=(s,e)=>{if(!finished)e.Cancel=true;};
            window.ContentRendered+=(s,e)=>{if(queued)return;queued=true;window.Dispatcher.BeginInvoke(DispatcherPriority.Background,new Action(()=>{
                try {value=read();} catch(Exception ex){error=ex;}
                finally {finished=true;window.Close();}
            }));};
            window.ShowDialog();
            if(error!=null) throw new InvalidOperationException("Không đọc được nguồn CAD.",error);
            return value;
        }
    }
}


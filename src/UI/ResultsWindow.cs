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
/// <summary>Hiển thị cặp biên hợp lệ và chẩn đoán; không tạo geometry.</summary>
public class ResultsWindow:ToolWindow
    {
        public ResultsWindow(List<PairRow> rows,int unpaired,int shortCount,int ambiguous,string diagnostic):
            base("02 / 03","Review boundary pairs","Each boundary pair creates one round duct along its centerline.",1080,700)
        {

            var summary=new TextBlock {Text=$"{rows.Count} VALID PAIRS     •     {unpaired} UNPAIRED     •     {shortCount} SHORT SEGMENTS     •     {ambiguous} AMBIGUOUS",
                Foreground=new SolidColorBrush(Color.FromRgb(184,86,105)),FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,18),TextWrapping=TextWrapping.Wrap};
            Body.Children.Add(summary);
            var grid=new DataGrid {ItemsSource=rows,AutoGenerateColumns=false,IsReadOnly=true,CanUserAddRows=false,HeadersVisibility=DataGridHeadersVisibility.Column,
                Background=Brushes.White,RowHeight=40,Height=300,BorderThickness=new Thickness(0),GridLinesVisibility=DataGridGridLinesVisibility.Horizontal,
                HorizontalGridLinesBrush=new SolidColorBrush(Color.FromRgb(232,226,218)),AlternatingRowBackground=new SolidColorBrush(Color.FromRgb(255,247,248)),
                EnableRowVirtualization=true,EnableColumnVirtualization=true};
            string[] names={"A","B","Length","Diameter","Delta","Start","End"};
            string[] headers={"Line A","Line B","Length (mm)","Diameter (mm)","Difference (mm)","Start X,Y","End X,Y"};
            for(int i=0;i<names.Length;i++)grid.Columns.Add(new DataGridTextColumn {
                Header=headers[i],Binding=new Binding(names[i]) {StringFormat=i>=2&&i<=4?"{0:0.###}":null},
                Width=new DataGridLength(i>=5?160:110)});
            Body.Children.Add(grid);
            Body.Children.Add(new Expander {Header="Detection details",Margin=new Thickness(0,18,0,0),
                Content=new TextBlock {Text=diagnostic,TextWrapping=TextWrapping.Wrap,Foreground=Muted,Margin=new Thickness(0,10,0,0)}});
            BackButton();
            Action("Close",false,()=>Close()).IsCancel=true;
            Action("Configure ducts  →",true,()=>DialogResult=true).IsEnabled=rows.Count>0;
        }

    }
}

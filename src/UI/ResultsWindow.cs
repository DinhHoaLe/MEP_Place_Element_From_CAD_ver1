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
            base("02 / 03","Kiểm tra các cặp biên","Mỗi cặp biên tạo một đoạn ống tròn theo đường tim.",1080,700)
        {

            var summary=new TextBlock {Text=$"{rows.Count} CẶP HỢP LỆ     •     {unpaired} CHƯA GHÉP     •     {shortCount} ĐOẠN NGẮN     •     {ambiguous} MƠ HỒ",
                Foreground=new SolidColorBrush(Color.FromRgb(15,118,110)),FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,18),TextWrapping=TextWrapping.Wrap};
            Body.Children.Add(summary);
            var grid=new DataGrid {ItemsSource=rows,AutoGenerateColumns=false,IsReadOnly=true,CanUserAddRows=false,HeadersVisibility=DataGridHeadersVisibility.Column,
                RowHeight=40,Height=360,BorderThickness=new Thickness(0),GridLinesVisibility=DataGridGridLinesVisibility.Horizontal,
                HorizontalGridLinesBrush=new SolidColorBrush(Color.FromRgb(236,240,246)),AlternatingRowBackground=new SolidColorBrush(Color.FromRgb(248,250,253)),
                EnableRowVirtualization=true,EnableColumnVirtualization=true};
            string[] names={"A","B","Length","Diameter","Delta","Start","End"};
            string[] headers={"Line A","Line B","Dài tim (mm)","Diameter (mm)","Chênh (mm)","Điểm đầu X,Y","Điểm cuối X,Y"};
            for(int i=0;i<names.Length;i++)grid.Columns.Add(new DataGridTextColumn {
                Header=headers[i],Binding=new Binding(names[i]) {StringFormat=i>=2&&i<=4?"{0:0.###}":null},
                Width=new DataGridLength(i>=5?160:110)});
            Body.Children.Add(grid);
            Body.Children.Add(new Expander {Header="Chi tiết nhận diện",Margin=new Thickness(0,18,0,0),
                Content=new TextBlock {Text=diagnostic,TextWrapping=TextWrapping.Wrap,Foreground=Muted,Margin=new Thickness(0,10,0,0)}});
            Action("Đóng",false,()=>Close()).IsCancel=true;
            Action("Cấu hình ống gió  →",true,()=>DialogResult=true).IsEnabled=rows.Count>0;
        }

    }
}

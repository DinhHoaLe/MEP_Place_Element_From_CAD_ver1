using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
namespace CadLayerTools
{
    public class CompletionWindow : ToolWindow
    {
        public CompletionWindow(string summary,List<CreatedElementRow> rows=null) : base("04 / 04", "Hoàn tất", "Phần tử được tạo trong file Revit hiện tại",1400,900)
        {
            rows=rows??new List<CreatedElementRow>();
            var layout=new Grid();layout.RowDefinitions.Add(new RowDefinition {Height=GridLength.Auto});layout.RowDefinitions.Add(new RowDefinition());layout.RowDefinitions.Add(new RowDefinition {Height=GridLength.Auto});
            layout.Children.Add(new TextBlock {Text=summary,FontSize=18,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,18)});
            var grid=new DataGrid {ItemsSource=rows,AutoGenerateColumns=false,IsReadOnly=true,CanUserAddRows=false,CanUserDeleteRows=false,HeadersVisibility=DataGridHeadersVisibility.Column,RowHeight=34,Background=System.Windows.Media.Brushes.White,GridLinesVisibility=DataGridGridLinesVisibility.Horizontal,HorizontalGridLinesBrush=new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232,226,218))};
            string[] headers={"Element ID","Category","Family","Type","Level","X (mm)","Y (mm)","Z (mm)","Workset","Mark","Comments"};
            string[] properties={"Id","Category","Family","Type","Level","X","Y","Z","Workset","Mark","Comments"};
            int[] widths={100,160,220,180,140,120,120,120,160,100,220};
            for(int i=0;i<headers.Length;i++)grid.Columns.Add(new DataGridTextColumn {Header=headers[i],Width=widths[i],Binding=new Binding(properties[i]) {StringFormat=i>=5&&i<=7?"{0:0.###}":null}});
            Grid.SetRow(grid,1);layout.Children.Add(grid);
            var note=new TextBlock {Text="Danh sách của lần tạo vừa hoàn tất. XYZ: tọa độ nội bộ model, đơn vị mm; tuyến dùng điểm đầu.\nBack giữ phần tử đã tạo. Undo trong Revit để hoàn tác lần tạo.",Foreground=Muted,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,14,0,0)};
            Grid.SetRow(note,2);layout.Children.Add(note);UseWorkspace(layout);
            BackButton();
            Action("Xuất Excel",false,()=>{
                var dialog=new SaveFileDialog {Title="Lưu danh sách phần tử đã tạo",Filter="Excel Workbook (*.xlsx)|*.xlsx",DefaultExt=".xlsx",AddExtension=true,OverwritePrompt=true,FileName="PhanTuDaTao_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".xlsx"};
                if(dialog.ShowDialog(this)!=true)return;
                try{CreatedElementExcel.Save(dialog.FileName,rows);Error.Text="Đã lưu "+rows.Count+" phần tử: "+dialog.FileName;}
                catch(Exception ex){MessageBox.Show(this,"Không xuất được Excel. "+ex.Message,"Xuất Excel",MessageBoxButton.OK,MessageBoxImage.Warning);}
            }).IsEnabled=rows.Count>0;
            Action("Đóng",true,()=>Close()).IsCancel=true;
        }
    }
}

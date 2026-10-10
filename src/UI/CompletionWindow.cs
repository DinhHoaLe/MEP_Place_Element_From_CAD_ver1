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
        public CompletionWindow(string summary,List<CreatedElementRow> rows=null) : base("04 / 04", "Complete", "Elements are created in the current Revit model",1400,900)
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
            var note=new TextBlock {Text="Results of the latest creation. XYZ: internal model coordinates in mm; routes use their start point.\nBack keeps created elements. Use Undo in Revit to undo the operation.",Foreground=Muted,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,14,0,0)};
            Grid.SetRow(note,2);layout.Children.Add(note);UseWorkspace(layout);
            BackButton();
            Action("Export Excel",false,()=>{
                var dialog=new SaveFileDialog {Title="Save created element list",Filter="Excel Workbook (*.xlsx)|*.xlsx",DefaultExt=".xlsx",AddExtension=true,OverwritePrompt=true,FileName="CreatedElements_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".xlsx"};
                if(dialog.ShowDialog(this)!=true)return;
                try{CreatedElementExcel.Save(dialog.FileName,rows);Error.Text="Saved "+rows.Count+" elements: "+dialog.FileName;}
                catch(Exception ex){MessageBox.Show(this,"Unable to export Excel. "+ex.Message,"Export Excel",MessageBoxButton.OK,MessageBoxImage.Warning);}
            }).IsEnabled=rows.Count>0;
            Action("Continue",true,()=>Close()).IsCancel=true;
        }
    }
}

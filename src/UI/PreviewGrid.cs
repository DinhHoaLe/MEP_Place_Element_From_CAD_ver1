using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
namespace CadLayerTools
{
    internal static class PreviewGrid
    {
        public static DataGrid Create(IEnumerable rows, string[] names, string[] headers)
        {
            var grid = new DataGrid { ItemsSource = rows, AutoGenerateColumns = false, IsReadOnly = true, CanUserAddRows = false,
                Height = 180, RowHeight = 34, Background = Brushes.White, BorderThickness = new Thickness(0),
                HeadersVisibility = DataGridHeadersVisibility.Column, GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
                HorizontalGridLinesBrush = new SolidColorBrush(Color.FromRgb(232,226,218)), Margin = new Thickness(0, 12, 0, 18) };
            for (int i=0; i<names.Length; i++) grid.Columns.Add(new DataGridTextColumn {
                Header = headers[i], Binding = new Binding(names[i]) { StringFormat = "{0:0.###}" }, Width = new DataGridLength(900.0/names.Length), MinWidth = 100 });
            return grid;
        }
    }
}

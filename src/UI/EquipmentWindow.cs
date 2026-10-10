using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
namespace CadLayerTools
{
    public class EquipmentWindow : ToolWindow
    {
        readonly ComboBox layers,categories,families,types,levels,worksets;
        readonly TextBox offset,comments;
        List<EquipmentPoint> points=new List<EquipmentPoint>();
        readonly List<ExistingEquipment> existing;
        readonly Button next,pickHost;
        readonly TextBlock count,previewLabel,hostLabel;
        readonly DataGrid grid;
        readonly CadSymbolPreview preview;
        readonly Image familyPreview;
        readonly TextBlock familyPreviewHint;
        readonly object hostFamilyId;
        readonly Action<EquipmentWindow> place;
        readonly Grid workspace;
        readonly ProgressBar overallProgress;
        readonly TextBlock progressText;
        bool placing;
        bool validConfiguration,updating,ready;
        string loadError;
        public bool PickHostRequested {get;private set;}
        public object TypeId => ((UiChoice)types.SelectedItem).Id;
        public object LevelId => ((UiChoice)levels.SelectedItem).Id;
        public object WorksetId => (worksets.SelectedItem as UiChoice)?.Id;
        public string Comments => comments.Text.Trim();
        public double Offset;
        public List<EquipmentPoint> SelectedPoints => points.Where(p=>p.IsSelected&&!p.HasExisting).ToList();
        public EquipmentWindow(List<UiChoice> layerChoices,Func<object,List<EquipmentPoint>> readLayer,List<UiChoice> symbols,
            List<UiChoice> levelChoices,object defaultLevel,List<ExistingEquipment> existing,CreationSettings saved,
            List<UiChoice> worksetChoices=null,object activeWorkset=null,object hostFamilyId=null,string hostDescription=null,List<string> categoryChoices=null,Func<object,ImageSource> readFamilyPreview=null,Action<EquipmentWindow> place=null)
            :base("03 / 04","Configure and preview","Place equipment · Select a layer, family and rows to create",1400,1040)
        {
            this.place=place;
            this.existing=existing??new List<ExistingEquipment>();this.hostFamilyId=hostFamilyId;
            saved=saved??new CreationSettings();
            var columns=new Grid();workspace=columns;columns.ColumnDefinitions.Add(new ColumnDefinition {Width=new GridLength(1.6,GridUnitType.Star)});
            columns.ColumnDefinitions.Add(new ColumnDefinition {Width=new GridLength(1,GridUnitType.Star)});
            var left=new Grid {Margin=new Thickness(0,0,24,0)};
            foreach(var height in new[]{GridLength.Auto,GridLength.Auto,new GridLength(1,GridUnitType.Star),GridLength.Auto,GridLength.Auto,new GridLength(.6,GridUnitType.Star),GridLength.Auto})left.RowDefinitions.Add(new RowDefinition {Height=height});
            var heading=new TextBlock {Text="Equipment from CAD",FontSize=22,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,16)};left.Children.Add(heading);
            var toolbar=new Grid {Margin=new Thickness(0,0,0,12)};
            toolbar.ColumnDefinitions.Add(new ColumnDefinition());toolbar.ColumnDefinitions.Add(new ColumnDefinition {Width=GridLength.Auto});
            var icons=new StackPanel {Orientation=Orientation.Horizontal};
            var all=SelectionButton("Select all",true);var none=SelectionButton("Deselect all",false);
            all.Click+=(s,e)=>SetSelection(true);none.Click+=(s,e)=>SetSelection(false);icons.Children.Add(all);icons.Children.Add(none);Grid.SetColumn(icons,1);toolbar.Children.Add(icons);
            layers=Choices(layerChoices);Compact(layers);layers.ToolTip="Layer CAD";layers.Margin=new Thickness(0,0,12,0);toolbar.Children.Add(layers);
            Grid.SetRow(toolbar,1);left.Children.Add(toolbar);
            grid=new DataGrid {AutoGenerateColumns=false,CanUserAddRows=false,CanUserDeleteRows=false,Background=Brushes.White,RowHeight=34,
                HeadersVisibility=DataGridHeadersVisibility.Column,SelectionMode=DataGridSelectionMode.Single,
                HorizontalGridLinesBrush=new SolidColorBrush(Color.FromRgb(232,226,218)),BorderBrush=new SolidColorBrush(Color.FromRgb(221,214,206)),GridLinesVisibility=DataGridGridLinesVisibility.Horizontal};
            var check=new FrameworkElementFactory(typeof(CheckBox));check.SetValue(CheckBox.HorizontalAlignmentProperty,HorizontalAlignment.Center);check.SetValue(CheckBox.VerticalAlignmentProperty,VerticalAlignment.Center);
            check.SetBinding(CheckBox.IsCheckedProperty,new Binding("IsSelected") {Mode=BindingMode.TwoWay,UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged});check.SetBinding(CheckBox.IsEnabledProperty,new Binding("CanSelect"));
            grid.Columns.Add(new DataGridTemplateColumn {Header="Select",Width=new DataGridLength(55,DataGridLengthUnitType.Star),CellTemplate=new DataTemplate {VisualTree=check}});
            AddColumn("No.","Index",50);AddColumn("Existing equipment","ExistingStatus",165);AddColumn("X (mm)","X",110);AddColumn("Y (mm)","Y",110);AddColumn("Z (mm)","PlacementZ",110);
            var progressPanel=new FrameworkElementFactory(typeof(StackPanel));
            var progressBar=new FrameworkElementFactory(typeof(ProgressBar));
            progressBar.SetValue(ProgressBar.MaximumProperty,100.0);progressBar.SetValue(ProgressBar.HeightProperty,6.0);progressBar.SetValue(ProgressBar.MarginProperty,new Thickness(4,5,4,2));
            progressBar.SetBinding(ProgressBar.ValueProperty,new Binding("Progress"));progressPanel.AppendChild(progressBar);
            var progressLabel=new FrameworkElementFactory(typeof(TextBlock));progressLabel.SetValue(TextBlock.FontSizeProperty,10.0);progressLabel.SetValue(TextBlock.MarginProperty,new Thickness(4,0,4,0));progressLabel.SetBinding(TextBlock.TextProperty,new Binding("ProgressStatus"));progressPanel.AppendChild(progressLabel);
            grid.Columns.Add(new DataGridTemplateColumn {Header="Progress",IsReadOnly=true,CellTemplate=new DataTemplate {VisualTree=progressPanel}});
            // Explicit viewport widths avoid WPF star-column collapse during the first modal layout.
            grid.SizeChanged+=(s,e)=>FitColumns();grid.Loaded+=(s,e)=>FitColumns();
            Grid.SetRow(grid,2);left.Children.Add(grid);
            count=new TextBlock {Foreground=Muted,Margin=new Thickness(0,8,0,10)};Grid.SetRow(count,3);left.Children.Add(count);
            previewLabel=new TextBlock {Text="CAD symbol",FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,7)};
            var previewTitles=new Grid();previewTitles.ColumnDefinitions.Add(new ColumnDefinition());previewTitles.ColumnDefinitions.Add(new ColumnDefinition());previewTitles.Children.Add(previewLabel);
            var familyTitle=new TextBlock {Text="Revit family",FontWeight=FontWeights.SemiBold,Margin=new Thickness(8,0,0,7)};Grid.SetColumn(familyTitle,1);previewTitles.Children.Add(familyTitle);Grid.SetRow(previewTitles,4);left.Children.Add(previewTitles);
            preview=new CadSymbolPreview {Height=double.NaN,MaxHeight=220,Margin=new Thickness(0,0,8,0)};
            var previews=new Grid();previews.ColumnDefinitions.Add(new ColumnDefinition());previews.ColumnDefinitions.Add(new ColumnDefinition());previews.Children.Add(preview);
            familyPreview=new Image {Stretch=Stretch.Uniform,Margin=new Thickness(8),MaxHeight=204};familyPreviewHint=new TextBlock {Text="Revit has no preview image for this type.",TextWrapping=TextWrapping.Wrap,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(12),Foreground=Muted};
            var familyImageBody=new Grid();familyImageBody.Children.Add(familyPreview);familyImageBody.Children.Add(familyPreviewHint);
            var familyFrame=new Border {Child=familyImageBody,Background=Brushes.White,BorderBrush=new SolidColorBrush(Color.FromRgb(221,214,206)),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(8),Margin=new Thickness(8,0,0,0),MaxHeight=220};Grid.SetColumn(familyFrame,1);previews.Children.Add(familyFrame);Grid.SetRow(previews,5);left.Children.Add(previews);
            var note=new TextBlock {Text="Red +: insertion point · Select a row to preview its symbol.",Foreground=Muted,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,7,0,0)};Grid.SetRow(note,6);left.Children.Add(note);
            grid.SelectionChanged+=(s,e)=>{preview.Point=grid.SelectedItem as EquipmentPoint;previewLabel.Text=preview.Point==null?"CAD symbol":"CAD symbol · #"+preview.Point.Index+(preview.Point.PreviewTruncated?" · maximum 2,000 strokes":"");note.Text="Red +: insertion point · "+(preview.Point?.PreviewInfo??"Select a row to preview its symbol.");};
            columns.Children.Add(left);
            var right=new StackPanel {Margin=new Thickness(20,0,0,0)};Grid.SetColumn(right,1);columns.Children.Add(right);
            right.Children.Add(new TextBlock {Text="Family placement settings",FontSize=22,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,16)});
            categories=new ComboBox {ItemsSource=(categoryChoices??new List<string>()).Concat(symbols.Select(s=>s.CategoryName??"Equipment")).Distinct().OrderBy(s=>s).ToList()};
            families=Choices(new UiChoice[0]);types=Choices(new UiChoice[0]);levels=Choices(levelChoices);
            FieldCompact(right,"Category",categories);FieldCompact(right,"Family",families);FieldCompact(right,"Type",types);
            foreach(var level in levelChoices)if(Equals(level.Id,saved.LevelId??defaultLevel))levels.SelectedItem=level;
            offset=new TextBox {Text=saved.Offset??"0"};
            Pair(right,"Level",levels,"Offset · mm",offset);
            pickHost=new Button {Content="Pick host in Revit",Margin=new Thickness(0),Padding=new Thickness(8,4,8,4),MinHeight=34,Height=34};
            pickHost.Click+=(s,e)=>{if(types.SelectedItem==null)return;PickHostRequested=true;DialogResult=false;};
            FieldCompact(right,"Host",pickHost);
            hostLabel=new TextBlock {Foreground=Muted,TextTrimming=TextTrimming.CharacterEllipsis,Margin=new Thickness(0,0,0,10)};right.Children.Add(hostLabel);
            worksetChoices=worksetChoices??new List<UiChoice>();
            worksets=Choices(worksetChoices.Count>0?worksetChoices:new List<UiChoice> {new UiChoice {Name="No worksets"}});
            worksets.IsEnabled=worksetChoices.Count>0;
            foreach(var ws in worksetChoices)if(Equals(ws.Id,saved.WorksetId??activeWorkset))worksets.SelectedItem=ws;
            comments=new TextBox {Text=saved.Comments??""};
            var advanceBody=new StackPanel {Margin=new Thickness(0,10,0,0)};FieldCompact(advanceBody,"Workset",worksets);FieldCompact(advanceBody,"Comments",comments);
            right.Children.Add(new Expander {Header="Advanced",Content=advanceBody,Margin=new Thickness(0,0,0,10)});
            right.Children.Add(new TextBlock {Text="Checks equipment in the host model at the selected XYZ, with a 1 mm tolerance. Only selected rows without existing equipment are created.",Foreground=Muted,TextWrapping=TextWrapping.Wrap,FontSize=12});
            var sessionLayout=new Grid();sessionLayout.RowDefinitions.Add(new RowDefinition());sessionLayout.RowDefinitions.Add(new RowDefinition {Height=GridLength.Auto});sessionLayout.Children.Add(columns);
            var progressFooter=new StackPanel {Margin=new Thickness(0,12,0,0)};
            overallProgress=new ProgressBar {Height=7,Maximum=100,Foreground=new SolidColorBrush(Color.FromRgb(98,84,60))};
            progressText=new TextBlock {Text="Ready to place",Foreground=Muted,Margin=new Thickness(0,4,0,0)};
            progressFooter.Children.Add(overallProgress);progressFooter.Children.Add(progressText);Grid.SetRow(progressFooter,1);sessionLayout.Children.Add(progressFooter);
            UseWorkspace(sessionLayout);
            BackButton();Action("Cancel",false,()=>Close()).IsCancel=true;
            next=Action("Place equipment",true,PlaceSelected);next.IsDefault=true;
            var savedSymbol=symbols.FirstOrDefault(s=>Equals(s.Id,saved.TypeId));
            categories.SelectionChanged+=(s,e)=>{
                var subset=symbols.Where(t=>(t.CategoryName??"Equipment")==categories.SelectedItem as string).ToList();
                var choices=subset.GroupBy(t=>t.FamilyId??t.FamilyName??t.Name).Select(g=>new UiChoice {Id=g.Key,Name=g.First().FamilyName??g.First().Name}).OrderBy(t=>t.Name).ToList();
                families.ItemsSource=choices;families.SelectedItem=choices.FirstOrDefault(f=>savedSymbol!=null&&Equals(f.Id,savedSymbol.FamilyId??savedSymbol.FamilyName??savedSymbol.Name))??choices.FirstOrDefault();
            };
            families.SelectionChanged+=(s,e)=>{
                var family=families.SelectedItem as UiChoice;
                var choices=symbols.Where(t=>(t.CategoryName??"Equipment")==categories.SelectedItem as string && Equals(t.FamilyId??t.FamilyName??t.Name,family?.Id)).ToList();
                types.ItemsSource=choices;types.SelectedItem=choices.FirstOrDefault(t=>Equals(t.Id,saved.TypeId))??choices.FirstOrDefault();
            };
            types.SelectionChanged+=(s,e)=>{var symbol=types.SelectedItem as UiChoice;bool needs=symbol?.HostKind==EquipmentHostKind.Element||symbol?.HostKind==EquipmentHostKind.Face;
                pickHost.IsEnabled=needs;bool matched=symbol!=null&&Equals(symbol.FamilyId,hostFamilyId)&&hostFamilyId!=null;
                hostLabel.Text=needs?(matched?hostDescription:"Pick a host in the current model."):symbol?.HostKind==EquipmentHostKind.Unsupported?"This family placement type is not supported.":"This family does not require a host.";
                hostLabel.ToolTip=hostLabel.Text;
                familyPreview.Source=symbol==null?null:readFamilyPreview?.Invoke(symbol.Id);familyPreview.ToolTip=symbol==null?null:symbol.FamilyName+" : "+symbol.Name;
                familyPreviewHint.Text=symbol==null?"No family or type is loaded in this category.":"Revit has no preview image for this type.";familyPreviewHint.Visibility=familyPreview.Source==null?Visibility.Visible:Visibility.Collapsed;RefreshPresence();};
            categories.SelectedItem=savedSymbol?.CategoryName??symbols.FirstOrDefault()?.CategoryName??categories.Items.Cast<string>().FirstOrDefault();
            levels.SelectionChanged+=(s,e)=>RefreshPresence();offset.TextChanged+=(s,e)=>RefreshPresence();
            Action load=()=>{
                foreach(var point in points)point.PropertyChanged-=PointChanged;
                try{loadError=null;points=layers.SelectedItem==null?new List<EquipmentPoint>():readLayer(((UiChoice)layers.SelectedItem).Id);}
                catch(Exception ex){loadError="Unable to read layer: "+ex.Message;points=new List<EquipmentPoint>();}
                foreach(var point in points)point.PropertyChanged+=PointChanged;
                grid.ItemsSource=points;grid.SelectedIndex=points.Count>0?0:-1;RefreshPresence();
            };
            layers.SelectionChanged+=(s,e)=>{if(ready)load();};
            if(saved.LayerId!=null)layers.SelectedItem=layerChoices.FirstOrDefault(l=>Equals(l.Id,saved.LayerId))??layerChoices.FirstOrDefault();
            ready=true;load();
            Closing+=(s,e)=>{
                if(placing){e.Cancel=true;return;}
                foreach(var point in points)point.PropertyChanged-=PointChanged;
                saved.LayerId=(layers.SelectedItem as UiChoice)?.Id;saved.TypeId=(types.SelectedItem as UiChoice)?.Id;saved.LevelId=(levels.SelectedItem as UiChoice)?.Id;
                saved.Offset=offset.Text;saved.WorksetId=WorksetId;saved.Comments=comments.Text;
            };
        }
        void PlaceSelected()
        {
            if(placing)return;
            RefreshPresence();if(!validConfiguration||SelectedPoints.Count==0)return;
            if(place==null){DialogResult=true;return;}
            placing=true;workspace.IsEnabled=false;Actions.IsEnabled=false;
            try {place(this);}
            catch(Exception ex){Error.Text=ex.Message;System.Windows.MessageBox.Show(this,ex.Message,"Placement failed",MessageBoxButton.OK,MessageBoxImage.Error);}
            finally {placing=false;workspace.IsEnabled=true;Actions.IsEnabled=true;RefreshPresence();}
        }
        public void UpdateProgress(EquipmentPoint point,int completed,int total,string status)
        {
            grid.ScrollIntoView(point);
            overallProgress.Value=total==0?0:Math.Min(status=="Placement complete"?100:99,100.0*completed/total);
            progressText.Text=status+" · "+completed+" / "+total;
            // Render progress without dispatching input or running model operations on another thread.
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render,new Action(()=>{}));
        }
        public void RefreshExisting(List<ExistingEquipment> snapshot)
        {
            existing.Clear();existing.AddRange(snapshot);RefreshPresence();
        }
        static void Compact(Control control){control.MinHeight=34;control.Height=34;control.Padding=new Thickness(10,5,10,5);}
        static void FieldCompact(Panel panel,string label,Control control)
        {
            panel.Children.Add(new TextBlock {Text=label,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,4)});Compact(control);control.Margin=new Thickness(0,0,0,10);panel.Children.Add(control);
        }
        static void Pair(Panel panel,string first,Control a,string second,Control b)
        {
            var row=new Grid();row.ColumnDefinitions.Add(new ColumnDefinition());row.ColumnDefinitions.Add(new ColumnDefinition());
            var left=new StackPanel {Margin=new Thickness(0,0,8,0)};var right=new StackPanel {Margin=new Thickness(8,0,0,0)};
            FieldCompact(left,first,a);FieldCompact(right,second,b);row.Children.Add(left);Grid.SetColumn(right,1);row.Children.Add(right);panel.Children.Add(row);
        }
        static Button SelectionButton(string label,bool select)
        {
            var icon=new System.Windows.Shapes.Path {Data=Geometry.Parse(select?"M 3,1 L 17,1 L 17,17 L 1,17 L 1,3 M 4,9 L 8,13 L 18,3":"M 1,1 L 17,1 L 17,17 L 1,17 Z M 5,9 L 13,9"),Stroke=select?Brushes.White:Ink,StrokeThickness=1.8,Width=18,Height=18,Stretch=Stretch.Uniform};
            var button=new Button {Content=icon,ToolTip=label,Width=36,Height=34,MinHeight=34,Padding=new Thickness(8),Margin=new Thickness(0,0,8,0)};
            if(!select){button.Background=Brushes.White;button.Foreground=Ink;}System.Windows.Automation.AutomationProperties.SetName(button,label);ToolTipService.SetInitialShowDelay(button,150);return button;
        }
        void AddColumn(string title,string property,double width)=>grid.Columns.Add(new DataGridTextColumn {Header=title,IsReadOnly=true,Width=new DataGridLength(width,DataGridLengthUnitType.Star),Binding=new Binding(property){StringFormat="{0:0.##}"}});
        void FitColumns()
        {
            double available=Math.Max(0,grid.ActualWidth-SystemParameters.VerticalScrollBarWidth-2);
            double[] weights={45,40,135,95,95,95,125};
            if(available<100)return;
            for(int i=0;i<weights.Length;i++)grid.Columns[i].Width=new DataGridLength(available*weights[i]/weights.Sum());
        }
        void PointChanged(object sender,PropertyChangedEventArgs args){if(!updating&&args.PropertyName=="IsSelected")UpdateCount();}
        public void SetSelection(bool selected){updating=true;try{foreach(var point in points)point.IsSelected=selected&&point.CanSelect;}finally{updating=false;}UpdateCount();}
        void RefreshPresence()
        {
            if(!ready)return;validConfiguration=false;updating=true;bool checkedPresence=false;
            try
            {
                if(loadError!=null)throw new Exception(loadError);
                if(types.SelectedItem==null)throw new Exception("No family or type is loaded in this category. Load a family or select another category.");
                if(levels.SelectedItem==null)throw new Exception("Select a level.");
                Offset=Number(offset,-100000,100000,"Offset");
                foreach(var point in points)point.PlacementZ=((UiChoice)levels.SelectedItem).ElevationMm+Offset;
                EquipmentPresence.Update(points,existing,((UiChoice)levels.SelectedItem).ElevationMm+Offset,TypeId);checkedPresence=true;
                var symbol=(UiChoice)types.SelectedItem;
                if(symbol.HostKind==EquipmentHostKind.Unsupported)throw new Exception("This family requires a placement method that is not supported.");
                if(symbol.HostKind!=EquipmentHostKind.None&&(hostFamilyId==null||!Equals(symbol.FamilyId,hostFamilyId)))throw new Exception("Pick a host in Revit before placing this family.");
                validConfiguration=true;Error.Text=points.Count==0?"This layer has no supported blocks.":"Only selected rows without existing equipment will be created.";
            }
            catch(Exception ex){Error.Text=ex.Message;if(!checkedPresence)foreach(var point in points)point.ExistingStatus="Not checked";}
            updating=false;UpdateCount();
        }
        void UpdateCount(){if(next==null)return;count.Text=$"{SelectedPoints.Count} selected / {points.Count} blocks · {points.Count(p=>p.HasExisting)} already placed";next.Content="Place "+SelectedPoints.Count+" devices";next.IsEnabled=!placing&&validConfiguration&&SelectedPoints.Count>0;}
    }
}

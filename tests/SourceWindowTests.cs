using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CadLayerTools;
class SourceWindowTests
{
    static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    static object Field(object instance,string name)=>instance.GetType().GetField(name,Flags).GetValue(instance);
    static ComboBox Combo(object instance,string name)=>(ComboBox)Field(instance,name);
    static IEnumerable<Button> Buttons(ToolWindow window)=>((StackPanel)typeof(ToolWindow).GetField("Actions",Flags).GetValue(window)).Children.OfType<Button>();
    static Button Next(ToolWindow window)=>Buttons(window).First(b=>b.IsDefault);
    static void Check(bool result,string message){if(!result)throw new Exception(message);}
    static void ModalClick(ToolWindow window,Button button)
    {
        window.Loaded+=(s,e)=>window.Dispatcher.BeginInvoke(DispatcherPriority.Background,new Action(()=>button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent))));
        window.ShowDialog();
    }
    static void Back(ToolWindow window){ModalClick(window,Buttons(window).First(b=>b.Content.ToString().Contains("Back")));Check(window.BackRequested,"Back flag");}
    [STAThread] static void Main(string[] args)
    {
        var layerChoices=new List<UiChoice> {new UiChoice {Id=20,Name="MEP-E-PA-EQP"},new UiChoice {Id=30,Name="EMPTY-LAYER"}};
        var cads=new List<CadSource> {new CadSource {Id=1,Name="MEP_Level_01.dwg",Layers=layerChoices}};
        var host=new ModelSource {Name="Host.rvt"};var link=new ModelSource {Name="MEP.rvt · ID 10",LinkId=10,Cads=cads};
        var second=new ModelSource {Name="MEP.rvt · ID 11",LinkId=11,Cads=cads};var unloaded=new ModelSource {Name="Unloaded",LinkId=12,UnavailableReason="Link chưa load"};
        Check(ModelSource.DefaultIndex(new List<ModelSource>{host,link})==0,"Current file default even with no CAD");
        Check(ModelSource.DefaultIndex(new List<ModelSource>{host,link,second})==0,"Current file default with multiple links");
        Check(ModelSource.DefaultIndex(new List<ModelSource>{new ModelSource {Cads=cads},link})==0,"Host CAD preferred");
        var sourceState=new SourceSettings();var sources=new List<ModelSource>{host,link,second,unloaded};
        var source=new SourceWindow(sources,PlacementMode.Equipment,sourceState);
        Check(!Next(source).IsEnabled,"No source chosen");Combo(source,"model").SelectedIndex=1;
        Check(Next(source).IsEnabled&&source.Layers.Count==2&&Equals(source.CadId,1),"Step 2 chooses model/CAD");
        Check(typeof(SourceWindow).GetField("layer",Flags)==null,"Step 2 has no layer field");
        Combo(source,"model").SelectedIndex=3;Check(!Next(source).IsEnabled,"Unloaded source blocked");Combo(source,"model").SelectedIndex=2;Back(source);
        var restoredSource=new SourceWindow(sources,PlacementMode.Equipment,sourceState);Check(Equals(restoredSource.LinkId,11),"Source restored after Back");Render(restoredSource,args[0],"source.png");
        var points=new List<EquipmentPoint>();for(int i=1;i<=13;i++)points.Add(new EquipmentPoint {Index=i,X=1200+i*1000,Y=2400,Angle=90});
        points[0].Preview=new List<CadStroke> {new CadStroke {X1=-150,Y1=-100,X2=150,Y2=-100},new CadStroke {X1=150,Y1=-100,X2=150,Y2=100},new CadStroke {X1=150,Y1=100,X2=-150,Y2=100},new CadStroke {X1=-150,Y1=100,X2=-150,Y2=-100},new CadStroke {X1=-150,Y1=-100,X2=150,Y2=100},new CadStroke {X1=-150,Y1=100,X2=150,Y2=-100}};
        int reads=0;Func<object,List<EquipmentPoint>> read=id=>{reads++;return Equals(id,20)?points:new List<EquipmentPoint>();};
        var symbols=new List<UiChoice> {
            new UiChoice {Id=1,Name="2500 kVA",CategoryName="Electrical Equipment",FamilyId=100,FamilyName="Transformer",HostKind=EquipmentHostKind.None},
            new UiChoice {Id=2,Name="1600 kVA",CategoryName="Electrical Equipment",FamilyId=100,FamilyName="Transformer",HostKind=EquipmentHostKind.None},
            new UiChoice {Id=3,Name="Type 01",CategoryName="Electrical Equipment",FamilyId=200,FamilyName="Wall equipment",HostKind=EquipmentHostKind.Element},
            new UiChoice {Id=4,Name="Type 01",CategoryName="Lighting Fixtures",FamilyId=300,FamilyName="Face light",HostKind=EquipmentHostKind.Face},
            new UiChoice {Id=5,Name="Type 01",CategoryName="Lighting Fixtures",FamilyId=400,FamilyName="Adaptive light",HostKind=EquipmentHostKind.Unsupported}
        };
        var levels=new List<UiChoice> {new UiChoice {Id=1,Name="Basement 5",ElevationMm=0},new UiChoice {Id=2,Name="Level 01",ElevationMm=3000}};
        var existing=new List<ExistingEquipment> {new ExistingEquipment {X=points[0].X,Y=points[0].Y,Z=0,TypeId=1}};
        var state=new CreationSettings {TypeId=1};var worksets=new List<UiChoice>{new UiChoice {Id=1,Name="Shared Levels and Grids"},new UiChoice {Id=2,Name="MEP - Equipment"}};
        var thumbnail=new DrawingImage(new GeometryDrawing(Brushes.LightCoral,null,new RectangleGeometry(new Rect(0,0,50,50))));
        var equipment=new EquipmentWindow(layerChoices,read,symbols,levels,1,existing,state,worksets,2,categoryChoices:new List<string>{"Walls","Furniture"},readFamilyPreview:id=>Equals(id,1)?thumbnail:null);
        Check(ReferenceEquals(((Image)Field(equipment,"familyPreview")).Source,thumbnail),"Selected type thumbnail displayed");
        Combo(equipment,"types").SelectedIndex=1;Check(((Image)Field(equipment,"familyPreview")).Source==null,"Missing thumbnail clears previous image");Combo(equipment,"types").SelectedIndex=0;
        Combo(equipment,"categories").SelectedItem="Furniture";Check(Combo(equipment,"types").Items.Count==0&&!Next(equipment).IsEnabled,"Empty categories visible and cannot create");Combo(equipment,"categories").SelectedItem="Electrical Equipment";
        Check(reads==1&&equipment.SelectedPoints.Count==12,"Initial layer and occupied position");
        Check(Combo(equipment,"families").Items.Count==2&&Combo(equipment,"types").Items.Count==2,"Category/family/type filtering");
        equipment.SetSelection(false);Check(!Next(equipment).IsEnabled,"No rows selected");equipment.SetSelection(true);Check(equipment.SelectedPoints.Count==12,"Select all skips occupied");
        Combo(equipment,"layers").SelectedIndex=1;Check(!Next(equipment).IsEnabled&&equipment.SelectedPoints.Count==0,"Empty layer clears preview/results");
        Combo(equipment,"layers").SelectedIndex=0;Check(reads==3&&equipment.SelectedPoints.Count==12,"Changing layer reloads");
        ((TextBox)Field(equipment,"offset")).Text="1000";Check(!points[0].HasExisting,"Presence follows offset");Check(points.All(p=>p.PlacementZ==1000),"Z column follows Level plus Offset");
        ((TextBox)Field(equipment,"offset")).Text="bad";Check(!Next(equipment).IsEnabled,"Invalid offset blocked");((TextBox)Field(equipment,"offset")).Text="0";
        Combo(equipment,"families").SelectedItem=Combo(equipment,"families").Items.Cast<UiChoice>().First(f=>Equals(f.Id,200));
        Check(!Next(equipment).IsEnabled&&((Button)Field(equipment,"pickHost")).IsEnabled,"Hosted Family requires host");
        ModalClick(equipment,(Button)Field(equipment,"pickHost"));Check(equipment.PickHostRequested&&Equals(state.TypeId,3),"Host pick saves selected family before returning to Revit");
        var hosted=new EquipmentWindow(layerChoices,read,symbols,levels,1,existing,state,worksets,2,200,"Wall · ID 900");Check(Next(hosted).IsEnabled,"Matching selected host unlocks creation");
        Combo(hosted,"categories").SelectedItem="Lighting Fixtures";
        Combo(hosted,"families").SelectedItem=Combo(hosted,"families").Items.Cast<UiChoice>().First(f=>Equals(f.Id,400));
        Check(!Next(hosted).IsEnabled&&!((Button)Field(hosted,"pickHost")).IsEnabled,"Unsupported placement blocked");
        Combo(hosted,"families").SelectedItem=Combo(hosted,"families").Items.Cast<UiChoice>().First(f=>Equals(f.Id,300));Check(!Next(hosted).IsEnabled,"Old host not reused for another family");Back(hosted);
        state.TypeId=1;state.WorksetId=2;state.Comments="Placed from CAD";
        var final=new EquipmentWindow(layerChoices,read,symbols,levels,1,existing,state,worksets,2);
        Check(Equals(final.WorksetId,2)&&final.Comments=="Placed from CAD","Advance values restored");
        final.Height=930;final.Width=1386;
        final.Loaded+=(s,e)=>{foreach(var expander in Visuals<Expander>(final))expander.IsExpanded=true;};
        Render(final,args[0],"equipment-preview.png",true);
        var noWorksets=new EquipmentWindow(layerChoices,read,symbols,levels,1,existing,new CreationSettings {TypeId=1});Check(!Combo(noWorksets,"worksets").IsEnabled&&Combo(noWorksets,"worksets").Text=="Không có WS","Non-workshared text");Back(noWorksets);
        Func<object,object,double,double,double,DuctAnalysis> analyze=(layer,marker,a,b,c)=>Equals(layer,20)?new DuctAnalysis {Pairs=new List<LengthPair>{new LengthPair {A=0,B=1}},Rows=new List<PairRow>{new PairRow {A=1,B=2,Length=2000,Diameter=250}},Diagnostic="Một cặp hợp lệ"}:new DuctAnalysis();
        var ductState=new CreationSettings();var duct=new DuctWindow(levels,levels,levels,1,layerChoices,analyze,ductState);
        Check(duct.Analysis.Pairs.Count==1,"Duct analysis at step 3");Combo(duct,"layer").SelectedIndex=1;Check(!Next(duct).IsEnabled&&duct.Analysis.Pairs.Count==0,"Duct layer refresh");Combo(duct,"layer").SelectedIndex=0;Back(duct);Check(Equals(ductState.LayerId,20),"Duct layer preserved");
        Render(new DuctWindow(levels,levels,levels,1,layerChoices,analyze,ductState),args[0],"duct-preview.png");
        var reportRows=new List<CreatedElementRow>{new CreatedElementRow {Id="4059661",Category="Electrical Equipment",Family="Thiết bị & <CAD>",Type="Type 01",Level="Basement 5",X=261476.954,Y=703350.897,Z=-10700,Comments="=1+1"}};
        CreatedElementExcel.Save(Path.Combine(args[0],"created-elements.xlsx"),reportRows);
        Render(new CompletionWindow("Đã tạo 1 thiết bị",reportRows),args[0],"completion.png");
        var complete=new CompletionWindow("Đã tạo 1 thiết bị",reportRows);Back(complete);
        var mode=new ModeWindow();Check(!Buttons(mode).Any(b=>b.Content.ToString().Contains("Back")),"Step 1 has no Back");((Button)Field(mode,"equipment")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Check(mode.Mode==PlacementMode.Equipment,"Mode button selection");Render(mode,args[0],"mode.png");
        int thread=System.Threading.Thread.CurrentThread.ManagedThreadId;Check(LoadingWindow.Load(IntPtr.Zero,()=>System.Threading.Thread.CurrentThread.ManagedThreadId)==thread,"Loading on UI thread");
        Console.WriteLine("PASS: source-only step 2; layer refresh for equipment/duct; cascade filters; workset/Comments; host gates; selection/presence; Back state; UI render/layout; loading thread.");
    }
    static IEnumerable<T> Visuals<T>(DependencyObject root) where T:DependencyObject
    {
        for(int i=0;i<VisualTreeHelper.GetChildrenCount(root);i++){var child=VisualTreeHelper.GetChild(root,i);if(child is T found)yield return found;foreach(var descendant in Visuals<T>(child))yield return descendant;}
    }
    static void Render(Window window,string directory,string name,bool checkLayout=false)
    {
        window.Show();window.UpdateLayout();
        if(checkLayout)
        {
            var table=(DataGrid)Field(window,"grid");
            Check(table.Columns.All(c=>c.ActualWidth>45),"Readable column widths after layout");
            Check(Math.Abs(table.Columns.Sum(c=>c.ActualWidth)-(table.ActualWidth-SystemParameters.VerticalScrollBarWidth-2))<3,"Columns fill table viewport");
            var card=(Border)typeof(ToolWindow).GetField("workspaceCard",Flags).GetValue(window);
            foreach(var nameField in new[]{"preview","comments","types","layers"})
            {
                var child=(FrameworkElement)Field(window,nameField);var p=child.TransformToAncestor(card).Transform(new Point(0,0));
                Check(p.Y>=0&&p.Y+child.ActualHeight<=card.ActualHeight+1,"No clipped control: "+nameField);
            }
        }
        var bitmap=new RenderTargetBitmap((int)window.ActualWidth,(int)window.ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(window);
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using(var stream=File.Create(Path.Combine(directory,name)))encoder.Save(stream);window.Close();
    }
}

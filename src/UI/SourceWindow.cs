using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
namespace CadLayerTools
{
    public class SourceWindow : ToolWindow
    {
        readonly ComboBox model,cad;
        public object LinkId => ((ModelSource)model.SelectedItem).LinkId;
        public object CadId => ((CadSource)cad.SelectedItem).Id;
        public string CadName => (cad.SelectedItem as CadSource)?.Name;
        public List<UiChoice> Layers => (cad.SelectedItem as CadSource)?.Layers ?? new List<UiChoice>();
        public SourceWindow(List<ModelSource> sources,PlacementMode mode=PlacementMode.Duct,SourceSettings saved=null)
            :base("02 / 04","Chọn nguồn CAD","Chọn model và CAD · Layer sẽ được chọn ở bước 3",1180,820)
        {
            Section("Thông tin nguồn","Dùng CAD trong file hiện tại hoặc trong Revit Link.");
            model=new ComboBox {ItemsSource=sources,DisplayMemberPath="DisplayName"};
            cad=new ComboBox {DisplayMemberPath="Name"};
            Field("Model nguồn / Revit Link",model);Field("CAD trong model",cad);
            BackButton();Action("Hủy",false,()=>Close()).IsCancel=true;
            var next=Action("Tiếp tục  →",true,()=>{if(cad.SelectedItem!=null)DialogResult=true;});next.IsDefault=true;next.IsEnabled=false;
            cad.SelectionChanged+=(s,e)=>{next.IsEnabled=cad.SelectedItem!=null;};
            model.SelectionChanged+=(s,e)=>{
                var selected=model.SelectedItem as ModelSource;
                var cads=selected?.Cads??new List<CadSource>();cad.ItemsSource=cads;cad.IsEnabled=cads.Count>0;
                cad.SelectedIndex=cads.Count>0?0:-1;
                Error.Text=cads.Count>0?"Chọn CAD để tiếp tục.":selected?.UnavailableReason??"Model nguồn chưa có CAD.";
            };
            model.SelectedIndex=ModelSource.DefaultIndex(sources);
            if(saved?.Saved==true)
            {
                model.SelectedItem=sources.FirstOrDefault(m=>Equals(m.LinkId,saved.LinkId));
                cad.SelectedItem=(model.SelectedItem as ModelSource)?.Cads.FirstOrDefault(c=>Equals(c.Id,saved.CadId));
            }
            if(model.SelectedItem==null){cad.IsEnabled=false;Error.Text="Chọn model nguồn trong danh sách.";}
            if(saved!=null)Closing+=(s,e)=>{
                saved.Saved=model.SelectedItem!=null;saved.LinkId=(model.SelectedItem as ModelSource)?.LinkId;saved.CadId=(cad.SelectedItem as CadSource)?.Id;
            };
        }
    }
}

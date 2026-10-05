using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
namespace CadLayerTools
{
    internal class EquipmentHost
    {
        public ElementId FamilyId,ElementId;
        public Reference FaceReference;
        public string Description;
    }
    internal static class EquipmentHostService
    {
        public static EquipmentHost Pick(UIDocument ui,FamilySymbol symbol)
        {
            var kind=EquipmentCreationService.HostKind(symbol);
            if(kind==EquipmentHostKind.Face)
            {
                var reference=ui.Selection.PickObject(ObjectType.Face,new HostFilter(ui.Document,true),"Chọn mặt phẳng host trong file hiện tại (không chọn mặt của Revit Link)");
                return new EquipmentHost {FamilyId=symbol.Family.Id,ElementId=reference.ElementId,FaceReference=reference,Description="Mặt host · ID "+reference.ElementId};
            }
            if(kind==EquipmentHostKind.Element)
            {
                var reference=ui.Selection.PickObject(ObjectType.Element,new HostFilter(ui.Document,false),"Chọn tường/sàn/trần/mái làm host trong file hiện tại");
                var element=ui.Document.GetElement(reference.ElementId);
                return new EquipmentHost {FamilyId=symbol.Family.Id,ElementId=element.Id,Description=element.Name+" · ID "+element.Id};
            }
            throw new InvalidOperationException("Family này không dùng lựa chọn host này.");
        }
        class HostFilter:ISelectionFilter
        {
            readonly Document doc;readonly bool face;
            public HostFilter(Document doc,bool face){this.doc=doc;this.face=face;}
            public bool AllowElement(Element element)=>face?!(element is RevitLinkInstance):element is Wall||element is Floor||element is Ceiling||element is RoofBase;
            public bool AllowReference(Reference reference,XYZ position)=>face&&doc.GetElement(reference.ElementId)?.GetGeometryObjectFromReference(reference) is PlanarFace;
        }
    }
}

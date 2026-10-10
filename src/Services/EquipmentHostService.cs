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
                var reference=ui.Selection.PickObject(ObjectType.Face,new HostFilter(ui.Document,true),"Pick a planar host face in the current model (not a Revit link face)");
                return new EquipmentHost {FamilyId=symbol.Family.Id,ElementId=reference.ElementId,FaceReference=reference,Description="Host face · ID "+reference.ElementId};
            }
            if(kind==EquipmentHostKind.Element)
            {
                var reference=ui.Selection.PickObject(ObjectType.Element,new HostFilter(ui.Document,false),"Pick a wall, floor, ceiling or roof host in the current model");
                var element=ui.Document.GetElement(reference.ElementId);
                return new EquipmentHost {FamilyId=symbol.Family.Id,ElementId=element.Id,Description=element.Name+" · ID "+element.Id};
            }
            throw new InvalidOperationException("This family does not support this host selection.");
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

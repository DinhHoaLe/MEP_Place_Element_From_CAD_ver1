using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
namespace CadLayerTools
{
    internal static class EquipmentCreationService
    {
        public static bool IsEquipmentSymbol(FamilySymbol symbol)=>IsEquipmentCategory(symbol.Category);
        public static EquipmentHostKind HostKind(FamilySymbol symbol)
        {
            switch(symbol.Family.FamilyPlacementType)
            {
                case FamilyPlacementType.OneLevelBased:return EquipmentHostKind.None;
                case FamilyPlacementType.OneLevelBasedHosted:return EquipmentHostKind.Element;
                case FamilyPlacementType.WorkPlaneBased:return EquipmentHostKind.Face;
                default:return EquipmentHostKind.Unsupported;
            }
        }
        static bool IsEquipmentCategory(Category category)
        {
            return category != null;
        }
        public static List<ExistingEquipment> Snapshot(Document doc)
        {
            return new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>()
                .Where(i=>i.Category?.CategoryType==CategoryType.Model && i.Location is LocationPoint)
                .Select(i=>{
                    var p=((LocationPoint)i.Location).Point;
                    return new ExistingEquipment {X=p.X*304.8,Y=p.Y*304.8,Z=p.Z*304.8,TypeId=i.Symbol.Id,Description=i.Symbol.FamilyName+" : "+i.Symbol.Name};
                }).ToList();
        }
        public static ElementCreationResult Create(Document doc, List<EquipmentPoint> points, FamilySymbol symbol, Level level, double offset, EquipmentHost host=null, object worksetId=null, string comments=null)
        {
            var kind=HostKind(symbol);
            if(!IsEquipmentSymbol(symbol)||kind==EquipmentHostKind.Unsupported)throw new InvalidOperationException("Kiểu đặt của Family này chưa hỗ trợ.");
            Element hostElement=null;PlanarFace hostFace=null;
            if(kind!=EquipmentHostKind.None)
            {
                if(host==null||!host.FamilyId.Equals(symbol.Family.Id))throw new InvalidOperationException("Chọn host cho Family đang dùng.");
                hostElement=doc.GetElement(host.ElementId);
                if(hostElement==null)throw new InvalidOperationException("Host không còn tồn tại.");
                if(kind==EquipmentHostKind.Face)
                {
                    hostFace=hostElement.GetGeometryObjectFromReference(host.FaceReference) as PlanarFace;
                    if(hostFace==null)throw new InvalidOperationException("Cần chọn một mặt phẳng host hợp lệ.");
                }
            }
            var result = new ElementCreationResult { CreatedIds = new List<ElementId>() };
            double z = level.ProjectElevation + offset/304.8;
            var occupied = Snapshot(doc);
            using (var tx = new Transaction(doc, "Đặt thiết bị từ CAD"))
            {
                tx.Start();
                var options = tx.GetFailureHandlingOptions();
                options.SetFailuresPreprocessor(new RollbackOnError()); options.SetClearAfterRollback(true); tx.SetFailureHandlingOptions(options);
                try
                {
                    if (!symbol.IsActive) { symbol.Activate(); doc.Regenerate(); }
                    foreach (var point in points.Where(p => p.IsSelected))
                    {
                        var position = new XYZ(point.X/304.8, point.Y/304.8, z);
                        if (occupied.Any(p => EquipmentPresence.SamePosition(point.X,point.Y,z*304.8,p))) { result.DuplicateCount++; continue; }
                        FamilyInstance instance;
                        if(kind==EquipmentHostKind.Face)
                        {
                            hostFace=hostElement.GetGeometryObjectFromReference(host.FaceReference) as PlanarFace;
                            if(hostFace==null)throw new InvalidOperationException("Mặt host đã thay đổi, hãy chọn lại host.");
                            var projected=hostFace.Project(position);
                            if(projected==null||!hostFace.IsInside(projected.UVPoint)||projected.XYZPoint.DistanceTo(position)>0.1/304.8)
                                throw new InvalidOperationException($"Block #{point.Index} không nằm trên mặt host. Chọn lại host hoặc chỉnh Level/Offset; tool không tự dời điểm CAD lên mặt khác.");
                            position=projected.XYZPoint;
                            // The face API requires a tangent direction, independent of CAD rotation.
                            var direction=XYZ.BasisX;
                            direction-=hostFace.FaceNormal*direction.DotProduct(hostFace.FaceNormal);
                            if(direction.GetLength()<1e-7)direction=XYZ.BasisY-hostFace.FaceNormal*XYZ.BasisY.DotProduct(hostFace.FaceNormal);
                            instance=doc.Create.NewFamilyInstance(host.FaceReference,position,direction.Normalize(),symbol);
                        }
                        else
                        {
                            instance=kind==EquipmentHostKind.Element
                                ?doc.Create.NewFamilyInstance(position,symbol,hostElement,level,StructuralType.NonStructural)
                                :doc.Create.NewFamilyInstance(position,symbol,level,StructuralType.NonStructural);
                        }
                        if(kind==EquipmentHostKind.None)
                        {
                            // A level-based family's elevation parameter is relative to its Level,
                            // while position.Z is an absolute model coordinate. Set the offset explicitly.
                            doc.Regenerate();
                            var elevation=instance.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
                            if(elevation!=null&&!elevation.IsReadOnly&&elevation.StorageType==StorageType.Double)
                            {
                                if(!elevation.Set(offset/304.8))throw new InvalidOperationException($"Block #{point.Index}: không gán được Offset so với Level.");
                                doc.Regenerate();
                            }
                            var placed=instance.Location as LocationPoint;
                            if(placed==null)throw new InvalidOperationException($"Block #{point.Index}: Family không trả về điểm đặt.");
                            // Some families expose another elevation constraint. Correct only Z for
                            // unhosted instances; leave XY and orientation untouched, then validate below.
                            double dz=position.Z-placed.Point.Z;
                            if(Math.Abs(dz)>0.1/304.8)
                                ElementTransformUtils.MoveElement(doc,instance.Id,new XYZ(0,0,dz));
                        }
                        if(doc.IsWorkshared && worksetId!=null)
                        {
                            var parameter=instance.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
                            int value=Convert.ToInt32(worksetId);
                            if(parameter==null||parameter.IsReadOnly||!parameter.Set(value)||parameter.AsInteger()!=value)
                                throw new InvalidOperationException("Không gán được Workset đã chọn.");
                        }
                        if(!string.IsNullOrWhiteSpace(comments))
                        {
                            var parameter=instance.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
                            if(parameter==null||parameter.IsReadOnly||!parameter.Set(comments))throw new InvalidOperationException("Không gán được Comments.");
                        }
                        doc.Regenerate();
                        var location = instance.Location as LocationPoint;
                        if(location==null)throw new InvalidOperationException($"Block #{point.Index}: Family không trả về điểm đặt. Đã hủy lần tạo.");
                        if(kind==EquipmentHostKind.None&&!instance.LevelId.Equals(level.Id))
                            throw new InvalidOperationException($"Block #{point.Index}: Family không thuộc Level đã chọn. Đã hủy lần tạo.");
                        if(location.Point.DistanceTo(position)>0.1/304.8)
                            throw new InvalidOperationException($"Block #{point.Index}: tọa độ Family không khớp (mm). Yêu cầu: X={position.X*304.8:0.###}, Y={position.Y*304.8:0.###}, Z={position.Z*304.8:0.###}. Thực tế: X={location.Point.X*304.8:0.###}, Y={location.Point.Y*304.8:0.###}, Z={location.Point.Z*304.8:0.###}. Đã hủy lần tạo.");
                        if(kind==EquipmentHostKind.Element && (instance.Host==null||!instance.Host.Id.Equals(host.ElementId)))throw new InvalidOperationException("Family không gắn đúng host đã chọn.");
                        if(kind==EquipmentHostKind.Face && (instance.HostFace==null||!instance.HostFace.ElementId.Equals(host.ElementId)))throw new InvalidOperationException("Family không gắn đúng mặt host đã chọn.");
                        result.CreatedIds.Add(instance.Id); occupied.Add(new ExistingEquipment {X=point.X,Y=point.Y,Z=z*304.8,TypeId=symbol.Id});
                    }
                    if (tx.Commit() != TransactionStatus.Committed) throw new InvalidOperationException("Revit từ chối đặt thiết bị; đã hoàn tác.");
                }
                catch { if (tx.GetStatus() == TransactionStatus.Started) tx.RollBack(); throw; }
            }
            return result;
        }
    }
}

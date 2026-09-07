using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Windows.Interop;


namespace CadLayerTools
{
/// <summary>Tạo toàn bộ duct trong một transaction. Kiểm tra trùng, gán Diameter, xác minh kết quả và hoàn tác nếu lỗi.</summary>
internal static class DuctCreationService
{
public static DuctCreationResult Create(Document doc,List<Boundary> lines,List<LengthPair> pairs,ElementId systemId,ElementId typeId,ElementId levelId,double z)
{
                            var ids=new List<ElementId>();int duplicates=0;
                            var verified=new List<Tuple<Duct,Route>>();
                            var existing=new FilteredElementCollector(doc).OfClass(typeof(Duct)).Cast<Duct>().ToList();
                            // Mọi thay đổi model nằm trong một transaction: Undo một lần cho cả lô, lỗi thì rollback.
using(var tx=new Transaction(doc,"Tạo ống gió tròn từ CAD"))
                            {
                                tx.Start();
                                try
                                {
                                    foreach(var pair in pairs)
                                    {
                                        // Dùng cùng hàm tính tim với bảng preview: Length là khoảng cách A–B, Diameter là khoảng cách hai biên.
var route=CenterlineService.Create(lines[pair.A],lines[pair.B],pair.Gap);
                                        if(route.Width<=0)throw new Exception("Khoảng cách hai biên không hợp lệ.");
                                        if(existing.Any(d=>DuctDuplicateService.SameDuct(d,route,z))){duplicates++;continue;}
                                        // Chuyển mm về feet đúng một lần khi gọi Revit API. z đã ở feet; không nhân 25,4 hoặc làm tròn Diameter.
var duct=Duct.Create(doc,(ElementId)systemId,(ElementId)typeId,(ElementId)levelId,
                                            new XYZ(route.A.X/304.8,route.A.Y/304.8,z),
                                            new XYZ(route.B.X/304.8,route.B.Y/304.8,z));
                                        DuctDimensionService.SetDimension(duct,BuiltInParameter.RBS_CURVE_DIAMETER_PARAM,route.Width/304.8);
                                        doc.Regenerate();DuctValidationService.VerifyDuct(duct,route,z);verified.Add(Tuple.Create(duct,route));ids.Add(duct.Id);existing.Add(duct);
                                    }
                                    doc.Regenerate();
                                    foreach(var item in verified)DuctValidationService.VerifyDuct(item.Item1,item.Item2,z);
                                    var failureOptions=tx.GetFailureHandlingOptions();
                                    failureOptions.SetFailuresPreprocessor(new RollbackOnError());
                                    failureOptions.SetClearAfterRollback(true);tx.SetFailureHandlingOptions(failureOptions);
                                    if(tx.Commit()!=TransactionStatus.Committed)throw new Exception("Revit từ chối tạo duct; đã hoàn tác cả lần tạo.");
                                }
                                catch {if(tx.GetStatus()==TransactionStatus.Started)tx.RollBack();throw;}
                            }
return new DuctCreationResult { CreatedIds=ids, DuplicateCount=duplicates };
}
}
}

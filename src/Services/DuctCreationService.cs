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
public static ElementCreationResult Create(Document doc,List<Boundary> lines,List<LengthPair> pairs,ElementId systemId,ElementId typeId,ElementId levelId,double z,Action<int,int,int,string> progress=null)
{
                            var ids=new List<ElementId>();int duplicates=0;int index=0;var skipped=new HashSet<int>();
                            var verified=new List<Tuple<Duct,Route>>();
                            var existing=new FilteredElementCollector(doc).OfClass(typeof(Duct)).Cast<Duct>().ToList();
                            // Mọi thay đổi model nằm trong một transaction: Undo một lần cho cả lô, lỗi thì rollback.
using(var tx=new Transaction(doc,"Create round ducts from CAD"))
                            {
                                tx.Start();
                                try
                                {
                                    foreach(var pair in pairs)
                                    {
                                        // Dùng cùng hàm tính tim với bảng preview: Length là khoảng cách A–B, Diameter là khoảng cách hai biên.
progress?.Invoke(index,10,index,"Creating");
var route=CenterlineService.Create(lines[pair.A],lines[pair.B],pair.Gap);
                                        if(route.Width<=0)throw new Exception("The boundary spacing is invalid.");
                                        if(existing.Any(d=>DuctDuplicateService.SameDuct(d,route,z))){duplicates++;skipped.Add(index);progress?.Invoke(index,100,index+1,"Skipped: existing");index++;continue;}
                                        // Chuyển mm về feet đúng một lần khi gọi Revit API. z đã ở feet; không nhân 25,4 hoặc làm tròn Diameter.
var duct=Duct.Create(doc,(ElementId)systemId,(ElementId)typeId,(ElementId)levelId,
                                            new XYZ(route.A.X/304.8,route.A.Y/304.8,z),
                                            new XYZ(route.B.X/304.8,route.B.Y/304.8,z));
                                        DuctDimensionService.SetDimension(duct,BuiltInParameter.RBS_CURVE_DIAMETER_PARAM,route.Width/304.8);
                                        doc.Regenerate();DuctValidationService.VerifyDuct(duct,route,z);verified.Add(Tuple.Create(duct,route));ids.Add(duct.Id);existing.Add(duct);progress?.Invoke(index,90,index+1,"Pending commit");index++;
                                    }
                                    doc.Regenerate();
                                    foreach(var item in verified)DuctValidationService.VerifyDuct(item.Item1,item.Item2,z);
                                    var failureOptions=tx.GetFailureHandlingOptions();
                                    failureOptions.SetFailuresPreprocessor(new RollbackOnError());
                                    failureOptions.SetClearAfterRollback(true);tx.SetFailureHandlingOptions(failureOptions);
                                    if(tx.Commit()!=TransactionStatus.Committed)throw new Exception("Revit rejected duct creation; the entire operation was rolled back.");
                                }
                                catch {if(tx.GetStatus()==TransactionStatus.Started)tx.RollBack();for(int i=0;i<pairs.Count;i++)progress?.Invoke(i,0,0,"Rolled back");throw;}
                            }
for(int i=0;i<pairs.Count;i++)progress?.Invoke(i,100,pairs.Count,skipped.Contains(i)?"Skipped":"Created");
return new ElementCreationResult { CreatedIds=ids, DuplicateCount=duplicates };
}
}
}

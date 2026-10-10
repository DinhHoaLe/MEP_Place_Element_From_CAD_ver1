using System;
using System.Collections.Generic;
using System.Linq;
namespace CadLayerTools
{
/// <summary>Thuật toán đang dùng: đoạn nối xác nhận hai biên cùng chiều dài, song song và chạy cùng phía.</summary>
public static class MarkerMatcher
    {
        public static string Diagnostics { get; private set; }
        public static List<LengthPair> Pair(List<Boundary> lines,List<Boundary> markers,
            double lengthTolerance,double endpointTolerance,double minLength,out int ambiguous)
        {
            var proposals=new Dictionary<string,LengthPair>();
            ambiguous=0; int shortMarkers=0, matchedEnds=0, lengthRejected=0, directionRejected=0, angleRejected=0, gapRejected=0; double closest=double.PositiveInfinity;
            foreach(var m in markers)
            {
                if(m.Length<endpointTolerance*2){shortMarkers++;continue;}
                var left=new List<int>();var right=new List<int>();
                for(int i=0;i<lines.Count;i++)
                {
                    var b=lines[i];
                    if(b.Length<minLength)continue;
                    closest=System.Math.Min(closest,System.Math.Min(
                        System.Math.Min((b.A-m.A).Length,(b.B-m.A).Length),
                        System.Math.Min((b.A-m.B).Length,(b.B-m.B).Length)));
                    if((b.A-m.A).Length<=endpointTolerance || (b.B-m.A).Length<=endpointTolerance)left.Add(i);
                    if((b.A-m.B).Length<=endpointTolerance || (b.B-m.B).Length<=endpointTolerance)right.Add(i);
                }
                if(left.Any(i=>right.Any(j=>i!=j)))matchedEnds++;
                var valid=new List<LengthPair>();
                foreach(int i in left)foreach(int j in right)
                {
                    if(i==j)continue;
                    var a=lines[i];var b=lines[j];
                    // Đoạn nối phải ngắn hơn hai biên để không nhận cả hai hướng của một hình chữ nhật. Vì vậy đoạn ống dài không hơn Diameter có thể bị bỏ qua.
if(m.Length>=System.Math.Min(a.Length,b.Length)-endpointTolerance)continue;
                    if(System.Math.Abs(a.Length-b.Length)>lengthTolerance+1e-8){lengthRejected++;continue;}
                    var aStart=(a.A-m.A).Length<=(a.B-m.A).Length?a.A:a.B;
                    var aEnd=object.ReferenceEquals(aStart,a.A)?a.B:a.A;
                    var bStart=(b.A-m.B).Length<=(b.B-m.B).Length?b.A:b.B;
                    var bEnd=object.ReferenceEquals(bStart,b.A)?b.B:b.A;
                    var u=(aEnd-aStart)*(1/a.Length);var v=(bEnd-bStart)*(1/b.Length);
                    // Hai biên phải chạy cùng phía từ đoạn nối và song song trong khoảng 0,05 độ.
if(u.Dot(v)<=0 || System.Math.Abs(u.Cross(v))>0.000872665){directionRejected++;continue;}
                    if(System.Math.Abs(u.Dot(m.Direction))>0.000872665){angleRejected++;continue;}
                    double gap=System.Math.Abs(u.Cross(bStart-aStart));
                    if(gap<=endpointTolerance*2){gapRejected++;continue;}
                    valid.Add(new LengthPair {A=System.Math.Min(i,j),B=System.Math.Max(i,j),
                        Gap=gap,Delta=System.Math.Abs(a.Length-b.Length)});
                }
                valid=valid.GroupBy(p=>p.A+":"+p.B).Select(g=>g.First()).ToList();
                // Chỉ nhận khi một marker xác định duy nhất một cặp. Marker ở cả hai đầu dùng cùng khóa để không tạo hai lần.
if(valid.Count==1)proposals[valid[0].A+":"+valid[0].B]=valid[0];
                else if(valid.Count>1)ambiguous++;
            }
            // Nếu một biên thuộc nhiều cặp được đề xuất, loại các cặp xung đột thay vì đoán.
var counts=new Dictionary<int,int>();
            foreach(var p in proposals.Values)foreach(int i in new[]{p.A,p.B})
                counts[i]=counts.ContainsKey(i)?counts[i]+1:1;
            var result=proposals.Values.Where(p=>counts[p.A]==1&&counts[p.B]==1).OrderBy(p=>p.A).ToList();
            ambiguous+=proposals.Count-result.Count;
            Diagnostics=$"Segments on connector layer: {markers.Count}; too short: {shortMarkers}.\r\n"+
                $"Connectors with endpoints touching distinct boundaries: {matchedEnds}.\r\n"+
                $"Rejected pairs: length mismatch {lengthRejected}; not parallel or on the same side {directionRejected}; not perpendicular {angleRejected}; spacing too small {gapRejected}.";
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
namespace CadLayerTools
{
/// <summary>Thuật toán cũ: ưu tiên khoảng cách gần nhất, mỗi line dùng một lần. Không dùng trong workflow hiện tại.</summary>
public static class UniqueLengthMatcher
    {
        // Greedy shortest gap first; deterministic ties; not a maximum-cardinality matching.
        public static List<LengthPair> Pair(List<Boundary> lines,double tolerance,double minLength,double maxGap)
        {
            var ordered=lines.Select((b,i)=>new {B=b,I=i}).Where(x=>x.B.Length>=minLength)
                .OrderBy(x=>x.B.Length).ToList();
            var candidates=new List<LengthPair>();
            for(int i=0;i<ordered.Count;i++)
                for(int j=i+1;j<ordered.Count;j++)
                {
                    var a=ordered[i];var b=ordered[j];
                    double delta=b.B.Length-a.B.Length;
                    if(delta>tolerance+1e-8)break;
                    var u=a.B.Direction;
                    if(System.Math.Abs(u.Cross(b.B.Direction))>0.000872664626)continue;
                    double d1=u.Cross(b.B.A-a.B.A),d2=u.Cross(b.B.B-a.B.A);
                    if(d1*d2<=0 || System.Math.Abs(d1-d2)>1)continue;
                    double gap=System.Math.Abs((d1+d2)/2);
                    if(gap<1 || gap>maxGap)continue;
                    double p=(b.B.A-a.B.A).Dot(u),q=(b.B.B-a.B.A).Dot(u);
                    double overlap=System.Math.Min(a.B.Length,System.Math.Max(p,q))-System.Math.Max(0,System.Math.Min(p,q));
                    if(overlap<System.Math.Min(a.B.Length,b.B.Length)*0.8)continue;
                    candidates.Add(new LengthPair {A=a.I,B=b.I,Gap=gap,Delta=delta});
                }
            var used=new HashSet<int>();var result=new List<LengthPair>();
            foreach(var p in candidates.OrderBy(p=>p.Gap).ThenBy(p=>p.Delta).ThenBy(p=>p.A).ThenBy(p=>p.B))
                if(!used.Contains(p.A)&&!used.Contains(p.B))
                {used.Add(p.A);used.Add(p.B);result.Add(p);}
            return result;
        }
    }
}

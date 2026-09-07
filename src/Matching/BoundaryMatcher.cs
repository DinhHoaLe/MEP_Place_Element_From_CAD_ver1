using System;
using System.Collections.Generic;
using System.Linq;
namespace CadLayerTools
{
/// <summary>Thuật toán cũ: ghép biên theo hướng, vị trí và khoảng cách. Giữ để tham khảo/test; workflow hiện tại không gọi.</summary>
public static class BoundaryMatcher
    {
        const double Angle = 0.000872664625997; // 0.05 degrees
        const double Epsilon = 1.0; // 1 mm positional tolerance

        public static MatchResult Match(IEnumerable<Boundary> input,double minWidth,double maxWidth,double minLength)
        { return Match(input,minWidth,maxWidth,minLength,null,0); }

        public static MatchResult Match(IEnumerable<Boundary> input,double minWidth,double maxWidth,
            double minLength,double[] diameters,double tolerance)
        {
            if(minWidth<=2 || maxWidth<minWidth || minLength<=0 || tolerance<0)
                throw new ArgumentException("Invalid limits.");
            var source=input.ToList();
            var candidates=new List<Route>();
            int total=0;
            // Conservative direction buckets: 0.001 degree. No global all-pairs scan.
            var groups=source.Where(b=>b.Length>=1).GroupBy(b=>{
                double a=Math.Atan2(b.B.Y-b.A.Y,b.B.X-b.A.X);
                if(a<0)a+=Math.PI;
                if(a>=Math.PI)a-=Math.PI;
                return (long)Math.Round(a/(Math.PI/180000))%180000;
            });
            foreach(var group in groups)
            {
                double angle=group.Key*Math.PI/180000;
                var u=new V2(Math.Cos(angle),Math.Sin(angle));
                var normal=new V2(-u.Y,u.X);
                var projected=group.Select(b=>new Projected {
                    Start=Math.Min(b.A.Dot(u),b.B.Dot(u)),End=Math.Max(b.A.Dot(u),b.B.Dot(u)),
                    Offset=(b.A.Dot(normal)+b.B.Dot(normal))/2
                }).OrderBy(b=>b.Offset).ThenBy(b=>b.Start).ToList();
                // Cluster collinear offsets, then union sorted longitudinal intervals.
                var merged=new List<Projected>();
                for(int i=0;i<projected.Count;)
                {
                    double offset=projected[i].Offset;
                    int end=i+1;
                    while(end<projected.Count && projected[end].Offset-offset<=Epsilon)end++;
                    var cluster=projected.GetRange(i,end-i).OrderBy(b=>b.Start).ToList();
                    Projected current=null;
                    foreach(var b in cluster)
                    {
                        if(current!=null && b.Start<=current.End+Epsilon)
                            current.End=Math.Max(current.End,b.End);
                        else
                        {
                            current=new Projected {Start=b.Start,End=b.End,Offset=offset};
                            merged.Add(current);
                        }
                    }
                    i=end;
                }
                merged=merged.OrderBy(b=>b.Offset).ToList();
                foreach(var b in merged)b.Id=total++;
                foreach(var a in merged)
                {
                    var ranges=diameters==null
                        ? new[]{new[]{minWidth,maxWidth}}
                        : diameters.Select(d=>new[]{Math.Max(minWidth,d-tolerance),Math.Min(maxWidth,d+tolerance)}).ToArray();
                    var seen=new HashSet<int>();
                    foreach(var range in ranges)
                    {
                        int lo=0,hi=merged.Count;
                        double lower=a.Offset+range[0],upper=a.Offset+range[1];
                        while(lo<hi){int m=(lo+hi)/2;if(merged[m].Offset<lower)lo=m+1;else hi=m;}
                        for(int j=lo;j<merged.Count && merged[j].Offset<=upper;j++)
                        {
                            var b=merged[j];
                            if(b.Id==a.Id || !seen.Add(b.Id))continue;
                            double start=Math.Max(a.Start,b.Start),end=Math.Min(a.End,b.End);
                            if(end-start<minLength || (end-start)/Math.Min(a.End-a.Start,b.End-b.Start)<0.8)continue;
                            double width=b.Offset-a.Offset;
                            candidates.Add(new Route {
                                A=u*start+normal*((a.Offset+b.Offset)/2),
                                B=u*end+normal*((a.Offset+b.Offset)/2),
                                Width=width,I=a.Id,J=b.Id
                            });
                        }
                    }
                }
            }
            var counts=new int[total];
            foreach(var r in candidates){counts[r.I]++;counts[r.J]++;}
            var unique=candidates.Where(r=>counts[r.I]==1 && counts[r.J]==1)
                .OrderBy(r=>Math.Min(r.A.X,r.B.X)).ToList();
            // Spatial sweep: compare only overlapping X/Y bounds.
            var active=new List<Route>();var blocked=new HashSet<Route>();
            foreach(var r in unique)
            {
                double x=Math.Min(r.A.X,r.B.X);
                active.RemoveAll(a=>Math.Max(a.A.X,a.B.X)<x);
                foreach(var a in active)
                    if(Math.Max(a.A.Y,a.B.Y)>=Math.Min(r.A.Y,r.B.Y) &&
                       Math.Min(a.A.Y,a.B.Y)<=Math.Max(r.A.Y,r.B.Y) && Crosses(a,r))
                    {blocked.Add(a);blocked.Add(r);}
                active.Add(r);
            }
            return new MatchResult {
                Routes=unique.Where(r=>!blocked.Contains(r)).ToList(),Cleaned=total,
                Removed=source.Count-total,Unpaired=counts.Count(n=>n==0),
                Ambiguous=candidates.Count-unique.Count+blocked.Count
            };
        }
        static bool Crosses(Route a, Route b)
        {
            V2 u=a.B-a.A,v=b.B-b.A,w=b.A-a.A;
            double cross=u.Cross(v);
            if(Math.Abs(cross)<1e-8) return false;
            double t=w.Cross(v)/cross,s=w.Cross(u)/cross;
            return t>=0 && t<=1 && s>=0 && s<=1;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
namespace CadLayerTools
{
/// <summary>Điểm/vector 2D của mặt bằng CAD, đơn vị mm. Không chứa cao độ Z.</summary>
public class V2
    {
        public double X, Y;
        public V2(double x, double y) { X = x; Y = y; }
        public static V2 operator +(V2 a, V2 b) => new V2(a.X+b.X,a.Y+b.Y);
        public static V2 operator -(V2 a, V2 b) => new V2(a.X-b.X,a.Y-b.Y);
        public static V2 operator *(V2 a, double b) => new V2(a.X*b,a.Y*b);
        public double Dot(V2 b) => X*b.X+Y*b.Y;
        public double Cross(V2 b) => X*b.Y-Y*b.X;
        public double Length => Math.Sqrt(Dot(this));
    }
}

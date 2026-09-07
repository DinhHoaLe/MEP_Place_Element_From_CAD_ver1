using System;
using System.Collections.Generic;
using System.Linq;
namespace CadLayerTools
{
/// <summary>Một đoạn biên thẳng với hai đầu A/B theo tọa độ nội bộ Revit đã đổi sang mm.</summary>
public class Boundary
    {
        public V2 A, B;
        public Boundary(V2 a, V2 b) { A=a; B=b; }
        public double Length => (B-A).Length;
        public V2 Direction => (B-A)*(1/Length);
    }
}

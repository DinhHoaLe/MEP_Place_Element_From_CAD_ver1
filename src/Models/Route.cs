using System;
using System.Collections.Generic;
using System.Linq;
namespace CadLayerTools
{
/// <summary>Tuyến tim dự kiến; A/B là hai đầu tim, Width được dùng làm Diameter cho duct tròn. Đơn vị mm.</summary>
public class Route
    {
        public V2 A, B;
        public double Width;
        public int I, J;
        public double Length => (B-A).Length;
    }
}

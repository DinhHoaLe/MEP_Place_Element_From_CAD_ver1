using System;
using System.Collections.Generic;
using System.Linq;
namespace CadLayerTools
{
/// <summary>Một cặp biên. A/B là chỉ số zero-based trong danh sách đọc CAD, không phải handle AutoCAD.</summary>
public class LengthPair
    {
        public int A,B;
        public double Gap,Delta;
    }
}

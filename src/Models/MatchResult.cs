using System;
using System.Collections.Generic;
using System.Linq;
namespace CadLayerTools
{
/// <summary>Kết quả và số liệu thống kê của thuật toán ghép theo khoảng cách cũ.</summary>
public class MatchResult
    {
        public List<Route> Routes = new List<Route>();
        public int Cleaned, Ambiguous, Unpaired, Removed;
    }
}

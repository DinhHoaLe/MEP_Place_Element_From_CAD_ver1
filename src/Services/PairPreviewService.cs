using System.Collections.Generic;
namespace CadLayerTools
{
    /// <summary>Chuẩn bị dữ liệu bảng xem trước từ cùng cách tính tim dùng khi tạo duct.</summary>
    internal static class PairPreviewService
    {
        public static List<PairRow> Create(List<Boundary> lines, List<LengthPair> pairs)
        {
            var rows = new List<PairRow>();
            foreach (var pair in pairs)
            {
                var route = CenterlineService.Create(lines[pair.A], lines[pair.B], pair.Gap);
                rows.Add(new PairRow
                {
                    A = pair.A + 1,
                    B = pair.B + 1,
                    Length = route.Length,
                    Diameter = route.Width,
                    Delta = pair.Delta,
                    Start = $"{route.A.X:0.##}, {route.A.Y:0.##}",
                    End = $"{route.B.X:0.##}, {route.B.Y:0.##}"
                });
            }
            return rows;
        }
    }
}

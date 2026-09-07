using System;
namespace CadLayerTools
{
    /// <summary>Coordinates and diameter are in mm. Handles reversed CAD endpoints.</summary>
    /// <summary>Tính tim từ hai biên. Căn cùng chiều hai line trước khi lấy trung điểm; tọa độ và Diameter đều là mm.</summary>
    internal static class CenterlineService
    {
        public static Route Create(Boundary first, Boundary second, double diameter)
        {
            V2 start = second.A;
            V2 end = second.B;
            if ((first.B - first.A).Dot(second.B - second.A) < 0)
            {
                start = second.B;
                end = second.A;
            }
            return new Route
            {
                A = (first.A + start) * 0.5,
                B = (first.B + end) * 0.5,
                Width = diameter
            };
        }
    }
}

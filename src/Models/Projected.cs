namespace CadLayerTools
{
    /// <summary>Intermediate interval used by the legacy whole-layer matcher.</summary>
    /// <summary>Khoảng chiếu trung gian của thuật toán cũ: vị trí dọc hướng và độ lệch vuông góc, đơn vị mm.</summary>
    internal class Projected
    {
        public double Start, End, Offset;
        public int Id;
    }
}

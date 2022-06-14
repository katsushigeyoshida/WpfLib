using System.Windows;

namespace WpfLib
{
    /// <summary>
    /// Pointの整数版(整数のPoint構造体)
    /// System.Drawing.Point(Int32)の代用
    /// System.WindowsのPointはDoubleのため、整数型が使えない
    /// </summary>
    public class PointI
    {
        public int X { get; set; }
        public int Y { get; set; }

        public PointI(int x, int y)
        {
            X = x;
            Y = y;
        }

        public PointI(Point dp)
        {
            X = (int)dp.X;
            Y = (int)dp.Y;
        }

        public void Add(PointI point)
        {
            X += point.X;
            Y += point.Y;
        }

        public bool Equals(PointI point)
        {
            return (X == point.X && Y == point.Y);
        }

        public void Offset(int x, int y)
        {
            X += x;
            Y += y;
        }

        public bool Parse(string str)
        {
            char[] separator = { ',', ' ' };
            string[] point = str.Split(separator);
            if (2 <= point.Length) {
                int tx, ty;
                if (int.TryParse(point[0], out tx) && int.TryParse(point[1], out ty)) {
                    X = tx;
                    Y = ty;
                    return true;
                }
            }
            return false;
        }

        public void Subtruct(PointI point)
        {
            X -= point.X;
            Y -= point.Y;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]", X, Y);
        }
    }
}

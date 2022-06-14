using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfLib
{
    /// <summary>
    /// Sizeの整数版
    /// System.Drawing.Size(Int32)の代用
    /// System.WindowsにはSizeがないため
    /// </summary>
    public class SizeI
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public SizeI(int width, int height)
        {
            Width  = Math.Abs(width);
            Height = Math.Abs(height);
        }

        public SizeI(PointI point)
        {
            Width  = Math.Abs(point.X);
            Height = Math.Abs(point.Y);
        }

        public SizeI(SizeI size)
        {
            Width  = size.Width;
            Height = size.Height;
        }

        public SizeI(Size size)
        {
            Width  = (int)size.Width;
            Height = (int)size.Height;
        }

        public bool Equals(SizeI size)
        {
            return (Width == size.Width && Height == size.Height);
        }

        public bool Parse(string str)
        {
            char[] separator = { ',', ' ' };
            string[] point = str.Split(separator);
            if (2 <= point.Length) {
                int tw, th;
                if (int.TryParse(point[0], out tw) && int.TryParse(point[1], out th)) {
                    Width = Math.Abs(tw);
                    Height = Math.Abs(th);
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]", Width, Height);
        }
    }
}

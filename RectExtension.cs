using System.Windows;

namespace WpfLib
{
    /// <summary>
    /// System.Windows.Pointの拡張クラス
    /// </summary>
    public static class RectExtension
    {
        /// <summary>
        /// Rectの領域を拡張する
        /// </summary>
        /// <param name="rect">Rect(自身の値を更新するにはrefが必要)</param>
        /// <param name="p">拡張する座標</param>
        public static void Extension(this ref Rect rect, Point p) {
            if (p.X < rect.X) {
                rect.Width += rect.X - p.X;
                rect.X = p.X;
            } else if (rect.Right < p.X) {
                rect.Width += p.X - rect.Right;
            }
            if (p.Y < rect.Y) {
                rect.Height += rect.Y - p.Y;
                rect.Y = p.Y;
            } else if (rect.Bottom < p.Y) {
                rect.Height += p.Y - rect.Bottom;
            }
        }

        /// <summary>
        /// Rectの領域を拡張する
        /// </summary>
        /// <param name="rect">自Rect</param>
        /// <param name="r">拡張Rect</param>
        public static void Extension(this ref Rect rect, Rect r) {
            rect.Extension(r.BottomRight);
            rect.Extension(r.TopLeft);
        }
    }
}

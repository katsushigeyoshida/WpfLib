using System;
using System.Windows;

namespace WpfLib
{
    /// <summary>
    /// Rectクラスの代用で四角の領域を定義するクラス
    /// Rectでは必ず Top < Bottom,Left < Right の関係に正規化してしまうため
    /// ワールド座標で天地を逆にすることができないため作成したクラス
    /// 基本はRectと同じだが正規化はしない
    /// 
    /// プロパティ
    ///     Left,Top,Right,Bottom
    ///     TopLeft,BottomRight
    ///     Width,Height
    ///     Location,Size,
    /// コンストラクタ
    ///     Box(Size size)
    ///     Box(Point ps, Point pe)
    ///     Box(Point ps, Size size)
    ///     Box(Point ps, Vector vector)
    ///     Box(double left, double top, double right, double bottom)
    /// 
    /// </summary>
    public class Box
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Right { get; set; }
        public double Bottom { get; set; }
        public Point TopLeft {
            get {
                return new Point(Left, Top);
            }
        }
        public Point BottomRight {
            get {
                return new Point(Right, Bottom);
            }
        }
        public double Width {
            get {
                return Math.Abs(Right - Left);
            }
            set {
                Right = Left + value;
            }
        }
        public double Height {
            get {
                return Math.Abs(Top - Bottom);
            }
            set {
                Bottom = Top - value;
            }
        }
        public Point Location {
            get {
                return new Point(Left, Top);
            }
            set {
                Left = value.X;
                Top = value.Y;
            }
        }
        public Size Size {
            get {
                return new Size(Math.Abs(Right - Left), Math.Abs(Top - Bottom));
            }
            set {
                Right = Left + value.Width;
                Bottom = Top + value.Height;
            }
        }

        public Box(Size size)
        {
            Left = 0;
            Top = 0;
            Right = Left + size.Width;
            Bottom = Top + size.Height;
        }

        public Box(Point ps, Point pe)
        {
            Left = ps.X;
            Top = ps.Y;
            Right = pe.X;
            Bottom = pe.Y;
        }

        public Box(Point ps, Size size)
        {
            Left = ps.X;
            Top = ps.Y;
            Right = Left + size.Width;
            Bottom = Top + size.Height;
        }

        public Box(Point ps, Vector vector)
        {
            Left = ps.X;
            Top = ps.Y;
            Right = Left + vector.X;
            Bottom = Top + vector.Y;
        }

        public Box(double left, double top, double right, double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public override string ToString()
        {
            return Left + " " + Top + " " + Right + " " + Bottom;
        }

        /// <summary>
        /// Pointの内外判定
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool insideChk(Point p)
        {
            if (Left < Right) {
                if (p.X < Left || Right < p.X)
                    return false;
            } else {
                if (p.X < Right || Left < p.X)
                    return false;
            }
            if (Top < Bottom) {
                if (p.Y < Top || Bottom < p.Y)
                    return false;
            } else {
                if (p.Y < Bottom || Top < p.Y)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// LineDデータの内外判定
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public bool insideChk(LineD l)
        {
            if (insideChk(l.ps) && insideChk(l.pe))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Rectデータの内外判定
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public bool insideChk(Rect r)
        {
            if (insideChk(r.TopLeft) && insideChk(r.BottomRight))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Boxデータの内外判定
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool insideChk(Box b)
        {
            if (insideChk(b.TopLeft) && insideChk(b.BottomRight))
                return true;
            else
                return false;
        }
    }
}

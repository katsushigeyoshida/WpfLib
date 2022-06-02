using System;
using System.Windows;

namespace WpfLib
{
    /// <summary>
    /// System.Windows.Pointの拡張クラス
    /// </summary>
    public static class PointExtension
    {
        /// <summary>
        /// 符号を反転する
        /// </summary>
        /// <param name="p">自座標</param>
        public static void invert(this ref Point p)
        {
            p.X *= -1.0;
            p.Y *= -1.0;
        }

        /// <summary>
        /// 原点からの角度(rad)
        /// </summary>
        /// <param name="p">自座標</param>
        /// <returns>角度(rad)</returns>
        public static double angle(this Point p)
        {
            return Math.Atan2(p.Y, p.X );
        }

        /// <summary>
        /// 指定点を原点とした角度(rad)
        /// </summary>
        /// <param name="p">自座標</param>
        /// <param name="pos">指定点</param>
        /// <returns>角度</returns>
        public static double angle(this Point p, Point pos)
        {
            double dx = pos.X - p.X;
            double dy = pos.Y - p.Y;
            return Math.Atan2(dy, dx);
        }

        /// <summary>
        /// 原点からの長さ
        /// </summary>
        /// <param name="p">自座標</param>
        /// <returns>長さ</returns>
        public static double length(this Point p)
        {
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        /// <summary>
        /// 指定点との距離
        /// </summary>
        /// <param name="p">自座標</param>
        /// <param name="pos">指定点</param>
        /// <returns>距離</returns>
        public static double distance(this Point p, Point pos)
        {
            double dx = pos.X - p.X;
            double dy = pos.Y - p.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 指定点とのベクトルを求める
        /// </summary>
        /// <param name="p">自座標</param>
        /// <param name="pos">指定点</param>
        /// <returns>ベクトル</returns>
        public static Point vector(this Point p, Point pos)
        {
            return new Point(pos.X - p.X, pos.Y - p.Y); 
        }

        /// <summary>
        /// 原点を基準としてスケーリングする
        /// </summary>
        /// <param name="p">自座標</param>
        /// <param name="scale">スケール</param>
        public static void scale(this ref Point p, double scale)
        {
            p.X *= scale;
            p.Y *= scale;
        }
    }
}

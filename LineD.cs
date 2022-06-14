using System;
using System.Windows;

namespace WpfLib
{
    /// <summary>
    /// 実数(double)のLineクラス
    /// </summary>
    public class LineD
    {
        public Point ps;
        public Point pe;
        private double mEps = 1E-8;

        public LineD(double psx, double psy, double pex, double pey)
        {
            ps = new Point(psx, psy);
            pe = new Point(pex, pey);
        }

        public LineD(Point ps, Point pe)
        {
            this.ps = new Point(ps.X, ps.Y);
            this.pe = new Point(pe.X, pe.Y);
        }

        /// <summary>
        /// ベクトルに変換
        /// </summary>
        /// <returns></returns>
        public Point vector()
        {
            return new Point(pe.X - ps.X, pe.Y - ps.Y);
        }

        /// <summary>
        /// Revtに変換
        /// </summary>
        /// <returns></returns>
        public Rect getRect()
        {
            return new Rect(ps, pe);
        }

        /// <summary>
        /// 線分の長さ
        /// </summary>
        /// <returns></returns>
        public double length()
        {
            Point v = vector();
            return Math.Sqrt(v.X * v.X + v.Y * v.Y);
        }

        /// <summary>
        /// ベクトルとしての角度(-π ～ π)
        /// </summary>
        /// <returns></returns>
        public double angle()
        {
            Point v = vector();
            return Math.Atan2(v.Y, v.X);
        }

        /// <summary>
        /// 2線分の角度(0 ～ π)
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public double angle(LineD l)
        {
            return Math.Abs(angle() - l.angle());
        }

        /// <summary>
        /// 平行線の判定
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public bool isParalell(LineD l)
        {
            Point v1 = vector();
            Point v2 = l.vector();
            return Math.Abs(v1.X * v2.Y - v2.X * v1.Y) < mEps;
        }

        /// <summary>
        /// 2線分の交点
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public Point intersection(LineD l)
        {
            Point v1 = vector();
            Point v2 = l.vector();
            double k = v1.X * v2.Y - v2.X * v1.Y;
            if (Math.Abs(k) < mEps)
                return new Point(double.NaN, double.NaN);
            double x = (-1 / k) * (v1.Y * v2.X * this.ps.X - v2.Y * v1.X * l.ps.X + (l.ps.Y - this.ps.Y) * v1.X * v2.X);
            double y = (1 / k) * (v1.X * v2.Y * this.ps.Y - v2.X * v1.Y * l.ps.Y + (l.ps.X - this.ps.X) * v1.Y * v2.Y);
            return new Point(x, y);
        }

        /// <summary>
        /// 点が線分上にあるかを判定
        /// </summary>
        /// <param name="pnt"></param>
        /// <returns></returns>
        public bool lineOnPoint(Point pnt)
        {
            if (pe.X < ps.X) {
                if (pnt.X < (pe.X - mEps) || (ps.X + mEps) < pnt.X)
                    return false;
            } else {
                if (pnt.X < (ps.X - mEps) || (pe.X + mEps) < pnt.X)
                    return false;
            }
            if (pe.Y < ps.Y) {
                if (pnt.Y < (pe.Y - mEps) || (ps.Y + mEps) < pnt.Y)
                    return false;
            } else {
                if (pnt.Y < (ps.Y - mEps) || (pe.Y + mEps) < pnt.Y)
                    return false;
            }
            return true;
        }

        private const int INSIDE = 0b0000;
        private const int LEFT = 0b0001;
        private const int RIGHT = 0b0010;
        private const int BOTTOM = 0b0100;
        private const int TOP = 0b1000;

        /// <summary>
        /// 点の位置
        /// Cohen-Sutherland aalgorithm
        ///
        /// 1001|1001|1010
        /// --------------
        /// 0001|0000|0010
        /// --------------
        /// 0101|0110|0110
        /// </summary>
        /// <param name="p">座標</param>
        /// <param name="rect">クリッピング領域</param>
        /// <returns></returns>
        private byte inOutAreaCode(Point p, Rect rect)
        {
            double xmin = rect.X;
            double ymin = rect.Y;
            double xmax = rect.X + rect.Width;
            double ymax = rect.Y + rect.Height;
            byte code = INSIDE;
            if (p.X < xmin) code |= LEFT;
            else if (xmax < p.X) code |= RIGHT;
            if (p.Y < ymin) code |= BOTTOM;
            else if (ymax < p.Y) code |= TOP;
            return code;
        }

        ///
        /// 線分矩形領域でクリッピングする(Cohen-Sutherland clipping algorithm)
        /// @param rect      クリッピングの矩形領域
        /// @return
        ///
        public LineD clippingLine(Rect rect)
        {
            double xmin = rect.X;
            double ymin = rect.Y;
            double xmax = rect.X + rect.Width;
            double ymax = rect.Y + rect.Height;
            LineD l2 = new LineD(ps, pe);
            byte outcode0 = inOutAreaCode(l2.ps, rect);
            byte outcode1 = inOutAreaCode(l2.pe, rect);

            while (true) {
                if ((outcode0 | outcode1) == 0) {         //  両端点がフレームの中(クリッピング不要)
                    break;
                } else if ((outcode0 & outcode1) != 0) {   //  確実に表示されない線分
                    l2 = null;
                    break;
                } else {                                //  クリッピング処理
                    double x = 0, y = 0;
                    int outcode = outcode0 != 0 ? outcode0 : outcode1;
                    if ((outcode & TOP) != 0) {
                        x = l2.ps.X + (l2.pe.X - l2.ps.X) * (ymax - l2.ps.Y) / (l2.pe.Y - l2.ps.Y);
                        y = ymax;
                    } else if ((outcode & BOTTOM) != 0) {
                        x = l2.ps.X + (l2.pe.X - l2.ps.X) * (ymin - l2.ps.Y) / (l2.pe.Y - l2.ps.Y);
                        y = ymin;
                    } else if ((outcode & RIGHT) != 0) {
                        y = l2.ps.Y + (l2.pe.Y - l2.ps.Y) * (xmax - l2.ps.X) / (l2.pe.X - l2.ps.X);
                        x = xmax;
                    } else if ((outcode & LEFT) != 0) {
                        y = l2.ps.Y + (l2.pe.Y - l2.ps.Y) * (xmin - l2.ps.X) / (l2.pe.X - l2.ps.X);
                        x = xmin;
                    }
                    if (outcode == outcode0) {
                        l2.ps.X = x;
                        l2.ps.Y = y;
                        outcode0 = inOutAreaCode(l2.ps, rect);
                    } else {
                        l2.pe.X = x;
                        l2.pe.Y = y;
                        outcode1 = inOutAreaCode(l2.pe, rect);
                    }
                }
            }
            return l2;
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        public Point TopLeft
        {
            get {
                return new Point(Left, Top);
            }
        }
        public Point BottomRight
        {
            get {
                return new Point(Right, Bottom);
            }
        }
        public double Width
        {
            get {
                return Math.Abs(Right - Left);
            }
            set {
                Right = Left + value;
            }
        }
        public double Height
        {
            get {
                return Math.Abs(Top - Bottom);
            }
            set {
                Bottom = Top - value;
            }
        }
        public Point Location
        {
            get {
                return new Point(Left, Top);
            }
            set {
                Left = value.X;
                Top = value.Y;
            }
        }
        public Size Size
        {
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
            return new Point(pe.X - ps.X, pe.Y - pe.Y);
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
            return Math.Atan2(v.X, v.Y);
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
            double y = ( 1 / k) * (v1.X * v2.Y * this.ps.Y - v2.X * v1.Y * l.ps.Y + (l.ps.X - this.ps.X) * v1.Y * v2.Y);
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
            if (p.X < xmin)      code |= LEFT;
            else if (xmax < p.X) code |= RIGHT;
            if (p.Y < ymin)      code |= BOTTOM;
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

    /// <summary>
    /// WFP ワールド座標のグラフィックライブラリ
    /// YDrawingShapesをベースにワールド座標系に対応
    /// 
    /// YDrawShapes( )  コンストラクタ
    /// YDrawShapes(Canvas c)   コンストラクタ
    /// YDrawShapes(Canvas c, double windowWidth, double windowHeight)  コンストラクタ
    /// etWindowSize(double width, double height)   スクリーン座標でのWindowサイズを設定を設定
    /// setViewArea(double left, double top, double right, double bottom)   Viewエリアの設定(スクリーン座標)
    /// setWorldWindow(double left, double top, double right, double bottom)    論理座標の設定(ワールド座標)
    /// setTextSize(double size)    文字サイズの設定
    /// setScreenTextSize(double size)  スクリーン座標で文字の大きさを設定
    /// getTextSize()       設定されている文字サイズの取得
    /// setAspectFix(bool fix)      論理描画のアスペクト比を1に固定
    /// cnvWorld2Screen(Point wp)   論理座標(ワールド座標)をスクリーン座標に変換
    /// cnvScreen2World(Point sp)   スクリーン座標を論理座標(ワールド座標)に変換
    /// cnvWorld2Screen(Rect wrect) 論理座標のRectをスクリーン座標のRectに変換
    /// cnvWordRect2Box(Rect wrect) RectクラスをBoxクラスに変換
    /// cnvWorld2ScreenX(double x)  X軸のワールド座標をスクリーン座標に変換
    /// cnvScreen2WorldX(double x)  X軸のスクリーン座標をワールド座標に変換
    /// cnvWorld2ScreenY(double y)  Y軸のワールド座標をスクリーン座標に変換
    /// cnvScreen2WorldY(double y)  Y軸のスクリーン座標をワールド座標に変換
    /// Size scaleSize(Size wsize)  ワールド座標のSizeをスクリーン座標のSizeに変換する
    /// double world2screenXlength(double x)    論理座標でのX方向の長さからスクリーン座標の長さを求める
    /// double world2screenYlength(double y)    論理座標でのY方向の長さからスクリーン座標の長さを求める
    /// double screen2worldXlength(double x)    スクリーン座標のX方向長さをワールド座標の長さに変換
    /// double screen2worldYlength(double y)    スクリーン座標のY方向長さをワールド座標の長さに変換
    /// drawLine(Point ps, Point pe)    線分の描画
    /// drawLine(double xs, double ys, double xe,double ye) 線分の描画
    /// drawArc(Point center, double radius, double startAngle, double endAngle)    円弧の描画
    /// drawArc(double cx, double cy, double radius, double startAngle, double endAngle)    円弧の描画
    /// drawCircle(double cx, double cy, double radius) 円の描画
    /// drawEllipse(Point center, Size radius, double startAngle, double endAngle, double rotate)   楕円弧の描画
    /// drawEllipse(double cx, double cy, double rx, double ry, double startAngle, double endAngle, double rotate)  楕円弧の描画
    /// drawOval(Rect rect, double rotate)  楕円の描画
    /// drawOval(Point center, double radiusX, double radiusY, double rotate)   楕円の描画
    /// drawRectangle(Point ps, Point pe, double rotate)    四角形の描画
    /// drawRectangle(Rect rect, double rotate) 四角形の描画
    /// drawRectangle(double left, double top, double width, double height, double rotate)  四角形の描画
    /// drawText(string text, double left, double top, double rotate)   文字列の描画
    /// drawText(string text, Point p, double rotate)   文字列の描画
    /// drawText(string text, Point p, double rotate, HorizontalAlignment ha, VerticalAlignment va) 文字列の描画
    /// measureText(string text)    文字列の大きさを取得
    /// drawBitmap(System.Drawing.Bitmap bitmap, Rect rect) リソースファイルを表示する
    /// drawBitmap(System.Drawing.Bitmap bitmap, Point org, Size size)
    /// drawImageFile(string filePath, Rect rect)   イメージファイルを表示する
    /// drawImageFile(string filePath, Point org, Size size)
    /// 
    /// </summary>
    public class YWorldShapes : YDrawingShapes
    {
        public Box mWorld;                  //  ワールド座標
        public Rect mView;                  //  描画領域
        public bool mClipping = false;      //  クリッピングの有無
        private bool mInvert = false;       //  倒立
        private bool mAspectFix = true;     //  アスペクト比固定
        private double mTextSize = 20;      //  テキストサイズ
        private bool mTextOverWrite = true; //  文字が重なった時に前の文字を透かす

        public YWorldShapes()
        {

        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="c">Canvas</param>
        public YWorldShapes(Canvas c)
        {
            mCanvas = c;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="c">Panel/Canvas</param>
        /// <param name="windowWidth">スクリーン座標の幅</param>
        /// <param name="windowHeight">スクリーン座標の高さ</param>
        public YWorldShapes(Canvas c, double windowWidth, double windowHeight)
        {
            mCanvas = c;
            mWindowWidth = windowWidth;
            mWindowHeight = windowHeight;

            mView = new Rect(0, 0, mWindowWidth, mWindowHeight);
            mWorld = new Box(0, 0, mWindowWidth, mWindowHeight);
            if (mAspectFix)
                aspectFix();
            setTextSize(mTextSize);
        }

        /// <summary>
        /// スクリーン座標でのWindowサイズを設定を設定
        /// </summary>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        public override void setWindowSize(double width, double height)
        {
            base.setWindowSize(width < 1 ? 100 : width, height < 1 ? 100 : height);
            mView = new Rect(0, 0, mWindowWidth, mWindowHeight);
            mWorld = new Box(0, 0, mWindowWidth, mWindowHeight);
            if (mAspectFix)
                aspectFix();
            setTextSize(mTextSize);
        }


        /// <summary>
        /// Viewエリアの設定(スクリーン座標)
        /// </summary>
        /// <param name="left">左座標</param>
        /// <param name="top">上座標</param>
        /// <param name="right">右座標</param>
        /// <param name="bottom">下座標</param>
        public void setViewArea(double left, double top, double right, double bottom)
        {
            //  Rectにデータを入れるとleft<right, top<bottomの関係に補正される
            mView = new Rect(new Point(left, top), new Point(right, bottom));
            if (mAspectFix)
                aspectFix();
            setTextSize(mTextSize);
        }

        /// <summary>
        /// 論理座標の設定(ワールド座標)
        /// topとbottomの値にかかわらず上向きを正とし、left.rightは右向きを正とする
        /// </summary>
        /// <param name="left">左座標</param>
        /// <param name="top">上座標</param>
        /// <param name="right">右座標</param>
        /// <param name="bottom">下座標</param>
        public void setWorldWindow(double left, double top, double right, double bottom)
        {
            //  Rectにデータを入れるとleft<right, top<bottomの関係に補正される
            mWorld = new Box(new Point(left, top), new Point(right, bottom));
            mInvert = top < bottom;
            if (mAspectFix)
                aspectFix();
            setTextSize(mTextSize);
        }

        /// <summary>
        /// 文字サイズの設定
        /// </summary>
        /// <param name="size"></param>
        public override void setTextSize(double size)
        {
            this.mTextSize = size;
            base.setTextSize(Math.Abs(world2screenYlength(size)));
        }

        /// <summary>
        /// スクリーン座標で文字の大きさを設定をする
        /// </summary>
        /// <param name="size">文字の大きさ</param>
        public void setScreenTextSize(double size)
        {
            base.setTextSize(size);
        }


        /// <summary>
        /// 設定されている文字サイズの取得
        /// </summary>
        /// <returns>文字サイズ</returns>
        public override double getTextSize()
        {
            return base.getTextSize() / Math.Abs(world2screenYlength(1));
        }


        /// <summary>
        /// 文字を上書きする場合に透かすようにする
        /// </summary>
        /// <param name="b">true:透かす / false:領域を塗潰す</param>
        public void setTextOverWrite(bool b)
        {
            mTextOverWrite = b;
        }

        /// <summary>
        /// 論理描画のアスペクト比を1に固定する
        /// しない場合はWindowの形状に合わせてアスペクト比が変わる
        /// </summary>
        /// <param name="fix">アスペクト比固定</param>
        public void setAspectFix(bool fix)
        {
            mAspectFix = fix;
        }

        /// <summary>
        /// 論理座標のアスペクト比を1にするように論理座標を変更する
        /// 常にビューポート内に収まるようにする
        /// </summary>
        private void aspectFix()
        {
            //  ビューポートに合わせて論理座標の大きさを求める
            double wHeight = mWorld.Width * mView.Height / mView.Width;
            double wWidth = mWorld.Height * mView.Width / mView.Height;
            //  縦横で小さい方を論理座標をビューポートに合わせる
            if (mWorld.Height < wHeight) {
                double dy = Math.Sign(mWorld.Bottom - mWorld.Top) * (wHeight - mWorld.Height);
                mWorld = new Box(new Point(mWorld.Left, mWorld.Top - dy / 2),
                    new Point(mWorld.Right, mWorld.Bottom + dy / 2));
            } else {
                double dx = Math.Sign(mWorld.Right - mWorld.Left) * (wWidth - mWorld.Width);
                mWorld = new Box(new Point(mWorld.Left - dx / 2, mWorld.Top),
                    new Point(mWorld.Right + dx / 2, mWorld.Bottom));
            }
        }

        /// <summary>
        /// 論理座標(ワールド座標)をスクリーン座標に変換する
        /// </summary>
        /// <param name="wp">ワールド座標</param>
        /// <returns>スクリーン座標</returns>
        private Point cnvWorld2Screen(Point wp)
        {
            Point sp = new Point();
            sp.X = cnvWorld2ScreenX(wp.X);
            sp.Y = cnvWorld2ScreenY(wp.Y);
            return sp;
        }

        private Size cnvWorld2Screen(Size wSize)
        {
            Size sSize = new Size();
            sSize.Width = cnvWorld2ScreenX(wSize.Width);
            sSize.Height = cnvWorld2ScreenY(wSize.Height);
            return sSize;
        }

        /// <summary>
        /// スクリーン座標を論理座標(ワールド座標)に変換
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public Point cnvScreen2World(Point sp)
        {
            Point wp = new Point();
            wp.X = cnvScreen2WorldX(sp.X);
            wp.Y = cnvScreen2WorldY(sp.Y);
            return wp;
        }

        public Size cnvScreen2World(Size sSize)
        {
            Size wSize = new Size();
            wSize.Width = cnvScreen2WorldX(sSize.Width);
            wSize.Height = cnvScreen2WorldY(sSize.Height);
            return wSize;
        }

        /// <summary>
        /// 論理座標のRectをスクリーン座標のRectに変換する
        /// </summary>
        /// <param name="wrect"></param>
        /// <returns></returns>
        private Rect cnvWorld2Screen(Rect wrect)
        {
            //  Rectにデータを入れるとleft<right, top<bottomの関係に補正されるので
            //  mWorldのTopとBottomは入れ替え変換することによってY軸を反転する
            Point ps = wrect.TopLeft;
            Point pe = new Point(
                mWorld.Left < mWorld.Right ? wrect.Left + wrect.Width : wrect.Left - wrect.Width,
                mWorld.Top < mWorld.Bottom ? wrect.Top + wrect.Height : wrect.Top - wrect.Height);
            Rect rect = new Rect(cnvWorld2Screen(ps), cnvWorld2Screen(pe));
            return rect;
        }

        /// <summary>
        /// RectクラスをBoxクラスに変換する
        /// </summary>
        /// <param name="wrect"></param>
        /// <returns></returns>
        public Box cnvWordRect2Box(Rect wrect)
        {
            //  Rectにデータを入れるとleft<right, top<bottomの関係に補正されるので
            //  mWorldのTopとBottomは入れ替え変換することによってY軸を反転する
            Point ps = wrect.TopLeft;
            Point pe = new Point(
                mWorld.Left < mWorld.Right ? wrect.Left + wrect.Width : wrect.Left - wrect.Width,
                mWorld.Top < mWorld.Bottom ? wrect.Top + wrect.Height : wrect.Top - wrect.Height);
            Box box = new Box(ps, pe);
            return box;
        }

        /// <summary>
        /// X軸のワールド座標をスクリーン座標に変換する
        /// </summary>
        /// <param name="x">ワールド座標</param>
        /// <returns>スクリーン座標</returns>
        public double cnvWorld2ScreenX(double x)
        {
            return (x - mWorld.Left) * (mView.Right - mView.Left) / (mWorld.Right - mWorld.Left) + mView.Left;
        }

        /// <summary>
        /// X軸のスクリーン座標をワールド座標に変換
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double cnvScreen2WorldX(double x)
        {
            return (x - mView.Left) * (mWorld.Right - mWorld.Left) / (mView.Right - mView.Left) + mWorld.Left;
        }

        /// <summary>
        /// Y軸のワールド座標をスクリーン座標に変換する
        /// </summary>
        /// <param name="y">ワールド座標</param>
        /// <returns>スクリーン座標</returns>
        public double cnvWorld2ScreenY(double y)
        {
            return (y - mWorld.Top) * (mView.Top - mView.Bottom) / (mWorld.Top - mWorld.Bottom) + mView.Top;
        }

        /// <summary>
        /// Y軸のスクリーン座標をワールド座標に変換
        /// </summary>
        /// <param name="y">スクリーン座標</param>
        /// <returns>ワールド座標</returns>
        public double cnvScreen2WorldY(double y)
        {
            return (y - mView.Top) * (mWorld.Bottom - mWorld.Top) / (mView.Bottom - mView.Top) + mWorld.Top;
        }

        /// <summary>
        /// ワールド座標のSizeをスクリーン座標のSizeに変換する
        /// </summary>
        /// <param name="wsize"></param>
        /// <returns></returns>
        private Size scaleSize(Size wsize)
        {
            Size size = new Size();
            size.Width  = Math.Abs(world2screenXlength(wsize.Width));
            size.Height = Math.Abs(world2screenYlength(wsize.Height));
            return size;
        }

        /// <summary>
        /// 論理座標でのX方向の長さからスクリーン座標の長さを求める
        /// </summary>
        /// <param name="x">ワールド座標でのX軸方向長さ</param>
        /// <returns>スクリーン座標での長さ</returns>
        public double world2screenXlength(double x)
        {
            return x * (mView.Right - mView.Left) / (mWorld.Right - mWorld.Left);
        }

        /// <summary>
        /// 論理座標でのY方向の長さからスクリーン座標の長さを求める
        /// </summary>
        /// <param name="y">ワールド座標でのY軸方向の長さ</param>
        /// <returns>スクリーン座標での長さ</returns>
        public double world2screenYlength(double y)
        {
            return y * (mView.Top - mView.Bottom) / (mWorld.Top - mWorld.Bottom);
        }

        /// <summary>
        /// スクリーン座標のX方向長さをワールド座標の長さに変換
        /// </summary>
        /// <param name="x">長さ</param>
        /// <returns></returns>
        public double screen2worldXlength(double x)
        {
            return x * (mWorld.Right - mWorld.Left) / (mView.Right - mView.Left);
        }

        /// <summary>
        /// スクリーン座標のY方向長さをワールド座標の長さに変換
        /// </summary>
        /// <param name="y">長さ</param>
        /// <returns></returns>
        public double screen2worldYlength(double y)
        {
            return y * (mWorld.Top - mWorld.Bottom) / (mView.Top - mView.Bottom);
        }

        /// <summary>
        /// 線分の描画
        /// </summary>
        /// <param name="ps">始点座標</param>
        /// <param name="pe">終点座標</param>
        public override void drawLine(Point lps, Point lpe)
        {
            Point ps = cnvWorld2Screen(lps);
            Point pe = cnvWorld2Screen(lpe);
            if (mClipping) {
                LineD l = new LineD(ps, pe);
                LineD cl = l.clippingLine(mView);
                if (cl != null)
                    base.drawLine(cl.ps, cl.pe);
            } else {
                base.drawLine(ps, pe);
            }
        }

        /// <summary>
        /// 線分の描画
        /// </summary>
        /// <param name="xs">始点X座標</param>
        /// <param name="ys">始点Y座標</param>
        /// <param name="xe">終点X座標</param>
        /// <param name="ye">終点Y座標</param>
        public override void drawLine(double xs, double ys, double xe, double ye)
        {
            Point ps = cnvWorld2Screen( new Point(xs, ys));
            Point pe = cnvWorld2Screen(new Point(xe, ye));
            if (mClipping) {
                LineD l = new LineD(ps, pe);
                LineD cl = l.clippingLine(mView);
                if (cl != null)
                    base.drawLine(cl.ps, cl.pe);
            } else {
                base.drawLine(ps, pe);
            }
        }


        /// <summary>
        /// 円弧の描画
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="radius">半径</param>
        /// <param name="startAngle">開始角度</param>
        /// <param name="endAngle">終了角度</param>
        public override void drawArc(Point center, double radius, double startAngle, double endAngle)
        {
            if (2 * Math.PI <= Math.Abs(endAngle - startAngle))
                base.drawCircle(cnvWorld2Screen(center), world2screenXlength(radius));
            else {
                //  円の大きさ
                Size size = new Size(Math.Abs(world2screenXlength(radius)), Math.Abs(world2screenYlength(radius)));     //  X軸半径,Y軸半径
                //  始点座標
                Point startPoint = new Point(radius * Math.Cos(startAngle), Math.Sign(mWorld.Top - mWorld.Bottom) * radius * Math.Sin(startAngle));
                startPoint.Offset(center.X, center.Y);
                startPoint = cnvWorld2Screen(startPoint);
                //  終点座標
                Point endPoint = new Point(radius * Math.Cos(endAngle), Math.Sign(mWorld.Top - mWorld.Bottom) * radius * Math.Sin(endAngle));
                endPoint.Offset(center.X, center.Y);
                endPoint = cnvWorld2Screen(endPoint);

                bool isLarge = (endAngle - startAngle) > Math.PI ? true : false; //  180°を超える円弧化かを指定
                                                                                 //                base.drawEllipse(cnvWorld2Screen(center), size, endPoint, startPoint, isLarge, SweepDirection.Counterclockwise, 0);
                base.drawEllipse(cnvWorld2Screen(center), size, startPoint, endPoint, isLarge, SweepDirection.Counterclockwise, 0);
            }
        }

        /// <summary>
        /// 円弧の描画
        /// </summary>
        /// <param name="cx">中心X座標</param>
        /// <param name="cy">中心Y座標</param>
        /// <param name="radius">半径</param>
        /// <param name="startAngle">開始角度</param>
        /// <param name="endAngle">終了角度</param>
        public override void drawArc(double cx, double cy, double radius, double startAngle, double endAngle)
        {
            Point center = new Point(cx, cy);
            this.drawArc(center, radius, startAngle, endAngle);
        }

        /// <summary>
        /// 円の描画
        /// </summary>
        /// <param name="cx">中心X座標</param>
        /// <param name="cy">中心Y座標</param>
        /// <param name="radius">半径</param>
        public override void drawCircle(double cx, double cy, double radius)
        {
            base.drawCircle(cnvWorld2ScreenX(cx), cnvWorld2ScreenY(cy), world2screenXlength(radius));
        }

        /// <summary>
        /// 楕円弧の描画
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="radius">半径(X軸め/Y軸)</param>
        /// <param name="startAngle">開始角(rad)</param>
        /// <param name="endAngle">終了角(rad)</param>
        /// <param name="rotate">回転角(rad)</param>
        public override void drawEllipse(Point center, Size radius, double startAngle, double endAngle, double rotate)
        {
            //  始点座標
            Point startPoint = new Point(radius.Width * Math.Cos(startAngle), Math.Sign(mWorld.Top - mWorld.Bottom) * radius.Height * Math.Sin(startAngle));
            startPoint.Offset(center.X, center.Y);
            startPoint = cnvWorld2Screen(startPoint);
            //  終点座標
            Point endPoint = new Point(radius.Width * Math.Cos(endAngle), Math.Sign(mWorld.Top - mWorld.Bottom) * radius.Height * Math.Sin(endAngle));
            endPoint.Offset(center.X, center.Y);
            endPoint = cnvWorld2Screen(endPoint);
            bool isLarge = (endAngle - startAngle) > Math.PI ? true : false; //  180°を超える円弧化かを指定
            base.drawEllipse(cnvWorld2Screen(center), radius, startPoint, endPoint, isLarge, SweepDirection.Counterclockwise, rotate);
        }

        /// <summary>
        /// 楕円弧の描画
        /// </summary>
        /// <param name="cx">中心X座標</param>
        /// <param name="cy">中心Y座標</param>
        /// <param name="rx">X軸半径</param>
        /// <param name="ry">Y軸半径</param>
        /// <param name="startAngle">開始角(rad)</param>
        /// <param name="endAngle">終了角(rad)</param>
        /// <param name="rotate">回転角(rad)</param>
        public override void drawEllipse(double cx, double cy, double rx, double ry, double startAngle, double endAngle, double rotate)
        {
            Point center = cnvWorld2Screen(new Point(cx, cy));
            //  円の大きさ
            Size size = new Size(Math.Abs(world2screenXlength(rx)), Math.Abs(world2screenYlength(ry)));     //  X軸半径,Y軸半径
            //  始点座標
            Point startPoint = new Point(rx * Math.Cos(startAngle), Math.Sign(mWorld.Top - mWorld.Bottom) * ry * Math.Sin(startAngle));
            startPoint.Offset(cx, cy);
            startPoint = cnvWorld2Screen(startPoint);
            //  終点座標
            Point endPoint = new Point(rx * Math.Cos(endAngle), Math.Sign(mWorld.Top - mWorld.Bottom) * ry * Math.Sin(endAngle));
            endPoint.Offset(cx, cy);
            endPoint = cnvWorld2Screen(endPoint);
            bool isLarge = (endAngle - startAngle) > Math.PI ? true : false; //  180°を超える円弧化かを指定
            base.drawEllipse(center, size, startPoint, endPoint, isLarge, SweepDirection.Counterclockwise, -180 * rotate / Math.PI);
        }

        /// <summary>
        /// 楕円の描画
        /// </summary>
        /// <param name="rect">楕円の領域座標</param>
        /// <param name="rotate">回転角(rad)</param>
        public override void drawOval(Rect rect, double rotate)
        {
            base.drawOval(cnvWorld2Screen(rect), -180 * rotate / Math.PI);
        }


        /// <summary>
        /// 楕円の描画
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="radiusX">X軸半径</param>
        /// <param name="radiusY">Y軸半径</param>
        /// <param name="rotate">回転角(rad)</param>
        public override void drawOval(Point center, double radiusX, double radiusY, double rotate)
        {
            base.drawOval(cnvWorld2Screen(center), Math.Abs(world2screenXlength(radiusX)), Math.Abs(world2screenYlength(radiusY)), -180 * rotate / Math.PI);
        }

        /// <summary>
        /// 四角形の描画
        /// </summary>
        /// <param name="ps">四角形の端点座標</param>
        /// <param name="pe">四角形の端点座標</param>
        /// <param name="rotate">回転角(rad)</param>
        public void drawRectangle(Point ps, Point pe, double rotate)
        {
            if (mClipping) {
                drawLine(ps.X, ps.Y, ps.X, pe.Y);
                drawLine(pe.X, ps.Y, pe.X, pe.Y);
                drawLine(ps.X, ps.Y, pe.X, ps.Y);
                drawLine(ps.X, pe.Y, pe.X, pe.Y);
            } else {
                Rect rect = new Rect(cnvWorld2Screen(ps), cnvWorld2Screen(pe));
                base.drawRectangle(rect, -180 * rotate / Math.PI);
            }
        }

        /// <summary>
        /// 四角形の描画
        /// </summary>
        /// <param name="rect">四角の座標(左,上,幅,高さ(下向き))</param>
        /// <param name="rotate">回転角(rad)</param>
        public override void drawRectangle(Rect rect, double rotate)
        {
            if (mClipping) {
                drawLine(rect.X, rect.Y, rect.X, rect.Bottom);
                drawLine(rect.Right, rect.Y, rect.Right, rect.Bottom);
                drawLine(rect.X, rect.Y, rect.Right, rect.Y);
                drawLine(rect.X, rect.Bottom, rect.Right, rect.Bottom);
            } else {
                Rect srect = cnvWorld2Screen(rect);
                base.drawRectangle(srect, -180 * rotate / Math.PI);
            }
        }

        /// <summary>
        /// 四角形の描画
        /// </summary>
        /// <param name="left">左座標</param>
        /// <param name="top">上座標</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ(下向き)</param>
        /// <param name="rotate">回転角(rad)</param>
        public override void drawRectangle(double left, double top, double width, double height, double rotate)
        {
            double w = world2screenXlength(width);
            double h = world2screenYlength(height);
            double x = cnvWorld2ScreenX(left);
            double y = cnvWorld2ScreenY(top);
            if (mClipping) {
                drawLine(left, top, left, top + height);
                drawLine(left + width, top, left + width, top + height);
                drawLine(left, top, left + width, top);
                drawLine(left, top + height, left + width, top + height);
            } else {
                base.drawRectangle(x, y, Math.Abs(w), Math.Abs(h), -180 * rotate / Math.PI);
            }
        }

        /// <summary>
        /// 文字列の描画
        /// </summary>
        /// <param name="text">文字列</param>
        /// <param name="left">左座標</param>
        /// <param name="top">上座標</param>
        /// <param name="rotate">回転角(rad)</param>
        public override void drawText(string text, double left, double top, double rotate)
        {
            base.drawText(text, cnvWorld2ScreenX(left), cnvWorld2ScreenY(top), -180 * rotate / Math.PI);
        }

        /// <summary>
        /// 文字列の描画
        /// </summary>
        /// <param name="text">文字列</param>
        /// <param name="left">文字原点</param>
        /// <param name="top">文字原点</param>
        /// <param name="rotate">回転角(rad)</param>
        /// <param name="ha">水平方向アライメント</param>
        /// <param name="va">垂直方向アライメント</param>
        public override void drawText(string text, double left, double top, double rotate, HorizontalAlignment ha, VerticalAlignment va)
        {
            //base.drawText(text, cnvWorld2ScreenX(left), cnvWorld2ScreenY(top), -180 * rotate / Math.PI, ha, va);
            drawText(text, new Point(left, top), rotate, ha, va);
        }

        /// <summary>
        /// 文字列の描画
        /// </summary>
        /// <param name="text">文字列</param>
        /// <param name="p">文字位置(左上)</param>
        /// <param name="rotate">回転角</param>
        public void drawText(string text, Point p, double rotate)
        {
            base.drawText(text, cnvWorld2ScreenX(p.X), cnvWorld2ScreenY(p.Y), -180 * rotate / Math.PI);
        }

        /// <summary>
        /// 文字列の描画
        /// </summary>
        /// <param name="text">文字列</param>
        /// <param name="p">文字位置</param>
        /// <param name="rotate">回転角(rad)</param>
        /// <param name="ha">水平方向アライメント</param>
        /// <param name="va">垂直方向アライメント</param>
        public void drawText(string text, Point p, double rotate, HorizontalAlignment ha, VerticalAlignment va)
        {
            if (!mTextOverWrite) {
                //  文字列の領域を白で塗潰す
                Size size = this.measureText(text);
                Brush col = getColor();
                setFillColor(Brushes.White);
                setColor(Brushes.White);
                this.drawRectangle(new Rect(p, size), 0);
                setColor(col);
            }
            base.drawText(text, cnvWorld2ScreenX(p.X), cnvWorld2ScreenY(p.Y), -180 * rotate / Math.PI, ha, va);
        }

        /// <summary>
        /// 文字列の大きさを取得する
        /// </summary>
        /// <param name="text">文字列</param>
        /// <returns>文字列の大きさ(Size.Width/Height)</returns>
        public override Size measureText(string text)
        {
            Size size = base.measureText(text);
            //  YDrawingShapsから取得した大きさをワールド座標の大きさに逆変換する
            size.Width = size.Width / Math.Abs(world2screenXlength(1));
            size.Height = size.Height / Math.Abs(world2screenYlength(1));

            return size;
        }

        /// <summary>
        /// Bitmapリソースファイル(内部ファイル)をWorld座標でCanvasに表示する
        /// リソースファイル例: Properties.Resources.offtile
        /// </summary>
        /// <param name="bitmap">Bitmapリソース</param>
        /// <param name="rect">原点と大きさ</param>
        public void drawBitmap(System.Drawing.Bitmap bitmap, Rect rect)
        {
            Rect r = cnvWorld2Screen(rect);
            base.drawBitmap(bitmap, r.X, r.Y, r.Width, r.Height);
        }

        /// <summary>
        /// Bitmapリソースファイル(内部ファイル)をWorld座標でCanvasに表示する
        /// リソースファイル例: Properties.Resources.offtile
        /// </summary>
        /// <param name="bitmap">Bitmapリソース</param>
        /// <param name="org">原点</param>
        /// <param name="size">大きさ</param>
        public void drawBitmap(System.Drawing.Bitmap bitmap, Point org, Size size)
        {
            Point pos = cnvWorld2Screen(org);
            double width = world2screenXlength(size.Width);
            double height = world2screenYlength(size.Height);
            base.drawBitmap(bitmap, pos.X, pos.Y, width, height);
        }

        /// <summary>
        /// イメージファイルをWorld座標でCanvasに表示する
        /// </summary>
        /// <param name="filePath">イメージファイルパス</param>
        /// <param name="rect">原点と大きさ</param>
        public void drawImageFile(string filePath, Rect rect)
        {
            Rect r = cnvWorld2Screen(rect);
            base.drawImageFile2(filePath, r); //  領域に合わせてトリミングする
        }

        /// <summary>
        /// イメージファイルをトリミングしてCanvasに張り付ける
        /// 仮想の画像サイズに対してトリミングしたイメージを表示領域に張り付ける
        /// アスペクト比は保持される
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="orgRect">貼付ける表示領域</param>
        /// <param name="trimmingRect">仮想の画像サイズに対するトリミングする領域</param>
        /// <param name="orgSize">仮想の画像サイズ</param>
        public void drawImageFile(string filePath, Rect orgRect, Rect trimmingPos, Size orgSize)
        {
            Rect or = cnvWorld2Screen(orgRect);
            Rect op = cnvWorld2Screen(trimmingPos);
            Size size = cnvWorld2Screen(orgSize);
            base.drawImageFile3(filePath, or, op, size); //  領域に合わせてトリミングする
        }

        /// <summary>
        /// イメージファイルをWorld座標でCanvasに表示する
        /// </summary>
        /// <param name="filePath">イメージファイルパス</param>
        /// <param name="org">原点</param>
        /// <param name="size">大きさ</param>
        public void drawImageFile(string filePath, Point org, Size size)
        {
            Point pos = cnvWorld2Screen(org);
            double width = world2screenXlength(size.Width);
            double height = world2screenYlength(size.Height);
            //base.drawImageFile(filePath, pos.X, pos.Y, width, height);
            base.drawImageFile2(filePath, pos.X, pos.Y, width, height); //  領域に合わせてトリミングする
        }

        /// <summary>
        /// イメージファイルをBitmapで取得する
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="rect">領域(トリミングするための)</param>
        /// <returns></returns>
        public System.Drawing.Bitmap getBitMapImage(string filePath, Rect rect)
        {
            Rect r = cnvWorld2Screen(rect);
            return getFile2Bitmap(filePath, 0, 0, r.Width, r.Height);
        }

        /// <summary>
        /// イメージファイルをトリミングしてBitmapで取り込む
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="trimmingPos">トリミング開始座標</param>
        /// <param name="orgSize">トリミングサイズ</param>
        /// <returns></returns>
        public System.Drawing.Bitmap getBitMapImage(string filePath, Rect trimmingPos, Size orgSize)
        {
            Rect op = cnvWorld2Screen(trimmingPos);
            Size size = cnvWorld2Screen(orgSize);
            return getFile2Bitmap(filePath, op, size);
        }
    }
}

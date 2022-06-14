using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfLib
{
    /// <summary>
    /// WFP グラフィックライブラリ
    /// System.Windows.Shapesを使ってグラフィック処理をおこなう
    /// もっともベースとなるグラフィックライブラリでCanvasに対して描画する
    /// スクリーン座標系で基本要素を描画する
    /// 
    /// https://docs.microsoft.com/ja-jp/dotnet/framework/wpf/graphics-multimedia/ 参照
    /// 
    /// YDrawingShapes()
    /// YDrawingShapes(Canvas c)
    /// YDrawingShapes(Canvas c, double width, double height)
    /// setCanvas(Canvas c)
    /// clear()
    /// backColor(Brush color)
    /// setWindowSize()
    /// getWindowSize()
    /// setWindowSize(double width, double height)  Windowのサイズを設定する
    /// getColor16(int colorNo) 16色の内から番号で指定した色を取得する
    /// getColor15(int colorNo) 白を除く15色から番号で指定した色を取得する
    /// setColor(Brush color)   描画要素の色を設定
    /// setColor(byte a,byte r, byte g, byte b) 描画要素の色設定をARGBでおこなう
    /// getColor()              描画要素の色の取得
    /// setThickness(double thickness)  線の太さを設定する
    /// getThickness()          線幅の取得
    /// setFillColor(Brush color)   要素内の塗潰しの色を設定
    /// getFillColor()          要素の塗潰し色の取得
    /// setTextColor(Brush color)   文字の色設定
    /// setTextSize(double size)    文字の大きさを設定
    /// getTextSize()           文字サイズの取得
    /// drawLine(Point ps, Point pe)    線分の表示
    /// drawLine(double xs, double ys, double xe, double ye)    線分の表示
    /// drawArc(Point center, double radius, double startAngle, double endAngle)    Pathオブジェクトを使って円弧を表示
    /// drawArc(double cx, double cy, double radius, double startAngle, double endAngle)    Pathオブジェクトを使って円弧を表示
    /// drawCircle(double cx, double cy, double radius) 円の表示
    /// drawCircle(Point center, double radius) 円の表示
    /// drawEllipse(Point center, Size size, double startAngle, double endAngle, double rotate) Pathオブジェクトを使って楕円弧を表示
    /// drawEllipse(double cx, double cy, double rx, double ry, double startAngle, double endAngle, double rotate)      Pathオブジェクトを使って楕円弧を表示
    /// drawEllipse(Point center, Size size, Point startPoint, Point endPoint, bool isLarge, SweepDirection sweepDirection, double rotate)  Pathオブジェクトを使って楕円弧を表示
    /// drawOval(Rect rect, double rotate)          楕円の表示
    /// drawOval(Point center, double radiusX, double radiusY, double rotate)   楕円の表示
    /// drawOval(double left, double top, double width, double height, double rotate)   楕円の表示
    /// drawRectangle(Rect rect, double rotate)     四角形の表示
    /// drawRectangle(double left, double top, double width, double height, double rotate)  四角形を表示
    /// drawText(string text, double left, double top, double rotate)   文字列の表示
    /// drawText(string text, double left, double top, double rotate, HorizontalAlignment ha, VerticalAlignment va) 文字列の表示
    /// measureText(string text)        文字列の大きさを求める
    /// measureTextRatio(string text)   文字列の縦横比を求める(高さ/幅)
    /// drawBitmap(System.Drawing.Bitmap bitmap, double ox, double oy, double width, double height) リソースファイルの表示
    /// drawImageFile(string filePath, double ox, double oy, double width, double height)   イメージファイルを表示
    /// cnvBitmap2BitmapImage(System.Drawing.Bitmap bitmap) BitmapをBitmapImageに変換
    /// cnvImageFile2BitmapImage(string filePath)   イメージファイルをBitmapImageに変換
    /// setCanvasBitmapImage(Canvas canvas, BitmapImage bitmapImage, double ox, double oy, double width, double height) BitmapImageをcanvasに登録
    /// screenCapture(int left, int top, int width, int height)     スクリーンキャプチャーしてClipBoardに入れる
    /// 
    /// 
    /// </summary>
    public class YDrawingShapes
    {
        protected Canvas mCanvas;   //  通常はCanvasを使う
                                    //  StackPanelだとLineとEllipseは表示れるかRectAngleやTextBlockが表示されない
        protected double mWindowWidth;
        protected double mWindowHeight;
        private string[] mColor16Title = {
            "Black", "Silver", "Gray",  "White",  "Maroon", "Red",  "Purple", "Fuchsia",
            "Green", "Lime",   "Olive", "Yellow", "Navy",   "Blue", "Teal",   "Aqua" };
        private Brush[] mColor16 = {Brushes.Black,Brushes.Silver,Brushes.Gray,Brushes.White,
                                Brushes.Maroon,Brushes.Red,Brushes.Purple,Brushes.Fuchsia,
                                Brushes.Green,Brushes.Lime,Brushes.Olive,Brushes.Yellow,
                                Brushes.Navy,Brushes.Blue,Brushes.Teal,Brushes.Aqua
                        };
        private string[] mColor15Title = {  //  white抜き
            "Black", "Silver", "Gray",  "Maroon", "Red",  "Purple", "Fuchsia",
            "Green", "Lime",   "Olive", "Yellow", "Navy", "Blue",   "Teal",    "Aqua" };
        private Brush[] mColor15 = {Brushes.Black,Brushes.Silver,Brushes.Gray,
                                Brushes.Maroon,Brushes.Red,Brushes.Purple,Brushes.Fuchsia,
                                Brushes.Green,Brushes.Lime,Brushes.Olive,Brushes.Yellow,
                                Brushes.Navy,Brushes.Blue,Brushes.Teal,Brushes.Aqua
                        };
        private string[] mColorTitle = {
            "AliceBlue","AntiqueWhite","Aqua","Aquamarine","Azure","Beige","Bisque","Black","BlanchedAlmond","Blue",
            "BlueViolet","Brown","BurlyWood","CadetBlue","Chartreuse","Chocolate","Coral","CornflowerBlue","Cornsilk","Crimson",
            "Cyan","DarkBlue","DarkCyan","DarkGoldenrod","DarkGray","DarkGreen","DarkKhaki","DarkMagenta","DarkOliveGreen","DarkOrange",
            "DarkOrchid","DarkRed","DarkSalmon","DarkSeaGreen","DarkSlateBlue","DarkSlateGray","DarkTurquoise","DarkViolet","DeepPink","DeepSkyBlue",
            "DimGray","DodgerBlue","Firebrick","FloralWhite","ForestGreen","Fuchsia","Gainsboro","GhostWhite","Gold","Goldenrod",
            "Gray","Green","GreenYellow","Honeydew","HotPink","IndianRed","Indigo","Ivory","Khaki","Lavender",
            "LavenderBlush","LawnGreen","LemonChiffon","LightBlue","LightCoral","LightCyan","LightGoldenrodYellow","LightGray","LightGreen","LightPink",
            "LightSalmon","LightSeaGreen","LightSkyBlue","LightSlateGray","LightSteelBlue","LightYellow","Lime","LimeGreen","Linen","Magenta",
            "Maroon","MediumAquamarine","MediumBlue","MediumOrchid","MediumPurple","MediumSeaGreen","MediumSlateBlue","MediumSpringGreen","MediumTurquoise","MediumVioletRed",
            "MidnightBlue","MintCream","MistyRose","Moccasin","NavajoWhite","Navy","OldLace","Olive","OliveDrab","Orange",
            "OrangeRed","Orchid","PaleGoldenrod","PaleGreen","PaleTurquoise","PaleVioletRed","PapayaWhip","PeachPuff","Peru","Pink",
            "Plum","PowderBlue","Purple","Red","RosyBrown","RoyalBlue","SaddleBrown","Salmon","SandyBrown","SeaGreen",
            "SeaShell","Sienna","Silver","SkyBlue","SlateBlue","SlateGray","Snow","SpringGreen","SteelBlue","Tan",
            "Teal","Thistle","Tomato","Transparent","Turquoise","Violet","Wheat","White","WhiteSmoke","Yellow",
            "YellowGreen",
        };
        private Brush[] mColor = {
            Brushes.AliceBlue,Brushes.AntiqueWhite,Brushes.Aqua,Brushes.Aquamarine,Brushes.Azure,
            Brushes.Beige,Brushes.Bisque,Brushes.Black,Brushes.BlanchedAlmond,Brushes.Blue,
            Brushes.BlueViolet,Brushes.Brown,Brushes.BurlyWood,Brushes.CadetBlue,Brushes.Chartreuse,
            Brushes.Chocolate,Brushes.Coral,Brushes.CornflowerBlue,Brushes.Cornsilk,Brushes.Crimson,
            Brushes.Cyan,Brushes.DarkBlue,Brushes.DarkCyan,Brushes.DarkGoldenrod,Brushes.DarkGray,
            Brushes.DarkGreen,Brushes.DarkKhaki,Brushes.DarkMagenta,Brushes.DarkOliveGreen,Brushes.DarkOrange,
            Brushes.DarkOrchid,Brushes.DarkRed,Brushes.DarkSalmon,Brushes.DarkSeaGreen,Brushes.DarkSlateBlue,
            Brushes.DarkSlateGray,Brushes.DarkTurquoise,Brushes.DarkViolet,Brushes.DeepPink,Brushes.DeepSkyBlue,
            Brushes.DimGray,Brushes.DodgerBlue,Brushes.Firebrick,Brushes.FloralWhite,Brushes.ForestGreen,
            Brushes.Fuchsia,Brushes.Gainsboro,Brushes.GhostWhite,Brushes.Gold,Brushes.Goldenrod,
            Brushes.Gray,Brushes.Green,Brushes.GreenYellow,Brushes.Honeydew,Brushes.HotPink,
            Brushes.IndianRed,Brushes.Indigo,Brushes.Ivory,Brushes.Khaki,Brushes.Lavender,
            Brushes.LavenderBlush,Brushes.LawnGreen,Brushes.LemonChiffon,Brushes.LightBlue,Brushes.LightCoral,
            Brushes.LightCyan,Brushes.LightGoldenrodYellow,Brushes.LightGray,Brushes.LightGreen,Brushes.LightPink,
            Brushes.LightSalmon,Brushes.LightSeaGreen,Brushes.LightSkyBlue,Brushes.LightSlateGray,Brushes.LightSteelBlue,
            Brushes.LightYellow,Brushes.Lime,Brushes.LimeGreen,Brushes.Linen,Brushes.Magenta,
            Brushes.Maroon,Brushes.MediumAquamarine,Brushes.MediumBlue,Brushes.MediumOrchid,Brushes.MediumPurple,
            Brushes.MediumSeaGreen,Brushes.MediumSlateBlue,Brushes.MediumSpringGreen,Brushes.MediumTurquoise,Brushes.MediumVioletRed,
            Brushes.MidnightBlue,Brushes.MintCream,Brushes.MistyRose,Brushes.Moccasin,Brushes.NavajoWhite,
            Brushes.Navy,Brushes.OldLace,Brushes.Olive,Brushes.OliveDrab,Brushes.Orange,
            Brushes.OrangeRed,Brushes.Orchid,Brushes.PaleGoldenrod,Brushes.PaleGreen,Brushes.PaleTurquoise,
            Brushes.PaleVioletRed,Brushes.PapayaWhip,Brushes.PeachPuff,Brushes.Peru,Brushes.Pink,
            Brushes.Plum,Brushes.PowderBlue,Brushes.Purple,Brushes.Red,Brushes.RosyBrown,
            Brushes.RoyalBlue,Brushes.SaddleBrown,Brushes.Salmon,Brushes.SandyBrown,Brushes.SeaGreen,
            Brushes.SeaShell,Brushes.Sienna,Brushes.Silver,Brushes.SkyBlue,Brushes.SlateBlue,
            Brushes.SlateGray,Brushes.Snow,Brushes.SpringGreen,Brushes.SteelBlue,Brushes.Tan,
            Brushes.Teal,Brushes.Thistle,Brushes.Tomato,Brushes.Transparent,Brushes.Turquoise,
            Brushes.Violet,Brushes.Wheat,Brushes.White,Brushes.WhiteSmoke,Brushes.Yellow,
            Brushes.YellowGreen,
        };
        private Brush mBrush = Brushes.Black;       //  要素の色
        private double mThickness = 1;              //  線の太さ
        private Brush mFillColor = null;            //  塗潰しカラー
        private double mTextSize = 20;              //  文字サイズ
        private Brush mTextColor = Brushes.Black;   //  文字色
        private SweepDirection mSweepDirection = SweepDirection.Clockwise;  //  描画の回転方向
        private int mLastIndex;                     //  最終登録要素番号

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public YDrawingShapes()
        {

        }

        /// <summary>
        /// コンストラクタ　ベースのCanvassの設定
        /// 表示領域はViewの領域を取得
        /// </summary>
        /// <param name="c"></param>
        public YDrawingShapes(Canvas c)
        {
            mCanvas = c;
            setWindowSize();
        }

        /// <summary>
        /// コンストラクタ　ベースのCanvassの設定と表示領域を指定する
        /// </summary>
        /// <param name="c"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public YDrawingShapes(Canvas c, double width, double height)
        {
            mCanvas = c;
            mWindowWidth = width;
            mWindowHeight = height;
        }

        /// <summary>
        /// 描画対象のPanel(通常はCanvsを使う)
        /// </summary>
        /// <param name="c"></param>
        public void setCanvas(Canvas c)
        {
            mCanvas = c;
        }

        /// <summary>
        /// 登録した図形データをクリアする
        /// </summary>
        public void clear()
        {
            mCanvas.Children.Clear();
        }

        /// <summary>
        /// 背景色を設定する
        /// </summary>
        /// <param name="color"></param>
        public void backColor(Brush color)
        {
            mCanvas.Background = color;
        }

        /// <summary>
        /// 設定されたCanvasかPanelからWindowのSizeが取得できれば
        /// それを設定値とする。幅・高さとも取得できた時はtrueを7返す
        /// </summary>
        /// <returns>取得の可否</returns>
        public bool setWindowSize()
        {
            bool result = true;
            if (!Double.IsNaN(mCanvas.ActualWidth))
                mWindowWidth = mCanvas.ActualWidth;
            else
                result = false;
            if (!Double.IsNaN(mCanvas.ActualHeight))
                mWindowHeight = mCanvas.ActualHeight;
            else
                result = false;
            return result;
        }

        /// <summary>
        /// 描画領域のサイズを取得
        /// </summary>
        /// <returns>Size(幅,高さ)</returns>
        public Size getWindowSize()
        {
            return new Size(mWindowWidth, mWindowHeight);
        }

        /// <summary>
        /// Windowのサイズを設定する
        /// </summary>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        public virtual void setWindowSize(double width, double height)
        {
            mWindowWidth = width;
            mWindowHeight = height;
        }

        /// <summary>
        /// 16色の内から番号で指定した色を取得する
        /// </summary>
        /// <param name="colorNo">色番号</param>
        /// <returns></returns>
        public Brush getColor16(int colorNo)
        {
            return mColor16[colorNo % 16];
        }

        /// <summary>
        /// 白を除く15色から番号で指定した色を取得する
        /// </summary>
        /// <param name="colorNo">色番号</param>
        /// <returns></returns>
        public Brush getColor15(int colorNo)
        {
            return mColor15[colorNo % 15];
        }

        /// <summary>
        /// 141色から番号指定で色を取得
        /// </summary>
        /// <param name="colorNo"></param>
        /// <returns></returns>
        public Brush getColor(int colorNo)
        {
            return mColor[colorNo % mColor.Length];
        }

        /// <summary>
        /// Color16で使用しているカラー名の配列を取得
        /// </summary>
        /// <returns></returns>
        public string[] getColor16Title()
        {
            return mColor16Title;
        }

        /// <summary>
        /// Color15で使用しているカラー名の配列を取得(Color16からWhiteを抜く)
        /// </summary>
        /// <returns></returns>
        public string[] getColor15Title()
        {
            return mColor15Title;
        }

        /// <summary>
        /// カラー名の配列を取得
        /// </summary>
        /// <returns></returns>
        public string[] getColorTitle()
        {
            return mColorTitle;
        }

        /// <summary>
        /// 描画要素の色を名前で設定する(16色中)
        /// </summary>
        /// <param name="colorName">カラー名</param>
        public void setColor16(string colorName)
        {
            int index = Array.IndexOf(mColor16Title, colorName);
            setColor(mColor16[index % 16]);
        }

        /// <summary>
        /// 描画要素の色を名前で設定する
        /// </summary>
        /// <param name="colorName">カラー名</param>
        public void setColor(string colorName)
        {
            int index = Array.IndexOf(mColorTitle, colorName);
            setColor(mColor[index % mColor.Length]);
        }

        /// <summary>
        /// 描画要素の色を設定
        /// </summary>
        /// <param name="color"></param>
        public void setColor(Brush color)
        {
            mBrush = color;
        }

        /// <summary>
        /// 指定描画の色をARGBで設定する
        /// </summary>
        /// <param name="a">α係数(不透明度)</param>
        /// <param name="r">赤</param>
        /// <param name="g">緑</param>
        /// <param name="b">青</param>
        public void setColor(byte a,byte r, byte g, byte b)
        {
            mBrush = new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }

        /// <summary>
        /// 描画要素の色の取得
        /// </summary>
        /// <returns>色</returns>
        public Brush getColor()
        {
            return mBrush;
        }

        /// <summary>
        /// 線の太さを設定する
        /// </summary>
        /// <param name="thickness"></param>
        public void setThickness(double thickness)
        {
            mThickness = thickness;
        }

        /// <summary>
        /// 線幅の取得
        /// </summary>
        /// <returns>線幅</returns>
        public double getThickness()
        {
            return mThickness;
        }

        /// <summary>
        /// 要素内の塗潰しの色を設定する
        /// </summary>
        /// <param name="color">色</param>
        public void setFillColor(Brush color)
        {
            mFillColor = color;
        }

        /// <summary>
        /// 要素の塗潰し色の取得
        /// </summary>
        /// <returns>色</returns>
        public Brush getFillColor()
        {
            return mFillColor;
        }

        /// <summary>
        /// 文字の色設定をする
        /// </summary>
        /// <param name="color">色(Brush)</param>
        public void setTextColor(Brush color)
        {
            mTextColor = color;
        }

        /// <summary>
        /// 文字の大きさを設定する
        /// </summary>
        /// <param name="size">文字の大きさ</param>
        public virtual void setTextSize(double size)
        {
            mTextSize = size;
        }

        /// <summary>
        /// 文字サイズの取得
        /// </summary>
        /// <returns>文字の大きさ</returns>
        public virtual double getTextSize()
        {
            return mTextSize;
        }

        /// <summary>
        /// 最後に登録した要素番号
        /// </summary>
        /// <returns></returns>
        public int getLastIndex()
        {
            return mLastIndex;
        }

        /// <summary>
        /// 登録された要素数を返す
        /// </summary>
        /// <returns></returns>
        public int getItemCount()
        {
            return mCanvas.Children.Count;
        }

        /// <summary>
        /// 指定した要素の削除
        /// </summary>
        /// <param name="index"></param>
        public void removeElement(int index)
        {
            if (0 <= index && index < mCanvas.Children.Count)
                mCanvas.Children.RemoveAt(index);
        }

        /// <summary>
        /// 線分の表示
        /// </summary>
        /// <param name="ps">始点座標</param>
        /// <param name="pe">終点座標</param>
        public virtual void drawLine(Point ps, Point pe)
        {
            Line line = new Line();
            line.X1 = ps.X;
            line.Y1 = ps.Y;
            line.X2 = pe.X;
            line.Y2 = pe.Y;
            line.Stroke = mBrush;
            line.StrokeThickness = mThickness;

            mLastIndex = mCanvas.Children.Add(line);
        }

        /// <summary>
        /// 線分の表示
        /// </summary>
        /// <param name="xs">始点X座標</param>
        /// <param name="ys">始点Y座標</param>
        /// <param name="xe">終点X座標</param>
        /// <param name="ye">終点Y座標</param>
        public virtual void drawLine(double xs, double ys, double xe, double ye)
        {
            Line line = new Line();
            line.X1 = xs;
            line.Y1 = ys;
            line.X2 = xe;
            line.Y2 = ye;
            line.Stroke = mBrush;
            line.StrokeThickness = mThickness;

            mLastIndex = mCanvas.Children.Add(line);
        }

        /// <summary>
        /// Pathオブジェクトを使って円弧を表示する
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="radius">半径</param>
        /// <param name="startAngle">開始角度(rad)</param>
        /// <param name="endAngle">終了角度(rad)</param>
        public virtual void drawArc(Point center, double radius, double startAngle, double endAngle)
        {
            if (Math.PI * 2 <= Math.Abs(startAngle - endAngle))
                this.drawCircle(center, radius);
            else
                this.drawEllipse(center.X, center.Y, radius, radius, startAngle, endAngle, 0);
        }

        /// <summary>
        ///  Pathオブジェクトを使って円弧を表示する
        /// </summary>
        /// <param name="cx">中心X座標</param>
        /// <param name="cy">中心Y座標</param>
        /// <param name="radius">半径</param>
        /// <param name="startAngle">開始角度(rad)</param>
        /// <param name="endAngle">終了角度(rad)</param>
        public virtual void drawArc(double cx, double cy, double radius, double startAngle, double endAngle)
        {
            if (Math.PI * 2 <= Math.Abs(startAngle - endAngle))
                this.drawCircle(cx, cy, radius);
            else
                this.drawEllipse(cx, cy, radius, radius, startAngle, endAngle, 0);
        }

        /// <summary>
        /// 円の表示
        /// </summary>
        /// <param name="cx">中心X座標</param>
        /// <param name="cy">中心Y座標</param>
        /// <param name="radius">半径</param>
        public virtual void drawCircle(double cx, double cy, double radius)
        {
            double left = cx - radius;
            double top = cy - radius;
            double width = radius * 2;
            double height = radius * 2;

            drawOval(left, top, width, height, 0);
        }

        /// <summary>
        /// 円の表示
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="radius">半径</param>
        public virtual void drawCircle(Point center, double radius)
        {
            double left = center.X - radius;
            double top = center.Y - radius;
            double width = radius * 2;
            double height = radius * 2;

            drawOval(left, top, width, height, 0);
        }

        /// <summary>
        /// Pathオブジェクトを使って楕円弧を表示
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="size">楕円の半径(X軸/Y軸)</param>
        /// <param name="startAngle">開始角(rad)</param>
        /// <param name="endAngle">終了角(rad)</param>
        /// <param name="rotate">回転角(rad)</param>
        public virtual void drawEllipse(Point center, Size size, double startAngle, double endAngle, double rotate)
        {

            //  始点座標
            Point startPoint = new Point(size.Width * Math.Cos(startAngle), size.Height * Math.Sin(startAngle));
            startPoint.Offset(center.X, center.Y);
            //  終点座標
            Point endPoint = new Point(size.Width * Math.Cos(endAngle), size.Height * Math.Sin(endAngle));
            endPoint.Offset(center.X, center.Y);
            bool isLarge = (endAngle - startAngle) > Math.PI ? true : false; //  180°を超える円弧化かを指定

            drawEllipse(center, size, startPoint, endPoint, isLarge, mSweepDirection, -180 * rotate / Math.PI);
        }

        /// <summary>
        /// Pathオブジェクトを使って楕円弧を表示
        /// </summary>
        /// <param name="cx">中心X座標</param>
        /// <param name="cy">中心Y座標</param>
        /// <param name="rx">X軸半径</param>
        /// <param name="ry">Y軸半径</param>
        /// <param name="startAngle">開始角度(rad)</param>
        /// <param name="endAngle">終了角度(rad)</param>
        /// <param name="rotate">回転角(rad)</param>
        public virtual void drawEllipse(double cx, double cy, double rx, double ry, double startAngle, double endAngle, double rotate)
        {
            //  円の大きさ
            Size size = new Size(rx, ry);     //  X軸半径,Y軸半径
            //  始点座標
            Point startPoint = new Point(rx * Math.Cos(startAngle), ry * Math.Sin(startAngle));
            startPoint.X += cx;
            startPoint.Y += cy;
            //  終点座標
            Point endPoint = new Point(rx * Math.Cos(endAngle), ry * Math.Sin(endAngle));
            endPoint.X += cx;
            endPoint.Y += cy;
            bool isLarge = (endAngle - startAngle) > Math.PI ? true : false; //  180°を超える円弧化かを指定

            drawEllipse(new Point(cx, cy), size, startPoint, endPoint, isLarge, mSweepDirection, -180 * rotate / Math.PI);
        }


        /// <summary>
        /// Pathオブジェクトを使って楕円弧を表示
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="size">楕円の半径(X軸半径,Y軸半径)</param>
        /// <param name="startPoint">始点</param>
        /// <param name="endPoint">終点</param>
        /// <param name="isLarge">大円弧? (180°＜ 円弧角度)</param>
        /// <param name="sweepDirection">円弧の描画方向</param>
        /// <param name="rotate">回転角(deg)</param>
        protected void drawEllipse(Point center, Size size, Point startPoint, Point endPoint, bool isLarge, SweepDirection sweepDirection, double rotate)
        {
            //  Pathオブジェクトの生成(Path→PathGeometry→PathFigure→ArcSegment)
            System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = startPoint;         //  始点
            bool isStorked = true;                      //  セグメントに線を付ける
            //  ArcSegmentのRotationAngleは始終点を変えずに楕円を回転させるため0にする
            ArcSegment arcSegment = new ArcSegment(endPoint, size, 0, isLarge, sweepDirection, isStorked);
            pathFigure.Segments.Add(arcSegment);
            pathGeometry.Figures.Add(pathFigure);
            //  楕円の回転角と回転中心
            RotateTransform rotateTransform = new RotateTransform(rotate, center.X, center.Y);
            pathGeometry.Transform = rotateTransform;
            //  色と線の太さを設定
            path.Stroke = mBrush;
            path.StrokeThickness = mThickness;
            path.Data = pathGeometry;

            mLastIndex = mCanvas.Children.Add(path);
        }

        /// <summary>
        /// 楕円の表示
        /// </summary>
        /// <param name="rect">楕円の領域座標</param>
        /// <param name="rotate">回転角(deg)</param>
        public virtual void drawOval(Rect rect, double rotate)
        {
            drawOval(rect.Left, rect.Top, rect.Width, rect.Height, rotate);
        }

        /// <summary>
        /// 楕円の表示
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="radiusX">X軸半径</param>
        /// <param name="radiusY">Y軸半径</param>
        /// <param name="rotate">回転角(deg)</param>
        public virtual void drawOval(Point center, double radiusX, double radiusY, double rotate)
        {
            double left = center.X - radiusX;
            double top = center.Y - radiusY;
            double width = radiusX * 2;
            double height = radiusY * 2;

            drawOval(left, top, width, height, rotate);
        }

        /// <summary>
        /// 楕円の表示
        /// </summary>
        /// <param name="left">左位置</param>
        /// <param name="top">上位置</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <param name="rotate">回転角(deg)</param>
        protected void drawOval(double left, double top, double width, double height, double rotate)
        {
            Ellipse ellipse = new Ellipse();
            double right = mWindowWidth - width;
            double bottom = mWindowHeight - height;
            ellipse.Margin = new Thickness(left, top, right, bottom);   // Marginによる位置の指定
            ellipse.Fill = mFillColor;
            ellipse.Width = Math.Abs(width);
            ellipse.Height = Math.Abs(height);
            ellipse.Stroke = mBrush;
            ellipse.StrokeThickness = mThickness;
            //  楕円の回転角と回転中心
            RotateTransform rotateTransform = new RotateTransform(rotate, width / 2, height / 2);
            ellipse.RenderTransform = rotateTransform;
            //RotateTransform rotateTransform = new RotateTransform(rotate);          //  回転角を設定(度)
            //TranslateTransform translateTransform = new TranslateTransform(30, 0);  //  移動距離(X軸,Y軸)
            //TransformGroup transformGroup = new TransformGroup();                   //  座標変換をまとめる
            //transformGroup.Children.Add(rotateTransform);
            //transformGroup.Children.Add(translateTransform);
            //ellipse.RenderTransform = transformGroup;

            mLastIndex = mCanvas.Children.Add(ellipse);
        }

        /// <summary>
        /// 四角形の表示
        /// </summary>
        /// <param name="rect">座標(左上,幅,高さ)</param>
        /// <param name="rotate">回転角(deg)</param>
        public virtual void drawRectangle(Rect rect, double rotate)
        {
            Rectangle rectangle = new Rectangle();
            double right = mWindowWidth - rect.Width;
            double bottom = mWindowHeight - rect.Height;
            rectangle.Margin = new Thickness(rect.Left, rect.Top, right, bottom); // Marginによる位置の指定
            rectangle.Fill = mFillColor;
            rectangle.Width = rect.Width;
            rectangle.Height = rect.Height;
            rectangle.Stroke = mBrush;
            rectangle.StrokeThickness = mThickness;
            RotateTransform rotateTransform = new RotateTransform(rotate, rect.Width / 2, rect.Height / 2);
            rectangle.RenderTransform = rotateTransform;

            mLastIndex = mCanvas.Children.Add(rectangle);
        }

        /// <summary>
        /// 四角形を表示
        /// </summary>
        /// <param name="left">左位置</param>
        /// <param name="top">上位置</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <param name="rotate">文字列角度(deg)</param>
        public virtual void drawRectangle(double left, double top, double width, double height, double rotate)
        {
            Rectangle rectangle = new Rectangle();
            double right = mWindowWidth - width;
            double bottom = mWindowHeight - height;
            rectangle.Margin = new Thickness(left, top, right, bottom); // Marginによる位置の指定
            rectangle.Fill = mFillColor;
            rectangle.Width = Math.Abs(width);
            rectangle.Height = Math.Abs(height);
            rectangle.Stroke = mBrush;
            rectangle.StrokeThickness = mThickness;
            RotateTransform rotateTransform = new RotateTransform(rotate, width / 2, height / 2);
            rectangle.RenderTransform = rotateTransform;

            mLastIndex = mCanvas.Children.Add(rectangle);
        }


        /// <summary>
        /// 文字列の表示
        /// </summary>
        /// <param name="text">文字列</param>
        /// <param name="left">左位置</param>
        /// <param name="top">上位置</param>
        /// <param name="rotate">文字列角度(deg)</param>
        public virtual void drawText(string text, double left, double top, double rotate)
        {
            if (text == null)
                return;
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = mTextColor;
            double width = mTextSize * text.Length;
            double height = mTextSize;
            double right = mWindowWidth - width;
            double bottom = mWindowHeight - height;
            textBlock.Margin = new Thickness(left, top, right, bottom);     // Marginによる位置の指定
            textBlock.FontSize = mTextSize;                                 //  文字サイズ
            RotateTransform rotateTransform = new RotateTransform(rotate);  //  文字列角度
            textBlock.RenderTransform = rotateTransform;                    //  文字列角度を設定
            textBlock.HorizontalAlignment = HorizontalAlignment.Right;
            textBlock.VerticalAlignment = VerticalAlignment.Bottom;

            mLastIndex = mCanvas.Children.Add(textBlock);
        }

        /// <summary>
        /// 文字列の表示
        /// </summary>
        /// <param name="text">文字列</param>
        /// <param name="left">配置座標(x)</param>
        /// <param name="top">配置座標(y)</param>
        /// <param name="rotate">回転角(deg)</param>
        /// <param name="ha">水平配置(Left/Center/Right)</param>
        /// <param name="va">垂直配置(Top/Center/Bottom)</param>
        public virtual void drawText(string text, double left, double top, double rotate, HorizontalAlignment ha, VerticalAlignment va)
        {
            if (text == null)
                return;
            Point op = new Point(left, top);
            Size size = measureText2(text);             //  文字列の大きさ(幅と高さ))
            size.Width  += size.Height * 0.1;
            size.Height *= 1.1;                         //  ベースラインを調整
            //  アライメントの調整
            if (ha == HorizontalAlignment.Center)
                left -= size.Width / 2;
            else if (ha == HorizontalAlignment.Right)
                left -= size.Width;
            if (va == VerticalAlignment.Center)
                top -= size.Height / 2;
            else if (va == VerticalAlignment.Bottom)
                top -= size.Height;
            //  文字列回転時の回転原点を求める(文字列の左上)
            double rot = rotate * Math.PI / 180;
            double dx = (left - op.X);
            double dy = (top - op.Y);
            left = op.X + dx * Math.Cos(rot) - dy * Math.Sin(rot);
            top  = op.Y + dx * Math.Sin(rot) + dy * Math.Cos(rot);

            //  文字列のパラメータ設定
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = mTextColor;
            double width  = mTextSize * text.Length;
            double height = mTextSize;
            double right  = mWindowWidth - width;
            double bottom = mWindowHeight - height;
            textBlock.Margin = new Thickness(left, top, right, bottom);     // Marginによる位置の指定
            textBlock.FontSize = mTextSize;                                 //  文字サイズ
            RotateTransform rotateTransform = new RotateTransform(rotate);  //  文字列角度
            textBlock.RenderTransform = rotateTransform;                    //  文字列角度を設定
            //  アライメントは使えていない？
            textBlock.HorizontalAlignment = HorizontalAlignment.Right;
            textBlock.VerticalAlignment = VerticalAlignment.Top;
            //textBlock.HorizontalAlignment = ha;
            //textBlock.VerticalAlignment = va;

            mLastIndex = mCanvas.Children.Add(textBlock);
        }

        /// <summary>
        /// 文字列の大きさを求める
        /// </summary>
        /// <param name="text">文字列</param>
        /// <returns>文字列の大きさ(Size.Width/Height)</returns>
        public virtual Size measureText(string text)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = mTextColor;
            textBlock.FontSize = mTextSize;             //  文字サイズ
            //  auto sized (https://code.i-harness.com/ja-jp/q/8d5d0e)
            textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            textBlock.Arrange(new Rect(textBlock.DesiredSize));

            return new Size(textBlock.ActualWidth, textBlock.ActualHeight);
        }

        /// <summary>
        /// 文字列の大きさを求める(measureTextと同じで内部用)
        /// </summary>
        /// <param name="text">文字列</param>
        /// <returns>文字列の大きさ(Size.Width/Height)</returns>
        private Size measureText2(string text)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = mTextColor;
            textBlock.FontSize = mTextSize;             //  文字サイズ
            //  auto sized (https://code.i-harness.com/ja-jp/q/8d5d0e)
            textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            textBlock.Arrange(new Rect(textBlock.DesiredSize));

            return new Size(textBlock.ActualWidth, textBlock.ActualHeight);
        }

        /// <summary>
        /// 文字列の縦横比を求める(高さ/幅)
        /// </summary>
        /// <param name="text">文字列</param>
        /// <returns>縦横比</returns>
        public virtual double measureTextRatio(string text)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.FontSize = mTextSize;             //  文字サイズ
            //  auto sized (https://code.i-harness.com/ja-jp/q/8d5d0e)
            textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            textBlock.Arrange(new Rect(textBlock.DesiredSize));
            //System.Diagnostics.Debug.WriteLine("measureTextRatio: " + mTextSize + " " + textBlock.ActualHeight + " " 
            //    + (mTextSize / textBlock.ActualHeight)+" "+ (mTextSize / textBlock.ActualWidth*text.Length));

            return textBlock.ActualHeight / textBlock.ActualWidth;
        }

        /// <summary>
        /// Bitmapリソースファイル(内部ファイル)をCanvasに表示する
        /// </summary>
        /// <param name="bitmap">リソースファイル</param>
        /// <param name="ox">左上原点X</param>
        /// <param name="oy">左上原点Y</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        public void drawBitmap(System.Drawing.Bitmap bitmap, double ox, double oy, double width, double height)
        {
            var bitmapImage = cnvBitmap2BitmapImage(bitmap);
            setCanvasBitmapImage(mCanvas, bitmapImage, ox, oy, width, height);
        }

        /// <summary>
        /// イメージファイルをCanvasに表示する
        /// </summary>
        /// <param name="filePath">イメージファイルパス</param>
        /// <param name="ox">左上原点X</param>
        /// <param name="oy">左上原点Y</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        public void drawImageFile(string filePath, double ox, double oy, double width, double height)
        {
            var bitmapImage = cnvImageFile2BitmapImage(filePath);
            setCanvasBitmapImage(mCanvas, bitmapImage, ox, oy, width, height);
        }

        /// <summary>
        /// イメージファイルをCanvasに表示する
        /// 表示エリアのアスペクトと画像のアスペクトが合わない場合には画像をトリミングする
        /// </summary>
        /// <param name="filePath">画像ファイルパス</param>
        /// <param name="ox">左上原点</param>
        /// <param name="oy">左上原点</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        public void drawImageFile2(string filePath, double ox, double oy, double width, double height)
        {
            //  ファイル画像をトリミングして取得
            System.Drawing.Bitmap newBitmap = getFile2Bitmap(filePath, ox, oy, width, height);
            if (newBitmap == null)
                return;
            //  BitmapからBitmapImageに変換
            BitmapImage bitmapImage = cnvBitmap2BitmapImage(newBitmap);
            newBitmap.Dispose();
            //  BitmapImageをCanvasに張り付ける
            setCanvasBitmapImage(mCanvas, bitmapImage, ox, oy, width, height);
        }

        /// <summary>
        /// イメージファイルをCanvasに表示する
        /// 表示エリアのアスペクトと画像のアスペクトが合わない場合には画像をトリミングする
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="rect">表示領域</param>
        public void drawImageFile2(string filePath, Rect rect)
        {
            //  ファイル画像をトリミングして取得
            System.Drawing.Bitmap newBitmap = getFile2Bitmap(filePath, rect.X, rect.Y, rect.Width, rect.Height);
            if (newBitmap == null)
                return;
            //  BitmapからBitmapImageに変換
            BitmapImage bitmapImage = cnvBitmap2BitmapImage(newBitmap);
            newBitmap.Dispose();
            //  BitmapImageをCanvasに張り付ける
            setCanvasBitmapImage(mCanvas, bitmapImage, rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// イメージファイルを読み込んでトリミングし、Canvasに張り付ける
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="orgRect">貼付けるの表示領域</param>
        /// <param name="trimRect">仮想の画像サイズに対するトリミング領域</param>
        /// <param name="size">仮想の画像サイズ</param>
        public void drawImageFile3(string filePath, Rect orgRect, Rect trimRect, Size size)
        {
            //  ファイル画像をトリミングして取得
            System.Drawing.Bitmap newBitmap = getFile2Bitmap(filePath, trimRect, size);
            if (newBitmap == null)
                return;
            //  BitmapからBitmapImageに変換
            BitmapImage bitmapImage = cnvBitmap2BitmapImage(newBitmap);
            newBitmap.Dispose();
            //  BitmapImageをCanvasに張り付ける
            setCanvasBitmapImage(mCanvas, bitmapImage, orgRect.X, orgRect.Y, orgRect.Width, orgRect.Height);
        }

        /// <summary>
        /// イメージファイルをBitmapで取得(トリミングあり)
        /// </summary>
        /// <param name="filePath">画像ファイルパス</param>
        /// <param name="ox">左上原点</param>
        /// <param name="oy">左上原点</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <returns>Bitmap</returns>
        public System.Drawing.Bitmap getFile2Bitmap(string filePath, double ox, double oy, double width, double height)
        {
            if (!File.Exists(filePath))
                return null;
            //  Bitmapでファイルから読み込む
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(filePath);
            //  画像をトリミングする
            int w = bitmap.Width;
            int h = (int)(bitmap.Width * height / width);
            if (bitmap.Height < h) {
                w = (int)(bitmap.Height * width / height);
                h = bitmap.Height;
            }
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, w, h);
            System.Drawing.Bitmap resultImg = bitmap.Clone(rect, bitmap.PixelFormat);
            bitmap.Dispose();
            return resultImg;
        }

        /// <summary>
        /// イメージファイルを読み込んでトリミングする
        /// アスペクト比は保持される
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="orgRect">画像を張り付けるの表示領域</param>
        /// <param name="trimRect">仮想の画像サイズに対するトリミング領域</param>
        /// <param name="size">仮想の画像サイズ</param>
        /// <returns></returns>
        public System.Drawing.Bitmap getFile2Bitmap(string filePath, Rect trimRect, Size size)
        {
            if (!File.Exists(filePath))
                return null;
            //  Bitmapでファイルから読み込む
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(filePath);
            //  画像をトリミングする
            int ox = (int)(trimRect.X * bitmap.Width / size.Width);
            int oy = (int)(trimRect.Y * bitmap.Height / size.Height);
            int w = (int)(trimRect.Width * bitmap.Width / size.Width);
            int h = (int)(trimRect.Height * bitmap.Height / size.Height);
            if (ox < 0 || oy < 0 || w <= 0 || h <= 0) {
                bitmap.Dispose();
                return null;
            }
            w = Math.Min(w, bitmap.Width - ox);
            h = Math.Min(h, bitmap.Height - oy);
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(ox, oy, w, h);
            System.Drawing.Bitmap resultImg = bitmap.Clone(rect, bitmap.PixelFormat);
            bitmap.Dispose();
            return resultImg;
        }

        /// <summary>
        /// BitmapをBitmapImageに変換
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        /// <returns>BitmapImage</returns>
        public BitmapImage cnvBitmap2BitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream()) {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                memory.Close();
                return bitmapImage;
            }
        }

        /// <summary>
        /// イメージファイルをBitmapImageに変換する
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>BitmapImage</returns>
        public BitmapImage cnvImageFile2BitmapImage(string filePath)
        {
            //  ファイルから解放可能なBitmapImageを読み込む
            //  http://neareal.net/index.php?Programming%2F.NetFramework%2FWPF%2FWriteableBitmap%2FLoadReleaseableBitmapImage
            FileStream stream = File.OpenRead(filePath);
            //  イメージデータをStream化してBitmapImageに使用
            BitmapImage bitmap = new BitmapImage();
            try {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;  //  作成に使用されたストリームを閉じる
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                stream.Close();
                Image img = new Image();
            } catch (Exception e) {
                //MessageBox.Show(e.Message);
                return null;
            }
            return bitmap;
        }

        /// <summary>
        /// BitmapImageをCanvasに登録する
        /// </summary>
        /// <param name="canvas">Canvas</param>
        /// <param name="bitmapImage">BitmapImage</param>
        /// <param name="ox">原点(左上)</param>
        /// <param name="oy">原点(左上)</param>
        /// <param name="width">イメージの幅</param>
        /// <param name="height">イメージの高さ</param>
        public void setCanvasBitmapImage(Canvas canvas, BitmapImage bitmapImage, double ox, double oy, double width, double height)
        {
            Image img = new Image();
            img.Source = bitmapImage;
            img.Width = width;
            img.Height = height;
            img.Margin = new Thickness(ox, oy, 0, 0);
            mLastIndex = canvas.Children.Add(img);
        }

        /// <summary>
        /// 画像を縦方向に連結
        /// https://imagingsolution.net/program/csharp/image_combine/
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public System.Drawing.Bitmap verticalCombineImage(System.Drawing.Bitmap[] src)
        {
            int dstWidth = 0;
            int dstHeight = 0;
            System.Drawing.Imaging.PixelFormat dstPixelFormat = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;   //  256色
            for (int i = 0; i < src.Length; i++) {
                //  サイズの拡張
                if (dstWidth < src[i].Width)
                    dstWidth = src[i].Width;
                dstHeight += src[i].Height;
                //  最大ビット数の検索
                if (System.Drawing.Bitmap.GetPixelFormatSize(dstPixelFormat) <
                    System.Drawing.Bitmap.GetPixelFormatSize(src[i].PixelFormat)) {
                    dstPixelFormat = src[i].PixelFormat;
                }
            }

            var dst = new System.Drawing.Bitmap(dstWidth, dstHeight, dstPixelFormat);
            var dstRect = new System.Drawing.Rectangle();

            using (var g = System.Drawing.Graphics.FromImage(dst)) {
                for (int i = 0; i < src.Length; i++) {
                    dstRect.Width = src[i].Width;
                    dstRect.Height = src[i].Height;
                    //  描画
                    g.DrawImage(src[i], dstRect, 0, 0, src[i].Width, src[i].Height, System.Drawing.GraphicsUnit.Pixel);
                    //  次の描画先
                    dstRect.Y = dstRect.Bottom;
                }
            }
            return dst;
        }

        /// <summary>
        /// 画像の水平方向に連結
        /// https://imagingsolution.net/program/csharp/image_combine/
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public System.Drawing.Bitmap horizontalCombineImage(System.Drawing.Bitmap[] src)
        {
            int dstWidth = 0;
            int dstHeight = 0;
            System.Drawing.Imaging.PixelFormat dstPixelFormat = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;   //  256色
            for (int i = 0; i < src.Length; i++) {
                //  サイズの拡張
                if (dstHeight < src[i].Height)
                    dstHeight = src[i].Height;
                dstWidth += src[i].Width;
                //  最大ビット数の検索
                if (System.Drawing.Bitmap.GetPixelFormatSize(dstPixelFormat) <
                    System.Drawing.Bitmap.GetPixelFormatSize(src[i].PixelFormat)) {
                    dstPixelFormat = src[i].PixelFormat;
                }
            }

            var dst = new System.Drawing.Bitmap(dstWidth, dstHeight, dstPixelFormat);
            var dstRect = new System.Drawing.Rectangle();

            using (var g = System.Drawing.Graphics.FromImage(dst)) {
                for (int i = 0; i < src.Length; i++) {
                    dstRect.Width = src[i].Width;
                    dstRect.Height = src[i].Height;
                    //  描画
                    g.DrawImage(src[i], dstRect, 0, 0, src[i].Width, src[i].Height, System.Drawing.GraphicsUnit.Pixel);
                    //  次の描画先
                    dstRect.X = dstRect.Right;
                }
            }
            return dst;
        }

        /// <summary>
        /// BitMap からBitmapSourceに変換
        /// https://qiita.com/YSRKEN/items/a24bf2173f0129a5825c
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public System.Windows.Media.Imaging.BitmapSource bitmap2BitmapSource(System.Drawing.Bitmap bitmap)
        {
            // MemoryStreamを利用した変換処理
            using (var ms = new System.IO.MemoryStream()) {
                // MemoryStreamに書き出す
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                // MemoryStreamをシーク
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                // MemoryStreamからBitmapFrameを作成
                // (BitmapFrameはBitmapSourceを継承しているのでそのまま渡せばOK)
                System.Windows.Media.Imaging.BitmapSource bitmapSource =
                    System.Windows.Media.Imaging.BitmapFrame.Create(
                        ms,
                        System.Windows.Media.Imaging.BitmapCreateOptions.None,
                        System.Windows.Media.Imaging.BitmapCacheOption.OnLoad
                    );
                return bitmapSource;
            }
        }

        /// <summary>
        /// 指定領域をキャプチャーしてクリップボードに入れる
        /// (画面全体をキャプチャーして指定領域を切り抜く)
        /// 例: Point sp = CvMapData.PointToScreen(new Point(0, 0));
        ///     ydraw.screenCapture((int)sp.X, (int)sp.Y, (int)CvMapData.ActualWidth, (int)CvMapData.ActualHeight);
        /// </summary>
        /// <param name="left">左座標</param>
        /// <param name="top">上座標</param>
        /// <param name="width">領域の幅</param>
        /// <param name="height">領域の高さ</param>
        public void screenCapture(int left, int top, int width, int height)
        {
            // 矩形領域
            var rectangle = new System.Drawing.Rectangle(left, top, width, height);
            var bitmap = new System.Drawing.Bitmap(rectangle.Width, rectangle.Height);
            var graphics = System.Drawing.Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(new System.Drawing.Point(rectangle.X, rectangle.Y), new System.Drawing.Point(0, 0), bitmap.Size);
            // グラフィックスの解放
            graphics.Dispose();

            // 画像の表示
            using (var stream = new MemoryStream()) {
                bitmap.Save(stream, ImageFormat.Png);
                Clipboard.SetImage(bitmap2BitmapSource(bitmap));
                //stream.Seek(0, SeekOrigin.Begin);
                //image.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }

    }
}

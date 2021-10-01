using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfLib
{
    /// <summary>
    /// 三次元ベクトル
    /// </summary>
    public class vector
    {
        public int r;
        public double[] e;

        public vector(int col)
        {
            e = new double[col];
        }
        public vector(vector v)
        {
            set(v);
        }
        public void set(vector v)
        {
            r = v.r;
            e = new double[v.e.Length];
            for (int i = 0; i < v.e.Length; i++)
                e[i] = v.e[i];
        }
    }

    /// <summary>
    /// 三次元のアフィン変換マトリックス
    /// </summary>
    public class matrix
    {
        public int c, r;
        public double[,] e;
        public matrix(int row, int col)
        {
            e = new double[row, col];
        }
        public matrix(matrix m)
        {
            set(m);
        }
        public void set(matrix m)
        {
            c = m.c;
            r = m.r;
            e = new double[m.e.GetLength(0), m.e.GetLength(1)];
            for (int i = 0; i < m.e.GetLength(0); i++)
                for (int j = 0; j < m.e.GetLength(1); j++)
                    e[i, j] = m.e[i, j];
        }
    }

    public class YTurtle : YDrawingShapes
    {
        public Box mWorld;                  //  ワールド座標
        private Rect mView;                 //  描画領域
        private bool mAspectFix = true;     //  アスペクト比固定
        private double mTextSize = 20;      //  テキストサイズ

        //  Turtleクラスのデフォルト値
        private double mWorldWidth = 640;   //  論理座標の幅
        private double mWorldHeight = 400;  //  論理座標の高さ
        private double mWorldXMargine = 10; //  ビューポートのXマージン
        private double mWorldYMargine = 10; //  ビューポートのYマージン
        private double x = 320;
        private double y = 200;
        private double _Theta = 0;
        private Brush[] mColor = {
            Brushes.White, Brushes.Silver, Brushes.Gray,  Brushes.Black,    //  Silver→LightGray
            Brushes.Red,   Brushes.Maroon, Brushes.Yellow,Brushes.Olive,    //  Maroon→Brown
            Brushes.Lime,  Brushes.Green,  Brushes.Aqua,  Brushes.Teal,     //  Aqua→Cyan
            Brushes.Blue,  Brushes.Navy,   Brushes.Fuchsia, Brushes.Purple, //  Fuchsia→Magenta
            //Brushes.Cyan, Brushes.DarkGray,Brushes.LightGray, Brushes.Magenta,Brushes.Orange, Brushes.Pink,         
        };

        private const int COL = 6;
        private const int ROW = 3;
        private const int DISTANCE = 1000;
        public vector Pos = new vector(COL);
        public matrix Base = new matrix(ROW, COL);

        public YTurtle()
        {

        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="c">Panel/Canvas</param>
        public YTurtle(Canvas c)
        {
            mCanvas = c;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="c">Panel/Canvas</param>
        /// <param name="windowWidth">スクリーン座標の幅</param>
        /// <param name="windowHeight">スクリーン座標の高さ</param>
        public YTurtle(Canvas c, double windowWidth, double windowHeight)
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
        /// スクリーン座標でのWindowサイズを設定
        /// </summary>
        /// <param name="width">スクリーン座標の幅</param>
        /// <param name="height">スクリーン座標の高さ</param>
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
            mView = new Rect(new System.Windows.Point(left, top), new Point(right, bottom));
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
            if (mAspectFix)
                aspectFix();
            setTextSize(mTextSize);
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
        /// グラフィック画面の初期化と開始
        /// </summary>
        public void graphicstart()
        {
            setViewArea(mWorldXMargine, mWorldYMargine, mWindowWidth - mWorldXMargine, mWindowHeight - mWorldYMargine);
            setWorldWindow(0, 0, mWorldWidth, mWorldHeight);

            clear();
            backColor(Brushes.White);
            setColor(Brushes.Blue);
            frame();
            _Theta = 0;
            setColor(Brushes.Black);
        }

        /// <summary>
        /// X軸のワールド座標をスクリーン座標に変換する
        /// </summary>
        /// <param name="x">ワールド座標</param>
        /// <returns>スクリーン座標</returns>
        private double cnvWorld2ScreenX(double x)
        {
            return (x - mWorld.Left) * (mView.Right - mView.Left) / (mWorld.Right - mWorld.Left) + mView.Left;
        }

        /// <summary>
        /// Y軸のワールド座標をスクリーン座標に変換する
        /// </summary>
        /// <param name="y">ワールド座標</param>
        /// <returns>スクリーン座標</returns>
        private double cnvWorld2ScreenY(double y)
        {
            return (y - mWorld.Top) * (mView.Top - mView.Bottom) / (mWorld.Top - mWorld.Bottom) + mView.Top;
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

        /// <summary>
        /// 線分の起点座標の設定
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void pmoveto(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// 長さlの線分表示
        /// </summary>
        /// <param name="l">長さ</param>
        public void move(double l)
        {
            double ex = x + l * Math.Cos(_Theta * Math.PI / 180);
            double ey = y + l * Math.Sin(_Theta * Math.PI / 180);
            plineto(ex, ey);
        }

        public void rlineto(double dx, double dy)
        {
            plineto(this.x + dx, this.y + dy);
        }

        /// <summary>
        /// 起点から指定座標(絶対座標)までの線分表示
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        public void plineto(double x, double y)
        {
            line(this.x, this.y, x, y);
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// 座標指定による線分表示(起点座標は変更しない)
        /// </summary>
        /// <param name="sx">始点X座標</param>
        /// <param name="sy">始点Y座標</param>
        /// <param name="ex">終点X座標</param>
        /// <param name="ey">終YX座標</param>
        private void line(double sx, double sy, double ex, double ey)
        {
            //drawLine(new Point(sx, sy), new Point(ex, ey));
            Point ps = new Point(sx, sy);
            Point pe = new Point(ex, ey);
            base.drawLine(cnvWorld2Screen(ps), cnvWorld2Screen(pe));
        }

        /// <summary>
        /// 向きを左に回転(相対角度で反時計回り)
        /// </summary>
        /// <param name="x">回転角(度)</param>
        public void left(double x)
        {
            _Theta += x;
        }

        /// <summary>
        /// 向きを右に回転(相対角度で時計回り)
        /// </summary>
        /// <param name="x">回転角(度)</param>
        public void right(double x)
        {
            _Theta -= x;
        }

        /// <summary>
        /// 現在の座標のX位置を取得
        /// </summary>
        /// <returns></returns>
        public double getX()
        {
            return x;
        }

        /// <summary>
        /// 現在の座標のY位置を取得
        /// </summary>
        /// <returns></returns>
        public double getY()
        {
            return y;
        }

        /// <summary>
        /// 表示画面の枠を表示
        /// </summary>
        private void frame()
        {
            setColor(Brushes.Blue);
            pmoveto(0, 0);
            plineto(mWorldWidth, 0);
            plineto(mWorldWidth, mWorldHeight);
            plineto(0, mWorldHeight);
            plineto(0, 0);
            setColor(Brushes.LightGray);
            line(mWorldWidth / 2, 0, mWorldWidth / 2, mWorldHeight);
            line(0, mWorldHeight / 2, mWorldWidth, mWorldHeight / 2);
        }

        /// <summary>
        /// 三次元変換マトリックスの初期化
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        public void world(int row, int col)
        {
            Pos.r = Base.r = row;
            Base.c = col;
            for (int i = 0; i < row; i++) {
                Pos.e[i] = 0;
                for (int j = 0; j < col; j++) {
                    Base.e[i, j] = kronecker(i, j);
                }
            }
        }

        private int kronecker(int i, int j)
        {
            if (i == j)
                return (1);
            else
                return (0);
        }

        /// <summary>
        /// 三次元変換マトリックスの回転パラメータ設定
        /// from-to 0-1 : Z軸 1-2 : X軸 2-0 : Y軸
        /// </summary>
        /// <param name="from">回転軸</param>
        /// <param name="to">回転軸</param>
        /// <param name="th">回転角(度)</param>
        /// <param name="b">変換マトリックス</param>
        public void turn(int from, int to, double th, ref matrix b)
        {
            double bx;
            double cs = Math.Cos(th * Math.PI / 180);
            double sn = Math.Sin(th * Math.PI / 180);
            for (int i = 0; i < b.r; i++) {
                bx = b.e[i, from];
                b.e[i, from] = cs * bx + sn * b.e[i, to];
                b.e[i, to] = cs * b.e[i, to] - sn * bx;
            }
        }

        /// <summary>
        /// 三次元ベクトルに変換マトリックスを反映する
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="len">長さ</param>
        /// <param name="b">変換マトリックス</param>
        /// <param name="p">ベクトル</param>
        public void mov(int dir, double len, matrix b, ref vector p)
        {
            for (int i = 0; i < p.r; p.e[i] += len * b.e[i, dir], i++) ;
        }

        /// <summary>
        /// 起点を透視投影で三次元座標から二次元座標に変換する
        /// </summary>
        public void MPOS()
        {
            pmoveto(projx(Pos), projy(Pos));
        }

        /// <summary>
        /// 透視投影で三次元座標から二次元座標に変換して線分を描画する
        /// </summary>
        public void DPOS()
        {
            plineto(projx(Pos), projy(Pos));
        }

        private double projx(vector v)
        {
            return (320 + DISTANCE * v.e[0] / (DISTANCE - v.e[2]));
        }

        private double projy(vector v)
        {
            return (200 - DISTANCE * v.e[1] / (DISTANCE - v.e[2]));
        }
    }
}

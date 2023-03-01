using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace WpfLib
{
    public class GraphData
    {
        public List<Point> mData { get; set; }
        public Rect mDataArea;
        public Brush mLineColor = Brushes.Black;
        public Brush mBarColor = Brushes.Blue;
        public Brush mPointColor = Brushes.Red;
        public Graph.GRAPHTYPE mGraphType = Graph.GRAPHTYPE.SEQUENTIAL;

        public GraphData(List<Point> data)
        {
            this.mData = data;
            mDataArea = getDataArea(data);
        }

        public GraphData(List<Point> data, Brush lineColor, Brush barColor, Brush pointColor)
        {
            this.mData = data;
            this.mLineColor = lineColor;
            this.mBarColor = barColor;
            this.mPointColor = pointColor;
            mDataArea = getDataArea(data);
        }

        private Rect getDataArea(List<Point> data)
        {
            Point pmin = new Point(data.Select(p => p.X).Min(), data.Select(p => p.Y).Min());
            Point pmax = new Point(data.Select(p => p.X).Max(), data.Select(p => p.Y).Max());
            return new Rect(pmin, pmax);
        }
    }

    public class Graph : YWorldShapes
    {
        public List<GraphData> mDataList;
        private List<Point> mData;                              //  グラフデータ
        private Rect mDataArea;                                 //  データの表示領域
        private Rect mGraphArea;                                 //  グラフの表示領域
        private double mTextSize = 15;                          //  文字の大きさ
        private double mPoistSize = 4.0;                        //  点表示の点の半径
        private int mBackColor = 138;                           //  WhiteSmoke 141色の138番目
        private double mStepYsize;                              //  グラフの補助線の間隔
        private double mStepXsize;                              //  グラフの補助線の間隔
        public enum GRAPHTYPE { NON = 0, BAR = 1, SEQUENTIAL = 2, POINT = 4, STACK = 8 };
        public GRAPHTYPE mGraphType = GRAPHTYPE.SEQUENTIAL;

        private YLib ylib = new YLib();

        public Graph(System.Windows.Controls.Canvas c)
        {
            mCanvas = c;
        }

        public void init(double width, double height)
        {
            setAspectFix(false);
            setWindowSize(width, height);
            setViewArea(0, 0, width, height);
        }

        public void setData(List<Point> data)
        {
            mData = data;
            mDataArea = getDataArea(data);
            mGraphArea = new Rect(mDataArea.TopLeft, mDataArea.BottomRight);
        }

        public void setData(GraphData data)
        {
            mDataList = new List<GraphData> { data };
            mDataArea = data.mDataArea;
            mGraphArea = new Rect(mDataArea.TopLeft, mDataArea.BottomRight);
        }

        public void addData(GraphData data)
        {
            mDataList.Add(data);
            mDataArea.Extension(data.mDataArea);
            mGraphArea = new Rect(mDataArea.TopLeft, mDataArea.BottomRight);
        }

        public void drawGraph()
        {
            //  初期設定
            backColor(getColor(mBackColor));
            setThickness(1);
            clear();

            setWorldArea();
            drawAxis();
            //  データ表示
            drawData(mDataList[0]);
            //  グラフ領域の枠
            setColor(Brushes.Black);
            setFillColor(null);
            drawWRectangle(new Rect(mGraphArea.X, mGraphArea.Y + mGraphArea.Height, mGraphArea.Width, mGraphArea.Height), 0);
        }

        private void setWorldArea()
        {
            //  仮のWindow Sizeを設定
            setWorldWindow(mDataArea.X - mDataArea.Width * 0.05, mDataArea.Y + mDataArea.Height * (1 + 0.05),
                mDataArea.X + mDataArea.Width * (1 + 0.05), mDataArea.Y - mDataArea.Height * 0.1);

            setScreenTextSize(mTextSize);
            mStepXsize = stepXSize(mDataArea, 5);
            mStepYsize = stepYSize(mDataArea, 5);

            //  グラフエリアのマージンを求める
            double leftMargine = 0;
            double bottomMargine = 0;
            double righMargine = Math.Abs(30 / world2screenXlength(1));
            double topMargine = Math.Abs(30 / world2screenYlength(1));
            double barWidth = 0.0;
            if (0 < (mGraphType & GRAPHTYPE.BAR)) {
                barWidth = mDataArea.Width / (mData.Count - 1);
                mGraphArea = new Rect(new Point(mDataArea.Left - barWidth / 2.0, mDataArea.Top),
                    new Point(mDataArea.Right + barWidth / 2.0, mDataArea.Bottom));
            }
            //  縦軸の目盛り文字列の最大幅から左側マージンを求める
            for (double y = mDataArea.Y; y <= mDataArea.Y + mDataArea.Height; y += mStepYsize) {
                Size size = measureWText(getYScaleValue(y));
                leftMargine = Math.Max(leftMargine, size.Width);
            }
            //  横軸の目盛り文字列の最大幅から下側マージンを求める
            for (double x = mDataArea.X; x <= mDataArea.X + mDataArea.Width; x += mStepXsize) {
                Size size = measureWText(getXScaleValue(x));
                bottomMargine = Math.Max(bottomMargine, size.Width);
            }
            leftMargine += righMargine;
            bottomMargine = Math.Abs(screen2worldYlength(world2screenXlength(bottomMargine)));
            bottomMargine += getTextSize();
            bottomMargine += topMargine;
            mGraphArea.Extension(new Point(mGraphArea.X, 0.0));

            //  グラフエリアの再設定
            setWorldWindow(mGraphArea.X - leftMargine, mGraphArea.Y + mGraphArea.Height + topMargine,
                            mGraphArea.X + mGraphArea.Width + righMargine, mGraphArea.Y - bottomMargine);
        }

        private void drawAxis()
        {
            setScreenTextSize(mTextSize);

            //  縦軸の目盛りと補助線の表示
            setColor(Brushes.Gray);
            for (double y = mGraphArea.Top; y <= mDataArea.Bottom; y += mStepYsize) {
                //  補助線
                drawWLine(new Point(mGraphArea.Left, y), new Point(mGraphArea.Right, y));
                //  目盛
                drawWText(getYScaleValue(y), new Point(mGraphArea.Left, y), 0,
                    HorizontalAlignment.Right, VerticalAlignment.Center);
                if (y == mDataArea.Top && y % mStepXsize != 0) {
                    y -= y % mStepYsize;
                }
            }
            //  横軸軸の目盛りと補助線の表示
            setColor(Brushes.Aqua);
            double textMargine = Math.Abs(3 / world2screenYlength(1));
            for (double x = mDataArea.Left; x <= mDataArea.Right; x += mStepXsize) {
                //  指定ステップで補助線表示
                drawWLine(new Point(x, mGraphArea.Y), new Point(x, mGraphArea.Y + mGraphArea.Height));
                //  目盛表示
                if (x < mDataArea.Right - mDataArea.Width * 0.05)
                    drawWText(getXScaleValue(x), new Point(x, mGraphArea.Y - textMargine),
                        -Math.PI / 2, HorizontalAlignment.Left, VerticalAlignment.Center);
                if (x == mDataArea.Left && x % mStepXsize != 0)
                    x -= x % mStepXsize;
            }
            drawWText(getXScaleValue(mDataArea.Right),
                new Point(mDataArea.Right, mGraphArea.Y - textMargine),
                    -Math.PI / 2, HorizontalAlignment.Left, VerticalAlignment.Center);
        }


        private void drawData(GraphData data)
        {
            setColor(data.mLineColor);
            double barWidth = 0.0;
            if (0 < (data.mGraphType & GRAPHTYPE.BAR)) {
                barWidth = mDataArea.Width / (data.mData.Count - 1) * 0.8;
            }
            double r = screen2worldXlength(mPoistSize);
            Point sp = data.mData[0];
            for (int i = 0; i < data.mData.Count; i++) {
                if (0 < (data.mGraphType & GRAPHTYPE.BAR)) {
                    setFillColor(data.mBarColor);
                    Point ps = new Point(data.mData[i].X - barWidth / 2.0, mGraphArea.Y);
                    Point pe = new Point(data.mData[i].X + barWidth / 2.0, data.mData[i].Y);
                    drawWRectangle(ps, pe, 0.0);
                } 
                if (0 < (data.mGraphType & GRAPHTYPE.SEQUENTIAL)) {
                    setColor(data.mLineColor);
                    if (i == 0) {
                        sp = data.mData[i];
                    } else {
                        drawWLine(sp, data.mData[i]);
                        sp = data.mData[i];
                    }
                }
                if (0 < (data.mGraphType & GRAPHTYPE.POINT)) {
                    setFillColor(data.mPointColor);
                    drawWCircle(data.mData[i], r);
                }
            }
        }

        private Rect getDataArea(List<Point> data)
        {
            Point pmin = new Point(data.Select(p => p.X).Min(), data.Select(p => p.Y).Min());
            Point pmax = new Point(data.Select(p => p.X).Max(), data.Select(p => p.Y).Max());
            return new Rect(pmin, pmax);
        }

        private string getYScaleValue(double y)
        {
            return y.ToString("#,##0");
        }

        private string getXScaleValue(double x)
        {
            return x.ToString("#,##0.0");
        }

        private double stepYSize(Rect area, int stepCount)
        {
            return ylib.graphStepSize(area.Height, stepCount);
        }

        private double stepXSize(Rect area, int stepCount)
        {
            return ylib.graphStepSize(area.Width, stepCount);
        }
    }
}

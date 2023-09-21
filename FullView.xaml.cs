using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfLib
{
    /// <summary>
    /// FullScreen.xaml の相互作用ロジック
    /// 
    /// 枠なしの全画面のイメージ表示をおこなう
    /// マウスでドラッギングして領域の指定ができる
    /// </summary>
    public partial class FullView : Window
    {
        private double mWindowWidth;                            //  ウィンドウの高さ
        private double mWindowHeight;                           //  ウィンドウ幅

        public BitmapSource mBitmapSource;      //  トリミングする画像データ
        public Point mStartPoint = new Point(); //  指定領域の始点
        public Point mEndPoint = new Point();   //  指定領域の終点
        public bool mFullScreen = true;         //  全画面表示
        public bool mIsModeless = false;
        private bool mMouseDown = false;

        public FullView()
        {
            InitializeComponent();

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (mFullScreen) {
                // タイトルバーと境界線を表示しない
                this.WindowStyle = WindowStyle.None;
                // 最大化表示
                this.WindowState = WindowState.Maximized;
            } else {
                WindowFormLoad();
            }
            if (mIsModeless) {
                imScreen2.Source = mBitmapSource;
            } else {
                imScreen.Source = mBitmapSource;

                canvas.Width = mBitmapSource.Width;
                canvas.Height = mBitmapSource.Height;

                Width = mBitmapSource.Width + 2 + 15;
                Height = mBitmapSource.Height + 30 + 10;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowFormSave();
        }

        /// <summary>
        /// Windowの状態を前回の状態にする
        /// </summary>
        private void WindowFormLoad()
        {
            //  前回のWindowの位置とサイズを復元する(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.Reload();
            if (Properties.Settings.Default.FullViewWidth < 100 ||
                Properties.Settings.Default.FullViewHeight < 100 ||
                SystemParameters.WorkArea.Height < Properties.Settings.Default.FullViewHeight) {
                Properties.Settings.Default.FullViewWidth = mWindowWidth;
                Properties.Settings.Default.FullViewHeight = mWindowHeight;
            } else {
                Top = Properties.Settings.Default.FullViewTop;
                Left = Properties.Settings.Default.FullViewLeft;
                Width = Properties.Settings.Default.FullViewWidth;
                Height = Properties.Settings.Default.FullViewHeight;
            }
        }

        /// <summary>
        /// Window状態を保存する
        /// </summary>
        private void WindowFormSave()
        {
            //  Windowの位置とサイズを保存(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.FullViewTop = Top;
            Properties.Settings.Default.FullViewLeft = Left;
            Properties.Settings.Default.FullViewWidth = Width;
            Properties.Settings.Default.FullViewHeight = Height;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// イメージの上に領域の枠を描画
        /// </summary>
        /// <param name="ps">始点</param>
        /// <param name="pe">終点</param>
        private void drawRect(Point ps, Point pe)
        {
            canvas.Children.Clear();
            canvas.Children.Add(imScreen);
            Point psx = new Point(ps.X, pe.Y);
            Point psy = new Point(pe.X, ps.Y);
            Line line0 = drawLine(ps, psx);
            Line line1 = drawLine(ps, psy);
            Line line2 = drawLine(pe, psy);
            Line line3 = drawLine(pe, psx);
            canvas.Children.Add(line0);
            canvas.Children.Add(line1);
            canvas.Children.Add(line2);
            canvas.Children.Add(line3);
        }

        /// <summary>
        /// 線分のデータ作成
        /// </summary>
        /// <param name="ps">始点</param>
        /// <param name="pe">終点</param>
        /// <returns>Lineデータ</returns>
        private Line drawLine(Point ps, Point pe)
        {
            Line line = new Line();
            line.X1 = ps.X;
            line.Y1 = ps.Y;
            line.X2 = pe.X;
            line.Y2 = pe.Y;
            line.Stroke = System.Windows.Media.Brushes.Green;
            line.StrokeThickness = 1.0;
            return line;
        }

        /// <summary>
        /// [マウス左ボタンダウン]
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!mMouseDown) {
                //  枠の始点
                mMouseDown = true;
                mStartPoint = e.GetPosition(canvas);
            }
        }

        /// <summary>
        /// [マウスの移動]
        /// 左ボタンを押した状態の移動で枠をドラッギング
        /// 左ボタンを離すと終点を設定してダイヤログを終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point point = e.GetPosition(canvas);
            if (mMouseDown) {
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) {
                    mEndPoint = point;
                    drawRect(mStartPoint, point);
                } else {
                    mMouseDown = false;
                    DialogResult = true;
                    Close();
                }
            }
        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool control = false;
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                control = true;
            if (e.Key == Key.Escape) {
                //  ESCキーで終了
                if (!mIsModeless)
                    DialogResult = false;
                Close();
            }
        }
    }
}

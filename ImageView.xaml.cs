using Shell32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfLib
{
    /// <summary>
    /// ImageView.xaml の相互作用ロジック
    /// </summary>
    public partial class ImageView : Window
    {
        private double mWindowWidth;                            //  ウィンドウの高さ
        private double mWindowHeight;                           //  ウィンドウ幅
        private double mPrevWindowWidth;                        //  変更前のウィンドウ幅
        private WindowState mWindowState = WindowState.Normal;  //  ウィンドウの状態(最大化/最小化)

        public string mImagePath;
        public List<string> mImageList = new List<string>();
        private Point mMousePosition = new Point(0, 0);         //  マウス位置
        private bool mImagMove = false;                         //  イメージ移動フラグ


        private YLib ylib = new YLib();

        public ImageView()
        {
            InitializeComponent();
            WindowFormLoad();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowFormSave();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (mImageList == null || mImageList.Count == 0) {
                string folder = Path.GetDirectoryName(mImagePath);
                mImageList = ylib.getFiles(Path.Combine(folder, "*.jpg")).ToList();
            }
            ImImage.Source = ylib.getBitmapImage(mImagePath);
            Title = "画像データ [" + Path.GetFileName(mImagePath) + "]";
            setPhotoInfo(mImagePath);
        }

        private void Window_LayoutUpdated(object sender, EventArgs e)
        {
            if (WindowState != mWindowState &&
                WindowState == WindowState.Maximized) {
                //  ウィンドウの最大化時
                mWindowWidth = SystemParameters.WorkArea.Width;
                mWindowHeight = SystemParameters.WorkArea.Height;
            } else if (WindowState != mWindowState ||
                mWindowWidth != Width ||
                mWindowHeight != Height) {
                //  ウィンドウサイズが変わった時
                mWindowWidth = Width;
                mWindowHeight = Height;
            } else {
                //  ウィンドウサイズが変わらない時は何もしない
                mWindowState = WindowState;
                return;
            }
            mWindowState = WindowState;
            //  ウィンドウの大きさに合わせてコントロールの幅を変更する
            double dx = mWindowWidth - mPrevWindowWidth;
            mPrevWindowWidth = mWindowWidth;
            //  表示の更新
            //sampleGraphInit();
            //drawSampleGraph(mStartPosition, mEndPosition);
        }

        /// <summary>
        /// Windowの状態を前回の状態にする
        /// </summary>
        private void WindowFormLoad()
        {
            //  前回のWindowの位置とサイズを復元する(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.Reload();
            if (Properties.Settings.Default.ImageViewWidth < 100 ||
                Properties.Settings.Default.ImageViewHeight < 100 ||
                SystemParameters.WorkArea.Height < Properties.Settings.Default.ImageViewHeight) {
                Properties.Settings.Default.ImageViewWidth = mWindowWidth;
                Properties.Settings.Default.ImageViewHeight = mWindowHeight;
            } else {
                Top = Properties.Settings.Default.ImageViewTop;
                Left = Properties.Settings.Default.ImageViewLeft;
                Width = Properties.Settings.Default.ImageViewWidth;
                Height = Properties.Settings.Default.ImageViewHeight;
            }
        }

        /// <summary>
        /// Window状態を保存する
        /// </summary>
        private void WindowFormSave()
        {
            //  Windowの位置とサイズを保存(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.ImageViewTop = Top;
            Properties.Settings.Default.ImageViewLeft = Left;
            Properties.Settings.Default.ImageViewWidth = Width;
            Properties.Settings.Default.ImageViewHeight = Height;
            Properties.Settings.Default.Save();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            double d = 1.0;
            double cx = ImImage.ActualWidth / 2.0;
            double cy = ImImage.ActualHeight / 2.0;
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                d = 0.5;
            if (e.Key == Key.Left) {                //  左に移動
                //  前のデータファイルを表示
                nextImage(-1);
            } else if (e.Key == Key.Right) {        //  右に移動
                //  次のデータファイルを表示
                nextImage(1);
            } else if (e.Key == Key.Up) {           //  上に移動
                //  拡大
                imageZoom(1.25, cx, cy);
            } else if (e.Key == Key.Down) {         //  下に移動
                //  縮小
                imageZoom(1 / 1.25, cx, cy);
            } else if (e.Key == Key.PageUp) {       //  拡大
            } else if (e.Key == Key.PageDown) {     //  縮小
            } else if (e.Key == Key.F5) {           //  再表示
            } else if (e.Key == Key.Home) {         //  初期状態
                //  イメージを初期状態にする
                ImImage.RenderTransform = new MatrixTransform(new Matrix());
            }
        }

        /// <summary>
        /// ステータスバーのボタン処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)e.Source;
            double cx = ImImage.ActualWidth / 2.0;
            double cy = ImImage.ActualHeight / 2.0;
            if (button.Name == "BtGInfo") {
                //  イメージのプロパティ表示
                string buf = ylib.getIPTCall(mImagePath);
                ExifInfo exifInfo = new ExifInfo(mImagePath);
                buf += "\n" + exifInfo.getExifInfoAll();
                messageBox(buf, "属性表示[" + Path.GetFileName(mImagePath) + "]");
            } else if (button.Name == "BtGZoomReset") {
                //  イメージを初期状態にする
                Matrix matrix = new Matrix();
                ImImage.RenderTransform = new MatrixTransform(matrix);
            } else if (button.Name == "BtGZoomUp") {
                //  拡大
                imageZoom(1.25, cx, cy);
            } else if (button.Name == "BtGZoomDown") {
                //  縮小
                imageZoom(1 / 1.25, cx, cy);
            } else if (button.Name == "BtRotate") {
                //  回転
                Matrix matrix = ((MatrixTransform)ImImage.RenderTransform).Matrix;
                matrix.RotateAt(90, cx, cy);
                ImImage.RenderTransform = new MatrixTransform(matrix);
            } else if (button.Name == "BtPrevImage") {
                //  前のデータファイルを表示
                nextImage(-1);
            } else if (button.Name == "BtNextImage") {
                //  次のデータファイルを表示
                nextImage(1);
            }
        }

        /// <summary>
        /// [MouseWheel]マウスホイールによる拡大縮小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point pos =  e.GetPosition(this);
            var scale = 1.25;
            if (e.Delta < 0)
                scale = 1 / scale;
            imageZoom(scale, pos.X, pos.Y);
        }

        /// <summary>
        /// [MouseLeftButtonDown]イメージ移動の開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mMousePosition = e.GetPosition(this);
            mImagMove = true;
        }

        /// <summary>
        /// [MouseLeftButtonUp]イメージ移動の終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mImagMove = false;
        }

        /// <summary>
        /// [MouseMove]イメージの移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (mImagMove) {
                Point pos = e.GetPosition(this);
                Matrix matrix = ((MatrixTransform)ImImage.RenderTransform).Matrix;
                matrix.Translate(pos.X - mMousePosition.X, pos.Y - mMousePosition.Y);
                ImImage.RenderTransform = new MatrixTransform(matrix);
                mMousePosition = pos;
            }
        }

        /// <summary>
        /// 画像ファイルの移動
        /// </summary>
        /// <param name="next"></param>
        private void nextImage(int next)
        {
            int n = mImageList.IndexOf(mImagePath);
            if (0 <= (n + next) && n < mImageList.Count - next) {
                mImagePath = mImageList[n + next];
                ImImage.Source = ylib.getBitmapImage(mImagePath);
                Title = "画像データ [" + Path.GetFileName(mImagePath) + "]";
                setPhotoInfo(mImagePath);
            }
        }

        /// <summary>
        /// イメージの拡大縮小
        /// </summary>
        /// <param name="scale">拡大率</param>
        /// <param name="cx">拡大中心座標X</param>
        /// <param name="cy">拡大中心座標Y</param>
        private void imageZoom(double scale, double cx, double cy)
        {
            Matrix matrix = ((MatrixTransform)ImImage.RenderTransform).Matrix;
            matrix.ScaleAt(scale, scale, cx, cy);
            ImImage.RenderTransform = new MatrixTransform(matrix);
        }

        /// <summary>
        /// ステータスバーに画像情報を表示する
        /// </summary>
        /// <param name="path"></param>
        private void setPhotoInfo(string path)
        {
            //  ファイルプロパティ表示
            System.Windows.Media.Imaging.BitmapImage bmpImage = ylib.getBitmapImage(path);
            ExifInfo exifInfo = new ExifInfo(path);
            Point coodinate = exifInfo.getExifGpsCoordinate();
            TbPhotoInfo.Text = exifInfo.getDateTime();
            if (coodinate.isEmpty())
                TbPhotoInfo.Text += " (座標なし)";
            TbPhotoInfo.Text += " " + ylib.getIPTC(path)[4] + exifInfo.getUserComment();
            TbPhotoInfo.Text += " [" + bmpImage.PixelWidth + "x" + bmpImage.PixelHeight + "]";
            TbPhotoInfo.Text += " " + exifInfo.getCamera("カメラ {0} {1}");
            TbPhotoInfo.Text += " " + exifInfo.getCameraSetting(" 1/{0} s F{1} ISO {2} 焦点距離 {3} mm");
        }

        /// <summary>
        /// メッセージ表示ダイヤログ
        /// </summary>
        /// <param name="buf">メッセージ</param>
        /// <param name="title">タイトル</param>
        private void messageBox(string buf, string title)
        {
            InputBox dlg = new InputBox();
            dlg.mMainWindow = this;           //  親Windowの中心に表示
            dlg.Title = title;
            dlg.mWindowSizeOutSet = true;
            dlg.mWindowWidth = 500.0;
            dlg.mWindowHeight = 400.0;
            dlg.mMultiLine = true;
            dlg.mReadOnly = true;
            dlg.mEditText = buf;
            dlg.ShowDialog();
        }
    }
}

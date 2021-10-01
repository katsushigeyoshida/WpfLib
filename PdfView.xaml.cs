using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WpfLib
{
    /// <summary>
    /// PdfView.xaml の相互作用ロジック
    /// </summary>
    public partial class PdfView : Window
    {
        private double mWindowWidth;                            //  ウィンドウの高さ
        private double mWindowHeight;                           //  ウィンドウ幅
        private double mPrevWindowWidth;                        //  変更前のウィンドウ幅
        private WindowState mWindowState = WindowState.Normal;  //  ウィンドウの状態(最大化/最小化)

        public string mPdfFile;                                 //  PDFファイル名
        private string mPdfFilePath;                            //  PDFファイルパス(実行環境)
        private int mPageNo = 0;                                //  表示ページ
        private Windows.Data.Pdf.PdfDocument mPdfDocument;      //  PdfDocument Class

        public PdfView()
        {
            InitializeComponent();

            mWindowWidth = this.Width;
            mWindowHeight = this.Height;
            mPrevWindowWidth = mWindowWidth;
            WindowFormLoad();       //  Windowの位置とサイズを復元
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mPdfFilePath = Path.GetFullPath(mPdfFile);      //  PDFファイルのフルパス化
            pdfOpen(mPdfFilePath);                          //  PDFファイルを開く
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowFormSave();       //  ウィンドの位置と大きさを保存
        }

        private void Window_LayoutUpdated(object sender, EventArgs e)
        {
            if (this.WindowState != mWindowState &&
                this.WindowState == WindowState.Maximized) {
                //  ウィンドウの最大化時
                mWindowWidth = System.Windows.SystemParameters.WorkArea.Width;
                mWindowHeight = System.Windows.SystemParameters.WorkArea.Height;
            } else if (this.WindowState != mWindowState ||
                mWindowWidth != this.Width ||
                mWindowHeight != this.Height) {
                //  ウィンドウサイズが変わった時
                mWindowWidth = this.Width;
                mWindowHeight = this.Height;
            } else {
                //  ウィンドウサイズが変わらない時は何もしない
                mWindowState = this.WindowState;
                return;
            }
            mWindowState = this.WindowState;
            //  ウィンドウの大きさに合わせてコントロールの幅を変更する
            double dx = mWindowWidth - mPrevWindowWidth;
            mPrevWindowWidth = mWindowWidth;
            //  表示の更新
            //pdfView(mPdfFilePath, mPageNo);
        }

        /// <summary>
        /// Windowの状態を前回の状態にする
        /// </summary>
        private void WindowFormLoad()
        {
            //  前回のWindowの位置とサイズを復元する(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.Reload();
            if (Properties.Settings.Default.PdfViewWindowWidth < 100 || Properties.Settings.Default.PdfViewWindowHeight < 100 ||
                System.Windows.SystemParameters.WorkArea.Height < Properties.Settings.Default.PdfViewWindowHeight) {
                Properties.Settings.Default.PdfViewWindowWidth = mWindowWidth;
                Properties.Settings.Default.PdfViewWindowHeight = mWindowHeight;
            } else {
                this.Top = Properties.Settings.Default.PdfViewWindowTop;
                this.Left = Properties.Settings.Default.PdfViewWindowLeft;
                this.Width = Properties.Settings.Default.PdfViewWindowWidth;
                this.Height = Properties.Settings.Default.PdfViewWindowHeight;
            }
        }

        /// <summary>
        /// Window状態を保存する
        /// </summary>
        private void WindowFormSave()
        {
            //  Windowの位置とサイズを保存(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.PdfViewWindowTop = this.Top;
            Properties.Settings.Default.PdfViewWindowLeft = this.Left;
            Properties.Settings.Default.PdfViewWindowWidth = this.Width;
            Properties.Settings.Default.PdfViewWindowHeight = this.Height;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// キー入力によるページ切り替え
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Next) {
                mPageNo++;
                pdfPage(mPageNo);
            } else if (e.Key == System.Windows.Input.Key.PageUp) {
                mPageNo--;
                pdfPage(mPageNo);
            } else if (e.Key == System.Windows.Input.Key.Home) {
                mPageNo = 0;
                pdfPage(mPageNo);
            } else if (e.Key == System.Windows.Input.Key.End) {
                mPageNo = int.MaxValue;
                pdfPage(mPageNo);
            }
        }

        /// <summary>
        /// マウスホイールによるページ切り替え
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Muse Wheel Delta: {0}", e.Delta);
            if (0 > e.Delta) {
                mPageNo++;
                pdfPage(mPageNo);
            } else if (0 < e.Delta) {
                mPageNo--;
                pdfPage(mPageNo);
            }
        }

        /// <summary>
        /// PDFファイルを表示する
        /// </summary>
        /// <param name="pdfFile">ファイル名(絶対パス)</param>
        private async void pdfOpen(string pdfFile)
        {
            if (File.Exists(pdfFile)) {
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(pdfFile);
                try {
                    // PDFファイルを読み込む
                    mPdfDocument = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(file);
                } catch {
                }
                pdfPage(mPageNo);
            } else {
                MessageBox.Show(pdfFile + "\nが見つかりません");
            }
        }

        /// <summary>
        /// 指定のページを表示する。
        /// </summary>
        /// <param name="pageNo">表示ページ</param>
        private async void pdfPage(int pageNo)
        {
            if (mPdfDocument != null) {
                if (pageNo < 0)
                    pageNo = 0;
                if (mPdfDocument.PageCount <= pageNo)
                    pageNo = (int)(mPdfDocument.PageCount - 1);
                // nページ目を読み込む
                using (Windows.Data.Pdf.PdfPage page = mPdfDocument.GetPage((uint)pageNo)) {
                    BitmapImage image = new BitmapImage();
                    using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream()) {
                        await page.RenderToStreamAsync(stream);
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = stream.AsStream();
                        image.EndInit();
                    }
                    ImPdf.Source = image;
                }
                mPageNo = pageNo;
            }
        }
    }
}

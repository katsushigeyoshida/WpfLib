using System.Windows;

namespace WpfLib
{
    /// <summary>
    /// HelpView.xaml の相互作用ロジック
    /// 
    /// ヘルプ用のテキスト表示とPDFファイルの選択表示をおこなう
    /// </summary>
    public partial class HelpView : Window
    {
        public string mHelpText;            //  ヘルプテキスト
        public string[] mPdfFile;           //  PDFファイル名配列

        private double mWindowWidth;        //  ウィンドウの高さ
        private double mWindowHeight;       //  ウィンドウ幅

        /// <summary>
        /// ヘルプデータの表示
        /// </summary>
        /// <param name="type"></param>
        public HelpView()
        {
            InitializeComponent();

            mWindowWidth = this.Width;
            mWindowHeight = this.Height;
            WindowFormLoad();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowFormSave();
        }

        /// <summary>
        /// Windowのサイズと位置を復元
        /// </summary>
        private void WindowFormLoad()
        {
            //  前回のWindowの位置とサイズを復元する(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.Reload();
            if (Properties.Settings.Default.HelpWindowWidth < 100 || Properties.Settings.Default.HelpWindowHeight < 100 ||
                System.Windows.SystemParameters.WorkArea.Height < Properties.Settings.Default.HelpWindowHeight) {
                Properties.Settings.Default.HelpWindowWidth = mWindowWidth;
                Properties.Settings.Default.HelpWindowHeight = mWindowHeight;
            } else {
                this.Top = Properties.Settings.Default.HelpWindowTop;
                this.Left = Properties.Settings.Default.HelpWindowLeft;
                this.Width = Properties.Settings.Default.HelpWindowWidth;
                this.Height = Properties.Settings.Default.HelpWindowHeight;
                double dy = this.Height - mWindowHeight;
            }
        }

        /// <summary>
        /// Windowのサイズと位置を保存
        /// </summary>
        private void WindowFormSave()
        {
            //  Windowの位置とサイズを保存(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.HelpWindowTop = this.Top;
            Properties.Settings.Default.HelpWindowLeft = this.Left;
            Properties.Settings.Default.HelpWindowWidth = this.Width;
            Properties.Settings.Default.HelpWindowHeight = this.Height;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// PDFファイル選択表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbPdfFile_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (0 <= CbPdfFile.SelectedIndex) {
                WpfLib.PdfView pdfView = new WpfLib.PdfView();
                pdfView.mPdfFile = CbPdfFile.Items[CbPdfFile.SelectedIndex].ToString();
                pdfView.Show();
            }
        }

        /// <summary>
        /// ヘルプテキストとPDFファイルの設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HelpTb.Text = mHelpText;            //  ヘルプテキスト
            CbPdfFile.ItemsSource = mPdfFile;   //  PDFファイル名配列
        }
    }
}

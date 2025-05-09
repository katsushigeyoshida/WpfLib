using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfLib
{
    /// <summary>
    /// DiffFolder.xaml の相互作用ロジック
    /// </summary>
    public partial class DiffFolder : Window
    {
        private double mWindowWidth;                            //  ウィンドウの高さ
        private double mWindowHeight;                           //  ウィンドウ幅
        private double mPrevWindowWidth;                        //  変更前のウィンドウ幅
        private WindowState mWindowState = WindowState.Normal;  //  ウィンドウの状態(最大化/最小化)

        public string mSrcFolder = ".";         //  比較元フォルダ
        public string mDestFolder = ".";        //  比較先フォルダ
        public string mSrcTitle = "";           //  比較元タイトル
        public string mDestTitle = "";          //  比較先タイトル

        public bool mDiffOnly = true;           //  差異ファイル表示と全ファイル表示の切替
        public bool mHashChk = true;            //  CRCによる差異表示/サイズと日付による再表示の切替

        public string mDiffTool = "";           //  ファイル比較ツール(WinMergeなど)

        private DirectoryDiff mDiffFolder;      //  フォルダ比較クラス
        private List<DiffFile> mDiffFileList;   //  比較結果リスト

        private YLib ylib = new YLib();

        public DiffFolder()
        {
            InitializeComponent();

            WindowFormLoad();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!mHashChk) {
                //  HashCodeを使わない時
                dgColSrcCrc.Visibility = Visibility.Collapsed;  //  空白も非表示
                dgColDstCrc.Visibility = Visibility.Collapsed;
            }
            if (0 < mSrcTitle.Length) tbSrcTitle.Text = mSrcTitle;
            if (0 < mDestTitle.Length) tbDestTitle.Text = mDestTitle;
            tbSrcFolder.Text = mSrcFolder;
            tbDestFolder.Text = mDestFolder;
            rbDiffFile.IsChecked = mDiffOnly;
            rbAllFile.IsChecked = !mDiffOnly;

            setDiffFolder(mSrcFolder, mDestFolder);
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
            if (Properties.Settings.Default.DiffFolderWindowWidth < 100 ||
                Properties.Settings.Default.DiffFolderWindowHeight < 100 ||
                SystemParameters.WorkArea.Height < Properties.Settings.Default.DiffFolderWindowHeight) {
                Properties.Settings.Default.DiffFolderWindowWidth = mWindowWidth;
                Properties.Settings.Default.DiffFolderWindowHeight = mWindowHeight;
            } else {
                Top = Properties.Settings.Default.DiffFolderWindowTop;
                Left = Properties.Settings.Default.DiffFolderWindowLeft;
                Width = Properties.Settings.Default.DiffFolderWindowWidth;
                Height = Properties.Settings.Default.DiffFolderWindowHeight;
            }
        }

        /// <summary>
        /// Window状態を保存する
        /// </summary>
        private void WindowFormSave()
        {
            //  Windowの位置とサイズを保存(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.DiffFolderWindowTop = Top;
            Properties.Settings.Default.DiffFolderWindowLeft = Left;
            Properties.Settings.Default.DiffFolderWindowWidth = Width;
            Properties.Settings.Default.DiffFolderWindowHeight = Height;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// [比較]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCompar_Click(object sender, RoutedEventArgs e)
        {
            setDiffFolder(mSrcFolder, mDestFolder);
        }

        /// <summary>
        /// [右へ更新]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btRightUpdate_Click(object sender, RoutedEventArgs e)
        {
            selectCopy(mSrcFolder, mDestFolder);
            setDiffFolder(mSrcFolder, mDestFolder);
        }

        /// <summary>
        /// [左へ更新]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btLeftUpdate_Click(object sender, RoutedEventArgs e)
        {
            selectCopy(mDestFolder, mSrcFolder);
            setDiffFolder(mSrcFolder, mDestFolder);
        }

        /// <summary>
        /// [終了]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// [差異/全ファイル]ラジオボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbDiffFile_Click(object sender, RoutedEventArgs e)
        {
            setDiffFolder(mSrcFolder, mDestFolder);
            //ColorRow(dgDiffFolder);
        }

        /// <summary>
        /// [比較ファイルリスト]ダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgDiffFolder_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int index = dgDiffFolder.SelectedIndex;
            if (0 <= index && 0 < mDiffTool.Length) {
                DiffFile fileData = (DiffFile)dgDiffFolder.Items[index];
                string srcPath = fileData.getPath(mSrcFolder);
                string destPath = fileData.getPath(mDestFolder);
                ylib.processStart(mDiffTool, $"\"{srcPath}\" \"{destPath}\"");
            }
        }

        /// <summary>
        /// 選択したファイルのみを更新(コピー)
        /// </summary>
        /// <param name="srcFolder"></param>
        /// <param name="destFolder"></param>
        private void selectCopy(string srcFolder, string destFolder)
        {
            IList selItems = dgDiffFolder.SelectedItems;
            int copyType = cbOverWriteForce.IsChecked == true ? 2 : 0;
            if (0 < selItems.Count) {
                pbCopyCount.Minimum = 0;
                pbCopyCount.Maximum = selItems.Count;
                pbCopyCount.Value = 0;
                if (ylib.messageBox(this.Owner, $"{srcFolder} から\n{destFolder} に\n{selItems.Count} ファイル コピーします",
                    "", "確認", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                    foreach (DiffFile fileData in selItems) {
                        string srcPath = fileData.getPath(srcFolder);
                        string destPath = fileData.getPath(destFolder);
                        System.Diagnostics.Debug.WriteLine($"{srcPath} {destPath}");
                        ylib.fileCopy(srcPath, destPath, copyType); pbCopyCount.Value++;
                    }
                }
            }
        }

        /// <summary>
        /// フォルダの比較と結果の表示
        /// </summary>
        /// <param name="srcFolder"></param>
        /// <param name="dstFolder"></param>
        private void setDiffFolder(string srcFolder, string dstFolder)
        {
            mDiffFolder = new DirectoryDiff(srcFolder, dstFolder, mHashChk);

            //  同一ファイルを除く
            List<FilesData> files = mDiffFolder.stripSameFile(rbDiffFile.IsChecked == true);

            if (mDiffFileList == null)
                mDiffFileList = new List<DiffFile>();
            mDiffFileList.Clear();
            foreach (FilesData filesData in files)
                mDiffFileList.Add(new DiffFile(filesData));
            dgDiffFolder.ItemsSource = new ReadOnlyCollection<DiffFile>(mDiffFileList);
        }

        /// <summary>
        /// 行ごとに色設定(あまりうまくいかない)
        /// </summary>
        /// <param name="dg"></param>
        private void ColorRow(DataGrid dg)
        {
            for (int i = 0; i < dg.Items.Count; i++) {
                DataGridRow row = (DataGridRow)dg.ItemContainerGenerator.ContainerFromIndex(i);
                if (row != null) {
                    int index = row.GetIndex();
                    if (index % 2 == 0) {
                        SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(100, 255, 104, 0));
                        row.Background = brush;
                    } else {
                        SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(100, 255, 232, 0));
                        row.Background = brush;
                    }
                }
            }
        }
    }
}

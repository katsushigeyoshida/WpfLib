using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace WpfLib
{
    /// <summary>
    /// MenuDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class MenuDialog : Window
    {
        public double mWindowWidth;                         //  ウィンドウの高さ
        public double mWindowHeight;                        //  ウィンドウ幅
        public bool mWindowSizeOutSet = false;              //  ウィンドウサイズの外部設定
        //  ウィンドウの位置
        public Window mMainWindow = null;                   //  親ウィンドウの設定
        public int mHorizontalAliment = -1;                 //  0: Left 1: Center 2:Right
        public int mVerticalAliment = -1;                   //  0:Top 1:Center 2:Bottom

        public List<string> mMenuList;
        public string mResultMenu;

        public MenuDialog()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowFormSave();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowFormLoad();

            lbMenuList.ItemsSource = mMenuList;
            Height = Math.Min(mMenuList.Count * 28, Height);

            if (mMainWindow != null) {
                //  親ウィンドウに対しての表示位置
                //  水平方向
                if (mHorizontalAliment == 0)
                    Left = mMainWindow.Left;                                    //  LEFT
                else if (mHorizontalAliment == 2)
                    Left = mMainWindow.Left + (mMainWindow.Width - Width);      //  RIGHT
                else
                    Left = mMainWindow.Left + (mMainWindow.Width - Width) / 2;  //  CENTER
                //  垂直方向
                if (mVerticalAliment == 0)
                    Top = mMainWindow.Top;                                      //  TOP
                else if (mVerticalAliment == 2)
                    Top = mMainWindow.Top + (mMainWindow.Height - Height);      //  BOTTOM
                else
                    Top = mMainWindow.Top + (mMainWindow.Height - Height) / 2;  //  CENTER
            }
            //  メニューリストにフォーカスを設定
            lbMenuList.Focus();
        }

        /// <summary>
        /// ダブルクリックによる選択終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbMenuList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            selectMenu();
        }

        /// <summary>
        /// [Key]ダウン エンターキーで選択終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbMenuList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                selectMenu();
            }
        }

        /// <summary>
        /// メニューを選択して終了
        /// </summary>
        private void selectMenu()
        {
            if (0 <= lbMenuList.SelectedIndex) {
                mResultMenu = lbMenuList.Items[lbMenuList.SelectedIndex].ToString();
            } else {
                mResultMenu = "";
            }
            Close();
        }

        /// <summary>
        /// Windowの状態を前回の状態にする
        /// </summary>
        private void WindowFormLoad()
        {
            //  前回のWindowの位置とサイズを復元する(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.Reload();
            if (Properties.Settings.Default.MenuDialgWidth < 100 ||
                Properties.Settings.Default.MenuDialgHeight < 100 ||
                SystemParameters.WorkArea.Height < Properties.Settings.Default.MenuDialgHeight) {
                Properties.Settings.Default.MenuDialgWidth = mWindowWidth;
                Properties.Settings.Default.MenuDialgHeight = mWindowHeight;
            } else {
                Top = Properties.Settings.Default.MenuDialgTop;
                Left = Properties.Settings.Default.MenuDialgLeft;
                Width = Properties.Settings.Default.MenuDialgWidth;
                Height = Properties.Settings.Default.MenuDialgHeight;
            }
        }

        /// <summary>
        /// Window状態を保存する
        /// </summary>
        private void WindowFormSave()
        {
            //  Windowの位置とサイズを保存(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.MenuDialgTop = Top;
            Properties.Settings.Default.MenuDialgLeft = Left;
            Properties.Settings.Default.MenuDialgWidth = Width;
            Properties.Settings.Default.MenuDialgHeight = Height;
            Properties.Settings.Default.Save();
        }
    }
}

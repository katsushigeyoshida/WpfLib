using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
        public Window mMainWindow = null;                   //  親ウィンドウの設定

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

            if (mMainWindow != null) {
                //  親ウィンドウの中心に表示
                Left = mMainWindow.Left + (mMainWindow.Width - Width) / 2;
                Top = mMainWindow.Top + (mMainWindow.Height - Height) / 2;
            }
            lbMenuList.ItemsSource = mMenuList;
        }

        private void lbMenuList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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

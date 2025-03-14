﻿using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace WpfLib
{
    /// <summary>
    /// 文字列編集ダイヤログ
    /// ・入力文字列   mEditText
    /// ・複数行対応   mMultiLine
    /// ・表示専用設定 mReadOnly
    /// ・文字サイズ   mFontSize
    /// ・文字ズームボタン mFontZoomButtonVisible
    /// </summary>
    public partial class InputBox : Window
    {
        public double mWindowWidth;                         //  ウィンドウの高さ
        public double mWindowHeight;                        //  ウィンドウ幅
        public bool mWindowSizeOutSet = false;              //  ウィンドウサイズの外部設定
        public Window mMainWindow = null;                   //  親ウィンドウの設定

        public string mEditText;
        public bool mMultiLine = false;                     //  複数行入力可否
        public bool mReadOnly = false;                      //  リードオンリー,OKボタン非表示
        public int mFontSize = 12;                          //  文字サイズ
        public bool mFontZoomButtomVisible = true;          //  文字ズームボタン表示/非表示

        private YLib ylib = new YLib();

        public InputBox()
        {
            InitializeComponent();

            mWindowWidth = this.Width;
            mWindowHeight = this.Height;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (mMainWindow != null) {
                //  親ウィンドウの中心に表示
                Left = mMainWindow.Left + (mMainWindow.Width - Width) / 2;
                Top = mMainWindow.Top + (mMainWindow.Height - Height) / 2;
            }

            //  編集文字列
            EditText.Text = mEditText;

            //  複数行入力設定
            if (mMultiLine) {
                EditText.AcceptsReturn = true;
                EditText.TextWrapping = TextWrapping.Wrap;
                EditText.VerticalContentAlignment = VerticalAlignment.Top;
                if (!mWindowSizeOutSet)
                    WindowFormLoad();
                else {
                    Width = mWindowWidth;
                    Height = mWindowHeight;
                }
            }
            //  表示専用(編集不可,OKボタン非表示)設定
            EditText.IsReadOnly = mReadOnly;
            //OK.Visibility = mReadOnly ? Visibility.Hidden : Visibility.Visible;
            Cancel.Visibility = mReadOnly ? Visibility.Hidden : Visibility.Visible;
            //  文字サイズ
            EditText.FontSize = mFontSize;
            //  文字サイズズームボタンの表示/非表示
            if (mFontZoomButtomVisible) {
                BtGZoomDown.Visibility = Visibility.Visible;
                BtGZoomUp.Visibility = Visibility.Visible;
            } else {
                BtGZoomDown.Visibility = Visibility.Hidden;
                BtGZoomUp.Visibility = Visibility.Hidden;
            }

            EditText.Focus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!mWindowSizeOutSet)
                WindowFormSave();
        }

        /// <summary>
        /// [OK} ＯＫボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            mEditText = EditText.Text;
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// [Cancel] キャンセルボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }


        /// <summary>
        /// Windowの状態を前回の状態にする
        /// </summary>
        private void WindowFormLoad()
        {
            //  前回のWindowの位置とサイズを復元する(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.Reload();
            if (Properties.Settings.Default.InputBoxWindowWidth < 100 ||
                Properties.Settings.Default.InputBoxWindowHeight < 100 ||
                SystemParameters.WorkArea.Height < Properties.Settings.Default.InputBoxWindowHeight) {
                Properties.Settings.Default.InputBoxWindowWidth = mWindowWidth;
                Properties.Settings.Default.InputBoxWindowHeight = mWindowHeight;
            } else {
                this.Top = Properties.Settings.Default.InputBoxWindowTop;
                this.Left = Properties.Settings.Default.InputBoxWindowLeft;
                this.Width = Properties.Settings.Default.InputBoxWindowWidth;
                this.Height = Properties.Settings.Default.InputBoxWindowHeight;
            }
        }

        /// <summary>
        /// Window状態を保存する
        /// </summary>
        private void WindowFormSave()
        {
            //  Windowの位置とサイズを保存(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.InputBoxWindowTop = this.Top;
            Properties.Settings.Default.InputBoxWindowLeft = this.Left;
            Properties.Settings.Default.InputBoxWindowWidth = this.Width;
            Properties.Settings.Default.InputBoxWindowHeight = this.Height;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// [+][-] 文字サイズズームボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = (Button)e.Source;
            if (bt.Name.CompareTo("BtGZoomDown") == 0) {
                mFontSize--;
            } else if (bt.Name.CompareTo("BtGZoomUp") == 0) {
                mFontSize++;
            }
            EditText.FontSize = mFontSize;
        }

        /// <summary>
        /// マウスダブルクリック操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            textOpen(lineSelect());
        }

        /// <summary>
        /// 選択文字列を開く
        /// </summary>
        private void textOpen(string word = "")
        {
            //  選択文字列
            if (word.Length <= 0)
                word = EditText.SelectedText;
            if (0 < word.Length && (0 <= word.IndexOf("http") || 0 <= word.IndexOf("file:"))) {
                int ps = word.IndexOf("http");
                if (ps < 0)
                    ps = word.IndexOf("file:");
                if (0 < ps) {
                    int pe = word.IndexOfAny(new char[] { ' ', '\t', '\n' }, ps);
                    if (0 < pe) {
                        word = word.Substring(ps, pe - ps);
                    } else {
                        word = word.Substring(ps);
                    }
                }
                ylib.openUrl(word);
            } else if (0 < word.Length && File.Exists(word)) {
                ylib.openUrl(word);
            }
        }

        /// <summary>
        /// 一行分を抽出
        /// </summary>
        /// <returns></returns>
        private string lineSelect()
        {
            int pos = EditText.SelectionStart;
            pos = pos >= EditText.Text.Length ? 0 : pos;
            int sp = EditText.Text.LastIndexOf("\n", pos);
            int ep = EditText.Text.IndexOf("\n", pos);
            if (0 <= sp && 0 <= ep && sp == ep) {
                pos++;
                ep = EditText.Text.IndexOf("\n", pos);
            }
            ep = ep < 0 ? EditText.Text.Length : ep;
            return EditText.Text.Substring(sp + 1, ep - sp - 1);
        }
    }
}

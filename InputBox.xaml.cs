using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        public string mEditText;                            //  編集テキスト
        public string mFilePath = "";                       //  編集中のファイルパス
        public bool mMultiLine = false;                     //  複数行入力可否
        public bool mTextRapping = true;                    //  折り返し
        public bool mReadOnly = false;                      //  リードオンリー,OKボタン非表示
        public bool mFileSelectMenu = false;                //  ファイル選択ダイヤログ
        public bool mDateMenu = false;                      //  日付挿入
        public bool mConvEscMenu = false;                   //  ESCシーケンス変換メニュー
        public bool mCalcMenu = false;                      //  計算メニュー
        public bool mHexCalcMenu = false;                   //  16進計算メニュー
        public int mFontSize = 12;                          //  文字サイズ
        public string mFontFamily = "";                     //  フォントファミリ
        public bool mFontZoomButtomVisible = true;          //  文字ズームボタン表示/非表示
        public bool mCallBackOn = false;                    //  コールバック有り
        public Action callback;                             //  コールバック関数

        private string[] mDateTimeMenu = {
            "今日の日付挿入 西暦(YYYY年MM月DD日)", "今日の日付挿入 西暦('YY年MM月DD日)", "今日の日付挿入 西暦付(YYYY/MM/DD)",
            "今日の日付挿入 和暦(令和YY年MM月DD日)",
            "現在時刻(HH時MM分SS秒)", "現在時刻(午前hh時MM分SS秒)", "現在時刻(HH:MM:SS)",
            "西暦→和暦変換", "和暦→西暦変換",
            "曜日の挿入(Sunday)","曜日の挿入(SUN)","曜日の挿入(日曜日)","曜日の挿入(日)"
        };

        private YLib ylib = new YLib();

        public InputBox()
        {
            InitializeComponent();

            mWindowWidth = Width;
            mWindowHeight = Height;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (mMainWindow != null) {
                //  親ウィンドウの中心に表示
                Left = mMainWindow.Left + (mMainWindow.Width - Width) / 2;
                Top = mMainWindow.Top + (mMainWindow.Height - Height) / 2;
            }

            if (mFontFamily != "")
                EditText.FontFamily = new System.Windows.Media.FontFamily(mFontFamily);

            EditText.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            EditText.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            if (mTextRapping)
                EditText.TextWrapping = TextWrapping.Wrap;          //  折り返しあり
            else
                EditText.TextWrapping = TextWrapping.NoWrap;        //  折り返し無し

            //  コンテキストメニュー
            if (mReadOnly) {
                tbCalculateMenu.Visibility = Visibility.Collapsed;
                tbAdressMenu.Visibility = Visibility.Collapsed;
            }
            if (!mFileSelectMenu)
                tbFileSelectMenu.Visibility = Visibility.Collapsed;
            if (!mDateMenu)
                tbDateTimeMenu.Visibility = Visibility.Collapsed;
            if (!mCalcMenu) {
                tbCalculateMenu.Visibility = Visibility.Collapsed;
                tbFuncListMenu.Visibility = Visibility.Collapsed;
            }
            if (!mHexCalcMenu) {
                tbHexCalculateMenu.Visibility = Visibility.Collapsed;
                tbDec2HexConvMenu.Visibility = Visibility.Collapsed;
            }
            if (!mConvEscMenu)
                tbAdressMenu.Visibility = Visibility.Collapsed;

            //  編集文字列
            EditText.Text = mEditText;

            //  複数行入力設定
            if (mMultiLine) {
                EditText.AcceptsReturn = true;
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

            if (mCallBackOn) {
                OK.Visibility = Visibility.Hidden;
                Cancel.Content = "終了";
            }

            EditText.Focus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mCallBackOn) {
                mEditText = EditText.Text;
                callback();
            }
            if (!mWindowSizeOutSet)
                WindowFormSave();
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
                Top = Properties.Settings.Default.InputBoxWindowTop;
                Left = Properties.Settings.Default.InputBoxWindowLeft;
                Width = Properties.Settings.Default.InputBoxWindowWidth;
                Height = Properties.Settings.Default.InputBoxWindowHeight;
            }
        }

        /// <summary>
        /// Window状態を保存する
        /// </summary>
        private void WindowFormSave()
        {
            //  Windowの位置とサイズを保存(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.InputBoxWindowTop = Top;
            Properties.Settings.Default.InputBoxWindowLeft = Left;
            Properties.Settings.Default.InputBoxWindowWidth = Width;
            Properties.Settings.Default.InputBoxWindowHeight = Height;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// [OK} ＯＫボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            mEditText = EditText.Text;
            if (!mCallBackOn)
                DialogResult = true;
            Close();
        }

        /// <summary>
        /// [Cancel] キャンセルボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (!mCallBackOn)
                DialogResult = false;
            Close();
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
        /// キー入力操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            keyCommand(e.Key, e.KeyboardDevice.Modifiers == ModifierKeys.Control, e.KeyboardDevice.Modifiers == ModifierKeys.Shift);
        }

        /// <summary>
        /// コンテキストメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            if (menuItem.Name.CompareTo("tbOpenMenu") == 0) {
                textOpen();
            } else if (menuItem.Name.CompareTo("tbCalculateMenu") == 0) {
                textCalculate();
            } else if (menuItem.Name.CompareTo("tbHexCalculateMenu") == 0) {
                textHexCalculate();
            } else if (menuItem.Name.CompareTo("tbDec2HexConvMenu") == 0) {
                textDec2HexCalculate();
            } else if (menuItem.Name.CompareTo("tbFuncListMenu") == 0) {
                funcListMenu();
            } else if (menuItem.Name.CompareTo("tbAdressMenu") == 0) {
                cnvEscapeString();
            } else if (menuItem.Name.CompareTo("tbFileSelectMenu") == 0) {
                fileSelect();
            } else if (menuItem.Name.CompareTo("tbDateTimeMenu") == 0) {
                textDateTime();
            }
        }

        /// <summary>
        /// モードレスダイヤログでのデータ更新
        /// </summary>
        public void updateData()
        {
            mEditText = EditText.Text;
        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        /// <param name="key">キーコード</param>
        /// <param name="control">Ctrlキーの有無</param>
        /// <param name="shift">Shiftキーの有無</param>
        private void keyCommand(Key key, bool control, bool shift)
        {
            if (control) {
                switch (key) {
                    default:
                        break;
                }
            } else if (shift) {
                switch (key) {
                    default: break;
                }
            } else {
                switch (key) {
                    case Key.Escape: break;                                 //  ESCキーでキャンセル
                    case Key.F6: if (mFileSelectMenu) fileSelect(); break;  //  ファイル選択
                    case Key.F7: if (mConvEscMenu) cnvEscapeString(); break;//  ESCシーケンス文字列を解除す
                    case Key.F8: if (mDateMenu) textDateTime(); break;      //  日付挿入変換
                    case Key.F9: if (mCalcMenu) textCalculate(); break;     //  選択テキストを計算
                    case Key.F11: if (mCalcMenu) textHexCalculate(); break; //  選択テキストを計算
                    case Key.F12: textOpen(); break;                        //  選択テキストで開く
                    case Key.Back: break;                                   //  ロケイト点を一つ戻す
                    default: break;
                }
            }
        }

        /// <summary>
        /// 選択文字列を計算する
        /// </summary>
        private void textCalculate()
        {
            YCalc calc = new YCalc();
            string text = EditText.SelectedText;
            //  数式文字以外を除く
            string express = ylib.stripControlCode(text);
            express = calc.stripExpressData(express);
            //  計算結果を挿入
            int pos = EditText.SelectionStart + EditText.SelectionLength;
            EditText.Select(pos, 0);
            EditText.SelectedText = " = " + calc.expression(express).ToString();
        }

        /// <summary>
        /// 選択文字列を16進で計算
        /// </summary>
        private void textHexCalculate()
        {
            YCalc calc = new YCalc();
            string text = EditText.SelectedText;
            //  数式文字以外を除く
            string express = ylib.stripControlCode(text);
            express = calc.convHexExpressData(express);
            //  計算結果を挿入
            int pos = EditText.SelectionStart + EditText.SelectionLength;
            EditText.Select(pos, 0);
            EditText.SelectedText = " => " + express;
            double result = calc.expression(express);
            EditText.SelectedText += $" = {result.ToString()}(0x{((long)result).ToString("X")})";
        }

        /// <summary>
        /// 10進数を16進に変換
        /// </summary>
        private void textDec2HexCalculate()
        {
            YCalc calc = new YCalc();
            string text = EditText.SelectedText;
            //  数式文字以外を除く
            string express = ylib.stripControlCode(text);
            express = calc.dec2hexExpressData(express);
            //  計算結果を挿入
            int pos = EditText.SelectionStart + EditText.SelectionLength;
            EditText.Select(pos, 0);
            EditText.SelectedText = " => " + express;
            //double result = calc.expression(express);
            //EditText.SelectedText += $" = {result.ToString()}(0x{((long)result).ToString("X")})";

        }

        /// <summary>
        /// 関数一覧表示・挿入
        /// </summary>
        private void funcListMenu()
        {
            MenuDialog dlg = new MenuDialog();
            dlg.mMainWindow = this;
            dlg.Title = "計算式関数メニュー";
            dlg.mMenuList = YCalc.mFuncList.ToList();
            dlg.mOneClick = true;
            dlg.ShowDialog();
            if (dlg.mResultMenu != null) {
                string text = EditText.SelectedText;
                EditText.SelectedText = dlg.mResultMenu.Substring(0, dlg.mResultMenu.IndexOf(' '));
            }
        }

        /// <summary>
        /// ファイルの選択
        /// </summary>
        private void fileSelect()
        {
            string path = ylib.fileSelect(".", "txt,exe,csv,html");
            if (path != null) {
                EditText.Select(EditText.SelectionStart, 0);
                EditText.SelectedText = path;
            }
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
        /// 選択文字列のURIエスケープシーケンスを解除する
        /// </summary>
        private void cnvEscapeString()
        {
            string text = EditText.SelectedText;
            EditText.SelectedText = Uri.UnescapeDataString(text);
        }

        /// <summary>
        /// 日時挿入・変換
        /// </summary>
        private void textDateTime()
        {
            string text = EditText.SelectedText;
            EditText.SelectedText = textDateTime(text);
        }

        /// <summary>
        /// 日時挿入・変換のメニューダイヤログを表示し、挿入・変換を行う
        /// </summary>
        /// <param name="text">選択文字列</param>
        /// <returns>変換文字列</returns>
        private string textDateTime(string text)
        {
            MenuDialog dlg = new MenuDialog();
            dlg.mMainWindow = this;
            dlg.Title = "日時挿入・変換メニュー";
            dlg.mMenuList = mDateTimeMenu.ToList();
            dlg.mOneClick = true;
            dlg.ShowDialog();
            int index = mDateTimeMenu.FindIndex(dlg.mResultMenu);
            DateTime now = DateTime.Now;
            switch (index) {
                case 0: text = now.ToString("yyyy年M月d日"); break;
                case 1: text = now.ToString("\'yy年M月d日"); break;
                case 2: text = now.ToString("yyyy/MM/dd"); break;
                case 3: text = ylib.toWareki(); break;
                case 4: text = now.ToString("HH時mm分ss秒"); break;
                case 5: text = now.ToString("tth時m分s秒"); break;
                case 6: text = now.ToString("T"); break;
                case 7: text = convDateFormat(text); break;
                case 8: text = convDateFormat(text, false); break;
                case 9: text = convDate2Week(text, 0); break;
                case 10: text = convDate2Week(text, 1); break;
                case 11: text = convDate2Week(text, 2); break;
                case 12: text = convDate2Week(text, 3); break;
            }

            return text;
        }

        /// <summary>
        /// 西暦の日付を和暦に変換、和暦の日付を西暦に変換
        /// </summary>
        /// <param name="text">日付文字列</param>
        /// <param name="wareki">和暦/西暦</param>
        /// <returns>変換日付</returns>
        private string convDateFormat(string text, bool wareki = true)
        {
            (int index, string dateStr) = ylib.getDateMatch(text);
            if (0 < dateStr.Length) {
                string date = ylib.cnvDateFormat(dateStr);
                if (0 < date.Length) {
                    DateTime dt = DateTime.Parse(date);
                    if (wareki) {
                        text = text.Replace(dateStr, ylib.toWareki(dt.ToString("yyyy/MM/dd")));
                    } else {
                        text = text.Replace(dateStr, dt.ToString("yyyy年M月d日"));
                    }
                }
            }
            return text;
        }

        /// <summary>
        /// 選択した日付に曜日を追加
        /// 曜日のタイプ  0:Sunday 1:SUN 2:日曜日 3:日
        /// </summary>
        /// <param name="text">日付文字列</param>
        /// <param name="type">曜日のタイプ</param>
        /// <returns>曜日付き日付</returns>
        private string convDate2Week(string text, int type)
        {
            (int index, string dateStr) = ylib.getDateMatch(text);
            if (0 < dateStr.Length) {
                string date = ylib.cnvDateFormat(dateStr);
                if (0 < date.Length) {
                    text += " " + ylib.cnvDateWeekday(type, date);
                }
            }
            return text;

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

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfLib
{

    /// <summary>
    /// Pointの整数版(整数のPoint構造体)
    /// System.Drawing.Point(Int32)の代用
    /// System.WindowsのPointはDoubleのため、整数型が使えない
    /// </summary>
    public class PointI
    {
        public int X { get; set; }
        public int Y { get; set; }

        public PointI(int x, int y)
        {
            X = x;
            Y = y;
        }

        public PointI(Point dp)
        {
            X = (int)dp.X;
            Y = (int)dp.Y;
        }

        public void Add(PointI point)
        {
            X += point.X;
            Y += point.Y;
        }

        public bool Equals(PointI point)
        {
            return (X == point.X && Y == point.Y);
        }

        public void Offset(int x, int y)
        {
            X += x;
            Y += y;
        }

        public bool Parse(string str)
        {
            char[] separator = { ',', ' ' };
            string[] point = str.Split(separator);
            if (2 <= point.Length) {
                int tx, ty;
                if (int.TryParse(point[0], out tx) && int.TryParse(point[1], out ty)) {
                    X = tx;
                    Y = ty;
                    return true;
                }
            }
            return false;
        }

        public void Subtruct(PointI point)
        {
            X -= point.X;
            Y -= point.Y;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]", X,Y);
        }
    }

    /// <summary>
    /// Sizeの整数版
    /// System.Drawing.Size(Int32)の代用
    /// System.EindowsにはSizeがないため
    /// </summary>
    public class SizeI
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public SizeI(int width, int height)
        {
            Width = Math.Abs(width);
            Height = Math.Abs(height);
        }

        public SizeI(PointI point)
        {
            Width = Math.Abs(point.X);
            Height = Math.Abs(point.Y);
        }

        public bool Equals(SizeI size)
        {
            return (Width == size.Width && Height == size.Height);
        }

        public bool Parse(string str)
        {
            char[] separator = { ',', ' ' };
            string[] point = str.Split(separator);
            if (2 <= point.Length) {
                int tw, th;
                if (int.TryParse(point[0], out tw) && int.TryParse(point[1], out th)) {
                    Width  = Math.Abs(tw);
                    Height = Math.Abs(th);
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]", Width, Height);
        }
    }

    /// <summary>
    /// System.Windows.Formsが見つからない時は参照の追加でアセンブリの中から選択する
    /// 
    /// システム関連
    /// bool getError()                 ERRORの発生を取得
    /// string getErrorMessage()        ERROR Messageの取得
    /// void DoEvents()                 コントロールを明示的に更新
    /// int getArgb2Uint(Byte a, Byte r, Byte g, Byte b)    ARGBからint値を作成(カラー値の変換)
    /// Brush getInt2Color(int color)                       4桁のHex数値からカラー値(Brush)を作成
    /// void Swap<T>(ref T lhs, ref T rhs)  ジェネリックによるswap関数
    /// 
    /// ネットワーク関連
    /// void WebSerach(string url, string searchWord)       Web検索Google,Bing..)のWebページを開く
    /// bool webFileDownload(string url, string filePath)   Web上のファイルをダウンロードする
    /// string getWebText(string url)                       URLのWebデータの読込
    /// string getWebText(string url, int encordType = 0)   エンコードタイプを指定してURLのWebデータの読込
    /// 
    /// HTML関連
    /// List<string> getPattern(string html, string pattern, string group)  正規表現を使ったHTMLデータからパターン抽出
    /// List<string[]> getPattern(string html, string pattern)  正規表現を使ったHTMLからのパターン抽出
    /// List<string[]> getPattern2(string html, string pattern) 正規表現を使ったHTMLからのパターン抽出
    /// List<string> getHtmlTagList(string html)            HTMLソースからTAGデータごとに分解したリストを作る
    /// int getHtmlTagType(string tagData)                  タグデータの種類を判別
    /// string getHtmlTagName(string tagData)                '<','>'で囲まれたタグデータからタグ名を抽出
    /// List<string> getHtmlTagDataAll(string html, int pos = 0)    HTMLソースからデータのみを抽出
    /// (string, string, string) getHtmlTagData(string html, string tag)    TAGデータの抽出(入れ子対応)
    /// (string, string, string, int, int) getHtmlTagData(string html, string tag, int pos) TAGデータの抽出(入れ子対応)
    /// string stripHtmlTagData(string html, string tag)    HTMLソースからタグで囲まれた領域を除く
    /// string getHtmlTagPara(string tagData, string paraName)   '<','>'で囲まれたタグデータからパラメータを抽出
    /// string getHtmlTagParaDataTitle(string tagData, string paraTitle, string paraData = "")  パラメータのタイトルでタグ全体を取得する
    /// string stripHtmlParaData(string para, string paraTitle) HTMLのタグパラメータからデータだけを取り出す
    /// int findHtmlParaDataTagPos(string para , string paraTitle)  HTMLのデータからタグパラメータの存在するタグの開始位置を取得する
    /// (int tagType, int start, int end) findHtmlTag(string html, string tagName, int pos = 0) タグを検索して種別と位置を求める
    /// 
    /// データ処理関係
    /// List<string[]> splitJson(string jsonData, string baseTitle = "")    JSON形式の文字列から[名前:値]の対データをリストデータとして取得
    /// string getJsonDataString(string jsonData)   JSON形式の文字列から{}内の文字列を取得
    /// 
    /// 文字列処理
    /// string stripBrackets(string text, char sb = '[', char eb = ']') 文字列から括弧で囲まれた領域を取り除く
    /// string trimControllCode(string buf)         文字列内のコントロールコードを除去
    /// bool IsNumberString(string num)             数値文字列かを判定
    /// bool boolParse(string str, bool val = true) 文字列を論理値に変換
    /// int intParse(string str, int val = 0)       文字列を整数に変換
    /// double doubleParse(string str, double val = 0.0)    文字列を実数に変換
    /// double string2Double(string str)            数値文字列を数値に変換
    /// double string2double(string num)            文字列の先頭が数値の場合、数値に変換
    /// string string2StringNum(string num)         文字列から数値に関係ない文字を除去し、実数に変換できる文字列にする
    /// string strZne2Han(string zenStr)            文字列内の全角英数字を半角に変換
    /// string strNumZne2Han(string zenStr)         文字列内の全角数値を半角に変換
    /// String strControlCodeCnv(String str)        文字列の中の改行コード、','、'"'を'\'付きコードに置き換える
    /// String strControlCodeRev(String str)        文字列の中の'\'付きコードを通常のコードに戻す
    /// String[] seperateString(String str)         文字列をカンマセパレータで分解して配列に格納
    /// int getWithoutCharIndex(string str, string withoutChar) 文字列から指定文字以外の文字位置を検索
    /// void setEncording(int n)                    ファイルのRead/Writeで文字コードを指定
    /// string array2csvString(string[] data)       文字配列をダブルクォーテーションで括ってカンマセパレーツで一行の文字列に変換
    /// String setDigitSeparator(String val)        文字列が数値の場合、桁区切り(3桁)を入れる
    /// string cnvHtmlSpecialCode(string html)      HTMLで使われいる{&#??;]のコードを通常の文字に置換
    /// 
    /// ストップウォッチ
    /// void stopWatchStartNew()                    ストップウォッチ機能初期化・計測開始
    /// TimeSpan stopWatchLapTime()                 ストップウォッチ機能ラップ時間取得
    /// TimeSpan stopWatchRestart()                 ストップウォッチ機能計測時間の取得と再スタート
    /// TimeSpan stopWatchStop()                    ストップウォッチ機能計測時間の取得と終了
    /// TimeSpan stopWatchTotalTime()               ストップウォッチ機能累積時間の取得
    /// 
    /// 日付・時間処理
    /// string second2String(double seconds)        秒数を時分秒の文字列に変換
    /// bool IsTime(string time)                    文字列の時刻タイプの判定
    /// int time2Seconds(string time)               文字列時刻を秒数に変換
    /// bool IsWeekday(string weekday)              文字列が曜日かの判定
    /// int WeekNo(string weekday)                  文字列から曜日の番号を返す
    /// string getWeekday(int weekNo)               曜日の位置から曜日文字列を返す
    /// string cnvDateFormat(string date)           各種日付をyyyy/mm/ddに変換
    /// bool IsDateString(string date)              文字列が日付かを判定
    /// bool IsDateYearString(string date)          文字列が年を表しているかを判
    /// bool IsDateMonthString(string date)         文字列が年月を表しているか判定
    /// bool IsDateWeekString(string date)          文字列が週単位の日付を表しているか判定
    /// int date2JulianDay(string date)             歴日からユリウス日に変換
    /// int date2JulianDay(int year, int month, int day)    日からユリウス日に変換
    /// string JulianDay2DateYear(int jd, bool sp)  ユリウス日から歴日文字列
    /// string JulianDay2DateYear(int jd, int type) 
    /// int subYear(string startDate, string endDate)   年差を求める
    /// int subYear(int jd1, int jd2)               ユリウス日から年数差を求める(jd2 < jd1 で　正数)
    /// int JulianDay2Year(int jd)                  ユリウス日から年を抽出
    /// int JulianDay2Month(int jd)                 ユリウス日から月を抽出
    /// int JulianDay2Day(int jd)                   ユリウス日から日を抽出
    /// string JulianDay2DateYear(int jd, int type) ユリウス日から歴日文字列に変換
    /// int wareki2JulianDay(string wareki)         和暦をユリウス日に変換
    /// double reiwa2Date(double reiwa)             令和年を西暦年に変換(yy.mmdd → yyyy.mmdd)
    /// double heisei2Date(double heisei)           成年を西暦年に変換(yy.mmdd → yyyy.mmdd)
    /// double shyouwa2Date(double shyouwa)         昭和年を西暦年に変換(yy.mmdd → yyyy.mmdd)
    /// double taisyou2Date(double taisyou)         大正年を西暦年に変換(yy.mmdd → yyyy.mmdd)
    /// double meiji2Date(double meiji)             明治年を西暦年に変換(yy.mmdd → yyyy.mmdd)
    /// 
    /// ファイル・ディレクトリ処理
    /// string folderSelect(string initFolder)      フォルダの選択ダイヤログの表示
    /// string filesSelect(string searchFolder, string ext)     ファイル選択ダイヤログ(複数の拡張子を指定できる)
    /// string fileSelect(string searchFolder, string ext)      ファイル選択ダイヤログ
    /// string saveFileSelect(string searchFolder, string ext)  ファイル選択保存ダイヤログ
    /// string[] getFiles(string path)              指定されたパスからファイルリストを作成
    /// string getAppFolderPath()                   実行ファイルのフォルダを取得
    /// String getLastFolder(String folder)         フルパスのディレクトリから最後のフォルダ名を取り出す
    /// String getLastFolder(String folder, int n)  フルパスのディレクトリから最後のフォルダ名を取り出す(位置指定)
    /// string getWorkFilePath(string fileName)     ワークファイル用のパス名を取得
    /// string getWorkFilePath(string fileName, bool systemfile)            ワークファイル用のパス名を取得
    /// void saveCsvData(string path, string[] format, List<string[]> data) タイトルをつけてCSV形式でListデータをファイルに保存
    /// List<String[]> loadCsvData(string filePath, string[] title) CSV形式のファイルを読み込みList<String[]>形式で出力
    /// void saveCsvData(string path, List<String[]> csvData)       データをCSV形式でファイルに書き込む
    /// List<String[]> loadCsvData(string filePath)                 CSV形式のファイルを読み込む
    /// void saveListData(string path, List<String> listData)       Listデータを行単位でファイルに保存
    /// List<String> loadListData(string filePath)                  ファイルデータを行単位でListデータとして取り込む
    /// void saveTextFile(string path, string buffer)   テキストファイルの保存
    /// string loadTextFile(string path)                テキストファイルの読込
    /// byte[] loadBinData(string path)                 バイナリファイルの読込
    /// void saveBinData(string path, byte[] buffer)    バイナリデータをファイルに書き込む
    /// bool gzipDecompress(string ipath, string opath) gzipファイルを解凍
    /// 
    /// 数値処理
    /// double movingAverage(List<double> data, int pos, int nearCount, bool center)    移動平均を求める
    /// double[] getMovingAverage(double[] data, int nearCount, bool center)    移動平均を行ったデータ配列を求める
    /// double graphStepSize(double range, double targetSteps, int fromBase = 10)   グラフ作成時の補助線間隔を求める
    /// double graphHeightSize(double height, double stepSize)  グラフの最大値を求める
    /// double log(double a, double b)          底指定の対数(log b a)
    /// float roundFloor(float val, int n)      指定の有効桁で切り捨てる
    /// float roundCeil(float val, int n)       指定の有効桁で切り上げる
    /// float roundRound(float val, int n)      指定の有効桁数で四捨五入する
    /// float roundRound2(float val, int n)
    /// Point cnvCoordinate(string coordinate)  度分秒表示の座標を度に変換する
    /// double coordinateDistance(double longi1, double lati1, double longi2, double lati2) 球面上の2点間座標の距離
    /// double coordinateDistance(Point ps, Point pe)   球面上の2点間座標の距離
    /// double azimuth(double longi1, double lati1, double longi2, double lati2)    球面上の2点間座標の方位
    /// double azimuth2(double longi1, double lati1, double longi2, double lati2)   球面上の2点間座標の方位
    /// double posDis(Point ps, Point pe)       2点間の距離
    /// int Lcm(int a, int b)                   最小公倍数
    /// int Gcd(int a, int b)                   最大公約数
    /// 
    /// バイナリ処理
    /// void binaryDump(byte[] data, int start, int size, string comment)   バイナリデータをコンソール出力
    /// string binary2HexString(byte[] data, int start, int size)   byte配列を16進の文字列に変換
    /// long bit7ConvertLong(byte[] data, int start)                下位7bitのSynchsafe整数をlongに変換
    /// long bit7ConvertLong(byte[] data, int start, int size)      下位7bitのSynchsafe整数をlongに変換
    /// long bitConvertLong(byte[] data, int start)                 byte配列をlongに変換
    /// long bitConvertLong(byte[] data, int start, int size)       byte配列をlongに変換
    /// long bitReverseConvertLong(byte[] data, int start, int size)    byte配列を逆順でlongに変換
    /// long bitConvertBit(byte[] data, int startBit, int bitSize)  byte配列からbit単位で数値を取り出す
    /// uint bitOn(uint a, int n)               nビット目の値を1にする
    /// uint bitOff(uint a, int n)              nビット目の値を0にする
    /// uint bitRevers(uint a, int n)           nビット目を反転
    /// int bitGet(uint a, int n)               nビット目を反転
    /// int bitsCount(long bits)                bitの数を数える(32bitまで)
    /// int bitsCount2(long bits)               bitの数を数える(64bitまで)
    /// Byte getInt2Byte(int value, int n)      int データからbyte単位で値を取得
    /// bool BinComp(byte[] a, byte[] b)        バイナリデータを比較
    /// bool BinComp(byte[] a, int astart, byte[] b, int bstart, int size)  バイナリデータを比較
    /// ByteCopy(byte[] a, int start, int size) byteデータをコピー
    /// 
    /// 
    /// 
    /// </summary>
    public class YLib
    {
        System.Diagnostics.Stopwatch mSw;       //  ストップウォッチクラス
        private TimeSpan mStopWatchTotalTime;   //  mSwの経過時間
        private Encoding[] mEncoding = {Encoding.UTF8, Encoding.GetEncoding("shift_jis"), Encoding.GetEncoding("euc-jp") };
        private int mEncordingType = 0;
        private double[] mKaigen = { 2019.0501, 1989.0108, 1926.1225, 1912.0730, 1868.0908, 1865.0407, 1864, 1861, 1860, 1854, 1848, 1844, 1830, 1818 };
        private string[] mGengou = { "令和", "平成", "昭和", "大正", "明治", "慶応", "元治", "文久", "万延", "安政", "嘉永", "弘化", "天保", "文政" };
        private string[] mMonth = { 
            "January","February","March","April","May","June","July","August","September","October","Novmber","December",
            "Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec" };

        private bool mError = false;
        private string mErrorMessage;

        public YLib()
        {

        }

        //  ---  システム関連  ---

        /// <summary>
        /// ERRORの発生を取得
        /// 取得後はERRORを解除
        /// </summary>
        /// <returns></returns>
        public bool getError()
        {
            bool error = mError;
            mError = false;
            return error;
        }

        /// <summary>
        /// ERROR Messageの取得、
        /// </summary>
        /// <returns></returns>
        public string getErrorMessage()
        {
            return mErrorMessage;
        }


        /// <summary>
        /// コントロールを明示的に更新するコード
        /// 参考: https://www.ipentec.com/document/csharp-wpf-implement-application-doevents
        /// </summary>
        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            var callback = new DispatcherOperationCallback(ExitFrames);
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, callback, frame);
            Dispatcher.PushFrame(frame);
        }

        /// <summary>
        /// DoEventsからのコールバック
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private object ExitFrames(object arg)
        {
            ((DispatcherFrame)arg).Continue = false;
            return null;
            //throw new NotImplementedException();
        }

        /// <summary>
        /// ARGBからint値を作成(カラー値の変換)
        /// </summary>
        /// <param name="a">α値</param>
        /// <param name="r">R値</param>
        /// <param name="g">G値</param>
        /// <param name="b">B値</param>
        /// <returns></returns>
        public int getArgb2Uint(Byte a, Byte r, Byte g, Byte b)
        {
            return (int)(a << (3 * 8) | r << (2 * 8) | g << 8 | b);
        }

        /// <summary>
        /// 4桁のHex数値からカラー値(Brush)を作成する
        /// ARGBの順に指定 
        ///     灰色     A= 0x1e, R=0x7d, G=0x7d, B=0x7d → 0x1e7d7d7d
        ///     シアン   A= 0x1e, R=0x00, G=0xff, B=0xff → 0x1e00ffff
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public Brush getInt2Color(int color)
        {
            return new SolidColorBrush(Color.FromArgb(
                getInt2Byte(color, 3), getInt2Byte(color, 2),
                getInt2Byte(color, 1), getInt2Byte(color, 0)));
        }

        /// <summary>
        /// ジェネリックによるswap関数
        /// 例: Swap<int>(ref a, ref b); Swap(ref a, ref b);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }


        //  ---  ネットワーク関連  ---

        /// <summary>
        /// Web検索Google,Bing..)のWebページを開く
        /// url= googleJpn,google,Bing,Discogs,Wikipedia,WikipediaJpn 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="searchWord"></param>
        public void WebSerach(string url, string searchWord)
        {
            searchWord = searchWord.Replace("\"", "");
            searchWord = searchWord.Replace("&", "");
            if (url.CompareTo("googleJpn") == 0) {
                searchWord = "http://www.google.co.jp/search?hl=ja&source=hp&q=" + searchWord;
            } else if (url.CompareTo("google") == 0) {
                searchWord = "http://www.google.com/search?source=hp&q=" + searchWord;
            } else if (url.CompareTo("Bing") == 0) {
                searchWord = "http://www.bing.com/search?q=" + searchWord;
            } else if (url.CompareTo("Discogs") == 0) {
                searchWord = searchWord.Replace(" ", "+");
                searchWord = "https://www.discogs.com/ja/search/?q=" + searchWord + "&type=all";
            } else if (url.CompareTo("Wikipedia") == 0) {
                searchWord = searchWord.Replace(" ", "_");
                searchWord = "https://en.wikipedia.org/wiki/" + searchWord;
            } else if (url.CompareTo("WikipediaJpn") == 0) {
                searchWord = searchWord.Replace(" ", "_");
                searchWord = "https://ja.wikipedia.org/wiki/" + searchWord;
            } else {
                return;
            }
            if (0 < searchWord.Length) {
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(searchWord);
            }
        }

        /// <summary>
        /// Web上のファイルをダウンロードする
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="filePath">ファイルパス</param>
        /// <returns></returns>
        public bool webFileDownload(string url, string filePath)
        {
           WebClient wc = null;
            try {
                wc = new WebClient();
                //  Microsoft Edgeの要求ヘッダーを模擬(403エラー対応)
                wc.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                wc.Headers.Add("user-agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Mobile Safari/537.36 Edg/92.0.902.67");    //  403対応
                //  データのダウンロード
                wc.DownloadFile(url, filePath);
                wc.Dispose();
            } catch (Exception e) {
                wc.Dispose();
                mError = true;
                mErrorMessage = e.Message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// URLのWebデータの読込
        /// EndodingのデフォルトはUTF8(mEncordingType == 0)
        /// </summary>
        /// <param name="url">URL</param>
        public string getWebText(string url)
        {
            return getWebText(url, mEncordingType);
        }

        /// <summary>
        /// エンコードタイプを指定してURLのWebデータの読込
        /// https://vdlz.xyz/Csharp/Porpose/WebTool/parse/parse.html
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="encordType">エンコードタイプ(default=URF8)</param>
        /// <returns></returns>
        public string getWebText(string url, int encordType = 0)
        {
            Stream st;
            StreamReader sr;
            string text;
            try {
                WebClient wc = new WebClient();
                //  Microsoft Edgeの要求ヘッダーを模擬(403エラー対応)
                wc.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                wc.Headers.Add("user-agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Mobile Safari/537.36 Edg/92.0.902.67");    //  403対応
                //  データのダウンロード
                st = wc.OpenRead(url);
                sr = new StreamReader(st, mEncoding[encordType]);
                text = sr.ReadToEnd();

            } catch (Exception e) {
                mError = true;
                mErrorMessage = e.Message;
                return null;
            }
            sr.Close();
            st.Close();

            return text;
        }

        //  --- HTML関係  ------

        /// <summary>
        /// 正規表現を使ったHTMLデータからパターン抽出
        /// 例: pattern = "<title>(?<title>.*?)</title>"
        ///     group = "title"
        /// </summary>
        /// <param name="html">HTMLソースデータ</param>
        /// <param name="pattern">抽出パターン</param>
        /// <param name="group">抽出グループ</param>
        /// <returns>抽出データリスト</returns>
        public List<string> getPattern(string html, string pattern, string group)
        {
            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = reg.Match(html);
            List<string> listData = new List<string>();
            while (m.Success) {
                listData.Add(m.Groups[group].Value);
                m = m.NextMatch();
            }
            return listData;
        }

        //  HTMLパターン抽出のオプション
        public RegexOptions mRegexOption = RegexOptions.IgnoreCase; //  | Singleline/Multiline/RightToLeft

        /// <summary>
        /// 正規表現を使ったHTMLからのパターン抽出
        /// 例:  pattern = "<a href=\"(.*?)\".*?title=\"(.*?)\">(.*?)</a>(.*?)</li>"
        /// </summary>
        /// <param name="html">HTMLソースデータ</param>
        /// <param name="pattern">抽出パターン</param>
        /// <returns>抽出データリスト</returns>
        public List<string[]> getPattern(string html, string pattern)
        {
            Regex reg = new Regex(pattern, mRegexOption);
            Match m = reg.Match(html);
            List<string[]> listData = new List<string[]>();
            while (m.Success) {
                System.Diagnostics.Debug.WriteLine($"{html.Length} {m.Index} {m.Length}");
                string[] data = new string[m.Groups.Count];
                for (int i = 0; i < m.Groups.Count; i++) {
                    data[i] = m.Groups[i].ToString();
                }
                listData.Add(data);
                m = m.NextMatch();
            }
            return listData;
        }

        /// <summary>
        /// 正規表現を使ったHTMLからのパターン抽出
        /// getPattern()と同じだか抽出方法にMatches()(一括検索)を使用
        /// 例:  pattern = "<a href=\"(.*?)\".*?title=\"(.*?)\">(.*?)</a>(.*?)</li>"
        /// </summary>
        /// <param name="html">HTMLソースデータ</param>
        /// <param name="pattern">抽出パターン</param>
        /// <returns>抽出データリスト</returns>
        public List<string[]> getPattern2(string html, string pattern)
        {
            Regex reg = new Regex(pattern, mRegexOption);
            MatchCollection mc = reg.Matches(html);
            List<string[]> listData = new List<string[]>();
            foreach (Match m in mc) {
                string[] data = new string[m.Groups.Count];
                for (int i = 0; i < m.Groups.Count; i++) {
                    data[i] = m.Groups[i].ToString();
                }
                listData.Add(data);
            }
            return listData;
        }


        /// <summary>
        /// HTMLソースからTAGデータごとに分解したリストを作る
        /// <tag para....>data</tag>
        /// 1.<tag para....>
        /// 2.data
        /// 3.</tag>tag>
        /// </summary>
        /// <param name="html">HTMLソース</param>
        /// <returns>TAGリスト</returns>
        public List<string> getHtmlTagList(string html)
        {
            List<string> tagList = new List<string>();
            char[] trimChar = { '\n', '\a', '\r', ' ', '\t' };
            int pos = 0;
            string data = "";
            int ep = html.IndexOf('>');
            int sp = html.IndexOf('<');
            if ((0 <= ep && 0 <= sp && ep < sp) || (0 <= ep && sp < 0)) {
                data = html.Substring(0, ep + 1).Trim(trimChar);
                if (0 < data.Length)
                    tagList.Add(data);
                pos = ep + 1;
            }
            while (pos < html.Length) {
                int st = html.IndexOf('<', pos);
                if (pos < st) {
                    data = html.Substring(pos, st - pos);
                    pos = st;
                } else if (pos == st) {
                    int ct = html.IndexOf('>', st);
                    if (0 <= ct) {
                        data = html.Substring(st, ct - st + 1);
                        pos = ct + 1;
                    } else {
                        data = html.Substring(st);
                        pos = html.Length;
                    }
                } else {
                    data = html.Substring(pos);
                    pos = html.Length;
                }
                string tag = data.Trim(trimChar);
                if (0 < tag.Length)
                    tagList.Add(tag);
            }
            return tagList;
        }

        /// <summary>
        ///  タグデータの種類を判別する
        ///  1: <TAG ..../>
        ///  2: <TAG ....>
        ///  3: </TAG>
        ///  4: <!....>
        ///  5: <!-- ....
        ///  6: ...-->
        ///  7: ...DATA...
        ///  0: ?
        /// </summary>
        /// <param name="tagData">タグデータ</param>
        /// <returns>タグの種類</returns>
        public int getHtmlTagType(string tagData)
        {
            int st = tagData.IndexOf('<');
            int et = tagData.IndexOf('>');
            if (st < 0 && et < 0) {
                return 7;                       //  データ(タグがない)
            } else if (st < 0 && 0 <= et) {
                if (0 <= tagData.IndexOf("-->"))
                    return 6;                   //  コメント終端(-->)
                else
                    return 0;                   //  不明(...>)
            } else if (0 <= st && et < 0) {
                if (0 <= tagData.IndexOf("<!--"))
                    return 5;                   //  コメント開始(<!--)
                else
                    return 0;                   //  不明(<...)
            } else if (0 <= st && 0 <= et) {
                if (0 <= tagData.IndexOf("/>"))
                    return 1;                   //  タグ(</TAG .../>)
                if (0 <= tagData.IndexOf("</"))
                    return 3;                   //  終端タグ(</TAG>
                if (0 <= tagData.IndexOf("<!"))
                    return 4;                   //  コメント(<!...>)
                return 2;                       //  開始タグ(<TAG ...>)
            } else
                return 0;                       //  不明
        }

        /// <summary>
        /// '<','>'で囲まれたタグデータからタグ名を抽出する
        /// </summary>
        /// <param name="tagData">タグデータ</param>
        /// <returns>タグ名</returns>
        public string getHtmlTagName(string tagData)
        {
            int st = tagData.IndexOf('<');
            if (st < 0)
                return "";                          //  タグがない
            int et = tagData.IndexOf(' ', st);
            if (et < 0)
                et = tagData.IndexOf("/>", st);
            if (et < 0)
                et = tagData.IndexOf(">", st);
            if (et < 0)
                return tagData.Substring(st + 1);   //  終端がない時のタグ名
            else
                return tagData.Substring(st + 1, et - st - 1);  //  タグ名
        }

        /// <summary>
        /// HTMLソースからデータのみを抽出
        /// </summary>
        /// <param name="html">HTMLソース</param>
        /// <param name="pos">抽出データList</param>
        /// <returns>タグリスト</returns>
        public List<string> getHtmlTagDataAll(string html, int pos = 0)
        {
            List<string> tagDataList = getHtmlTagList(html.Substring(pos));
            List<string> dataList = new List<string>();
            foreach (string data in tagDataList) {
                if (data.IndexOf('<') < 0 && data.IndexOf('>') < 0)
                    dataList.Add(cnvHtmlSpecialCode(data));
            }
            return dataList;
        }

        /// <summary>
        /// HTMLソースから最初のデータ部を抽出(TAGは含まない)
        /// </summary>
        /// <param name="html">HTMLソース</param>
        /// <returns>データ文字列</returns>
        public string getHtmlTagData(string html, int pos = 0)
        {
            if (pos < 0)
                return "";
            int sp = html.IndexOf('>', pos);
            if (0 <= sp) {
                int ep = html.IndexOf('<', sp);
                if (0 <= ep)
                    return html.Substring(sp + 1, ep - sp - 1);
                else {
                    ep = html.IndexOf('<');
                    if (0 < ep)
                        return html.Substring(0, ep - 1);
                    else
                        return html.Substring(sp + 1);
                }
            } else {
                int ep = html.IndexOf('<');
                if (0 < ep)
                    return html.Substring(0, ep - 1);
                else
                    return html;
            }
        }

        /// <summary>
        /// TAGデータの抽出(入れ子対応)
        /// <TAG ...>data...</TAG>のdata部分を抽出、dataの中にタグが入れ子構造にも対応
        /// </summary>
        /// <param name="html">HTMLソース</param>
        /// <param name="tag">タグ名</param>
        /// <returns>(タグパラメータ, 抽出データ, 残りHTML)</returns>
        public (string, string, string) getHtmlTagData(string html, string tag)
        {
            (string tagPara, string tagData, string nextSrc, int start, int end) = getHtmlTagData(html, tag, 0);
            return (tagPara, tagData, nextSrc);
        }

        /// <summary>
        /// TAGデータの抽出(入れ子対応)
        /// </summary>
        /// <param name="html">HTMLソース</param>
        /// <param name="tag">対象タグ名</param>
        /// <param name="pos">検索開始位置</param>
        /// <returns>(タグパラメータ, 抽出データ, 残りHTML, タグ開始位置, タグ終了位置)</returns>
        public (string, string, string, int, int) getHtmlTagData(string html, string tag, int pos)
        {
            int startPos = pos;                 //  TAGの開始位置(TAGを含む)
            int endPos = pos;                   //  TAGの終了位置(TAGを含む)
            int count = 0;                      //  ネストの数
            int st, tagType, start, end;        //  st, et, 検索したタグの種別,開始位置,終了位置
            string tagPara = "";                //  タグのパラメータ(初回検索タグ)
            st = pos;                           //  タグデータの開始位置
            do {
                (tagType, start, end) = findHtmlTag(html, tag, pos);
                switch (tagType) {
                    case 1: break;                 //  完結タグ
                    case 2: count++; break;        //  開始タグ
                    case 3: count--; break;        //  終了タグ
                    case 0: return ("", "", html, startPos, endPos); //  不明タグでreturn
                }
                if (st == pos) {
                    //  初回設定
                    startPos = start;
                    st = end + 1;
                    if (html[start + tag.Length + 1] == ' ') {
                        tagPara = html.Substring(start + tag.Length + 2,
                                end - (tagType == 1 ? 2 : 1) - (start + tag.Length + 1));
                    }
                }
                pos = end;
            } while (0 < count);
            start = start <= st ? end + 1 : start;

            return (tagPara, html.Substring(st, start - st), html.Substring(end + 1), startPos, end);
        }

        /// <summary>
        /// TAGの開始位置と終了位置の検索(TAGを含む)
        /// TAGが見つからない場合 開始位置 >= 終了位置
        /// </summary>
        /// <param name="html">HTMLソース</param>
        /// <param name="tag">タグ名(<>は含まない)</param>
        /// <param name="pos">検索開始位置</param>
        /// <returns>(開始位置,終了位置)</returns>
        public (int, int) getHtmlTagDataPos(string html, string tag, int pos = 0)
        {
            int startPos = pos;
            int endPos = pos;
            int count = 0;                      //  ネストの数
            int st, tagType, start, end;        //  st, et, 検索したタグの種別,開始位置,終了位置
            st = pos;                           //  タグデータの開始位置
            do {
                (tagType, start, end) = findHtmlTag(html, tag, pos);
                switch (tagType) {
                    case 1: break;                 //  完結タグ
                    case 2: count++; break;        //  開始タグ
                    case 3: count--; break;        //  終了タグ
                    case 0: return (startPos, endPos); //  不明タグでreturn  (検索開始位置を返す)
                }
                if (st == pos) {
                    startPos = start;
                    st = end + 1;
                }
                pos = end;
            } while (0 < count);

            return (startPos, end);
        }

        /// <summary>
        /// HTMLソースからタグで囲まれた領域を除く
        /// </summary>
        /// <param name="html">HTMLソース</param>
        /// <param name="tag">タグ名(<>は含まない)</param>
        /// <returns>取り除かれたHTMLソース</returns>
        public string stripHtmlTagData(string html, string tag)
        {
            string buffer = "";
            int pos = html.IndexOf("<" + tag);
            if (pos < 0)
                return html;
            buffer = html.Substring(0, pos);
            (string tagPara, string tagData, string nextSrc) = getHtmlTagData(html.Substring(pos), tag);
            buffer += stripHtmlTagData(nextSrc, tag);
            return buffer;
        }

        /// <summary>
        /// '<','>'で囲まれたタグデータからパラメータを抽出する
        /// <a href="...." title="TITLE">
        /// data = getHtmlTagPara(tagData, "title");
        /// </summary>
        /// <param name="tagData">タグデータ</param>
        /// <param name="paraName">パラメータ名</param>
        /// <returns>パラメータデータ</returns>
        public string getHtmlTagPara(string tagData, string paraName)
        {
            int st = tagData.IndexOf('<');
            if (st < 0)
                return "";
            st = tagData.IndexOf(paraName, st);
            if (0 <= st) {
                st = tagData.IndexOf('\"', st);
                if (0 <= st) {
                    int et = tagData.IndexOf('\"', st + 1);
                    if (0 < et)
                        return tagData.Substring(st + 1, et - st - 1);
                }
            }
            return "";
        }

        /// <summary>
        /// パラメータのタイトルでタグ全体を取得する
        /// パラメータのタイトルのデータを指定したい場合には対象データのあるタグデータを検索する
        /// </summary>
        /// <param name="tagData">HTMLソース</param>
        /// <param name="paraTitle">パラメータタイトル</param>
        /// <param name="paraData">パラメータタイトルのデータ(省略可)</param>
        /// <returns>'<''>'で囲まれたタグデータ</returns>
        public string getHtmlTagParaDataTitle(string tagData, string paraTitle, string paraData = "")
        {
            int s = 0;
            int m, n, l;
            do {
                m = tagData.IndexOf(paraTitle + "=\"", s);
                if (m < 0)
                    return "";
                if (paraData.Length == 0)
                    break;
                s = m + 1;
                n = tagData.IndexOf("\"", m);
                l = tagData.IndexOf("\"", n + 1);
            } while (tagData.Substring(n + 1, l - n - 1).CompareTo(paraData) != 0);

            int sp = tagData.LastIndexOf('<', m);
            int ep = tagData.IndexOf('>', m);
            if (0 <= sp && 0 <= ep)
                return tagData.Substring(sp, ep - sp + 1);
            return "";
        }

        /// <summary>
        /// HTMLのタグパラメータからデータだけを取り出す
        /// 例: html = "<a herf="/wiki/para" title="タイトル"> </a>";
        ///     (string para, string tdata, string next) = ylib.getHtmlTagData(html, "a");
        ///         [para = "herf=\"/wiki/para\" title=\"タイトル\"";]
        ///     data = stripParaData(para, "title");
        /// </summary>
        /// <param name="para">タグパラメータ</param>
        /// <param name="paraTitle">パラメータの種別</param>
        /// <returns>パラメータのデータ</returns>
        public string stripHtmlParaData(string para, string paraTitle)
        {
            int m = para.IndexOf(paraTitle + "=\"");
            if (m < 0)
                return "";
            int n = para.IndexOf('\"', m);
            if (n < 0)
                return "";
            int o = para.IndexOf('\"', n + 1);
            if (o < 0)
                return para.Substring(o + 1);
            return para.Substring(n + 1, o - 1 - n);
        }

        /// <summary>
        /// HTMLのデータからタグパラメータの存在するタグの開始位置を取得する
        /// </summary>
        /// <param name="para">HTMLデータ</param>
        /// <param name="paraTitle">タグパラメータ名</param>
        /// <returns>タグ開始位置</returns>
        public int findHtmlParaDataTagPos(string para , string paraTitle)
        {
            int m = para.IndexOf(paraTitle + "=\"");
            if (m < 0)
                return 0;
            return para.LastIndexOf('<', m);
        }

        /// <summary>
        /// HTMLソースからタグを検索し開始位置を求める
        /// </summary>
        /// <param name="html">HTMLソース</param>
        /// <param name="tagName">タグ名</param>
        /// <param name="pos">検索開始位置</param>
        /// <returns>(検索位置, パラメータ)</returns>
        public (int, string) findHtmlTagPos(string html, string tagName, int pos = 0)
        {
            pos = pos < 0 ? 0 : pos;
            string startTag = "<" + tagName + '>';
            string startTag2 = "<" + tagName + ' ';
            int sp = html.IndexOf(startTag, pos);
            if (0 <= sp)
                return (sp, "");
            int sp2 = html.IndexOf(startTag2, pos);
            if (0 <= sp2) {
                int ep = html.IndexOf("/>", sp2);
                if (0 <= ep)
                    return (sp2, html.Substring(sp2 + startTag2.Length, ep - (sp2 + startTag2.Length)));
                ep = html.IndexOf(">", sp2);
                if (0 <= ep)
                    return (sp2, html.Substring(sp2 + startTag2.Length, ep - (sp2 + startTag2.Length)));
            }
            return (-1, "");
        }

        /// <summary>
        /// タグを検索して種別と位置を求める
        /// 種別  1: <TAG ..../>  完結タグ
        ///       2: <TAG ...>    開始タグ
        ///       3: </TAG>       終了タグ
        ///       0: 不明         タグが見つからない
        /// </summary>
        /// <param name="html">HTMLソース</param>
        /// <param name="tagName">タグ名(</>は含まない)</param>
        /// <param name="pos">検索開始位置</param>
        /// <returns>(種別, タグの起点, タグの終点)</returns>
        public (int tagType, int start, int end) findHtmlTag(string html, string tagName, int pos = 0)
        {
            pos = pos < 0 ? 0 : pos;
            string startTag = "<" + tagName +'>';
            string startTag2 = "<" + tagName + ' ';
            string endTag = "</" + tagName + ">";
            int sp = html.IndexOf(startTag, pos);
            int sp2 = html.IndexOf(startTag2, pos);
            sp = (0 <= sp && 0 <= sp2) ? Math.Min(sp, sp2) : (0 <= sp ? sp : (0 <= sp2 ? sp2 : -1));    //  タグの開始位置
            int sep = html.IndexOf('>', sp < 0 ? pos : sp);     //  タグの終了位置
            int seep = html.IndexOf("/>", sp < 0 ? pos : sp);   //  タグの終了位置
            int ep = html.IndexOf(endTag, pos);                 //  終了タグの開始位置
            if (0 <= sp) {
                if ((0 <= ep && sp < ep) || ep < 0) {
                    if (0 <= sep && seep == sep - 1)
                        return (1, sp, sep);
                    else if (0 <= sep && seep != sep - 1)
                        return (2, sp, sep < 0 ? ep : sep);
                }
                if (0 <= ep && ep < sp) {
                    sep = html.IndexOf('>', ep);
                    return (3, ep, sep < 0 ? ep : sep);
                }
            } else if (0 <= ep) {
                sep = html.IndexOf('>', ep);
                return (3, ep, sep < 0 ? ep : sep);
            }
            sp = sp < 0 ? pos : sp;
            ep = ep < 0 ? pos : ep;
            return (0, ep, sep < 0 ? ep : sep);
        }


        //  --- データ処理関係  ------

        /// <summary>
        /// JSON形式の文字列から[名前:値]の対データをリストデータとして取得する
        /// </summary>
        /// <param name="jsonData">JSON形式の文字列</param>
        /// <param name="baseTitle">オブジェクト名(省略可)</param>
        /// <returns>対データのリストデータ</returns>
        public List<string[]> splitJson(string jsonData, string baseTitle = "")
        {
            List<string[]> jsonList = new List<string[]>();
            string[] data = new string[2];
            int i = 0;
            int j = 0;
            while (i < jsonData.Length) {
                if (jsonData[i] == '\"') {
                    data[j] = "";
                    i++;
                    while (jsonData[i] != '\"' && i < jsonData.Length) {
                        data[j] += jsonData[i];
                        i++;
                    }
                }
                if (jsonData[i] == ',' || i == jsonData.Length - 1) {
                    if (i == jsonData.Length - 1)
                        data[j] += jsonData[i];
                    data[0] = (0 < baseTitle.Length ? baseTitle + " " : "") + data[0].Trim();
                    data[1] = data[1].Trim();
                    if (0 < data[0].Length)
                        jsonList.Add(data);
                    j = 0;
                    data = new string[2];
                } else if (jsonData[i] == ':') {
                    j = 1;
                } else if (jsonData[i] == '\"') {

                } else if (jsonData[i] == '{') {
                    string jsonData2 = getJsonDataString(jsonData.Substring(i));
                    List<string[]> jsonList2 = splitJson(jsonData2, data[0]);
                    foreach (string[] data2 in jsonList2)
                        jsonList.Add(data2);
                    data[0] = "";
                    i += jsonData2.Length + 1;
                    while (jsonData[i] != '}' && i < jsonData.Length)
                        i++;
                } else {
                    data[j] += jsonData[i];
                }
                i++;
            }
            return jsonList;
        }

        /// <summary>
        /// JSON形式の文字列から{}内の文字列を取得する
        /// 入れ子構造に対応
        /// </summary>
        /// <param name="jsonData">JSON形式文字列</param>
        /// <returns>抽出したJSON形式文字列</returns>
        public string getJsonDataString(string jsonData)
        {
            string buffer = "";
            int brCount = 0;
            int i = 0;
            while (i < jsonData.Length) {
                if (jsonData[i] == '{') {
                    brCount++;
                    if (brCount <= 1) {
                        i++;
                        continue;
                    }
                } else if (jsonData[i] == '}') {
                    brCount--;
                    if (brCount == 0)
                        break;
                }
                buffer += jsonData[i];
                i++;
            }
            return buffer.Trim();
        }


        //  ---  文字列処理  ------

        /// <summary>
        /// 文字列を前から検索する
        /// </summary>
        /// <param name="text">検索される文字列</param>
        /// <param name="val">検索する文字列</param>
        /// <param name="count">検索回数</param>
        /// <returns>検索位置</returns>
        int indexOf(string text, string val, int count = 1)
        {
            var n = 0;
            for (int i = 0; i < count; i++) {
                n = text.IndexOf(val, n + 1);
            }
            return n;
        }

        /// <summary>
        /// 文字列を後から検索する
        /// </summary>
        /// <param name="text">検索される文字列</param>
        /// <param name="val">検索する文字列</param>
        /// <param name="count">検索回数</param>
        /// <returns>検索位置</returns>
        public int lastIndexOf(string text, string val, int count = 1)
        {
            var n = text.Length;
            for (int i = 0; i < count; i++) {
                n = text.LastIndexOf(val, n - 1);
            }
            return n;
        }

        /// <summary>
        /// 文字列から括弧で囲まれた領域を取り除く
        /// </summary>
        /// <param name="text">文字列</param>
        /// <param name="sb">開始括弧文字(省略時'[')</param>
        /// <param name="eb">終了括弧文字(省略時']')</param>
        /// <returns>文字列</returns>
        public string stripBrackets(string text, char sb = '[', char eb = ']')
        {
            int n = text.LastIndexOf(sb);
            if (n < 0)
                return text;
            int m = text.IndexOf(eb, n);
            if (0 <= n && 0 <= m) {
                text = text.Substring(0, n) + text.Substring(m + 1);
                text = stripBrackets(text, sb, eb);
            } else if (0 <= n && m < 0) {
                text = text.Substring(0, n);
            } else if (n < 0 && 0 <= m) {
                text = text.Substring(m + 1);
            }
            return text;
        }


        /// <summary>
        /// 文字列内のコントロールコードを除去する
        /// 0x20 <= (半角文字) <0xf0 and 0x100<= (全角文字)を通過させる
        /// UNICODEのHeader(0xFEFF)も除く
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public string trimControllCode(string buf)
        {
            if (buf == null)
                return "";
            string obuf = "";
            for (int i = 0; i < buf.Length; i++)
                if ((0x20 <= (int)buf[i] && (int)buf[i] < 0xf0) ||
                    ((0xff < (int)buf[i] && (int)buf[i] != 0xFEFF && !(0xFFFE <= (int)buf[i]))))
                    obuf += buf[i];
            return obuf;
        }

        /// <summary>
        /// 数値文字列かを判定する
        /// 数値以外の文字があっても数値にできるものは数値として判定
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public bool IsNumberString(string num)
        {
            if (num == null)
                return false;
            if (num.Length == 1 && num.CompareTo("0") == 0)
                return true;
            string nbuf = string2StringNum(num);
            double val;
            return double.TryParse(nbuf, out val) ? true : false;
        }

        /// <summary>
        /// 文字列を論理値に変換
        /// 変換できない場合はdefaultを使用
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="val">default値(省略時はtrue)</param>
        /// <returns></returns>
        public bool boolParse(string str, bool val = true)
        {
            bool b;
            if (bool.TryParse(str, out b))
                return b;
            else
                return val;
        }

        /// <summary>
        /// 文字列を整数に変換
        /// 変換できない場合はdefaultを使用
        /// </summary>
        /// <param name="str">数値文字列</param>
        /// <param name="val">default値(省略時は0)</param>
        /// <returns></returns>
        public int intParse(string str, int val = 0)
        {
            int i;
            str = str.Replace(",", "");
            if (int.TryParse(str.Trim(), out i))
                return i;
            else
                return val;
        }

        /// <summary>
        /// 文字列を実数に変換
        /// 変換できない場合はdefaultを使用
        /// </summary>
        /// <param name="str">数値文字列</param>
        /// <param name="val">default値(省略時は0)</param>
        /// <returns></returns>
        public double doubleParse(string str, double val = 0.0)
        {
            double d;
            if (double.TryParse(str, out d))
                return d;
            else
                return val;
        }

        /// <summary>
        /// 数値文字列を数値に変換、数値以外の時0を返す
        /// TryParseを使用
        /// </summary>
        /// <param name="str">数値文字列</param>
        /// <returns></returns>
        public double string2Double(string str)
        {
            if (str == null)
                return 0;
            double n;
            if (double.TryParse(str, out n))
                return n;
            else
                return 0;
        }

        /// <summary>
        /// 文字列の先頭が数値の場合、数値に変換する
        /// 変換できない時は0を返す
        /// 先頭の空白や0、後尾の数値以外の文字は除外して変換する
        /// </summary>
        /// <param name="num">数値入り文字列</param>
        /// <returns></returns>
        public double string2double(string num)
        {
            if (num == null)
                return 0;

            string nbuf = string2StringNum(num);
            double val;
            val = double.TryParse(nbuf, out val) ? val : 0;
            return val;
        }

        private string startNum = "+-123456789";
        private string numChar = ".0123456789";

        /// <summary>
        /// 文字列から数値に関係ない文字を除去し、実数に変換できる文字列にする
        /// </summary>
        /// <param name="num">数値文字列</param>
        /// <returns></returns>
        public string string2StringNum(string num)
        {
            string buf = "";
            num = num.Replace(",", "");             //  桁区切りのカンマを除く
            num = num.TrimStart(' ');               //  先頭の空白と0を除く
            for (int i = 0; i < num.Length; i++) {
                if (buf.Length == 0 || num[i] == 'E' || num[i] == 'e' ) {
                    if (0 <= startNum.IndexOf(num[i])) {
                        buf += num[i];
                    } else if (buf.Length == 0 && (i + 1 < num.Length) && num[i] == '0' && num[i + 1] != '.') {
                        //  先頭の不要な0を除く
                    } else if (i + 1 < num.Length && num[i] == '0' && num[i+1] == '.') {
                        buf += num[i++];
                        buf += num[i];
                    } else if (i + 1 < num.Length && (num[i] == 'E' || num[i] == 'e') && 0 <= startNum.IndexOf(num[i + 1])) {
                        buf += num[i++];
                        buf += num[i];
                    } else {
                        break;
                    }
                } else {
                    if (0 <= numChar.IndexOf(num[i])) {
                        buf += num[i];
                    } else {
                        break;
                    }
                }
            }
            if (buf.Length == 1 && (buf[0] == '+' || buf[0] == '-' || buf[0] == '.'))
                return "";
            for ( int i = buf.Length - 1; 0 <= i; i--) {
                if (0 <= numChar.IndexOf(buf[i])) {
                    buf = buf.Substring(0, i + 1);
                    break;
                }
            }

            return buf;
        }

        //private string string2stringNum(string num)
        //{
        //    num = num.Replace(",", "");             //  桁区切りのカンマを除く
        //    num = num.TrimStart(' ');               //  先頭の空白と0を除く
        //    if (1 < num.Length && num[1] != '.')
        //        num = num.TrimStart('0');           //  先頭の空白と0を除く
        //    if (num.Length <= 0)
        //        return "";
        //    int n = 0;
        //    string nbuf = "";
        //    if (Char.IsDigit(num[0]) || num[0] == '-')
        //        nbuf += num[0];
        //    else
        //        return "";
        //    for (int i = 1; i < num.Length; i++) {
        //        if (Char.IsDigit(num[i]) || num[i] == '.')
        //            nbuf += num[i];
        //        else
        //            break;
        //        if (0 < n && num[i] == '.')
        //            break;
        //    }
        //    return nbuf;
        //}

        private string ZenCode =
            "　！”＃＄％＆’（）＊＋，－．／" +
            "０１２３４５６７８９：；＜＝＞？" +
            "＠ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯ" +
            "ＰＱＲＳＴＵＶＷＸＹＺ［￥］＾＿" +
            "‘ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏ" +
            "ｐｑｒｓｔｕｖｗｘｙｚ｛｜｝～";
        private string HanCode =
            " !\"#$%&'()*+,-./" +
            "0123456789:;<=>?" +
            "@ABCDEFGHIJKLMNO" +
            "PQRSTUVWXYZ[\\]^_" +
            "`abcdefghijklmno" +
            "pqrstuvwxyz{|}~";
        /// <summary>
        /// 文字列内の全角英数字を半角に変換する
        /// </summary>
        /// <param name="zenStr"></param>
        /// <returns></returns>
        public string strZne2Han(string zenStr)
        {
            string buf = "";
            for (int i = 0; i < zenStr.Length; i++) {
                int n = ZenCode.IndexOf(zenStr[i]);
                if (0 <= n && n < HanCode.Length) {
                    buf += HanCode[n];
                } else {
                    buf += zenStr[i];
                }
            }
            return buf;
        }

        private string ZenNumCode = "０１２３４５６７８９．＋－";
        private string HanNumCode = "0123456789.+-";
        /// <summary>
        /// 文字列内の全角数値を半角に変換する
        /// </summary>
        /// <param name="zenStr"></param>
        /// <returns></returns>
        public string strNumZne2Han(string zenStr)
        {
            string buf = "";
            for (int i = 0; i < zenStr.Length; i++) {
                int n = ZenNumCode.IndexOf(zenStr[i]);
                if (0 <= n && n < HanCode.Length) {
                    buf += HanNumCode[n];
                } else {
                    buf += zenStr[i];
                }
            }
            return buf;
        }

        /// <summary>
        /// 文字列の中の改行コード、','、'"'を'\'付きコードに置き換える
        /// '"'はCSVの仕様に合わせて'""'にする
        /// </summary>
        /// <param name="str">文字列</param>
        /// <returns>'\'付き文字列</returns>
        public String strControlCodeCnv(String str)
        {
            String buffer;
            buffer = str.Replace("\\", "\\\\");         //  \→\\
            buffer = buffer.Replace("\r\n", "\\\\n");   //  \r\n→\\\n
            buffer = buffer.Replace("\n", "\\\\n");     //  \n→\\\n
            buffer = buffer.Replace("\r", "\\\\n");     //  \r→\\\n
            buffer = buffer.Replace(",", "\\,");        //  ,→\,
            buffer = buffer.Replace("\"", "\"\"");      //  "→""
            return buffer;
        }

        /// <summary>
        /// 文字列の中の'\'付きコードを通常のコードに戻す
        /// </summary>
        /// <param name="str">'\'付き文字列</param>
        /// <returns>文字列</returns>
        public String strControlCodeRev(String str)
        {
            String buffer;
            buffer = str.Replace("\\\\n", "\r\n");      //  \\\n→\n
            buffer = buffer.Replace("\\\\", "\\");      //  \\→\
            buffer = buffer.Replace("\\,", ",");        //  \,→,
            buffer = buffer.Replace("\"\"", "\"");      //  ""→"
            buffer = buffer.Replace("\\\"", "\"");      //  \"→"
            return buffer;
        }


        /// <summary>
        /// 文字列をカンマセパレータで分解して配列に格納する
        /// ""で囲まれた中で""→"、\"→"、\,→,、\\n→\nにする
        /// </summary>
        /// <param name="str">文字列</param>
        /// <returns></returns>
        public String[] seperateString(String str)
        {
            List<String> data = new List<string>();
            String buf = "";
            int i = 0;
            while (i < str.Length) {
                if (str[i] == '"' && i < str.Length - 1) {
                    //  ダブルクォーテーションで囲まれている場合
                    i++;
                    while (str[i] != '"' || (i + 1 < str.Length && str[i] == '"' && str[i + 1] == '"')) {
                        if (str[i] == '\\' && i + 1 < str.Length) {
                            //  コントロールコードの処理('\\"','\\,','\\\n','\\\r')
                            if (str[i + 1] == '"' || str[i + 1] == ',' || str[i + 1] == '\n') {
                                buf += str[i + 1];
                                i += 2;
                            } else if (str[i + 1] == '\r') {
                                buf += '\n';
                                i += 2;
                            } else {
                                buf += str[i++];
                            }
                        } else if (str[i] == '"' && i + 1 < str.Length) {
                            if (str[i + 1] == '"') {
                                buf += str[i + 1];
                                i += 2;
                            } else {
                                buf += str[i++];
                            }
                        } else {
                            buf += str[i++];
                        }
                        if (i == str.Length)
                            break;
                    }
                    data.Add(buf);
                    i++;
                } else if (str[i] == ',' && i < str.Length - 1) {
                    //  区切りカンマ
                    if (i == 0 || (0 < i && str[i - 1] == ','))
                        data.Add(buf);
                    i++;
                } else {
                    //  カンマ区切りの場合
                    if (str[i] == ',' && i < str.Length - 1) {
                        i++;
                    } else {
                        while (str[i] != ',') {
                            buf += str[i++];
                            if (i == str.Length)
                                break;
                        }
                    }
                    data.Add(buf);
                    i++;
                }
                buf = "";
            }
            return data.ToArray();
        }

        /// <summary>
        /// 文字列から指定文字以外の文字位置を検索する
        /// 指定文字以外の文字がなければ元の文字長さを返す
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="withoutChar">検索除外文字</param>
        /// <returns>検索位置</returns>
        public int getWithoutCharIndex(string str, string withoutChar)
        {
            for (int i = 0; i < str.Length; i++) {
                if (withoutChar.IndexOf(str[i]) < 0)
                    return i;
            }
            return str.Length;
        }

        /// <summary>
        /// ファイルのRead/Writeで文字コードを指定する
        /// UTF8 : 0  Shift-jis : 1 Euc-Jp:2
        /// </summary>
        /// <param name="n">文字コードの番号</param>
        public void setEncording(int n)
        {
            mEncordingType = n;
        }

        /// <summary>
        /// 文字配列をダブルクォーテーションで括ってカンマセパレーツで
        /// 一行の文字列に変換する
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string array2csvString(string[] data)
        {
            string buf = "";
            for (int i = 0; i < data.Length; i++) {
                buf += "\"" + (data[i] == null ? "" : data[i].Replace("\"", "\\\"")) + "\"";
                if (i != data.Length - 1)
                    buf += ",";
            }
            return buf;
        }

        /// <summary>
        /// 文字列が数値の場合、桁区切り(3桁)を入れる
        /// </summary>
        /// <param name="val">数値文字列</param>
        /// <returns>区切り入り文字列</returns>
        public String setDigitSeparator(String val)
        {
            double x;
            if (!Double.TryParse(val, out x))
                return val;
            //  '.''E''e'以降に桁区切りを入れないために対象の文字列長さを求める
            String buf = "";
            int n = val.IndexOf('.');
            int e = Math.Max(val.IndexOf('E'), val.IndexOf('e'));
            if (e < 0 && n < 0)
                n = val.Length;
            else if (e < 0 && 0 <= n)
                buf = val.Substring(n);
            else if (0 <= e && n < 0) {
                n = e;
                buf = val.Substring(n);
            } else
                buf = val.Substring(n);
            //  桁区切りを入れる
            int m = 1;
            for (int i = n - 1; 0 <= i; i--) {
                buf = val[i] + buf;
                if ((val[0] != '-' && 0 < i) || (val[0] == '-' && 1 < i)) {
                    if (m++ % 3 == 0)
                        buf = ',' + buf;
                }
            }
            return buf;
        }

        /// <summary>
        /// HTMLで使われいる{&#??;]のコードを通常の文字に置換える
        /// {&#???;] 文字を10進で表示  [&#x???;] 文字を16進で表示
        /// &xxx; の文字変換もおこなう
        /// </summary>
        /// <param name="html">HTMLソース</param>
        /// <returns>変換データ</returns>
        public string cnvHtmlSpecialCode(string html)
        {
            html = html.Replace("&lt;", "<");
            html = html.Replace("&gt;", ">");
            html = html.Replace("&amp;", "&");
            html = html.Replace("&quot;", "\"");
            html = html.Replace("&nbsp;", " ");

            int m = html.IndexOf("&#");
            int n = 0;
            if (0 <= m)
                n = html.IndexOf(";", m);
            char code;
            while (0 <= m && 0 <= n) {
                string specialCode = html.Substring(m, n - m);
                if (specialCode[2] == 'x')
                    code = (char)Convert.ToInt32(specialCode.Substring(3), 16);
                else
                    code = (char)int.Parse(specialCode.Substring(2));
                html = html.Replace(specialCode + ";", code.ToString());
                m = html.IndexOf("&#");
                if (0 <= m)
                    n = html.IndexOf(";", m);
            }
            return html;
        }


        // ---  ストップウォッチ  ---

        /// <summary>
        /// ストップウォッチ機能初期化・計測開始
        /// </summary>
        public void stopWatchStartNew()
        {
            mSw = System.Diagnostics.Stopwatch.StartNew();
            mStopWatchTotalTime = new TimeSpan();
        }

        /// <summary>
        /// ストップウォッチ機能ラップ時間取得
        /// </summary>
        /// <returns>計測時間</returns>
        public TimeSpan stopWatchLapTime()
        {
            mSw.Stop();
            TimeSpan lap = mSw.Elapsed;
            mSw.Start();
            return lap;
        }

        /// <summary>
        /// ストップウォッチ機能計測時間の取得と再スタート
        /// 累積時間には追加
        /// </summary>
        /// <returns>計測時間</returns>
        public TimeSpan stopWatchRestart()
        {
            mSw.Stop();
            TimeSpan lap = mSw.Elapsed;
            mStopWatchTotalTime += lap;
            mSw.Restart();
            return lap;
        }

        /// <summary>
        /// ストップウォッチ機能計測時間の取得と終了
        /// </summary>
        /// <returns>計測時間</returns>
        public TimeSpan stopWatchStop()
        {
            mSw.Stop();
            TimeSpan lap = mSw.Elapsed;
            mStopWatchTotalTime += lap;
            return lap;
        }

        /// <summary>
        /// ストップウォッチ機能累積時間の取得
        /// </summary>
        /// <returns>計測時間</returns>
        public TimeSpan stopWatchTotalTime()
        {
            return mStopWatchTotalTime;
        }

        //  ---  日付・時間処理  ------

        /// <summary>
        /// 秒数を時分秒の文字列に変換 s → mm:ss or hh:mm:ss
        /// </summary>
        /// <param name="seconds">時間(秒)</param>
        /// <param name="auto">自動で選択(mm:ss or hh:mm:ss)</param>
        /// <returns>時刻文字列</returns>
        public string second2String(double seconds, bool auto)
        {
            long second = (long)seconds % 60;
            long minute = (long)(seconds / 60) % 60;
            long hour = (long)(seconds / 3600);
            if (0 < hour || !auto)
                return string.Format("{0:00}:{1:00}:{2:00}", hour, minute, second);
            else
                return string.Format("{0:00}:{1:00}", minute, second);
        }

        /// <summary>
        /// 文字列の時刻タイプの判定
        /// hh:mm:ss,hh:mm,hh:mm AM/PM, h時m分s秒, h時m分
        /// </summary>
        /// <param name="time">文字列</param>
        /// <returns></returns>
        public bool IsTime(string time)
        {
            if (Regex.IsMatch(time, "(^[01][0-9]|^2[0123]):([0-5][0-9]):([0-5][0-9])") ||     //  24時:分:秒
                Regex.IsMatch(time, "(^[1-9]|^1[012]):([0-5][0-9]):([0-5][0-9])") ||          //  12時:分:秒
                Regex.IsMatch(time, "(^[01][0-9]|^2[0123]):([0-5][0-9])") ||                  //  時:分
                Regex.IsMatch(time, "(^[1-9]|^1[012]):([0-5][0-9]) (AM|PM)") ||               //  時:分 PM
                Regex.IsMatch(time, "(^[0-9]|^[01][0-9]|^2[0123])時([0-9]|[0-5][0-9])分([0-9]|[0-5][0-9])秒") ||
                Regex.IsMatch(time, "(^[0-9]|^[01][0-9]|^2[0123])時([0-9]|[0-5][0-9])分"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 文字列時刻を秒数に変換
        /// hh:mm:ss,hh:mm,hh:mm AM/PM,hh時mm分ss秒,hh時mm分
        /// </summary>
        /// <param name="time">時刻文字列</param>
        /// <returns>秒数</returns>

        public int time2Seconds(string time)
        {
            int h = 0;
            int m = 0;
            int s = 0;
            if (Regex.IsMatch(time, "([01][0-9]|2[0123]):([0-5][0-9]):([0-5][0-9])")) {     //  24時:分:秒
                h = int.Parse(time.Substring(0, 2));
                m = int.Parse(time.Substring(3, 2));
                s = int.Parse(time.Substring(6, 2));
            } else if (Regex.IsMatch(time, "([1-9]|1[012]):([0-5][0-9]):([0-5][0-9])")) {   //  12時:分:秒
                h = int.Parse(time.Substring(0, time.IndexOf(':')));
                m = int.Parse(time.Substring(time.IndexOf(':') + 1, time.IndexOf(':', time.IndexOf(':') + 1) - time.IndexOf(':') - 1));
                s = int.Parse(time.Substring(time.IndexOf(':', time.IndexOf(':') + 1) + 1));
            } else if (Regex.IsMatch(time, "([01][0-9]|2[0123]):([0-5][0-9])")) {           //  時:分
                h = int.Parse(time.Substring(0, time.IndexOf(':')));
                m = int.Parse(time.Substring(time.IndexOf(':') + 1));
            } else if (Regex.IsMatch(time, "([1-9]|1[012]):([0-5][0-9]) (AM|PM)")) {        //  時:分 PM
                h = int.Parse(time.Substring(0, time.IndexOf(':')));
                m = int.Parse(time.Substring(time.IndexOf(':') + 1, time.IndexOf(' ') - time.IndexOf(':')));
                if (0 < time.IndexOf("PM"))
                    h += 12;
            } else if (Regex.IsMatch(time, "([0-9]|[01][0-9]|2[0123])時([0-9]|[0-5][0-9])分([0-9]|[0-5][0-9])秒")) {
                h = int.Parse(time.Substring(0, time.IndexOf('時')));
                int hp = time.IndexOf('時') + 1;
                int mp = time.IndexOf('分') - time.IndexOf('時') - 1;
                m = int.Parse(time.Substring(time.IndexOf('時') + 1, time.IndexOf('分') - time.IndexOf('時') - 1));
                s = int.Parse(time.Substring(time.IndexOf('分') + 1, time.IndexOf('秒') - time.IndexOf('分') - 1));
            } else if (Regex.IsMatch(time, "([0-9]|[01][0-9]|2[0123])時([0-9]|[0-5][0-9])分")) {
                h = int.Parse(time.Substring(0, time.IndexOf('時')));
                m = int.Parse(time.Substring(time.IndexOf('時') + 1, time.IndexOf('分') - time.IndexOf('時') - 1));
            } else {
                return 0;
            }
            return ((h * 60) + m) * 60 + s;
        }

        /// <summary>
        /// 文字列が曜日かの判定
        /// </summary>
        /// <param name="weekday">曜日</param>
        /// <returns></returns>
        public bool IsWeekday(string weekday)
        {
            if (Regex.IsMatch(weekday, "^[日月火水木金土]|^[日月火水木金土]曜|^[日月火水木金土]曜日") ||
                Regex.IsMatch(weekday, "(Sunday|Monday|Tuesday|Wednesday|Thursday|Friday|Saturday)") ||
                Regex.IsMatch(weekday, "(Sun|Mon|Tue|Wed|Thu|Fri|Sat)"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 文字列から曜日の番号を返す
        /// 日曜日を0とする
        /// </summary>
        /// <param name="weekday">曜日</param>
        /// <returns>曜日の位置</returns>
        public int WeekNo(string weekday)
        {
            if (weekday.Length < 1)
                return -1;
            int weekNo = "日月火水木金土".IndexOf(weekday.Substring(0, 1));
            if (weekNo < 0 && 2 < weekday.Length) {
                weekNo = "SunMonTueWedThuFriSat".IndexOf(weekday.Substring(0, 3));
                weekNo = weekNo < 0 ? weekNo : weekNo / 3;
            }
            return weekNo;
        }

        /// <summary>
        /// 曜日の位置から曜日文字列を返す
        /// </summary>
        /// <param name="weekNo">曜日の位置</param>
        /// <returns>曜日文字列</returns>
        public string getWeekday(int weekNo, int type)
        {
            string[,] weekday = {
                { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" },
                { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" },
                { "日曜日", "月曜日", "火曜日", "水曜日", "木曜日", "金曜日", "土曜日" },
                { "日", "月", "火", "水", "木", "金", "土" },
            };
            return weekday[type % weekday.GetLength(0), weekNo % weekday.GetLength(1)];
        }

        /// <summary>
        /// 各種の日付パターンをyyyy/mm/ddに変換する
        /// </summary>
        /// <param name="date">日付文字列</param>
        /// <returns></returns>
        public string cnvDateFormat(string date)
        {
            string[] datePattern = {
                //  [month] dd, yyyy
                "(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec).*?([12][0-9]|3[01]|[1-9]),.*?(19[0-9][0-9]|2[01][0-9][0-9])",
                //  [元号]yy年mm月dd日
                "(令和|平成|昭和|大正|明治)([1-9]|[0-9][0-9])年([1-9]|[01][0-2])月([0-2][0-9]|3[01]|[1-9])日",
                //  yyyy年ww周
                "(^19[0-9][0-9]|2[01][0-9][0-9])年([1-5][0-9]|[1-9])週",
                //  yyyy年mm月w週
                "(^19[0-9][0-9]|2[01][0-9][0-9])年([1-9]|1[0-2])月([1-5])週",
                //  yyyy年mm月dd日
                "(19[0-9][0-9]|2[01][0-9][0-9])年([1-9]|1[0-2])月([12][0-9]|3[01]|[1-9])日",
                //  yyyy年mm月
                "(19[0-9][0-9]|2[01][0-9][0-9])年(1[0-2]|[1-9])月",
                //  yyyy年
                "(19[0-9][0-9]|2[01][0-9][0-9])年",
                //  mm月dd日
                "([1-9]|1[0-2])月([1-9]|[12][0-9]|3[01])日",
                //  yyyy/mm/dd
                "(19[0-9][0-9]|2[01][0-9][0-9])/([1-9]|1[0-2]|0[1-9])/([12][0-9]|3[01]|0[1-9]|[1-9])",
                //  yyyy/mm
                "(19[0-9][0-9]|2[01][0-9][0-9])/([1-9]|1[0-2]|0[1-9])",
                //  yyyy-mm-dd
                "(19[0-9][0-9]|2[01][0-9][0-9])-([1-9]|1[0-2]|0[1-9])-([12][0-9]|3[01]|0[1-9]|[1-9])",
                //  dd/mm/yyyy
                "(1[0-2]|0[1-9]|[1-9])/([1-9]|[12][0-9]|3[01]|0[1-9])/(19[0-9][0-9]|2[01][0-9][0-9])",
                //  yyyymmdd
                "(19[0-9][0-9]|2[01][0-9][0-9])(1[0-2]|0[1-9])([12][0-9]|3[01]|0[1-9])",
                //  yyyy
                "(19[0-9][0-9]|2[01][0-9][0-9])",
                };
            int year = 0;
            int month = 0;
            int day = 0;
            int week = 0;
            int jd = 0;
            for (int i = 0; i < datePattern.Length; i++) {
                if (Regex.IsMatch(date, datePattern[i])) {
                    List<string[]> dateList = getPattern(date, datePattern[i]);
                    switch (i) {
                        case 0: //  [manth] dd, yyyy
                            year = intParse(dateList[0][3]);
                            month = Array.IndexOf(mMonth, dateList[0][1]) % 12 + 1;
                            day = intParse(dateList[0][2]);
                            break;
                        case 1: //  [元号]yy年mm月dd日
                            year = intParse(dateList[0][2]) + (int)mKaigen[Array.IndexOf(mGengou, dateList[0][1])];
                            month = intParse(dateList[0][3]);
                            day = intParse(dateList[0][4]);
                            break;
                        case 2: //  yyyy年ww周
                            year = intParse(dateList[0][1]);
                            week = intParse(dateList[0][2]);
                            jd = (week - 1) * 7 + (date2JulianDay(year, 1, 1) / 7) * 7 - 1;
                            month = JulianDay2Month(jd);
                            day = JulianDay2Day(jd);
                            break;
                        case 3: //  yyyy年mm月w週
                            year = intParse(dateList[0][1]);
                            month = intParse(dateList[0][2]);
                            week = intParse(dateList[0][3]);
                            jd = (week - 1) * 7 + (date2JulianDay(year, month, 1) / 7) * 7 - 1;
                            day = JulianDay2Day(jd);
                            break;
                        case 4: //  yyyy年mm月dd日
                            year = intParse(dateList[0][1]);
                            month = intParse(dateList[0][2]);
                            day = intParse(dateList[0][3]);
                            break;
                        case 5: //  yyyy年mm月
                            year = intParse(dateList[0][1]);
                            month = intParse(dateList[0][2]);
                            day = 1;
                            break;
                        case 6: //  yyyy年
                            year = intParse(dateList[0][1]);
                            month = 1;
                            day = 1;
                            break;
                        case 7: //  mm月dd日
                            year = DateTime.Now.Year;
                            month = intParse(dateList[0][1]);
                            day = intParse(dateList[0][2]);
                            break;
                        case 8: //   yyyy/mm/dd
                            year = intParse(dateList[0][1]);
                            month = intParse(dateList[0][2]);
                            day = intParse(dateList[0][3]);
                            break;
                        case 9: //  yyyy/mm
                            year = intParse(dateList[0][1]);
                            month = intParse(dateList[0][2]);
                            day = 1;
                            break;
                        case 10: //  yyyy-mm-dd
                            year = intParse(dateList[0][1]);
                            month = intParse(dateList[0][2]);
                            day = intParse(dateList[0][3]);
                            break;
                        case 11: //  mm/dd/yyyy
                            year = intParse(dateList[0][3]);
                            month = intParse(dateList[0][1]);
                            day = intParse(dateList[0][2]);
                            break;
                        case 12: //  yyyymmdd
                            year = intParse(dateList[0][1]);
                            month = intParse(dateList[0][2]);
                            day = intParse(dateList[0][3]);
                            break;
                        case 13: //  yyyy
                            year = intParse(dateList[0][1]);
                            month = 1;
                            day = 1;
                            break;
                    }

                    string buf = string.Format("{0:0000}/{1:00}/{2:00}", year, month, day);
                    //  デバッグ用
                    //buf = "(" + i + ") " + buf + " ";
                    //foreach (string[] datas in dateList) {
                    //    for (int j = 0; j < datas.Length; j++)
                    //        buf += j + ": " + datas[j] + " ";
                    //}
                    return buf;
                }
            }
            return "";      //  UnMatch!!
        }

        /// <summary>
        /// 文字列が日付かを判定する
        /// yyyy年mm月dd日,mm月dd日,yyyy/mm/dd,yyyy-mm-dd,yyyymmdd,mm/dd/yyyy,GGyy年mm月dd日
        /// yyyy年mm月,yyyy年は含めない
        /// </summary>
        /// <param name="date">文字列</param>
        /// <returns></returns>
        public bool IsDateString(string date)
        {
            if (Regex.IsMatch(date, "(^19[0-9][0-9]|^2[01][0-9][0-9])年([1-9]|1[0-2])月([1-9]|[12][0-9]|3[01])日") ||            //  yyyy年mm月dd日
                Regex.IsMatch(date, "(^[1-9]|^1[0-2])月([1-9]|[12][0-9]|3[01])日") ||                                           //  mm月dd日
                Regex.IsMatch(date, "(^19[0-9][0-9]|^2[01][0-9][0-9])/([1-9]|1[0-2]|0[1-9])/([1-9]|[12][0-9]|3[01]|0[1-9])") ||   //  yyyy/mm/dd
                Regex.IsMatch(date, "(^19[0-9][0-9]|^2[01][0-9][0-9])-([1-9]|1[0-2]|0[1-9])-([1-9]|[12][0-9]|3[01]|0[1-9])") ||   //  yyyy-mm-dd
                Regex.IsMatch(date, "(^[1-9]|1[0-2]|^0[1-9])/([1-9]|[12][0-9]|3[01]|0[1-9])/(19[0-9][0-9]|2[01][0-9][0-9])") ||   //  mm/dd/yyyy
                (Regex.IsMatch(date, "(^19[0-9][0-9]|^2[01][0-9][0-9])(1[0-2]|0[1-9])([12][0-9]|3[01]|0[1-9])") && (date.Length == 8)) ||//  yyyymmdd
                Regex.IsMatch(date, "(^令和|^平成|^昭和|^大正|^明治)([1-9]|[0-9][0-9])年([1-9]|[01][0-2])月([1-9]|[0-2][0-9]|3[01])日"))  //  [元号]yy年mm月dd日
                return true;
            return false;
        }

        /// <summary>
        /// 文字列が年を表しているかを判定する
        /// yyyy年
        /// </summary>
        /// <param name="date">文字列</param>
        /// <returns></returns>
        public bool IsDateYearString(string date)
        {
            if (Regex.IsMatch(date, "(19[0-9][0-9]|2[01][0-9][0-9])年"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 文字列が年月を表しているか判定する
        /// yyyy年mm月
        /// </summary>
        /// <param name="date">文字列</param>
        /// <returns></returns>
        public bool IsDateMonthString(string date)
        {
            if (Regex.IsMatch(date, "(^19[0-9][0-9]|^2[01][0-9][0-9])年([1-9]|1[0-2])月"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 文字列が週単位の日付を表しているか判定する
        /// yyyy年mm月ww週,yyyy年ww週
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool IsDateWeekString(string date)
        {
            if (Regex.IsMatch(date, "(^19[0-9][0-9]|^2[01][0-9][0-9])年([1-9]|1[0-2])月([1-5])週") ||
                Regex.IsMatch(date, "(^19[0-9][0-9]|^2[01][0-9][0-9])年([1-9]|[1-5][0-9])週"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 歴日からユリウス日に変換
        /// yyyy年mm月dd日,yyyy/mm/dd,yyyy-mm-dd,yyyymmdd,mm/dd/yyyy
        /// </summary>
        /// <param name="date">文字列(歴日)</param>
        /// <returns>ユリウス日</returns>
        public int date2JulianDay(string date)
        {
            int yp = 0, mp = 0, dp = 0, wp = 0;
            int year = 0, month = 1, day = 1, week= 1;
            date = date.Replace(" ", "");
            if (Regex.IsMatch(date, "^(令和|平成|昭和|大正|明治)([1-9]|[0-9][0-9])年([1-9]|[01][0-2])月([1-9]|[0-2][0-9]|3[01])日")) {
                //  和暦(平成yy年MM月dd日)
                return wareki2JulianDay(date);
            } else if (Regex.IsMatch(date, "^(^19[0-9][0-9]|^2[01][0-9][0-9])年([1-9]|[1-5][0-9])週")) {
                //  西暦と週(yyyy年ww週)
                yp = date.IndexOf("年");
                wp = date.IndexOf("週");
                year = int.Parse(date.Substring(0, yp));
                week = int.Parse(date.Substring(yp + 1, wp - yp - 1));
                return (week - 1) * 7 + (date2JulianDay(year, 1, 1) / 7) * 7 - 1;
            } else if (Regex.IsMatch(date, "^(^19[0-9][0-9]|^2[01][0-9][0-9])年([1-9]|1[0-2])月([1-5])週")) {
                //  西暦と月と週(yyyy年MM月ww週)
                yp = date.IndexOf("年");
                mp = date.IndexOf("月");
                wp = date.IndexOf("週");
                year = int.Parse(date.Substring(0, yp));
                month = int.Parse(date.Substring(yp + 1, mp - yp - 1));
                week = int.Parse(date.Substring(mp + 1, wp - mp - 1));
                return (week - 1) * 7 + (date2JulianDay(year, month, 1) / 7) * 7 - 1;
            } else if (Regex.IsMatch(date, "^(19[0-9][0-9]|2[01][0-9][0-9])年([1-9]|1[0-2])月([1-9]|[12][0-9]|3[01])日")) {
                //  西暦(yyyy年MM月dd日)
                yp = date.IndexOf("年");
                mp = date.IndexOf("月");
                dp = date.IndexOf("日");
                year = int.Parse(date.Substring(0, yp));
                month = int.Parse(date.Substring(yp + 1, mp - yp - 1));
                day = int.Parse(date.Substring(mp + 1, dp - mp - 1));
            } else if (Regex.IsMatch(date, "^(19[0-9][0-9]|2[01][0-9][0-9])年([1-9]|1[0-2])月")) {
                //  西暦と月 (yyyy年MM月)
                yp = date.IndexOf("年");
                mp = date.IndexOf("月");
                year = int.Parse(date.Substring(0, yp));
                month = int.Parse(date.Substring(yp + 1, mp - yp - 1));
            } else if (Regex.IsMatch(date, "^(19[0-9][0-9]|2[01][0-9][0-9])年")) {
                //  西暦の年のみ(yyyy年)
                yp = date.IndexOf("年");
                year = int.Parse(date.Substring(0, yp));
            } else if (Regex.IsMatch(date, "^([1-9]|1[0-2])月([1-9]|[12][0-9]|3[01])日")) {
                //  年なし(MM月dd日)
                mp = date.IndexOf("月");
                dp = date.IndexOf("日");
                year = DateTime.Now.Year;
                month = int.Parse(date.Substring(0, mp));
                day = int.Parse(date.Substring(mp + 1, dp - mp - 1));
            } else if (Regex.IsMatch(date, "^(19[0-9][0-9]|2[01][0-9][0-9])/([1-9]|1[0-2]|0[1-9])/([1-9]|[12][0-9]|3[01]|0[1-9])")) {
                //  yyyy/MM/dd
                yp = date.IndexOf("/");
                mp = date.IndexOf("/", yp + 1);
                dp = getWithoutCharIndex(date, "0123456789/");
                year = int.Parse(date.Substring(0, yp));
                month = int.Parse(date.Substring(yp + 1, mp - yp - 1));
                day = int.Parse(date.Substring(mp + 1, dp - mp - 1));
            } else if (Regex.IsMatch(date, "^(19[0-9][0-9]|2[01][0-9][0-9])/([1-9]|1[0-2]|0[1-9])")) {
                //  yyyy/MM
                yp = date.IndexOf("/");
                year = int.Parse(date.Substring(0, yp));
                month = int.Parse(date.Substring(yp + 1));
            } else if (Regex.IsMatch(date, "^(19[0-9][0-9]|2[01][0-9][0-9])-([1-9]|1[0-2]|0[1-9])-([1-9]|[12][0-9]|3[01]|0[1-9])")) {
                //  yyyy-MM-dd
                yp = date.IndexOf("-");
                mp = date.IndexOf("-", yp + 1);
                dp = getWithoutCharIndex(date, "0123456789-");
                year = int.Parse(date.Substring(0, yp));
                month = int.Parse(date.Substring(yp + 1, mp - yp - 1));
                day = int.Parse(date.Substring(mp + 1, dp - mp - 1));
            } else if (Regex.IsMatch(date, "^([1-9]|1[0-2]|0[1-9])/([1-9]|[12][0-9]|3[01]|0[1-9])/(19[0-9][0-9]|2[01][0-9][0-9])")) {
                //  MM/dd/yyyy
                mp = date.IndexOf("/");
                dp = date.IndexOf("/", mp + 1);
                yp = getWithoutCharIndex(date, "0123456789/");
                month = int.Parse(date.Substring(0, mp));
                day = int.Parse(date.Substring(mp + 1, dp - mp - 1));
                year = int.Parse(date.Substring(dp + 1, yp - dp - 1));
            } else if (Regex.IsMatch(date, "^(19[0-9][0-9]|2[01][0-9][0-9])(1[0-2]|0[1-9])([12][0-9]|3[01]|0[1-9])")) {
                //  yyyyMMdd
                year = int.Parse(date.Substring(0, 4));
                month = int.Parse(date.Substring(4, 2));
                day = int.Parse(date.Substring(6, 2));
            } else if (Regex.IsMatch(date, "^(19[0-9][0-9]|2[01][0-9][0-9])")) {
                //  yyyy
                year = int.Parse(date.Substring(0, 4));
            } else {
                return 0;
            }
            return date2JulianDay(year, month, day);
        }

        /// <summary>
        /// 歴日からユリウス日に変換
        /// </summary>
        /// <param name="year">西暦年</param>
        /// <param name="month">月</param>
        /// <param name="day">日</param>
        /// <returns></returns>
        public int date2JulianDay(int year, int month, int day)
        {
            if (month <= 2) {
                month += 12;
                year--;
            }
            if ((year * 12 + month) * 31 + day >= (1582 * 12 + 10) * 31 + 15) {
                //  1582/10/15以降はグレゴリオ暦
                day += 2 - year / 100 + year / 400;
            }
            return (int)Math.Floor(365.25 * (year + 4716)) + (int)(30.6001 * (month + 1)) + day - 1524;
        }

        /// <summary>
        /// ユリウス日から歴日文字列
        /// </summary>
        /// <param name="jd">ユリウス日</param>
        /// <param name="sp">セパレータ有無</param>
        /// <returns>歴日</returns>
        public string JulianDay2DateYear(int jd, bool sp)
        {
            return JulianDay2DateYear(jd, sp ? 0 : 1);
        }

        /// <summary>
        /// 年差を求める
        /// </summary>
        /// <param name="startDate">開始年(yyyy/MM/dd)</param>
        /// <param name="endDate">終了年(yyyy/MM/dd)</param>
        /// <returns></returns>
        public int subYear(string startDate, string endDate)
        {
            int jd1 = date2JulianDay(startDate);
            int jd2 = date2JulianDay(endDate);
            if (jd1 == 0) {
                DateTime dt = DateTime.Now;
                string c = dt.ToString("yyyy/MM/dd");
                jd1 = date2JulianDay(c);
            }
            if (jd2 == 0) {
                DateTime dt = DateTime.Now;
                string c = dt.ToString("yyyy/MM/dd");
                jd2 = date2JulianDay(c);
            }
            return subYear(jd1, jd2);

        }


        /// <summary>
        /// ユリウス日から年数差を求める(jd2 < jd1 で　正数)
        /// 年齢を求めるのに使用
        /// </summary>
        /// <param name="jd1">開始年月日(ユリウス日)</param>
        /// <param name="jd2">終了年月日(ユリウス日)</param>
        /// <returns></returns>
        public int subYear(int jd1, int jd2)
        {
            int year = JulianDay2Year(jd2) - JulianDay2Year(jd1);
            int month = JulianDay2Month(jd2) - JulianDay2Month(jd1);
            if (0 < month) {
                return year;
            } else if (month == 0) {
                int day = JulianDay2Day(jd2) - JulianDay2Day(jd1);
                if (0 <= day) {
                    return year;
                } else {
                    return year - 1;
                }
            } else {
                return year - 1;
            }
        }

        /// <summary>
        /// ユリウス日から年を抽出
        /// </summary>
        /// <param name="jd">ユリウス日</param>
        /// <returns>西洋年</returns>
        public int JulianDay2Year(int jd)
        {
            int jdc = jd;
            if (jdc >= 2299161) {
                //  1582/10+15以降はグレゴリオ暦
                int t = (int)((jdc - 1867216.25) / 365.25);
                jdc += 1 + t / 100 - t / 400;
            }
            jdc += 1524;
            int y = (int)Math.Floor(((jdc - 122.1) / 365.25));
            jdc -= (int)Math.Floor(365.25 * y);
            int m = (int)(jdc / 30.6001);
            jdc -= (int)(30.6001 * m);
            int day = jdc;
            int month = m - 1;
            int year = y - 4716;
            if (month > 12) {
                month -= 12;
                year++;
            }
            return year;
        }


        /// <summary>
        /// ユリウス日から月を抽出
        /// </summary>
        /// <param name="jd">ユリウス日</param>
        /// <returns>月</returns>
        public int JulianDay2Month(int jd)
        {
            int jdc = jd;
            if (jdc >= 2299161) {
                //  1582/10+15以降はグレゴリオ暦
                int t = (int)((jdc - 1867216.25) / 365.25);
                jdc += 1 + t / 100 - t / 400;
            }
            jdc += 1524;
            int y = (int)Math.Floor(((jdc - 122.1) / 365.25));
            jdc -= (int)Math.Floor(365.25 * y);
            int m = (int)(jdc / 30.6001);
            jdc -= (int)(30.6001 * m);
            int day = jdc;
            int month = m - 1;
            int year = y - 4716;
            if (month > 12) {
                month -= 12;
                year++;
            }
            return month;
        }

        /// <summary>
        /// ユリウス日から日を抽出
        /// </summary>
        /// <param name="jd">ユリウス日</param>
        /// <returns>日</returns>

        public int JulianDay2Day(int jd)
        {
            int jdc = jd;
            if (jdc >= 2299161) {
                //  1582/10+15以降はグレゴリオ暦
                int t = (int)((jdc - 1867216.25) / 365.25);
                jdc += 1 + t / 100 - t / 400;
            }
            jdc += 1524;
            int y = (int)Math.Floor(((jdc - 122.1) / 365.25));
            jdc -= (int)Math.Floor(365.25 * y);
            int m = (int)(jdc / 30.6001);
            jdc -= (int)(30.6001 * m);
            int day = jdc;
            int month = m - 1;
            int year = y - 4716;
            if (month > 12) {
                month -= 12;
                year++;
            }
            return day;
        }

        /// <summary>
        /// ユリウス日から歴日文字列に変換
        /// 0: yyyy/mm/dd 1: yyyymmdd 2:yyyy-mm-dd  3: mm/dd/yyyy 
        /// 4: yyyy年mm月dd日 5: yyyy年mm月 6: yyyy年
        /// 7: yyyy年mm月ww週 8: yyyy年ww週
        /// </summary>
        /// <param name="jdc">ユリウス日</param>
        /// <param name="type">歴日文字列タイプ</param>
        /// <returns>歴日文字列</returns>
        public string JulianDay2DateYear(int jd, int type)
        {
            string date = "";
            int jdc = jd;
            if (jdc >= 2299161) {
                //  1582/10+15以降はグレゴリオ暦
                int t = (int)((jdc - 1867216.25) / 365.25);
                jdc += 1 + t / 100 - t / 400;
            }
            jdc += 1524;
            int y = (int)Math.Floor(((jdc - 122.1) / 365.25));
            jdc -= (int)Math.Floor(365.25 * y);
            int m = (int)(jdc / 30.6001);
            jdc -= (int)(30.6001 * m);
            int day = jdc;
            int month = m - 1;
            int year = y - 4716;
            if (month > 12) {
                month -= 12;
                year++;
            }
            int weekNo = 0;
            switch (type) {
                case 0:     //  yyyy/mm/dd
                    date = string.Format("{0:0000}/{1:00}/{2:00}", year, month, day);
                    break;
                case 1:     //  yyyymmdd
                    date = string.Format("{0:0000}{1:00}{2:00}", year, month, day);
                    break;
                case 2:     //  yyyy-mm-dd
                    date = string.Format("{0:0000}-{1:00}-{2:00}", year, month, day);
                    break;
                case 3:     //  mm/dd/yyyy
                    date = string.Format("{0:00}/{1:00}/{2:0000}", month, day, year);
                    break;
                case 4:     //  yyyy年mm月dd日
                    date = string.Format("{0}年{1}月{2}日", year, month, day);
                    break;
                case 5:     //  yyyy年mm月
                    date = string.Format("{0}年{1}月", year, month);
                    break;
                case 6:   //    yyyy年  
                    date = string.Format("{0}年", year);
                    break;
                case 7:     //  yyyy年mm月ww週
                    int jdt = date2JulianDay(year, month, 1) + 1;
                    weekNo = (jd + 1) / 7 - jdt /  7  + 1;
                    date = string.Format("{0}年{1}月{2}週", year, month, weekNo);
                    break;
                case 8:     //  yyyy年ww週
                    weekNo = (jd + 1) / 7 - (date2JulianDay(year, 1, 1) + 1) / 7 + 1;
                    date = string.Format("{0}年{1}週", year, weekNo);
                    break;
                default:    //  yyyy/mm/dd
                    date = string.Format("{0:0000}/{1:00}/{2:00}", year, month, day);
                    break;
            }
            return date;
        }

        /// <summary>
        /// 和暦をユリウス日に変換
        /// </summary>
        /// <param name="wareki"></param>
        /// <returns></returns>
        public int wareki2JulianDay(string wareki)
        {
            string era = wareki.Substring(0, 2);
            int yp = wareki.IndexOf("年");
            int mp = wareki.IndexOf("月");
            int dp = wareki.IndexOf("日");
            int year = int.Parse(wareki.Substring(2, yp - 2));
            int month = int.Parse(wareki.Substring(yp + 1, mp - yp - 1));
            int day = int.Parse(wareki.Substring(mp + 1, dp - mp - 1));
            if (era.CompareTo("令和") == 0) {
                year += (int)mKaigen[0] - 1;
            } else if (era.CompareTo("平成") == 0) {
                year += (int)mKaigen[1] - 1;
            } else if (era.CompareTo("昭和") == 0) {
                year += (int)mKaigen[2] - 1;
            } else if (era.CompareTo("大正") == 0) {
                year += (int)mKaigen[3] - 1;
            } else if (era.CompareTo("明治") == 0) {
                year += (int)mKaigen[4] - 1;
            }
            return date2JulianDay(year, month, day);
        }


        /// <summary>
        /// 令和年を西暦年に変換(yy.mmdd → yyyy.mmdd)
        /// </summary>
        /// <param name="reiwa">令和年</param>
        /// <returns>西暦年</returns>
        private double reiwa2Date(double reiwa)
        {
            double dyear = (int)mKaigen[0] + (int)reiwa - 1;
            return dyear + (reiwa % 1);
        }

        /// <summary>
        /// 平成年を西暦年に変換(yy.mmdd → yyyy.mmdd)
        /// </summary>
        /// <param name="heisei">平成年</param>
        /// <returns>西暦年</returns>
        private double heisei2Date(double heisei)
        {
            double dyear = (int)mKaigen[1] + (int)heisei - 1;
            return dyear + (heisei % 1);
        }

        /// <summary>
        /// 昭和年を西暦年に変換(yy.mmdd → yyyy.mmdd)
        /// </summary>
        /// <param name="shyouwa">昭和年</param>
        /// <returns>西暦年</returns>
        private double shyouwa2Date(double shyouwa)
        {
            double dyear = (int)mKaigen[2] + (int)shyouwa - 1;
            return dyear + (shyouwa % 1);
        }

        /// <summary>
        /// 大正年を西暦年に変換(yy.mmdd → yyyy.mmdd)
        /// </summary>
        /// <param name="taisyou">大正年</param>
        /// <returns>西暦年</returns>
        private double taisyou2Date(double taisyou)
        {
            double dyear = (int)mKaigen[3] + (int)taisyou - 1;
            return dyear + (taisyou % 1);
        }

        /// <summary>
        /// 明治年を西暦年に変換(yy.mmdd → yyyy.mmdd)
        /// </summary>
        /// <param name="meiji">明治年</param>
        /// <returns>西暦年</returns>
        private double meiji2Date(double meiji)
        {
            double dyear = (int)mKaigen[4] + (int)meiji - 1;
            return dyear + (meiji % 1);
        }


        //  ---  ファイル・ディレクトリ関連  ------

        /// <summary>
        /// フォルダの選択ダイヤログの表示
        /// </summary>
        /// <param name="initFolder">初期ディレクトリ</param>
        /// <returns></returns>
        public string folderSelect(string initFolder)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "フォルダの選択";
            dlg.SelectedPath = initFolder;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                return dlg.SelectedPath;
            return "";
        }

        /// <summary>
        /// ファイル選択ダイヤログ(複数の拡張子を指定できる)
        /// 例 ext = "MP3ファイル(*.mp3)|*.mp3|FLACファイル(*.flac)|*.flac|全てのファイル (*.*)|*.*"
        /// </summary>
        /// <param name="searchFolder"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public string filesSelect(string searchFolder, string ext)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = ext;
            dialog.Multiselect = true;
            if (0 < searchFolder.Length)
                dialog.InitialDirectory = searchFolder;

            // ダイアログを表示する
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                foreach (string filepath in dialog.FileNames) {
                    //  ファイルをリストに追加する
                    return filepath;
                }
            }
            return "";
        }

        /// <summary>
        /// ファイル選択ダイヤログ
        /// </summary>
        /// <param name="searchFolder">検索フォルダ</param>
        /// <param name="ext">拡張子(ex. mp3)</param>
        /// <returns>検索ファイルパス</returns>
        public string fileSelect(string searchFolder, string ext)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = ext + "ファイル (*." + ext + ")|*." + ext + "|全てのファイル (*.*)|*.*";
            dialog.Multiselect = true;
            if (0 < searchFolder.Length)
                dialog.InitialDirectory = searchFolder;

            // ダイアログを表示する
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                foreach (string filepath in dialog.FileNames) {
                    //  ファイルをリストに追加する
                    return filepath;
                }
            }
            return "";
        }

        /// <summary>
        /// ファイル選択ダイヤログ
        /// </summary>
        /// <param name="searchFolder">検索フォルダ</param>
        /// <param name="exts">拡張子リスト(. なし)</param>
        /// <returns>選択ファイルパスリスト</returns>
        public List<string> fileSelect(string searchFolder, List<string> exts)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();

            // ファイルの種類を設
            string filter = "";
            foreach (string ext in exts) {
                filter += ext + "ファイル (*." + ext + ")|*." + ext + "|";
            }
            dialog.Filter = filter + "全てのファイル (*.*)|*.*";
            dialog.Multiselect = true;
            if (0 < searchFolder.Length)
                dialog.InitialDirectory = searchFolder;

            // ダイアログを表示する
            List<string> files = new List<string>();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                foreach (string filepath in dialog.FileNames) {
                    //  ファイルをリストに追加する
                    files.Add(filepath);
                }
            }
            return files;
        }

        /// <summary>
        /// ファイル選択保存ダイヤログ
        /// </summary>
        /// <param name="searchFolder">初期フォルダ</param>
        /// <param name="ext">拡張子(.なし)</param>
        /// <returns></returns>
        public string saveFileSelect(string searchFolder, string ext)
        {
            // ダイアログのインスタンスを生成
            var dialog = new SaveFileDialog();

            // ファイルの種類を設定
            dialog.Filter = ext + "ファイル (*." + ext + ")|*." + ext + "|全てのファイル (*.*)|*.*";
            if (0 < searchFolder.Length)
                dialog.InitialDirectory = searchFolder;

            // ダイアログを表示する
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                return dialog.FileName;
            }
            return "";
        }

        /// <summary>
        /// 指定されたパスからファイルリストを作成
        /// 指定されたpathからフォルダ名とワイルドカードのファイル名に分けて
        /// ファイルリストを取得する
        /// 例 path = D:\folder\*.flac
        /// </summary>
        /// <param name="path">パス名</param>
        /// <returns>ファイルリスト</returns>
        public string[] getFiles(string path)
        {
            try {
                string folder = Path.GetDirectoryName(path);
                string ext = Path.GetFileName(path);
                return Directory.GetFiles(folder, ext);
            } catch (Exception e) {
                return null;
            }
        }

        /// <summary>
        /// 実行ファイルのフォルダを取得(アプリケーションが存在するのディレクトリ)
        /// </summary>
        /// <returns>ディレクトリ</returns>
        public string getAppFolderPath()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// フルパスのディレクトリから最後のフォルダ名を取り出す
        /// </summary>
        /// <param name="folder">ディレクトリ</param>
        /// <returns>抽出フォルダ名</returns>
        public String getLastFolder(String folder)
        {
            return getLastFolder(folder, 1);
        }

        /// <summary>
        /// フルパスのディレクトリから最後のフォルダ名を取り出す
        /// </summary>
        /// <param name="folder">ディレクトリ</param>
        /// <param name="n">後ろから位置 n番目のフォルダ</param>
        /// <returns>抽出フォルダ名</returns>
        public String getLastFolder(String folder, int n)
        {
            String[] buf = folder.Split('\\');
            if (0 < buf.Length)
                return buf[buf.Length - n];
            else
                return "";
        }

        /// <summary>
        /// ワークファイル用のパス名を取得する
        /// </summary>
        /// <param name="fileName">ワークファイルの種類</param>
        /// <returns>ファイルパス</returns>
        public string getWorkFilePath(string fileName)
        {
            return getWorkFilePath(fileName, false);
        }

        /// <summary>
        /// ワークファイル用のパス名を取得する
        /// </summary>
        /// <param name="fileName">ワークファイルの種類</param>
        /// <param name="systemfile">システムファイル</param>
        /// <returns>ファイルパス</returns>
        public string getWorkFilePath(string fileName, bool systemfile)
        {
            return getAppFolderPath() + "\\" + "YDoc" + (systemfile ? "" : "_") + fileName + ".csv";
        }

        /// <summary>
        /// タイトルをつけてCSV形式でListデータをファイルに保存
        /// </summary>
        /// <param name="path">ファイル名パス</param>
        /// <param name="format">タイトル列</param>
        /// <param name="data">Listデータ</param>
        public void saveCsvData(string path, string[] format, List<string[]> data)
        {
            List<string[]> dataList = new List<string[]>();
            dataList.Add(format);
            foreach (string[] v in data)
                dataList.Add(v);

            saveCsvData(path, dataList);
        }

        /// <summary>
        /// CSV形式のファイルを読み込みList<String[]>形式で出力する
        /// title[]が指定されて一行目にタイトルが入っていればタイトルの順番に合わせて取り込む
        /// titleがnullであればそのまま配列に置き換える
        /// </summary>
        /// <param name="filePath">ファイル名パス</param>
        /// <param name="title">タイトルの配列</param>
        /// <returns>取得データ(タイトル行なし)</returns>
        public List<String[]> loadCsvData(string filePath, string[] title)
        {
            //	ファイルデータの取り込み
            List<string[]> fileData = loadCsvData(filePath);
            if (fileData == null)
                return null;

            //	フォーマットの確認(タイトル行の展開)
            int start = 1;
            int[] titleNo = new int[title.Length];
            if (title != null && 0 < fileData.Count) {
                if (fileData[0][0].CompareTo(title[0]) == 0) {
                    //	データの順番を求める
                    for (int n = 0; n < title.Length; n++) {
                        titleNo[n] = -1;
                        for (int m = 0; m < fileData[0].Length; m++) {
                            if (title[n].CompareTo(fileData[0][m]) == 0) {
                                titleNo[n] = m;
                                break;
                            }
                        }
                    }
                    start = 1;
                } else {
                    //  タイトルがない場合そのまま順で追加
                    for (int n = 0; n < title.Length; n++)
                        titleNo[n] = n;
                    start = 0;
                }
            } else {
                return null;
            }

            //  CSVデータを配列にしてListに登録
            List<string[]> listData = new List<string[]>();
            for (int i = start; i < fileData.Count; i++) {
                //  指定のタイトル順に並べ替えて追加
                string[] buffer = new string[title.Length];
                for (int n = 0; n < title.Length; n++) {
                    if (0 <= titleNo[n] && titleNo[n] < fileData[i].Length) {
                        buffer[n] = fileData[i][titleNo[n]];
                    } else {
                        buffer[n] = "";
                    }
                }
                listData.Add(buffer);
            }
            return listData;
        }

        /// <summary>
        /// データをCSV形式でファイルに書き込む
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="csvData">データリスト</param>
        public void saveCsvData(string path, List<string[]> csvData)
        {
            if (0 < csvData.Count) {
                string folder = Path.GetDirectoryName(path);
                if (0 < folder.Length && !Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                using (StreamWriter dataFile = new StreamWriter(path, false, mEncoding[mEncordingType])) {
                    foreach (string[] data in csvData) {
                        string buf = "";
                        for (int i = 0; i < data.Length; i++) {
                            data[i] = data[i] == null ? "" : data[i].Replace("\"", "\\\"");
                            buf += "\"" + data[i] + "\"";
                            if (i != data.Length - 1)
                                buf += ",";
                        }
                        dataFile.WriteLine(buf);
                    }
                    //dataFile.Close(); //  usingの場合は不要 Disposeを含んでいる
                }
            }
        }

        /// <summary>
        /// CSV形式のファイルを読み込む
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>データリスト</returns>
        public List<string[]> loadCsvData(string filePath, bool tabSep = false)
        {
            List<string[]> csvData = new List<string[]>();

            if (File.Exists(filePath)) {
                try {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                        using (StreamReader dataFile = new StreamReader(filePath, mEncoding[mEncordingType])) {
                            csvData.Clear();
                            string line;
                            while ((line = dataFile.ReadLine()) != null) {
                                string[] buf;
                                if (tabSep) {
                                    buf = line.Split('\t');
                                } else {
                                    buf = seperateString(line);
                                }
                                if (0 < buf.Length)
                                    csvData.Add(buf);
                            }
                            //dataFile.Close(); //  usingの場合は不要 Disposeを含んでいる
                        }
                        return csvData;
                    }
                } catch (Exception e) {
                    mError = true;
                    mErrorMessage = e.Message;
                    return null;
                }
            } else {
                mError = true;
                mErrorMessage = filePath + " のファイルが存在しません";
                return null;
            }
        }

        /// <summary>
        /// Listデータを行単位でファイルに保存する
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="listData">Listデータ</param>
        public void saveListData(string path, List<string> listData)
        {
            if (0 < listData.Count) {
                string folder = Path.GetDirectoryName(path);
                if (0 < folder.Length && !Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                using (StreamWriter dataFile = new StreamWriter(path, false,
                    mEncoding[mEncordingType])) {
                    foreach (string data in listData) {
                        dataFile.WriteLine(data);
                    }
                    //                    dataFile.Close(); //  usingの場合は不要 Disposeを含んでいる
                }
            }
        }

        /// <summary>
        /// ファイルデータを行単位でListデータとして取り込む
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>Listデータ</returns>
        public List<string> loadListData(string filePath)
        {
            List<string> listData = new List<string>();

            if (File.Exists(filePath)) {
                using (StreamReader dataFile = new StreamReader(filePath,
                    mEncoding[mEncordingType])) {
                    listData.Clear();
                    string line;
                    while ((line = dataFile.ReadLine()) != null) {
                        listData.Add(line);
                    }
                    //                    dataFile.Close(); //  usingの場合は不要 Disposeを含んでいる
                }
                return listData;
            }
            return null;
        }

        /// <summary>
        /// JSON形式のファイルを読み込む
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>データリスト</returns>
        public List<string[]> loadJsonData(string filePath)
        {
            List<string[]> jsonList = new List<string[]>();
            if (!File.Exists(filePath)) {
                mError = true;
                mErrorMessage = filePath + " のファイルが存在しません";
                return null;
            }
            try {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    using (System.IO.StreamReader dataFile = new StreamReader(filePath,
                       mEncoding[mEncordingType])) {
                        jsonList.Clear();
                        string line;
                        while ((line = dataFile.ReadLine()) != null) {
                            List<string[]> jsonData = splitJson(getJsonDataString(line));
                            if (0 < jsonData.Count) {
                                if (jsonList.Count == 0) {
                                    string[] buf = new string[jsonData.Count];
                                    for (int i = 0; i < jsonData.Count; i++)
                                        buf[i] = jsonData[i][0];
                                    jsonList.Add(buf);
                                    buf = new string[jsonData.Count];
                                    for (int i = 0; i < jsonData.Count; i++)
                                        buf[i] = jsonData[i][1];
                                    jsonList.Add(buf);
                                } else {
                                    string[] buf = new string[jsonData.Count];
                                    for (int i = 0; i < jsonData.Count; i++)
                                        buf[i] = jsonData[i][1];
                                    jsonList.Add(buf);
                                }
                            }
                        }
                        //dataFile.Close(); //  usingの場合は不要 Disposeを含んでいる
                    }
                }
            } catch (Exception e) {
                mError = true;
                mErrorMessage = e.Message;
                return null;
            }

            return jsonList;
        }

        /// <summary>
        /// テキストファイルの保存
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="buffer">ファイルデータ</param>
        public void saveTextFile(string path, string buffer)
        {
            string folder = Path.GetDirectoryName(path);
            if (0 < folder.Length && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            StreamWriter sw = new StreamWriter(path, false, mEncoding[mEncordingType]);
            sw.Write(buffer);
            sw.Close();
        }

        /// <summary>
        /// テキストファイルの読込
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>ファイルデータ</returns>
        public string loadTextFile(string path)
        {
            string buffer = "";
            if (File.Exists(path)) {
                StreamReader sr = new StreamReader(path, mEncoding[mEncordingType]);
                buffer = sr.ReadToEnd();
                sr.Close();
            }
            return buffer;
        }

        /// <summary>
        /// バイナリファイルの読込
        /// </summary>
        /// <param name="path">ファイル名</param>
        /// <param name="size">読込サイズ(省略可)</param>
        /// <returns>読込データ</returns>
        public byte[] loadBinData(string path, int size = 0)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path))) {
                int ret;
                FileInfo fi = new FileInfo(path);
                size = size == 0 ? (int)fi.Length : Math.Min(size, (int)fi.Length);
                byte[] buf = new byte[size];
                ret = reader.Read(buf, 0, size);
                return buf;
            }
        }

        /// <summary>
        /// バイナリデータをファイルに書き込む
        /// </summary>
        /// <param name="path">ファイル名</param>
        /// <param name="buffer">バイナリデータ</param>
        public void saveBinData(string path, byte[] buffer)
        {
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(path))) {
                // 書き込み
                writer.Write(buffer);
            }
        }

        /// <summary>
        /// gzipファイルを解凍する
        /// 拡張子が gz 出ない場合はリネームしてから解凍
        /// </summary>
        /// <param name="ipath"></param>
        /// <param name="opath"></param>
        /// <returns></returns>
        public bool gzipDecompress(string ipath, string opath)
        {
            if (!File.Exists(ipath))
                return false;
            //  gzipファイル化の確認
            byte[] header = loadBinData(ipath, 2);
            if (header[0] != 0x1f || header[1] != 0x8b)
                return false;
            string inpath = "";
            if (!ipath.ToLower().EndsWith(".gz")) {
                inpath = Path.Combine(Path.GetDirectoryName(ipath), Path.GetFileNameWithoutExtension(ipath) + ".gz");
                if (File.Exists(inpath))
                    File.Delete(inpath);
                File.Move(ipath, inpath);
            }

            int num;
            byte[] buf = new byte[1024];                // 1Kbytesずつ処理する

            FileStream inStream                         // 入力ストリーム
              = new FileStream(inpath, FileMode.Open, FileAccess.Read);

            GZipStream decompStream                     // 解凍ストリーム
              = new GZipStream(
                inStream,                               // 入力元となるストリームを指定
                CompressionMode.Decompress);            // 解凍（圧縮解除）を指定

            FileStream outStream                        // 出力ストリーム
              = new FileStream(opath, FileMode.Create);

            using (inStream)
            using (outStream)
            using (decompStream) {
                while ((num = decompStream.Read(buf, 0, buf.Length)) > 0) {
                    outStream.Write(buf, 0, num);
                }
            }
            return true;
        }



        //  ---  数値処理  ---

        /// <summary>
        /// 移動平均を求める
        /// </summary>
        /// <param name="data">データリスト</param>
        /// <param name="pos">データ位置</param>
        /// <param name="nearCount">平均値のデータ数</param>
        /// <param name="center">移動平均の中心合わせ</param>
        /// <returns>指定値の平均値</returns>
        public double movingAverage(List<double> data, int pos, int nearCount, bool center)
        {
            double sum = 0;
            int count = 0;
            int startCount = center ? nearCount / 2 : nearCount;
            for (int i = Math.Max(0, pos - startCount); i < Math.Min(data.Count, pos + startCount); i++) {
                sum += data[i];
                count++;
            }
            return sum / count;
        }

        /// <summary>
        /// 移動平均を行ったデータ配列を求める
        /// </summary>
        /// <param name="data">対象データ</param>
        /// <param name="nearCount">平均値のデータ数</param>
        /// <param name="center">移動平均の中心合わせ</param>
        /// <returns>移動平均データの配列</returns>
        public double[] getMovingAverage(double[] data, int nearCount, bool center)
        {
            int startCount = center ? nearCount / 2 : nearCount;
            int endCount = nearCount - startCount;
            double[] movingAverageData = new double[data.Length];
            for (int i = 0; i < data.Length; i++) {
                movingAverageData[i] = 0;
                int start = Math.Max(i - startCount + 1, 0);
                int end = Math.Min(i + endCount + 1, data.Length);
                for (int j = start; j < end; j++) {
                    movingAverageData[i] += data[j];
                }
                movingAverageData[i] /= end - start;
            }
            return movingAverageData;
        }

        /// <summary>
        /// 最上位桁が1,2,5になるように下側に丸める
        /// </summary>
        /// <param name="val">数値</param>
        /// <returns>丸めた値</returns>
        public double floorStepSize(double val)
        {
            double mag = Math.Floor(Math.Log10(Math.Abs(val)));
            double magPow = Math.Floor(Math.Abs(val) / Math.Pow(10, mag));
            double magMsd = 0;
            if (magPow >= 5)
                magMsd = 5 * Math.Pow(10, mag);
            else if (magPow >= 2)
                magMsd = 2 * Math.Pow(10, mag);
            else
                magMsd = Math.Pow(10, mag);
            return magMsd * Math.Sign(val);
        }


        /// <summary>
        /// グラフ作成時の補助線間隔を求める
        /// 補助線の間隔は1,2,5の倍数
        /// </summary>
        /// <param name="range">補助線の範囲</param>
        /// <param name="targetSteps">補助線の数</param>
        /// <param name="fromBase">基底数(10進,60進,24進...)</param>
        /// <returns>補助線の間隔</returns>
        public double graphStepSize(double range, double targetSteps, int fromBase = 10)
        {
            // calculate an initial guess at step size
            double tempStep = range / targetSteps;                  //  仮の間隔

            // get the magnitude of the step size
            double mag = Math.Floor(log(tempStep, fromBase));       //  ステップサイズの桁数
            double magPow = Math.Pow(fromBase, mag);                //  最上位数

            // calculate most significant digit of the new step size
            double magMsd = (int)(tempStep / magPow + 0.5);         //  ステップサイズの最上位桁

            // promote the MSD to either 1, 2, or 5
            if (magMsd > fromBase / 2.0)
                magMsd = fromBase;
            else if (magMsd > fromBase / 5.0)
                magMsd = Math.Floor(fromBase / 2.0);
            else if (magMsd > fromBase / 10.0)
                magMsd = Math.Floor(fromBase / 5.0);

            return magMsd * magPow;
        }

        /// <summary>
        /// グラフの最大値を求める
        /// </summary>
        /// <param name="height">データの最大値</param>
        /// <param name="stepSize">目盛間隔</param>
        /// <returns></returns>
        public double graphHeightSize(double height, double stepSize)
        {
            //  グラフ高さの調整
            if (height < 0)
                return ((int)(height / stepSize) - 1) * stepSize;
            else
                return ((int)(height / stepSize) + 1) * stepSize;
        }

        /// <summary>
        /// 底指定の対数(log b a)
        /// </summary>
        /// <param name="a">数値</param>
        /// <param name="b">底(10進,60進とか)</param>
        /// <returns></returns>
        public double log(double a, double b)
        {
            return Math.Log10(a) / Math.Log10(b);
        }

        /// <summary>
        /// 指定の有効桁で切り捨てる
        /// </summary>
        /// <param name="val">数値</param>
        /// <param name="n">丸める有効桁</param>
        /// <returns>丸めた値</returns>
        public float roundFloor(float val, int n)
        {
            if (val == 0f)
                return val;
            float sign = Math.Sign(val);
            if (val < 0)
                val *= sign;
            float mag = (float)Math.Floor(Math.Log10(val));     //  桁数を求める
            float magPow = (float)Math.Pow(10, mag - n + 1);    //  
            return (float)Math.Floor(val * sign / magPow) * magPow;
        }

        /// <summary>
        /// 指定の有効桁で切り上げる
        /// </summary>
        /// <param name="val">数値</param>
        /// <param name="n">丸める有効桁</param>
        /// <returns>丸めた値</returns>
        public float roundCeil(float val, int n)
        {
            if (val == 0f)
                return val;
            float sign = Math.Sign(val);
            if (val < 0)
                val *= sign;
            float mag = (float)Math.Floor(Math.Log10(val));
            float magPow = (float)Math.Pow(10, mag - n + 1);
            return (float)Math.Ceiling(val * sign / magPow) * magPow;
        }

        /// <summary>
        /// 指定の有効桁数で四捨五入する
        /// </summary>
        /// <param name="val">数値</param>
        /// <param name="n">丸める有効桁数</param>
        /// <returns>丸めた値</returns>
        public float roundRound(float val, int n)
        {
            if (val == 0f)
                return val;
            float sign = Math.Sign(val);
            if (val < 0)
                val *= sign;
            float mag = (float)Math.Floor(Math.Log10(val));
            float magPow = (float)Math.Pow(10, mag - n + 1);
            return (float)Math.Round(val * sign / magPow) * magPow;
        }

        public float roundRound2(float val, int n)
        {
            if (val == 0f)
                return val;
            float sign = Math.Sign(val);
            if (val < 0)
                val *= sign;
            float magPow = (float)Math.Pow(10, n);
            return (float)Math.Round(val * sign / magPow) * magPow;
        }

        /// <summary>
        /// 度分秒表示の抽出するための正規表現パターン
        /// </summary>
        private string[] mCoordinatePattern = {
                    "北緯(.*?)度(.*?)分(.*?)秒.*?東経(.*?)度(.*?)分(.*?)秒",
                    "北緯(.*?)度(.*?)分.*?東経(.*?)度(.*?)分",
                    "北緯(.*?)度.*?東経(.*?)度",
                };

        /// <summary>
        /// 度分秒表示の座標があればその文字列を返す
        /// </summary>
        /// <param name="coordinate">対象文字列</param>
        /// <returns>検索文字列</returns>
        public string getCoordinatePattern(string coordinate)
        {
            foreach (string pattern in mCoordinatePattern) {
                List<string[]> datas = getPattern(coordinate.Trim(), pattern);
                if (0 < datas.Count) {
                    return datas[0][0];
                }
            }
            return "";
        }

        /// <summary>
        /// 度分秒表示の座標を度に変換する(IndexOf使うことで正規表現より早い)
        /// 北緯45度22分21秒 東経141度00分57秒 → 緯度 45.3725 経度 141.015833333333 
        /// </summary>
        /// <param name="coordinate">度分秒文字列</param>
        /// <returns>座標値</returns>
        public Point cnvCoordinate(string coordinate)
        {
            char[] stripChars = new char[] { ' ', '.' };
            double latitude = 0.0;
            double longitude = 0.0;
            int a1 = coordinate.IndexOf("北緯");
            int b1 = coordinate.IndexOf("東経");
            if (a1 < 0 || b1 < 0)
                return new Point(0, 0);
            int n = coordinate.IndexOf("北緯", b1);
            if (0 <= n)
                coordinate = coordinate.Substring(0, n);
                int a2 = coordinate.IndexOf("度", a1);
            if (0 <= a2 && a2 < b1) {
                string buf = coordinate.Substring(a1 + 2, a2 - a1 - 2);
                latitude = buf.Length == 0 ? 0 : double.Parse(buf.Trim(stripChars));
                int a3 = coordinate.IndexOf("分", a1);
                if (0 <= a3 && a3 < b1) {
                    buf = coordinate.Substring(a2 + 1, a3 - a2 - 1);
                    latitude += buf.Length == 0 ? 0 : double.Parse(buf.Trim(stripChars)) / 60.0;
                    int a4 = coordinate.IndexOf("秒", a1);
                    if (0 <= a4 && a4 < b1) {
                        buf = coordinate.Substring(a3 + 1, a4 - a3 - 1);
                        latitude += buf.Length == 0 ? 0 : double.Parse(buf.Trim(stripChars)) / 3600.0;
                    }
                }
            }

            int b2 = coordinate.IndexOf("度", b1);
            if (0 <= b2) {
                string buf = coordinate.Substring(b1 + 2, b2 - b1 - 2);
                longitude = buf.Length == 0 ? 0 : double.Parse(buf.Trim(stripChars));
                int b3 = coordinate.IndexOf("分", b1);
                if (0 <= b3) {
                    buf = coordinate.Substring(b2 + 1, b3 - b2 - 1);
                    longitude += buf.Length == 0 ? 0 : double.Parse(buf.Trim(stripChars)) / 60.0;
                    int b4 = coordinate.IndexOf("秒", b1);
                    if (0 <= b4) {
                        buf = coordinate.Substring(b3 + 1, b4 - b3 - 1);
                        longitude += buf.Length == 0 ? 0 : double.Parse(buf.Trim(stripChars)) / 3600.0;
                    }
                }
            }

            return new Point(longitude, latitude);
        }

        /// <summary>
        /// 度分秒表示の座標を度に変換する(正規表現を使う)
        /// 北緯45度22分21秒 東経141度00分57秒 → 緯度 45.3725 経度 141.015833333333 
        /// </summary>
        /// <param name="coordinate">座標</param>
        /// <returns></returns>
        public Point cnvCoordinate2(string coordinate)
        {
            char[] stripChars = new char[] { ' ', '.' };
            List<string[]> datas = getPattern(coordinate.Trim(), mCoordinatePattern[0]);
            if (0 < datas.Count) {
                double latitude = 0.0;
                double longitude = 0.0;
                foreach (string[] data in datas) {
                    if (6 < data.Length) {
                        latitude   = data[1].Length == 0 ? 0 : double.Parse(data[1].TrimEnd(stripChars));
                        latitude  += data[2].Length == 0 ? 0 : double.Parse(data[2].TrimEnd(stripChars)) / 60.0;
                        latitude  += data[3].Length == 0 ? 0 : double.Parse(data[3].TrimEnd(stripChars)) / 3600.0;
                        longitude  = data[4].Length == 0 ? 0 : double.Parse(data[4].TrimEnd(stripChars));
                        longitude += data[5].Length == 0 ? 0 : double.Parse(data[5].TrimEnd(stripChars)) / 60.0;
                        longitude += data[6].Length == 0 ? 0 : double.Parse(data[6].TrimEnd(stripChars)) / 3600.0;
                        break;
                    }
                }

                return new Point(longitude, latitude);
            }
            //  秒なし
            datas = getPattern(coordinate.Trim(), mCoordinatePattern[1]);
            if (0 < datas.Count) {
                double latitude = 0.0;
                double longitude = 0.0;
                foreach (string[] data in datas) {
                    if (4 < data.Length) {
                        latitude   = data[1].Length == 0 ? 0 : double.Parse(data[1].TrimEnd(stripChars));
                        latitude  += data[2].Length == 0 ? 0 : double.Parse(data[2].TrimEnd(stripChars)) / 60.0;
                        longitude  = data[3].Length == 0 ? 0 : double.Parse(data[3].TrimEnd(stripChars));
                        longitude += data[4].Length == 0 ? 0 : double.Parse(data[4].TrimEnd(stripChars)) / 60.0;
                        break;
                    }
                }

                return new Point(longitude, latitude);
            }
            //  分・秒なし
            datas = getPattern(coordinate.Trim(), mCoordinatePattern[2]);
            if (0 < datas.Count) {
                double latitude = 0.0;
                double longitude = 0.0;
                foreach (string[] data in datas) {
                    if (2 < data.Length) {
                        latitude  = data[1].Length == 0 ? 0 : double.Parse(data[1].TrimEnd(stripChars));
                        longitude = data[2].Length == 0 ? 0 : double.Parse(data[2].TrimEnd(stripChars));
                        break;
                    }
                }

                return new Point(longitude, latitude);
            }

            return new Point(0.0, 0.0);
        }


        //	地球上の２地点の緯度・経度を指定して最短距離とその方位角を計算
        //	地球を赤道半径r=6378.137kmを半径とする球体として計算しています。
        //	方位角は北:0度、東:90度、南:180度、西:270度。
        //	地点A(経度x1, 緯度y1)、地点B(経度x2, 緯度y2)
        //	ABの距離(km) d = r*acos(sin(y1)*sin(y2)+cos(y1)*cos(y2)*cos(x2-x1))
        //	方位角　φ = 90 - atan2(sin(x2-x1), cos(y1)*tan(y2) - sin(y1)*cos(x2-x1))
        //	http://keisan.casio.jp/has10/SpecExec.cgi

        /// <summary>
        /// 球面上の2点間座標の距離
        /// </summary>
        /// <param name="longi1">座標1経度</param>
        /// <param name="lati1">座標1緯度</param>
        /// <param name="longi2">座標2経度</param>
        /// <param name="lati2">座標2緯度</param>
        /// <returns>距離(km)</returns>
        public double coordinateDistance(double longi1, double lati1, double longi2, double lati2)
        {
            double r = 6378.137;
            double x1 = longi1 / 180 * Math.PI;
            double y1 = lati1 / 180 * Math.PI;
            double x2 = longi2 / 180 * Math.PI;
            double y2 = lati2 / 180 * Math.PI;
            double dis = r * Math.Acos(Math.Sin(y1) * Math.Sin(y2) + Math.Cos(y1) * Math.Cos(y2) * Math.Cos(x2 - x1));
            return double.IsNaN(dis) ? 0 : dis;
        }

        /// <summary>
        /// 球面上の2点間座標の距離
        /// </summary>
        /// <param name="ps">緯度経度座標</param>
        /// <param name="pe">緯度経度座標</param>
        /// <returns>距離(km)</returns>
        public double coordinateDistance(Point ps, Point pe)
        {
            return coordinateDistance(ps.X, ps.Y, pe.X, pe.Y);
        }


        /// <summary>
        /// 球面上の2点間座標の方位
        /// </summary>
        /// <param name="longi1">座標1経度</param>
        /// <param name="lati1">座標1緯度</param>
        /// <param name="longi2">座標2経度</param>
        /// <param name="lati2">座標2緯度</param>
        /// <returns>方位角(°)</returns>
        public double azimuth(double longi1, double lati1, double longi2, double lati2)
        {
            double x1 = longi1 / 180 * Math.PI;
            double y1 = lati1 / 180 * Math.PI;
            double x2 = longi2 / 180 * Math.PI;
            double y2 = lati2 / 180 * Math.PI;
            double phai = 90 - (Math.Atan2(Math.Sin(x2 - x1), Math.Cos(y1) * Math.Tan(y2) - Math.Sin(y1) * Math.Cos(x2 - x1))) * 180 / Math.PI;
            return phai;
        }

        //    以下の方法は地球を球体とみなして球面三角法で解く簡便なものなので測量には使えませんが、
        //    日本付近の緯度での２点間の距離400kmほどで誤差は0.1度を少し超える程度です。
        //
        //    地点Aの経度と緯度をそれぞれL1,B1とし、
        //    地点Bの経度と緯度をそれぞれL2,B2とし、
        //    地点Aからみた地点Bの方位（真北を0度として東回りにはかった角度）をθとすると以下の手順で
        //    求められます。
        //
        //    Y = cos(B2) * sin(L2 - L1)
        //    X = cos(B1) * sin(B2) - sin(B1) * cos(B2) * cos(L2 - L1)
        //    θ[rad] = atan2(Y, X)
        //    もし θ[rad]<0 なら θ = θ + 2π とし結果を0から2π未満に収めます。
        //    θ[deg] = θ[rad] * 180 / π
        //
        //    ※ 「*」は乗算、「/」は除算、sin()は正弦関数、cos()は余弦関数、
        //    　　atan2(y, x)は逆正接関数（返り値は-2π～+2π）、
        //    　　θ[rad]は弧度法でのラジアン単位の角度、θ[deg]は度単位の角度をそれぞれ表す。
        //    　　経度は東経を「+」西経を「-」、緯度は北緯を「+」南緯を「-」の数として扱います。
        //	http://oshiete.goo.ne.jp/qa/721140.html

        /// <summary>
        /// 球面上の2点間座標の方位
        /// </summary>
        /// <param name="longi1">座標1経度</param>
        /// <param name="lati1">座標1緯度</param>
        /// <param name="longi2">座標2経度</param>
        /// <param name="lati2">座標2緯度</param>
        /// <returns>方位角(°)</returns>
        public double azimuth2(double longi1, double lati1, double longi2, double lati2)
        {
            double l1 = longi1 / 180 * Math.PI;         //	経度
            double b1 = lati1 / 180 * Math.PI;          //	緯度
            double l2 = longi2 / 180 * Math.PI;
            double b2 = lati2 / 180 * Math.PI;
            double Y = Math.Cos(b2) * Math.Sin(l2 - l1);
            double X = Math.Cos(b1) * Math.Sin(b2) - Math.Sin(b1) * Math.Cos(b2) * Math.Cos(l2 - l1);
            double phai = (Math.Atan2(Y, X)) * 180 / Math.PI;
            return phai < 0 ? phai + 360 : phai;
        }

        /// <summary>
        /// 2点間の距離
        /// </summary>
        /// <param name="ps">始点</param>
        /// <param name="pe">終点</param>
        /// <returns>距離(double)</returns>
        public double posDis(Point ps, Point pe)
        {
            return Math.Sqrt((ps.X - pe.X) * (ps.X - pe.X) + (ps.Y - pe.Y) * (ps.Y - pe.Y));
        }

        /// <summary>
        /// 最小公倍数
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public int Lcm(int a, int b)
        {
            return a * b / Gcd(a, b);
        }

        /// <summary>
        /// 最大公約数(ユークリッドの互除法)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public int Gcd(int a, int b)
        {
            if (a < b)
                return Gcd(b, a);
            while (b != 0) {
                var remainder = a % b;
                a = b;
                b = remainder;
            }
            return a;
        }


        //  ---  バイナリデータ処理  ---

        /// <summary>
        /// バイナリデータをコンソール出力する
        /// </summary>
        /// <param name="data">byteデータ</param>
        /// <param name="start">開始位置</param>
        /// <param name="size">サイズ</param>
        /// <param name="comment">コメント</param>
        public void binaryDump(byte[] data, int start, int size, string comment)
        {
            if (0 < comment.Length)
                Console.Write(comment);
            for (int i = start; i < start + size && i < data.Length; i++) {
                if ((i - start) % 16 == 0)
                    Console.Write("\n{0:X3}: {1:X2} ", i, data[i]);
                else
                    Console.Write("{0:X2} ", data[i]);
            }
            Console.WriteLine();
        }

        /// <summary>
        /// byte配列を16進の文字列に変換する
        /// </summary>
        /// <param name="data">byte配列</param>
        /// <param name="start">開始位置</param>
        /// <param name="size">サイズ</param>
        /// <returns>文字列</returns>
        public string binary2HexString(byte[] data, int start, int size)
        {
            string buf = "";
            for (int i = start; i < start + size && i < data.Length; i++) {
                buf += string.Format("{0:X2} ", data[i]);
            }
            return buf;
        }

        /// <summary>
        /// 下位7bitのSynchsafe整数をlongに変換
        /// </summary>
        /// <param name="data">byte配列</param>
        /// <param name="start">開始位置</param>
        /// <returns></returns>
        public long bit7ConvertLong(byte[] data, int start)
        {
            return bit7ConvertLong(data, start, 4);
        }

        /// <summary>
        /// 下位7bitのSynchsafe整数をlongに変換
        /// </summary>
        /// <param name="data">byte配列</param>
        /// <param name="start">開始位置</param>
        /// <param name="size">サイズ</param>
        /// <returns></returns>
        public long bit7ConvertLong(byte[] data, int start, int size)
        {
            long val = 0;
            start--;
            for (int i = start + size; start < i; i--) {
                val *= 128;
                val += data[i] & 0x7f;
            }
            return val;
        }

        /// <summary>
        /// byte配列をlongに変換する
        /// </summary>
        /// <param name="data">byte配列</param>
        /// <param name="start">開始位置</param>
        /// <returns></returns>
        public long bitConvertLong(byte[] data, int start)
        {
            return bitConvertLong(data, start, 4);
        }

        /// <summary>
        /// byte配列をlongに変換する
        /// </summary>
        /// <param name="data">byte配列</param>
        /// <param name="start">開始位置</param>
        /// <param name="size">サイズ</param>
        /// <returns></returns>
        public long bitConvertLong(byte[] data, int start, int size)
        {
            long val = 0;
            start--;
            for (int i = start + size; start < i; i--) {
                val *= 256;
                val += data[i];
            }
            return val;
        }

        /// <summary>
        /// byte配列を逆順でlongに変換する
        /// </summary>
        /// <param name="data">byte配列</param>
        /// <param name="start">開始位置</param>
        /// <param name="size">サイズ</param>
        /// <returns></returns>
        public long bitReverseConvertLong(byte[] data, int start, int size)
        {
            long val = 0;
            for (int i = start ; i < start + size; i++) {
                val *= 256;
                val += data[i];
            }
            return val;
        }

        /// <summary>
        /// byte配列からbit単位で数値を取り出す
        /// </summary>
        /// <param name="data">byte配列</param>
        /// <param name="startBit">開始bit位置</param>
        /// <param name="bitSize">bitサイズ</param>
        /// <returns></returns>
        public long bitConvertBit(byte[] data, int startBit, int bitSize)
        {
            long val = 0;
            //  先頭の未使用部を削除
            int start = startBit / 8;
            int n = startBit % 8;
            if (0 < n) {
                val = data[start] << n;
                val = (val & 0xff) >> n;
                start++;
            }
            //  使用部の値を加算する
            for (int i = start; i < (int)Math.Ceiling((startBit + bitSize) / 8f); i++) {
                val *= 256;
                val += data[i];
            }
            //  末部の未使用部を削除
            n = 8 - (startBit + bitSize) % 8;
            if (0 < n && n < 8) {
                val >>= n;
            }
            return val;
        }

        /// <summary>
        /// nビット目の値を1にする
        /// </summary>
        /// <param name="a">bit配列データ(uint)</param>
        /// <param name="n">nビット目</param>
        /// <returns>^変更後のデータ</returns>
        public uint bitOn(uint a, int n)
        {
            uint b = 1;
            b <<= n;
            return a | b;
        }

        /// <summary>
        /// nビット目の値を0にする
        /// </summary>
        /// <param name="a">bit配列データ(uint)</param>
        /// <param name="n">nビット目</param>
        /// <returns>^変更後のデータ</returns>
        public uint bitOff(uint a, int n)
        {
            uint b = 1;
            b <<= n;
            return a & (~b);
        }

        /// <summary>
        /// nビット目を反転する
        /// </summary>
        /// <param name="a">bit配列データ</param>
        /// <param name="n">nビット目</param>
        /// <returns>変更後のデータ</returns>
        public uint bitRevers(uint a, int n)
        {
            uint b = 1;
            b <<= n;
            return a ^ b;
        }

        /// <summary>
        /// C
        /// </summary>
        /// <param name="a">bit配列データ(uint)</param>
        /// <param name="n">nビット目</param>
        /// <returns>値(0/1)</returns>
        public int bitGet(uint a, int n)
        {
            uint b = 1;
            b <<= n;
            return (int)(0 == (a & b) ? 0 : 1);
        }

        /// <summary>
        /// bitの数を数える(32bitまで)
        /// </summary>
        /// <param name="bits">数値</param>
        /// <returns>bit数</returns>
        public int bitsCount(long bits)
        {
            bits = (bits & 0x55555555) + (bits >> 1 & 0x55555555);
            bits = (bits & 0x33333333) + (bits >> 2 & 0x33333333);
            bits = (bits & 0x0f0f0f0f) + (bits >> 4 & 0x0f0f0f0f);
            bits = (bits & 0x00ff00ff) + (bits >> 8 & 0x00ff00ff);
            return (int)((bits & 0x0000ffff) + (bits >> 16 & 0x0000ffff));
        }

        /// <summary>
        /// bitの数を数える(64bitまで)
        /// </summary>
        /// <param name="bits">数値</param>
        /// <returns>bit数</returns>
        public int bitsCount2(long bits)
        {
            long a = bits & 0xffffffff;
            long b = bits >> 32;
            return bitsCount(a) + bitsCount(b);
        }

        /// <summary>
        /// int データからbyte単位で値を取得
        /// </summary>
        /// <param name="value">int値</param>
        /// <param name="n">n byte目</param>
        /// <returns>byte値</returns>
        public Byte getInt2Byte(int value, int n)
        {
            return (Byte)((value >> (n * 8)) & 0xff);
        }

        /// <summary>
        /// バイナリデータを比較する
        /// </summary>
        /// <param name="a">byte配列 a</param>
        /// <param name="b">byte配列 b</param>
        /// <returns></returns>
        public bool ByteComp(byte[] a, byte[] b)
        {
            if (a.Length == b.Length) {
                for (int i = 0; i < a.Length; i++) {
                    if (a[i] != b[i])
                        return false;
                }
                return true;
            } else
                return false;
        }

        /// <summary>
        /// バイナリデータを比較する
        /// </summary>
        /// <param name="a">byte配列 a</param>
        /// <param name="b">byte配列 b</param>
        /// <param name="size">比較するサイズ</param>
        /// <returns></returns>
        public bool ByteComp(byte[] a, int astart, byte[] b, int bstart, int size)
        {
            if ((a.Length - astart) < size || (b.Length - bstart) < size)
                return false;
            for (int i = 0; i < size; i++) {
                if (a[astart + i] != b[bstart + i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// byteデータをコピーする
        /// </summary>
        /// <param name="a"></param>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public byte[] ByteCopy(byte[] a, int start, int size)
        {
            byte[] b = new byte[size];
            for (int i = 0; i < size; i++)
                b[i] = a[i + start];
            return b;
        }


    }
}

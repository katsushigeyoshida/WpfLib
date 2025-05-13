using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfLib
{
    /// <summary>
    /// 計算式を評価して計算をおこなう。計算式に対して引数を設定して計算を行うこともできる
    /// 引数なしの計算
    ///     expression(計算式) :　計算式(引数なし)の演算処理を行う
    /// 引数ありの計算
    ///     setExpression(計算式)  :　引数ありの計算式を設定する
    ///     getArgKey()             : 設定された計算式から引数名のリストを取得
    ///     setArgData(key,data)    : 引数のデータ値を設定
    ///     replaceArg()            : 引数ありの計算式にデータ値を設定する
    ///     calculate()             : 引数をデータ値に置き換えた計算式を計算する
    /// その他
    ///     stripExpressData()      : 数式以外の文字列の除去
    ///  計算後のエラー
    ///     エラーの有無  mError = true
    ///     エラーの内容  mErrorMsg
    ///     
    /// 計算式のルール
    /// 演算子の優先順位  +,- < *,/,% < ^(べき乗) < 関数 < 括弧
    ///   3+8/4/2  →  3+((8/4)/2)
    /// 計算は左から順におこなう
    ///   8/4/2  →  (8/4)/2      2^3^4  → (2^3)^4
    /// 
    /// 
    /// int setExpression(string str)               計算式を入れて引数を設定
    /// double calculate()                          設定された計算式で計算
    /// int argKeySet(string str)                   計算式から引数([]内)を取り出しHashTableに保存
    /// void setArgValue(string key, string data)   引数リストに値を設定する
    /// Dictionary<string, string> getArgDic()      計算式の引数のHashTableを取り出す
    /// string[] getArgKey()                        引数のキーワードを文字列の配列で取得
    /// string replaceArg()                         引数を数値に置き換えた式を作成する
    /// ----  単独関数  ----
    /// string stripExpressData(string str)         数式文字(数値、予約関数名,括弧でくくられた文字)以外の文字を除く
    /// string expressString(string express, Dictionary<string, string> argList)    数式の変数を変数辞書で置換える
    /// List<string> getArgs(string express)        数式から変数を抽出
    /// Dictionary<string, string> getExpressList(string str)   [a]=xxxxxの等式から引数の数式辞書の作成(;区切の複数式)
    /// Dictionary<string, string> getExpressList(List<string> strList) [a]=xxxxxの等式の数式リストから引数の数式辞書(変数名、数式)の作成
    /// (string key, string val) getExpressArgVal(string express)   式から変数と数式に分割
    /// ----  数式処理関数  ----
    /// List<string> expressList(string str)        文字列を数値と演算子と括弧内文字列に分解してLISTを作る
    /// int getBracketSize(string str, int start)   文字列内の括弧の対を検索しその中の文字数を求める(括弧は含めない)
    /// string getBracketString(string str, int start = 0)  文字列の中の括弧内の文字列を抽出する(括弧は含まない)
    /// int getMonadicstring(string str, int start) 定数または単項演算子の場合のサイズを返す(括弧を含む)
    /// string[] getFuncArgArray(string func)       関数の引数を配列にして取出す
    /// string[] stringSeperate(string str)         文字列を括弧を考慮してカンマで分割して配列で返す(括弧の中は分割しない)
    /// double monadicExpression(string str)        定数と単項演算子の計算
    /// int express3(int i, ref double x, List<string> expList) べき乗のみを優先して計算
    /// int express2(int i, ref double x, List<string> expList) 剰余のみ優先して計算するための関数
    /// double expression(string str)               計算式の実行
    /// ----  算術関数  ----
    /// double asinh(double x)                      逆双曲関数 sinh^-1 = log(x±√(x^2+1))
    /// double acosh(double x)                      逆双曲関数 cosh^-1 = log(x±√(x^2-1))
    /// double atanh(double x)                      逆双曲関数 tanh^-1 = 1/2log((1+x)/(1-x))
    /// int permutation(int n, int r)               順列の組合せの数(nPr)
    /// int combination(int n, int k)               組み合わせの数 n個の中からk個選ぶ nCk
    /// double factorial(int x)                     階乗の計算 n!
    /// double fibonacci(int n)                     フィボナッチ数列を求める
    /// int Lcm(int a, int b)                       最小公倍数
    /// int Gcd(int a, int b)                       最大公約数(ユークリッドの互除法)
    /// double getJD(int nYear, int nMonth, int nDay, int nHour = 0, int nMin = 0, int nSec = 0)    ユリウス日の取得
    /// double getMJD(int nYear, int nMonth, int nDay, int nHour, int nMin, int nSec)   準ユリウス日の取得
    /// double JD2Date(double jd)                   ユリウス日を年月日に変換して yyyymmdd の実数で返す
    /// (int year, int month, int day) JulianDay2Date(double jd)    ユリウス日から年月日を求める
    /// double sum(string express, int n, int k)    式(f(x)のxがnからkまでの合計を求める
    /// double sum(string[]  arg)                   引数の合計を求める
    /// double product(string express, int n, int k)    式(f(x)のxがnからkまでの積を求める
    /// double product(string[] arg)                引数の積を求める
    /// double repeat(string express, double initVal, int n, int k) 式 y = f(x,y) を nからkまで繰り返した結果を求める
    /// double deg2dms(double deg)                  度(時)(ddd.dddd)を度分秒(時分秒)表記(ddd.mmss)にする
    /// double dms2deg(double dms)                  度分秒(時分秒)表記(ddd.mmss)を度(時)(ddd.dddd)にする 
    /// double hour2hms(double hour)                時(hhh.hhhh)を時分秒表記(hh.mmss)にする
    /// double hms2hour(double hms)                 時分秒表記(hh.mmss)を度(時)(hh.hhhh)にする 
    /// double deg2hour(double deg)                 度から時(hh.hhhh)に変換
    /// double hour2deg(double hour)                時(hh.hhhh)から度に変換
    /// double rad2hour(double rad)                 ラジアンから時(hh.hhhh)に変換
    /// double hour2rad(double hour)                時(hh.hhhh)からラジアンに変換
    /// </summary>
    public class YCalc
    {
        //  関数リスト説明用
        public static string[] mFuncList = {
            "PI 円周率",
            "E 自然対数の底",
            "RAD(x) 度をラジアンに変換する",
            "DEG(x) ラジアンを度に変換する",
            "deg2hour(x) 度を時単位に変換する",
            "hour2deg(x) 時単位を度に変換する",
            "rad2hour(x) ラジアンを時単位に変換する",
            "hour2rad(x) 時単位をラジアンに変換する",
            "mod(x,y) 剰余(割算の余り",
            "pow(x,y) 累乗",
            "max(x,y) 大きい方",
            "min(x,y) 小さい方",
            "combi(n,r) 組合せの数(nCr)",
            "permu(n,r) 順列の数(nPr)",
            "sin(x) 正弦",
            "cos(x) 余弦",
            "tan(x) 正接",
            "asin(x) 逆正接",
            "acos(x) 逆余弦",
            "atan(x) 逆正接",
            "atan2(x,y) 逆正接",
            "sinh(x) 双曲線正弦",
            "cosh(x) 双曲線余弦",
            "tanh(x) 双曲線正接",
            "asinh(x) 逆双曲線正弦",
            "acosh(x) 逆双曲線余弦",
            "atanh(x) 逆双曲線正接",
            "exp(x) eの累乗",
            "ln(x) eを底とする自然対数",
            "log(x) 10を底とする対数",
            "log(x,y) xを底とするyの対数",
            "sqrt(x) 平方根",
            "abs(x) 絶対値",
            "ceil(x) 切上げ(x以上で最小の整数値)",
            "floor(x) 切捨て(小数点以下の数の内最大の整数値)",
            "round(x) 四捨五入(もっとも近い整数値)",
            "trunc(x) 浮動小数点の整数部",
            "sign(x) 符号示す値(1/0/-1)",
            "round(x,y) yの倍数に丸める",
            "equals(x,y) 等価判定 x==y ⇒ 1,x!=y ⇒ 0",
            "lt(x,y) 大小判定(less than) x > y ⇒ 1,以外は0",
            "gt(x,y) 大小判定(greater than) x < y ⇒ 1,以外は0",
            "compare(x,y) 大小判定 x > y ⇒ 1,x==y ⇒ 0,x<y ⇒ -1",
            "deg2dms(x) 度(ddd.dddd) → 度分秒(ddd.mmss)",
            "dms2dig(x) 度分秒(ddd.mmss) → 度(ddd.dddd)",
            "hour2hms(x) 時(hh.hhhh) → 時分秒(hh.mmss)",
            "hms2hour(x) 時分秒(hh.mmss) → 時(hh.hhhh)",
            "fact(x) 階乗",
            "fib(x) フィボナッチ数列",
            "gcd(x,y) 最大公約数",
            "lcm(x,y) 最小公倍数",
            "JD(y,m,d) 西暦年月日からユリウス日を求める",
            "MJD(y,m,d) 西暦年月日から準ユリウス日を求める",
            "JD2Date(x) ユリウス日を年月日に変換して yyyymmdd の実数にする",
            "sum(f([@]),n,k) 級数の和 nからkまで連続し値を計算式f([@])で演算した値の合計を求める",
            "sum(f([@]),n1,n2...nm) 級数の和 n1からnmまで値を計算式f([@])で演算した値の合計を求める",
            "product(f([@]),n,k) 級数の積 nからkまで連続し値を計算式f([@])で演算した値の積を求める",
            "product(f([@]),,n1,n2...nm) 級数の積 n1からnmまで値を計算式f([@])で演算した値の積を求める",
            "repeat(f([@],[%]),i,n,k) 計算式の[@]にnからkまで入れて繰返す,[%]に計算結果が入る,iは[%]の初期値",
        };
        //  数式処理で使うキーワード(予約語)
        private string[] mKeyWord = {
            "+", "-", "*", "/", "%", "^", "PI", "E",
            "RAD", "DEG", "deg2hour", "hour2deg", "rad2hour", "hour2rad",
            "mod", "pow", "max", "min", "combi", "permu", "sin", "cos", "tan", "asin", "acos", "atan", "atan2", 
            "sinh", "cosh", "tanh", "asinh", "acosh", "atanh", "exp", "ln", "log", "log", "sqrt",
            "abs", "ceil", "floor", "round", "trunc", "sign", "round",
            "equals", "lt", "gt", "compare", "deg2dms", "dms2dig", "hour2hms", "hms2hour",
            "fact", "fib", "gcd", "lcm", "JD", "MJD", "JD2Date",
            "sum", "sum", "product", "product", "repeat"
        };
        //  内部で使う引数
        public string[] mInnerParameter = {
            "[#]", "[@]", "[%]"
        };
        private string mExpression;
        private Dictionary<string, string> mArgDic;
        public bool mError = false;
        public string mErrorMsg;
        public bool mDebugWrite = false;

        private YLib ylib = new YLib();

        private void debugWrite(string str)
        {
            if (mDebugWrite)
                System.Diagnostics.Debug.WriteLine(str);
        }

        public YCalc()
        {
            mArgDic = new Dictionary<string, string>();
        }

        /// <summary>
        /// 計算式を入れて引数を設定する
        /// </summary>
        /// <param name="str">計算式</param>
        /// <returns>引数の数</returns>
        public int setExpression(string str)
        {
            mExpression = str;
            return argKeySet(mExpression);
        }

        /// <summary>
        /// 設定された計算式で計算する
        /// </summary>
        /// <returns>計算結果</returns>
        public double calculate()
        {
            string express = replaceArg();
            return expression(express);
        }

        /// <summary>
        /// 計算式から引数([]内)を取り出しHashTableに保存(初期かあり)
        /// </summary>
        /// <param name="str">計算式</param>
        /// <returns>引数の数</returns>
        public int argKeySet(string str)
        {
            mArgDic.Clear();
            return argKeyAdd(str);
        }

        /// <summary>
        /// 計算式から引数([]内)を取り出しHashTableに保存(初期化なし)
        /// </summary>
        /// <param name="str">計算式</param>
        /// <returns></returns>
        public int argKeyAdd(string str)
        {
            string buf = "";
            int i = 0;
            while (i < str.Length) {
                if (str[i] == '[') {
                    buf += str[i];
                } else if (str[i] == ']') {
                    buf += str[i];
                    if (!mArgDic.ContainsKey(buf)) {
                        mArgDic.Add(buf, "");
                    }
                    buf = "";
                } else {
                    if (0 < buf.Length)
                        buf += str[i];
                }
                i++;
            }
            return mArgDic.Count;
        }

        /// <summary>
        /// 引数リストに値を設定する
        /// key がない場合は設定されない
        /// </summary>
        /// <param name="key">引数名</param>
        /// <param name="data">引数の値</param>
        public void setArgValue(string key, string data)
        {
            if (mArgDic.ContainsKey(key))
                mArgDic[key] = data;
        }

        /// <summary>
        /// 計算式の引数のHashTableを取り出す
        /// このHashTableを編集して計算式を作り直す
        /// </summary>
        /// <returns>引数のHashTable</returns>
        public Dictionary<string, string> getArgDic()
        {
            return mArgDic;
        }

        /// <summary>
        /// 引数のキーワードを文字列の配列で取得
        /// </summary>
        /// <returns>引数のキーワード</returns>
        public string[] getArgKey()
        {
            string[] keys = new string[mArgDic.Count];
            int i = 0;
            foreach (string key in mArgDic.Keys) {
                keys[i++] = key;
            }
            return keys;
        }

        /// <summary>
        /// 引数を数値に置き換えた式を作成する
        /// </summary>
        /// <returns>引数を置き換えた計算式</returns>
        public string replaceArg()
        {
            string exprrss = mExpression;
            foreach (KeyValuePair<string, string> kvp in mArgDic) {
                //debugWrite(kvp.Key + " " + kvp.Value);
                if (0 < kvp.Value.Length)
                    exprrss = exprrss.Replace(kvp.Key, kvp.Value);
            }
            return exprrss;
        }

        //  ----  単独関数  ----

        /// <summary>
        /// 数式文字(数値、予約関数名,括弧でくくられた文字)以外の文字を除く
        /// </summary>
        /// <param name="str">文字列</param>
        /// <returns>数式文字列</returns>
        public string stripExpressData(string str)
        {
            List<string> list = expressList(str);
            List<string> expList = new List<string>();
            for (int i  = 0; i < list.Count; i++) {
                if (Char.IsNumber(list[i][0])) {
                    expList.Add(list[i]);
                } else if (0 <= "({[\"\'".IndexOf(list[i][0])) {
                    expList.Add(list[i]);
                } else {
                    int p = list[i].IndexOfAny(new char[] { '(', '[', '{' });
                    string buf;
                    if (0 < p)
                        buf = list[i].Substring(0, p);
                    else
                        buf = list[i];
                    if (0 <= mKeyWord.FindIndex(buf))
                        expList.Add(list[i]);
                }
            }
            return string.Join(null, expList);
        }

        /// <summary>
        /// 16進数式文字列の式を10進文字列の式に変換
        /// </summary>
        /// <param name="str">16進数式文字列</param>
        /// <returns>10進数式文字列</returns>
        public string convHexExpressData(string str)
        {
            List<string> list = expressList(str, true);
            List<string> expList = new List<string>();
            for (int i = 0; i < list.Count; i++) {
                if (ylib.isHexNumber(list[i])) {
                    //  16進文字列を10進に変換
                    long hex = ylib.longHexParse(list[i]);
                    expList.Add(hex.ToString());
                } else if (0 <= "({[\"\'".IndexOf(list[i][0])) {
                    expList.Add(list[i]);
                } else {
                    int p = list[i].IndexOfAny(new char[] { '(', '[', '{' });
                    string buf;
                    if (0 < p)
                        buf = list[i].Substring(0, p);
                    else
                        buf = list[i];
                    if (0 <= mKeyWord.FindIndex(buf))
                        expList.Add(list[i]);
                }
            }
            return string.Join(null, expList);
        }

        /// <summary>
        /// 10進数式文字列の式を16進文字列の式に変換
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string dec2hexExpressData(string str)
        {
            List<string> list = expressList(str, true);
            List<string> expList = new List<string>();
            for (int i = 0; i < list.Count; i++) {
                if (ylib.isDecNumber(list[i])) {
                    //  10進文字列を16進に変換
                    long hex = ylib.longParse(list[i]);
                    expList.Add(hex.ToString("X"));
                } else if (0 <= "({[\"\'".IndexOf(list[i][0])) {
                    expList.Add(list[i]);
                } else {
                    int p = list[i].IndexOfAny(new char[] { '(', '[', '{' });
                    string buf;
                    if (0 < p)
                        buf = list[i].Substring(0, p);
                    else
                        buf = list[i];
                    if (0 <= mKeyWord.FindIndex(buf))
                        expList.Add(list[i]);
                }
            }
            return string.Join(null, expList);
        }

        /// <summary>
        /// 数式の変数を変数辞書で置換える
        /// </summary>
        /// <param name="express">数式文字列</param>
        /// <param name="argList">変数辞書</param>
        /// <returns>置き換え後の数式</returns>
        public string expressString(string express, Dictionary<string, string> argList)
        {
            List<string> args = getArgs(express);
            foreach (var arg in args) {
                if (argList.ContainsKey(arg)) {
                    string argval = argList[arg];
                    if (0 < argval.Length) {
                        string argExpress = expressString(argval, argList);
                        express = express.Replace(arg, argExpress);
                    }
                }
            }

            return express;
        }

        /// <summary>
        /// 数式から変数を抽出
        /// </summary>
        /// <param name="express">数式</param>
        /// <returns>変数リスト</returns>
        public List<string> getArgs(string express)
        {
            List<string> argList = new List<string>();
            int p = 0;
            while (p < express.Length) {
                int ps = express.IndexOf("[", p);
                if (ps < 0) break;
                int pe = express.IndexOf("]", ps);
                if (ps < pe) {
                    string arg = express.Substring(ps, pe - ps + 1);
                    if (!argList.Contains(arg))
                        argList.Add(arg);
                    p = pe + 1;
                } else
                    p = ps + 1;
            }
            return argList;
        }

        /// <summary>
        /// [a]=xxxxxの等式から引数の数式辞書の作成(;区切の複数式)
        /// </summary>
        /// <param name="str">;区切の複数式</param>
        /// <returns>数式辞書</returns>
        public Dictionary<string, string> getArgDic(string str)
        {
            string[] strList = str.Split(';');
            return getArgDic(strList.ToList());
        }

        /// <summary>
        /// [a]=xxxxxの等式の数式リストから引数の数式辞書(変数名、数式)の作成
        /// </summary>
        /// <param name="str">数式リスト</param>
        /// <returns>数式辞書</returns>
        public Dictionary<string, string> getArgDic(List<string> strList)
        {
            Dictionary<string, string> expressList = new Dictionary<string, string>();
            for (int i = 0; i < strList.Count; i++) {
                (string key, string val) = getExpressArgVal(strList[i]);
                if (0 < key.Length && !expressList.ContainsKey(key))
                    expressList.Add(key, val);
            }
            return expressList;
        }

        /// <summary>
        /// 等式から変数と数式に分割
        /// </summary>
        /// <param name="express">等式</param>
        /// <returns>(変数,数式)</returns>
        public (string key, string val) getExpressArgVal(string express)
        {
            int p = express.IndexOf('=');
            if (0 < p) {
                string key = express.Substring(0, p).Trim();
                string val = express.Substring(p + 1, express.Length - p - 1).Trim();
                if (0 < key.Length)
                    return (key, val);
            }
            return ("", "");
        }


        //  ----  数式処理関数 -----

        /// <summary>
        /// 文字列を数値と演算子と括弧内文字列に分解してLISTを作る
        /// 例: 1+23*4+sin(1.57)+(1+2)*5
        ///     →  1,+,23,*,sin(1.57),+,(1+2),*,5
        /// </summary>
        /// <param name="str">計算式文字列</param>
        /// <returns>List配列</returns>
        public List<string> expressList(string str, bool hex = false)
        {
            List<string> expList = new List<string>();
            expList.Clear();
            string buf = "";
            for (int i = 0; i < str.Length; i++) {
                if (!hex && (Char.IsNumber(str[i]) || str[i] == '.' ||
                    (i == 0 && str[i] == '-') ||
                    (0 < i && (str[i] == 'E' || str[i] == 'e') && Char.IsNumber(str[i - 1])) ||
                    (0 < i && (str[i - 1] == 'E' || str[i - 1] == 'e') && (str[i] == '-' || str[i] == '+')))) {
                    //  数値
                    buf += str[i];
                } else if (hex &&  ylib.isHexNumber(str[i])) {
                    //  16進数値
                    buf += str[i];
                } else if (str[i] == ' ') {
                    //  空白は読み飛ばす
                } else {
                    if (0 < buf.Length) {
                        //  バッファの文字列をリストに格納
                        expList.Add(buf);
                        buf = "";
                    }
                    if (str[i] == '(' || str[i] == '[') {
                        //  括弧内の文字列を格納(括弧を含む)
                        int n = getBracketSize(str, i, str[i]);
                        buf = str.Substring(i, n + 2);
                        expList.Add(buf);
                        buf = "";
                        i += n + 1;
                    } else if (str[i] == '+' || str[i] == '-' || str[i] == '*' ||
                               str[i] == '/' || str[i] == '%' || str[i] == '^') {
                        //  2項演算子を格納
                        expList.Add(str[i].ToString());
                    } else if (Char.IsLetter(str[i])) {     //  アルファベットの確認(a-z,A-Z,全角も)
                        //  定数または単項演算子を格納
                        int n = getMonadicString(str, i);
                        if (0 < n) {
                            expList.Add(str.Substring(i, n));
                            i += n - 1;
                        }
                    } else {
                        //  上記以外の記号は無視
                    }
                }
            }
            if (0 < buf.Length)
                expList.Add(buf);
            return expList;
        }

        /// <summary>
        /// 文字列内の括弧の対を検索しその中の文字数を求める(括弧は含めない)
        /// 括弧の対が見つからなかった場合は0を返す
        /// 例： express(sin(23)+5)+25
        /// </summary>
        /// <param name="str">文字式</param>
        /// <param name="start">開始位置</param>
        /// <param name="braket">括弧の種類</param>
        /// <returns>括弧内の文字数</returns>
        public int getBracketSize(string str, int start, char bracket = '(')
        {
            char[] startBracket = { '(', '{', '[', '<', '\"', '\'' };
            char[] endBracket =   { ')', '}', ']', '>', '\"', '\'' };
            int bracketType = startBracket.FindIndex(bracket);
            if (0 > bracketType) {
                mError = true;
                mErrorMsg = "括弧ではない";
                return 0;
            }

            int bracketCount = 0;
            int startPos = 0;
            for (int i = start; i < str.Length; i++) {
                if (str[i] == startBracket[bracketType]) {
                    bracketCount++;
                    if (bracketCount == 1)
                        startPos = i;
                } else if (str[i] == endBracket[bracketType]) {
                    bracketCount--;
                    if (bracketCount == 0)
                        return i - startPos - 1;
                }
            }
            if (0 < bracketCount) {
                mError = true;
                mErrorMsg = "括弧があっていない";
            }
            return 0;
        }

        /// <summary>
        /// 文字列の中の括弧内の文字列を抽出する(括弧は含まない)
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="start">検索開始位置(省略時=0)</param>
        /// <returns>抽出した文字列</returns>
        public string getBracketString(string str, int start = 0)
        {
            int m = str.IndexOf("(", start);
            int n = getBracketSize(str, start);
            if (n == 0)
                return "";
            return str.Substring(m + 1, n);
        }

        /// <summary>
        /// 定数または単項演算子の場合のサイズを返す(括弧を含む)
        /// 例: 定数 PI､Eなど+-*/%を末尾として開始位置からの文字数を返す
        /// 　　単項演算子の場合は sin(...)など対括弧の終わりを末尾として文字数を返す
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="start">開始位置</param>
        /// <returns>開始位置からの文字数</returns>
        private int getMonadicString(string str, int start)
        {
            int i = start;
            while (i < str.Length) {
                if (str[i] == '(') {
                    int n = getBracketSize(str, i);
                    return i - start + n + 2;
                } else if (str[i] == '+' || str[i] == '-' || str[i] == '*' ||
                    str[i] == '/' || str[i] == '%' || str[i] == '^' || str[i] == ' ') {
                    return i - start;
                }
                i++;
            }
            return i - start;
        }

        /// <summary>
        /// 関数の引数を配列にして取出す
        /// func(equal([@],0), [2:a:-1]:[13:b:0]);
        /// ⇒  equal([@],0), [2:a:-1]:[13:b:0]
        /// </summary>
        /// <param name="func">関数呼び出し文字列</param>
        /// <returns>引数配列</returns>
        public string[] getFuncArgArray(string func)
        {
            var sp = func.IndexOf("(");
            if (sp < 0)
                return null;
            var ss = getBracketSize(func, 1);
            var arg = func.Substring(sp + 1, ss);
            return stringSeperate(arg);
        }


        /// <summary>
        /// 文字列を括弧を考慮してカンマで分割して配列で返す(括弧の中は分割しない)
        /// 例:  ((12),pow(1,2))
        ///     → (12) , pow(1,2)
        /// </summary>
        /// <param name="str">文字列</param>
        /// <returns>分割した配列</returns>
        public string[] stringSeperate(String str)
        {
            List<String> strList = new List<string>();
            int i = 0;
            int bracketCount = 0;
            string buf = "";
            while (i < str.Length) {
                if (str[i] == '(') {
                    bracketCount++;
                } else if (str[i] == ')') {
                    bracketCount--;
                }
                if (bracketCount == 0 && str[i] == ',') {
                    strList.Add(buf);
                    buf = "";
                } else {
                    buf += str[i];
                }
                i++;
            }
            if (0 < bracketCount) {
                mError = true;
                mErrorMsg = "括弧があっていない";
            }
            if (0 < buf.Length)
                strList.Add(buf);
            string[] strArray = strList.ToArray();
            return strArray;
        }

        /// <summary>
        /// 定数と単項演算子の計算
        /// </summary>
        /// <param name="str">文字列</param>
        /// <returns>計算結果</returns>
        private double monadicExpression(string str)
        {
            double result = 0;
            double x, y, z;
            if (str.IndexOf('(') < 0) {
                //  定数
                if (str.CompareTo("PI") == 0) {
                    result = Math.PI;                       //  円周率
                } else if (str.CompareTo("E") == 0) {
                    result = Math.E;                        //  自然対数の底e
                } else {
                    mError = true;
                    mErrorMsg = "未サポート定数 " + str;
                }
                return result;
            }
            string ope = str.Substring(0, str.IndexOf('('));
            string data = str.Substring(str.IndexOf('(') + 1, str.Length - str.IndexOf('(') - 2);
            //debugWrite(ope+" "+data);
            string[] datas = stringSeperate(data);
            if (0 == datas.Length) {
            } else if (1 == datas.Length) {
                //  引数が1個の単項演算子
                x = expression(datas[0]);
                if (ope.CompareTo("RAD") == 0) {            //  degree→radian
                    result = x * Math.PI / 180d;
                } else if (ope.CompareTo("DEG") == 0) {     //  radian→degree
                    result = x * 180d / Math.PI;
                } else if (ope.CompareTo("deg2hour") == 0) {//  度 → 時
                    result = deg2hour(x);
                } else if (ope.CompareTo("hour2deg") == 0) {//  時 →度
                    result = hour2deg(x);
                } else if (ope.CompareTo("rad2hour") == 0) {//  ラジアン → 時
                    result = rad2hour(x);
                } else if (ope.CompareTo("hour2rad") == 0) {//  時 → ラジアン
                    result = hour2rad(x);
                } else if (ope.CompareTo("deg2dms") == 0) { //  度 → 度分秒
                    result = deg2dms(x);
                } else if (ope.CompareTo("dms2deg") == 0) { //  度分秒 → 度
                    result = dms2deg(x);
                } else if (ope.CompareTo("hour2hms") == 0) { //  時 → 時分秒
                    result = hour2hms(x);
                } else if (ope.CompareTo("hms2hour") == 0) { //  時分秒 → 時
                    result = hms2hour(x);
                } else if (ope.CompareTo("fact") == 0) {    //  階乗
                    result = factorial((int)x);
                } else if (ope.CompareTo("fib") == 0) {     //  フィボナッチ数列
                    result = fibonacci((int)x);
                } else if (ope.CompareTo("sin") == 0) {     //  正弦
                    result = Math.Sin(x);
                } else if (ope.CompareTo("cos") == 0) {     //  余弦
                    result = Math.Cos(x);
                } else if (ope.CompareTo("tan") == 0) {     //  正接
                    result = Math.Tan(x);
                } else if (ope.CompareTo("asin") == 0) {    //  逆正弦
                    result = Math.Asin(x);
                } else if (ope.CompareTo("acos") == 0) {    //  逆余弦
                    result = Math.Acos(x);
                } else if (ope.CompareTo("atan") == 0) {    //  逆正接
                    result = Math.Atan(x);
                } else if (ope.CompareTo("sinh") == 0) {    //  双曲線正弦
                    result = Math.Sinh(x);
                } else if (ope.CompareTo("cosh") == 0) {    //  双曲線余弦
                    result = Math.Cosh(x);
                } else if (ope.CompareTo("tanh") == 0) {    //  双曲線正接
                    result = Math.Tanh(x);
                } else if (ope.CompareTo("asinh") == 0) {    //  逆双曲線正弦
                    result = asinh(x);
                } else if (ope.CompareTo("acosh") == 0) {    //  逆双曲線余弦
                    result = acosh(x);
                } else if (ope.CompareTo("atanh") == 0) {    //  逆双曲線正接
                    result = atanh(x);
                } else if (ope.CompareTo("exp") == 0) {     //  eの累乗値
                    result = Math.Exp(x);
                } else if (ope.CompareTo("ln") == 0) {      //  eを底とする自然対数
                    result = Math.Log(x);
                } else if (ope.CompareTo("log") == 0) {     //  10を底とする対数
                    result = Math.Log10(x);
                } else if (ope.CompareTo("sqrt") == 0) {    //  平方根
                    result = Math.Sqrt(x);
                } else if (ope.CompareTo("abs") == 0) {     //  絶対値
                    result = Math.Abs(x);
                } else if (ope.CompareTo("ceil") == 0) {    //  (切上げ)指定の数以上で最小の整数値
                    result = Math.Ceiling(x);
                } else if (ope.CompareTo("floor") == 0) {   //  (切捨て)小数点以下の数の内最大の整数値
                    result = Math.Floor(x);
                } else if (ope.CompareTo("round") == 0) {   //  (四捨五入)最も近い整数値に丸める
                    result = Math.Round(x);
                } else if (ope.CompareTo("trunc") == 0) {   //  浮動小数点の整数部を返す
                    result = Math.Truncate(x);
                } else if (ope.CompareTo("sign") == 0) {    //  符号を示す値を返す
                    result = Math.Sign(x);
                }　else if (ope.CompareTo("JD2Date") == 0)
                {    //  符号を示す値を返す
                    result = JD2Date(x);
                }
                else {
                    mError = true;
                    mErrorMsg = "未サポート関数 " + ope;
                }
            } else if (2 == datas.Length) {
                //  引数が2個の単項演算子
                x = expression(datas[0]);
                y = expression(datas[1]);
                if (ope.CompareTo("pow") == 0) {            //  べき乗(累乗)
                    result = Math.Pow(x, y);
                } else if (ope.CompareTo("mod") == 0) {     //  剰余
                    result = x % y;
                } else if (ope.CompareTo("round") == 0) {   //  xをｙの単位で丸める
                    result = Math.Floor(x / y) * y;
                } else if (ope.CompareTo("atan2") == 0) {   //  逆正接
                    result = Math.Atan2(x, y);
                } else if (ope.CompareTo("log") == 0) {     //  指定した底の対数
                    result = Math.Log(x, y);
                } else if (ope.CompareTo("max") == 0) {     //  大きい方の値を返す
                    result = Math.Max(x, y);
                } else if (ope.CompareTo("min") == 0) {     //  小さい方の値を返す
                    result = Math.Min(x, y);
                } else if (ope.CompareTo("gcd") == 0) {     //  最大公約数
                    result = Gcd((int)x, (int)y);
                } else if (ope.CompareTo("lcm") == 0) {     //  最小公倍数
                    result = Lcm((int)x, (int)y);
                } else if (ope.CompareTo("combi") == 0) {   //  組合せの数
                    result = combination((int)x, (int)y);
                } else if (ope.CompareTo("permu") == 0) {   //  順列の数
                    result = permutation((int)x, (int)y);
                } else if (ope.CompareTo("equals") == 0) {  //  比較  x == y ⇒ 1, x != y ⇒ 0
                    result = x == y ? 1 : 0;
                } else if (ope.CompareTo("gt") == 0) {      //  比較  x < y ⇒ 1, x >= y ⇒ 0
                    result = x < y ? 1 : 0;
                } else if (ope.CompareTo("lt") == 0) {      //  比較  x > y ⇒ 1, x <= y ⇒ 0
                    result = x > y ? 1 : 0;
                } else if (ope.CompareTo("compare") == 0) { //  比較  x > y ⇒ 1, x == y ⇒ 0, x < y ⇒ -1
                    result = x > y ? 1 : (x < y ? -1 : 0);
                } else {
                    mError = true;
                    mErrorMsg = "未サポート関数 " + ope;
                }
            } else if (3 == datas.Length) {
                //  引数が3個の単項演算子
                //x = expression(datas[1]);
                //y = expression(datas[2]);
                if (ope.CompareTo("sum") == 0) {            //  級数の和
                    result = sum(datas);
                    //result = sum(datas[0], (int)x, (int)y);
                } else if (ope.CompareTo("product") == 0) { //  級数の積
                    result = product(datas);
                    //result = product(datas[0], (int)x, (int)y);
                } else if (ope.CompareTo("JD") == 0) {      //  ユリウス日を求める
                    result = getJD((int)expression(datas[0]), (int)expression(datas[1]), (int)expression(datas[2]));
                } else if (ope.CompareTo("MJD") == 0) {     //  準ユリウス日を求める
                    result = getMJD((int)expression(datas[0]), (int)expression(datas[1]), (int)expression(datas[2]));
                } else {
                    mError = true;
                    mErrorMsg = "未サポート関数 " + ope;
                }
            } else if (4 == datas.Length) {
                //  引数が4個の単項演算子
                if (ope.CompareTo("sum") == 0) {            //  級数の和
                    result = sum(datas);
                } else if (ope.CompareTo("product") == 0) { //  級数の積
                    result = product(datas);
                } else if (ope.CompareTo("repeat") == 0) {  //  繰り返し処理
                    x = expression(datas[1]);
                    y = expression(datas[2]);
                    z = expression(datas[3]);
                    result = repeat(datas[0], x, (int)y, (int)z);
                } else {
                    mError = true;
                    mErrorMsg = "未サポート関数 " + ope;
                }
            } else {
                if (ope.CompareTo("sum") == 0) {            //  級数の和
                    result = sum(datas);
                } else if (ope.CompareTo("product") == 0) { //  級数の積
                    result = product(datas);
                } else {
                    mError = true;
                    mErrorMsg = "不正引数 " + ope;
                }
            }
            return result;
        }

        /// <summary>
        /// べき乗のみを優先して計算
        /// 2^3^4 は (2^3)^4 として左から計算する
        /// </summary>
        /// <param name="i">計算式の位置</param>
        /// <param name="x">計算結果</param>
        /// <param name="expList">計算式の配列リスト</param>
        /// <returns>次の計算式の位置</returns>
        private int express3(int i, ref double x, List<string> expList)
        {
            double y;
            if (i + 2 < expList.Count()) {
                y = expression(expList[i]);
                while (i + 2 < expList.Count()) {
                    string ope = expList[i + 1];
                    double z = expression(expList[i + 2]);
                    debugWrite("express3: " + i + ": " + y + " " + ope + " " + z);
                    if (ope.CompareTo("^") == 0) {
                        x = Math.Pow(y, z);
                    } else {
                        break;
                    }
                    y = x;
                    i += 2;
                }
            }
            return i;
        }

        /// <summary>
        /// 剰余のみ優先して計算するための関数
        /// 一つ先の演算を確認して*/%があれば先に計算する
        /// </summary>
        /// <param name="i">計算式の位置</param>
        /// <param name="x">計算結果</param>
        /// <param name="expList">計算式の配列List</param>
        /// <returns>次の計算式の位置</returns>
        private int express2(int i, ref double x, List<string> expList)
        {
            double y;
            if (i + 2 < expList.Count()) {
                y = expression(expList[i]);
                while (i + 2 < expList.Count()) {
                    string ope = expList[i + 1];
                    double z = expression(expList[i + 2]);
                    debugWrite("express2:" + i + ":" + y + " " + ope + " " + z);
                    if (ope.CompareTo("*") == 0) {
                        i = express3(i, ref z, expList);
                        x = y * z;
                    } else if (ope.CompareTo("/") == 0) {
                        i = express3(i, ref z, expList);
                        if (z == 0d)
                            return -1;
                        x = y / z;
                    } else if (ope.CompareTo("%") == 0) {
                        i = express3(i, ref z, expList);
                        if (x == 0d) {
                            mError = true;
                            mErrorMsg = "0割り";
                            return -1;
                        }
                        x = y % z;
                    } else if (ope.CompareTo("^") == 0) {
                        x = Math.Pow(y, z);
                    } else {
                        break;
                    }
                    y = x;
                    i += 2;
                }
            }
            return i;
        }

        /// <summary>
        /// 計算式の実行
        /// 優先順位  - < ;,- < *,/,% < ^
        /// </summary>
        /// <param name="str">計算式文字列</param>
        /// <returns>計算結果</returns>
        public double expression(string str)
        {
            debugWrite("expression: [" + str + "]");
            List<string> expList;
            mError = false;
            //  文字列を数値と演算子、括弧内の分解リストを作成
            expList = expressList(str);
            //  分解リストを順次計算していく
            double result = 0;
            double x;
            string ope = "";
            int i = 0;
            try {
                while (i < expList.Count()) {
                    bool success = true;
                    if (expList[i][0] == '(') {
                        //  括弧内を計算
                        x = expression(expList[i].Substring(1, expList[i].Length - 2));
                    } else if (Char.IsLetter(expList[i][0])) {
                        //  単項演算子の計算
                        x = monadicExpression(expList[i]);
                    } else {
                        //  数値の判定、数値であればxに返す
                        success = Double.TryParse(expList[i], out x);
                    }
                    //  数値の場合、前の演算子で計算する
                    if (success) {
                        debugWrite("expression: " + i + ": " + result + " " + ope + " " + x);
                        if (ope.CompareTo("+") == 0) {
                            i = express2(i, ref x, expList);     //  剰除が先にあれば計算しておく
                            result += x;
                        } else if (ope.CompareTo("-") == 0) {
                            i = express2(i, ref x, expList);     //  剰除が先にあれば計算しておく
                            result -= x;
                        } else if (ope.CompareTo("*") == 0) {
                            i = express3(i, ref x, expList);     //  べき乗が先にあれば計算しておく
                            result *= x;
                        } else if (ope.CompareTo("/") == 0) {
                            i = express3(i, ref x, expList);     //  べき乗が先にあれば計算しておく
                            if (x == 0d) {
                                mError = true;
                                mErrorMsg = "0割り";
                                return -1;
                            }
                            result /= x;
                        } else if (ope.CompareTo("%") == 0) {
                            i = express3(i, ref x, expList);     //  べき乗が先にあれば計算しておく
                            if (x == 0d)
                                return -1;
                            result %= x;
                        } else if (ope.CompareTo("^") == 0) {
                            result = Math.Pow(result, x);
                        } else {
                            if (0 < i) {
                                mError = true;
                                mErrorMsg = "未演算子";
                            } else
                                result = x;
                        }
                        ope = "";
                    } else {
                        ope = expList[i];
                    }
                    if (i < 0)
                        return -1;
                    i++;
                }
            } catch (Exception e) {
                mError = true;
                mErrorMsg = e.Message;
            }
            return result;
        }

        /// ----  算術関数  ----

        /// <summary>
        /// 逆双曲関数 sinh^-1 = log(x±√(x^2+1))
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double asinh(double x)
        {
            return Math.Log(x + Math.Sqrt(x * x + 1));
        }

        /// <summary>
        /// 逆双曲関数 cosh^-1 = log(x±√(x^2-1))
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double acosh(double x)
        {
            return Math.Log(x + Math.Sqrt(x * x - 1));
        }

        /// <summary>
        /// 逆双曲関数 tanh^-1 = 1/2log((1+x)/(1-x))
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double atanh(double x)
        {
            return Math.Log((1 + x) / (1 - x)) / 2.0;
        }


        /// <summary>
        /// 順列の組合せの数(nPr)
        /// n個の中からr個を取出した時の順列の数
        /// nPr = n(n-1)(n-2)...(n-r+1)
        /// </summary>
        /// <param name="n">全体の数</param>
        /// <param name="r">選択した数</param>
        /// <returns>順列の数</returns>
        public int permutation(int n, int r)
        {
            int result = 1;
            if (1 < n && 1 < r && r < n)
                for (int i = n-r+1; i <= n; i++)
                    result *= i;
            else
                result = 1;
            return result;
        }

        /// <summary>
        /// 組み合わせの数 n個の中からk個選ぶ nCk
        /// nCk = n(n-1)(n-2)...(n-k+1) / 1*2*3....k
        /// </summary>
        /// <param name="n">全体の数</param>
        /// <param name="k">選択する数</param>
        /// <returns>組み合わせの数</returns>
        public int combination(int n, int k)
        {
            if (k == 0 || k == n)
                return 1;
            else
                return combination(n - 1, k - 1) + combination(n - 1, k);
        }

        /// <summary>
        /// 階乗の計算 n!
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double factorial(int x)
        {
            double result = 1;
            if (1 < x) {
                for (int n = 2; n <= x; n++)
                    result *= n;
            } else
                result = 1;
            return result;
        }

        /// <summary>
        /// フィボナッチ数列を求める
        /// f(1) = f(2) =1, f(n+2) = f(n) + f(n+1)
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public double fibonacci(int n)
        {
            if (n <= 2)
                return 1;
            return fibonacci(n - 2) + fibonacci(n - 1);
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

        //  ユリウス日の取得 (https://www.dinop.com/vc/getjd.html)
        //  年月日は西暦、時間はUTC
        public double getJD(int nYear, int nMonth, int nDay, int nHour = 0, int nMin = 0, int nSec = 0)
        {
            //  引数の妥当性はチェックしない
            //  ユリウス日の計算
            if (nMonth == 1 || nMonth == 2) {
                nMonth += 12;
                nYear--;
            }
            double dJD = (double)((int)(nYear * 365.25) + (int)(nYear / 400) -
                (int)(nYear / 100) + (int)(30.59 * (nMonth - 2)) + nDay - 678912 + 2400000.5 +
                (double)nHour / 24 + (double)nMin / (24 * 60) + (double)nSec / (24 * 60 * 60));
            return dJD;
        }

        //  準ユリウス日の取得
        //  年月日はグレゴリオ暦（普通の西暦）、時間はUTCで渡すこと
        public double getMJD(int nYear, int nMonth, int nDay, int nHour = 0, int nMin = 0, int nSec = 0)
        {
            double dJD = getJD(nYear, nMonth, nDay, nHour, nMin, nSec);
            if (dJD == 0.0)
                return 0.0;
            else
                return dJD - 2400000.5;
        }

        /// <summary>
        /// ユリウス日を年月日に変換して yyyymmdd の実数で返す
        /// </summary>
        /// <param name="jd">ユリウス日</param>
        /// <returns>年月日</returns>
        public double JD2Date(double jd)
        {
            (int year, int month, int day) = JulianDay2Date(jd);
            return year * 10000 + month * 100 + day;
        }

        /// <summary>
        /// ユリウス日から年月日を求める
        /// </summary>
        /// <param name="jd">ユリウス日</param>
        /// <returns>(年,月,日)</returns>
        public (int year, int month, int day) JulianDay2Date(double jd)
        {
            int jdc = (int)(jd + 0.5);
            if (jdc >= 2299161)
            {
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
            if (month > 12)
            {
                month -= 12;
                year++;
            }
            return (year, month, day);
        }

        /// <summary>
        /// 式(f(x)のxがnからkまでの合計を求める
        /// 式は[@]を変数として記述し[@]にnからkまでの1づつ増加する値が入る
        /// sum("2*[@]",3,5) ⇒  2*3+2*4+2*5 = 24
        /// </summary>
        /// <param name="express">集計に使う式</param>
        /// <param name="n">開始の変数値</param>
        /// <param name="k">終了の変数値</param>
        /// <returns>計算結果</returns>
        public double sum(string express, int n, int k)
        {
            double result = 0;
            YCalc calc = new YCalc();
            calc.setExpression(express);
            for (int i = n; i <= k; i++) {
                calc.setArgValue("[@]", "(" + i + ")");
                result += calc.calculate();
            }
            return result;
        }

        /// <summary>
        /// 引数の合計を求める
        /// 引数の数が3以下
        ///     sum(f([@],n,k)   nからkまで1づつ増加でf([@])の[@]に代入し合計を求める
        ///     例: sum("2*[@]",3,5) ⇒  2*3+2*4+2*5 = 24
        /// 引数が4以上
        ///     sum(f([@],n1,n2,n3・・・nm)  n1からnmまでを[@]に代入してf([@])の合計を求める
        ///     例: sum([@]^2,3,5,10,2) ⇒  3^2+5^2+10^2+2^2 = 138
        /// arg[0] : f([@])の計算式
        /// </summary>
        /// <param name="arg">引数</param>
        /// <returns>演算結果</returns>
        public double sum(string[]  arg)
        {
            double result = 0;
            YCalc calc = new YCalc();
            calc.setExpression(arg[0]);
            if (arg.Length < 4) {
                int n = (int)expression(arg[1]);
                int k = (int)expression(arg[2]);
                if (n > k) YLib.Swap(ref n, ref k);
                for (int i = n; i <= k; i++) {
                    calc.setArgValue("[@]", "(" + i + ")");
                    result += calc.calculate();
                }
            } else {
                for (int i = 1; i < arg.Length; i++) {
                    calc.setArgValue("[@]", "(" + arg[i] + ")");
                    result += calc.calculate();
                }
            }
            return result;
        }

        /// <summary>
        /// 式(f(x)のxがnからkまでの積を求める
        /// 式は[@]を変数として記述し[@]にnからkまでの1づつ増加する値が入る
        /// product("[@]^2",3,5) ⇒  3^2*4^2+5^2 = 3600
        /// </summary>
        /// <param name="express">集計に使う式</param>
        /// <param name="n">開始の変数値</param>
        /// <param name="k">終了の変数値</param>
        /// <returns>計算結果</returns>
        public double product(string express, int n, int k)
        {
            double result = 1;
            YCalc calc = new YCalc();
            calc.setExpression(express);
            for (int i = n; i <= k; i++) {
                calc.setArgValue("[@]", "(" + i + ")");
                result *= calc.calculate();
            }
            return result;
        }

        /// <summary>
        /// 引数の積を求める
        /// 引数の数が3以下
        ///     product(f([@],n,k)   nからkまで1づつ増加でf([@])の[@]に代入し積を求める
        ///     例: product(2*[@],3,5) ⇒  2*3*2*4*2*5 = 480
        /// 引数が4以上
        ///     product(f([@],n1,n2,n3・・・nm)  n1からnmまでを[@]に代入してf([@])の積を求める
        ///     例: product([@]^2,3,5,10,2) ⇒  3^2*5^2*10^2*2^2 = 90,000
        /// arg[0] : f([@])の計算式
        /// </summary>
        /// <param name="arg">引数</param>
        /// <returns>演算結果</returns>
        public double product(string[] arg)
        {
            double result = 1;
            YCalc calc = new YCalc();
            calc.setExpression(arg[0]);
            if (arg.Length < 4) {
                int n = (int)expression(arg[1]);
                int k = (int)expression(arg[2]);
                if (n > k) YLib.Swap(ref n, ref k);
                for (int i = n; i <= k; i++) {
                    calc.setArgValue("[@]", "(" + i + ")");
                    result *= calc.calculate();
                }
            } else {
                for (int i = 1; i < arg.Length; i++) {
                    calc.setArgValue("[@]", "(" + arg[i] + ")");
                    result *= calc.calculate();
                }
            }
            return result;
        }

        /// <summary>
        /// 式 y = f(x,y) を nからkまで繰り返した結果を求める
        /// result = f([@],[%]),初期値,開始値,終了値)
        /// [@] : 初期値から終了値までの値(増分は1
        /// [%] : 前回の計算結果、初回は初期値が入る
        /// repeat([%]*1.02,10000,1,5) → ((((10000*1.02)*1.02)*1.02)*1.02))*1.02 = 11040.808
        /// </summary>
        /// <param name="express">数式</param>
        /// <param name="initVal">初期値</param>
        /// <param name="n">開始値</param>
        /// <param name="k">終了値</param>
        /// <returns></returns>
        public double repeat(string express, double initVal, int n, int k)
        {
            double result = initVal;
            YCalc calc = new YCalc();
            calc.setExpression(express);
            for (int i = n; i <= k; i++) {
                calc.setArgValue("[@]", "(" + i + ")");
                calc.setArgValue("[%]", "(" + result + ")");
                result = calc.calculate();
            }
            return result;
        }

        /// <summary>
        /// 度(時)(ddd.dddd)を度分秒(時分秒)表記(ddd.mmss)にする
        /// </summary>
        /// <param name="deg">度(ddd.dddddd)</param>
        /// <returns>度分秒(ddd.mmss)</returns>
        public double deg2dms(double deg)
        {
            double tmp = deg;
            double degree = Math.Floor(tmp);
            tmp = (tmp - degree) * 60d;
            double minutes = Math.Floor(tmp);
            tmp = (tmp - minutes) * 60d;
            return degree + minutes / 100d + tmp / 10000d;
        }

        /// <summary>
        /// 度分秒(時分秒)表記(ddd.mmss)を度(時)(ddd.dddd)にする 
        /// </summary>
        /// <param name="dms">度分秒(ddd.mmss)</param>
        /// <returns>度(ddd.ddddd)</returns>
        public double dms2deg(double dms)
        {
            double deg = Math.Floor(dms);
            double tmp = (dms - deg) * 100d;
            double min = Math.Floor(tmp);
            double sec = (tmp - min) * 100d;
            return deg + min / 60d + sec / 3600d;
        }

        /// <summary>
        /// 時(hhh.hhhh)を時分秒表記(hh.mmss)にする
        /// </summary>
        /// <param name="hour">時(hhh.dddddd)</param>
        /// <returns>時分秒(hh.mmss)</returns>
        public double hour2hms(double hour)
        {
            double tmp = hour;
            double degree = Math.Floor(tmp);
            tmp = (tmp - degree) * 60d;
            double minutes = Math.Floor(tmp);
            tmp = (tmp - minutes) * 60d;
            return degree + minutes / 100d + tmp / 10000d;
        }

        /// <summary>
        /// 時分秒表記(hh.mmss)を度(時)(hh.hhhh)にする 
        /// </summary>
        /// <param name="hms">時分秒(hhh.mmss)</param>
        /// <returns>時(hhh.ddddd)</returns>
        public double hms2hour(double hms)
        {
            double deg = Math.Floor(hms);
            double tmp = (hms - deg) * 100d;
            double min = Math.Floor(tmp);
            double sec = (tmp - min) * 100d;
            return deg + min / 60d + sec / 3600d;
        }

        /// <summary>
        /// 度から時(hh.hhhh)に変換
        /// </summary>
        /// <param name="deg">度</param>
        /// <returns>時</returns>
        public double deg2hour(double deg)
        {
            return deg * 24.0 / 360.0;
        }

        /// <summary>
        /// 時(hh.hhhh)から度に変換
        /// </summary>
        /// <param name="hour">時</param>
        /// <returns>度</returns>
        public double hour2deg(double hour)
        {
            return hour * 360.0 /  24.0;
        }

        /// <summary>
        /// ラジアンから時(hh.hhhh)に変換
        /// </summary>
        /// <param name="rad">ラジアン</param>
        /// <returns>時</returns>
        public double rad2hour(double rad)
        {
            return rad * 12.0 / Math.PI;
        }

        /// <summary>
        /// 時(hh.hhhh)からラジアンに変換
        /// </summary>
        /// <param name="hour">時</param>
        /// <returns>ラジアン</returns>
        public double hour2rad(double hour)
        {
            return hour * Math.PI / 12.0;
        }
    }
}

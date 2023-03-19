using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Xml.Linq;
using WpfLib;

namespace WpfLib
{
    /// <summary>
    /// 画像ファイルのExif情報を抽出する
    /// 参考: 画像のExif情報を取得/設定 https://dobon.net/vb/dotnet/graphics/getexifinfo.html
    /// </summary>
    public class ExifInfo
    {
        //  Exif情報
        private Dictionary<int, string> mExifTag = new Dictionary<int, string>() {
            {0x0000,"GPSタグのバージョン"},
            {0x0001,"北緯(N) or 南緯(S)"},
            {0x0002,"緯度(数値)"},
            {0x0003,"東経(E) or 西経(W)"},
            {0x0004,"経度(数値)"},
            {0x0005,"高度の基準"},
            {0x0006,"高度(数値)"},
            {0x0007,"GPS時間(原子時計の時間)"},
            {0x0008,"測位に使った衛星信号"},
            {0x0009,"GPS受信機の状態"},
            {0x000A,"GPSの測位方法"},
            {0x000B,"測位の信頼性"},
            {0x000C,"速度の単位"},
            {0x000D,"速度(数値)"},
            {0x000E,"進行方向の単位"},
            {0x000F,"進行方向(数値)"},
            {0x0010,"撮影した画像の方向の単位"},
            {0x0011,"撮影した画像の方向(数値)"},
            {0x0012,"測位に用いた地図データ"},
            {0x0013,"目的地の北緯(N) or 南緯(S)"},
            {0x0014,"目的地の緯度(数値)"},
            {0x0015,"目的地の東経(E) or 西経(W)"},
            {0x0016,"目的地の経度(数値)"},
            {0x0017,"目的地の方角の単位"},
            {0x0018,"目的の方角(数値)"},
            {0x0019,"目的地までの距離の単位"},
            {0x001A,"目的地までの距離(数値)"},
            {0x001B,"測位方式の名称"},
            {0x001C,"測位地点の名称"},
            {0x001D,"GPS日付"},
            {0x001E,"GPS補正測位"},
            {0x001F,"水平方向測位誤差"},
            {0x0100,"画像の幅"},
            {0x0101,"画像の高さ"},
            {0x0102,"画像のビットの深さ"},
            {0x0103,"圧縮の種類"},
            {0x0106,"画素構成"},
            {0x010E,"画像タイトル"},
            {0x010F,"画像入力機器のメーカー名"},
            {0x0110,"画像入力機器のモデル名"},
            {0x0111,"画像データのロケーション"},
            {0x0112,"画像方向"},
            {0x0115,"コンポーネント数"},
            {0x0116,"1ストリップあたりの行の数"},
            {0x0117,"ストリップの総バイト数"},
            {0x011A,"画像の幅の解像度"},
            {0x011B,"画像の高さの解像度"},
            {0x011C,"画像データの並び"},
            {0x0128,"画像の幅と高さの解像度の単位"},
            {0x012D,"再生階調カーブ特性"},
            {0x0131,"ソフトウェア"},
            {0x0132,"ファイル変更日時"},
            {0x013B,"アーティスト"},
            {0x013E,"参照白色点の色度座標値"},
            {0x013F,"原色の色度座標値"},
            {0x0201,"JPEGのSOIへのオフセット"},
            {0x0202,"JPEGデータのバイト数"},
            {0x0211,"色変換マトリクス係数"},
            {0x0212,"YCCの画素構成(Cの間引き率)"},
            {0x0213,"YCCの画素構成(YとCの位置)"},
            {0x0214,"参照黒色点値と参照白色点値"},
            {0x501B,"サムネイル画像" },
            {0x8298,"撮影著作権者/編集著作権者"},
            {0x829A,"露出時間"},
            {0x829D,"Fナンバー"},
            {0x83BB,"IPTC TAG" },
            {0x8769,"Exifタグ"},
            {0x8822,"露出プログラム"},
            {0x8824,"スペクトル感度"},
            {0x8825,"GPSタグ"},
            {0x8827,"撮影感度"},
            {0x8828,"光電変換関数"},
            {0x8830,"感度種別"},
            {0x8831,"標準出力感度"},
            {0x8832,"推奨露光指数"},
            {0x8833,"ISOスピード"},
            {0x8834,"ISOスピードラチチュードyyy"},
            {0x8835,"ISOスピードラチチュードzzz"},
            {0x9000,"Exifバージョン"},
            {0x9003,"原画像データの生成日時"},
            {0x9004,"デジタルデータの作成日時"},
            {0x9101,"各コンポーネントの意味"},
            {0x9102,"画像圧縮モード"},
            {0x9201,"シャッタースピード"},
            {0x9202,"絞り値"},
            {0x9203,"輝度値"},
            {0x9204,"露光補正値"},
            {0x9205,"レンズ最小Ｆ値"},
            {0x9206,"被写体距離"},
            {0x9207,"測光方式"},
            {0x9208,"光源"},
            {0x9209,"フラッシュ"},
            {0x920A,"レンズ焦点距離"},
            {0x9214,"被写体領域"},
            {0x927C,"メーカノート"},
            {0x9286,"ユーザコメント"},
            {0x9290,"DateTimeのサブセック"},
            {0x9291,"DateTimeOriginalのサブセック"},
            {0x9292,"DateTimeDigitizedのサブセック"},
            {0xA000,"対応フラッシュピックスバージョン"},
            {0xA001,"色空間情報"},
            {0xA002,"実効画像幅"},
            {0xA003,"実効画像高さ"},
            {0xA004,"関連音声ファイル"},
            {0xA005,"互換性IFDへのポインタ"},
            {0xA20B,"フラッシュ強度"},
            {0xA20C,"空間周波数応答"},
            {0xA20E,"焦点面の幅の解像度"},
            {0xA20F,"焦点面の高さの解像度"},
            {0xA210,"焦点面解像度単位"},
            {0xA214,"被写体位置"},
            {0xA215,"露出インデックス"},
            {0xA217,"センサ方式"},
            {0xA300,"ファイルソース"},
            {0xA301,"シーンタイプ"},
            {0xA302,"CFAパターン"},
            {0xA401,"個別画像処理"},
            {0xA402,"露出モード"},
            {0xA403,"ホワイトバランス"},
            {0xA404,"デジタルズーム倍率"},
            {0xA405,"35mm換算レンズ焦点距離"},
            {0xA406,"撮影シーンタイプ"},
            {0xA407,"ゲイン制御"},
            {0xA408,"撮影コントラスト"},
            {0xA409,"撮影彩度"},
            {0xA40A,"撮影シャープネス"},
            {0xA40B,"撮影条件記述情報"},
            {0xA40C,"被写体距離レンジ"},
            {0xA420,"画像ユニークID"},
            {0xA430,"カメラ所有者名"},
            {0xA431,"カメラシリアル番号"},
            {0xA432,"レンズの仕様情報"},
            {0xA433,"レンズのメーカ名"},
            {0xA434,"レンズのモデル名"},
            {0xA435,"レンズシリアル番号"},
            {0xA500,"再生ガンマ"},
        };
        //  ストロボ(0x9209)
        private Dictionary<int, string> mFlashMode = new Dictionary<int, string>() {
            {  1, "起動" },
            {  4, "ストロボ戻りライトなし" },
            {  6, "ストロボ戻りライトあり" },
            {  8, "発光" },
            { 16, "抑制" },
            { 24, "自動" },
            { 32, "なし" },
            { 64, "赤目軽減" },
        };
        //  写真の向き(0x0112)
        private Dictionary<int, string> mOrientation = new Dictionary<int, string>() {
            {1, "水平(0°)" },
            {2, "水平反転" },
            {3, "180°回転" },
            {4, "垂直反転" },
            {5, "水平反転,270°回転" },
            {6, "90°回転" },
            {7, "水平反転,90°回転" },
            {8, "270°回転" }
        };

        private System.Drawing.Bitmap mBitMap;      //  対象ファイルのBitmap
        private string mFilePath = "";

        private YLib ylib = new YLib();
        private YCalc ycalc = new YCalc();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public ExifInfo(string path)
        {
            mFilePath = path;
            //mBitMap = new System.Drawing.Bitmap(path);
            mBitMap = ylib.getBitmap(path);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="bmp">Bitmap</param>
        public ExifInfo(System.Drawing.Bitmap bmp)
        {
            mBitMap = bmp;
        }

        public void close()
        {
            mBitMap.Dispose();
        }

        /// <summary>
        /// ファイルに保存
        /// </summary>
        public void save()
        {
            save(mFilePath);
            //  ファイルが開放されていない場合の対応
            //string path = Path.Combine(Path.GetDirectoryName(mFilePath), "tmpImage.jpg");
            //if (File.Exists(path))
            //    File.Delete(path);
            //save(path);
            //if (File.Exists(path)) {
            //    File.Delete(mFilePath);
            //    if (!File.Exists(mFilePath))
            //        File.Move(path, mFilePath);
            //}
        }

        /// <summary>
        /// JPEGでファイルに保存する
        /// </summary>
        /// <param name="path">保存ファイル名</param>
        public void save(string path)
        {
            mBitMap.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
            mBitMap.Dispose();
        }

        /// <summary>
        /// GPSの座標取得
        /// </summary>
        /// <returns>座標(経度,緯度)</returns>
        public Point getExifGpsCoordinate()
        {
            Point coodinatep = new Point();
            string latitudeSign = getExifInfoId(1);
            string latitudeStr = getExifInfoId(2);     //  緯度
            string longitudeSign = getExifInfoId(3);
            string longitudeStr = getExifInfoId(4);    //  経度
            coodinatep.X = dms2Double(longitudeStr) * (longitudeSign.CompareTo("E") == 0 ? 1 : -1);
            coodinatep.Y = dms2Double(latitudeStr) * (latitudeSign.CompareTo("N") == 0 ? 1 : -1);

            return coodinatep;
        }

        /// <summary>
        /// GPSの座標を変更
        /// </summary>
        /// <param name="coord">座標</param>
        /// <returns></returns>
        public bool setExifGpsCoordinate(Point coord)
        {
            string latiSign = coord.Y < 0 ? "S" : "N";
            if (!setExifStringData2(1, latiSign))
                return false;
            List<int> latiIntList = double2DmsIntList(Math.Abs(coord.Y));
            if (!setExifRatinalData(2, latiIntList))
                return false;
            string longiSign = coord.Y < 0 ? "W" : "E";
            if (!setExifStringData2(3, longiSign))
                return false;
            List<int> longiIntList = double2DmsIntList(Math.Abs(coord.X));
            if (!setExifRatinalData(4, longiIntList))
                return false;
            return true;
        }



        /// <summary>
        /// 写真の向き
        /// </summary>
        /// <returns>向き</returns>
        public string getOrientation()
        {
            int n = ylib.intParse(getExifInfoId(0x0112));
            if (mOrientation.ContainsKey(n)) {
                return mOrientation[n];
            } else {
                return "";
            }
        }

        /// <summary>
        /// フラッシュの状態
        /// </summary>
        /// <returns>状態</returns>
        public string getFlash()
        {
            int n = ylib.intParse(getExifInfoId(0x0112));
            if (mFlashMode.ContainsKey(n))
                return mFlashMode[n];
            else
                return "";
        }

        /// <summary>
        /// 撮影設定の取得
        /// https://www.dinop.com/vc/exif04.html
        /// </summary>
        /// <param name="form">文字列のフォーマット</param>
        /// <returns>撮影設定文字列</returns>
        public string getCameraSetting(string form = "露出時間: 1/{0} s 絞り値: F{1} ISO値: {2} 焦点距離:{3} mm")
        {
            string shutter = getExifInfoId(0x829A);
            if (shutter.Length == 0)
                shutter = "1/(2^("+getExifInfoId(0x9201)+"))";          //  APEX値 ShutterTime = 1/2^APEX
            string shutterSpeed = (1 / ycalc.expression(shutter)).ToString("0");  //  露出
            string FNumStr = getExifInfoId(0x829D);
            if (FNumStr.Length == 0)
                FNumStr = getExifInfoId(0x9202);
            string FNumber = ycalc.expression(FNumStr).ToString("0.0"); //  F値
            string isoNumber = getExifInfoId(0x8827);                   //  ISO値
            string apertureValue =getExifInfoId(0x9202);                //  絞り値
            string focalLength = ycalc.expression(getExifInfoId(0x920A)).ToString("0"); //  レンズの焦点距離
            return string.Format(form, shutterSpeed, FNumber, isoNumber, focalLength);
        }

        /// <summary>
        /// 撮影機材の取得
        /// </summary>
        /// <param name="form">文字列のフォーマット</param>
        /// <returns>データ文字列</returns>
        public string getCamera(string form = "メーカー: {0} モデル: {1}")
        {
            string maker = getExifInfoId(0x010F);   //  メーカー
            string model = getExifInfoId(0x0110);   //  モデル
            return string.Format(form,maker.Trim(), model.Trim());
        }

        /// <summary>
        /// 撮影日時の取得(yyyy:mm:dd hh:mm:ss)
        /// </summary>
        /// <returns>日時</returns>
        public string getDateTime()
        {
            return getExifInfoId(0x9003);
        }

        /// <summary>
        /// ユーザーコメントの取得
        /// </summary>
        /// <returns></returns>
        public string getUserComment()
        {
            return getExifInfoId(0x9286);
        }

        /// <summary>
        /// ユーザーコメントを設定
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public bool setUserComment(string comment)
        {
            return setExifStringData7(0x9286, comment);
        }

        /// <summary>
        /// IPTC情報の取得　??
        /// </summary>
        /// <returns></returns>
        public string getIPTC()
        {
            return getExifInfoId(0x83BB) + "," + getExifInfoId(0x8568);
        }

        /// <summary>
        /// テキスト情報(Type=2)を更新する
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="data">文字列</param>
        /// <returns></returns>
        private bool setExifStringData2(Int32 id, string data)
        {
            int no = getExifIdNo(id);
            System.Drawing.Imaging.PropertyItem item;
            if (0 <= no) {
                item = mBitMap.PropertyItems[no];
            } else {
                item = mBitMap.PropertyItems[0];
                if (item != null)
                    item.Id = id;
            }
            if (item != null) {
                item.Type = 2;
                item.Value = System.Text.Encoding.ASCII.GetBytes(data + '\0');
                item.Len = item.Value.Length;
                mBitMap.SetPropertyItem(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// テキスト情報(Type=7)を更新する
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="data">文字列</param>
        /// <returns></returns>
        private bool setExifStringData7(Int32 id, string comment)
        {
            int no = getExifIdNo(id);
            System.Drawing.Imaging.PropertyItem item;
            if (0 <= no) {
                item = mBitMap.PropertyItems[no];
            } else {
                item = mBitMap.PropertyItems[0];
                if (item != null)
                    item.Id = id;
            }
            if (item != null) {
                item.Type = 7;
                //comment = new string('\0', 8) + comment;
                //item.Value = System.Text.Encoding.GetEncoding("shift_jis").GetBytes(comment);
                item.Value = cnvString2UserComment(comment, "Unicode");
                item.Len = item.Value.Length;
                mBitMap.SetPropertyItem(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 整数リスト(Type=5)を更新する
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="valList">整数リスト</param>
        /// <returns></returns>
        private bool setExifRatinalData(Int32 id, List<Int32> valList)
        {
            int no = getExifIdNo(id);
            if (0 <= no) {
                //  更新
                System.Drawing.Imaging.PropertyItem item = mBitMap.PropertyItems[no];
                if (item != null && item.Type == 5) {
                    item.Value = YLib.intList2ByteArray(valList);
                    item.Len = item.Value.Length;
                    mBitMap.SetPropertyItem(item);
                    return true;
                }
            } else {
                //  追加
                System.Drawing.Imaging.PropertyItem item = mBitMap.PropertyItems[0];
                if (item != null) {
                    item.Id = id;
                    item.Type = 5;
                    item.Value = YLib.intList2ByteArray(valList);
                    item.Len = item.Value.Length;
                    mBitMap.SetPropertyItem(item);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 指定IDの位置番号を取得する
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>位置番号</returns>
        private int getExifIdNo(Int32 id)
        {
            for (int i = 0; i < mBitMap.PropertyItems.Length; i++) {
                System.Drawing.Imaging.PropertyItem item = mBitMap.PropertyItems[i];
                if (item != null && id == item.Id) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// EXIFのIDを指定してデータを取得
        /// </summary>
        /// <param name="id">ID番号</param>
        /// <returns>データ文字列</returns>
        private string getExifInfoId(Int32 id)
        {
            int idNo = getExifIdNo(id);
            if (0 <= idNo) {
                System.Drawing.Imaging.PropertyItem item = mBitMap.PropertyItems[idNo];
                return getExifData(item);
            }
            return "";
        }


        /// <summary>
        /// EXif情報の全取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string getExifInfoAll()
        {
            string buf = "Exif情報";
            for (int i = 0; i < mBitMap.PropertyItems.Length; i++) {
                System.Drawing.Imaging.PropertyItem item = mBitMap.PropertyItems[i];
                //if (item != null) {
                if (item != null && item.Id != 0x501B && mExifTag.ContainsKey(item.Id)) {   //  0x501Bはサムネイルデータ
                    string key = mExifTag.ContainsKey(item.Id) ? mExifTag[item.Id] : item.Id.ToString();
                    string val = getExifData(item);
                    buf += "\n" + $"{i.ToString("00")}: {item.Id.ToString("X4")} [{item.Type}] {key} [{item.Len}] {val}";
                }
            }
            return buf;
        }

        /// <summary>
        /// Itemデータからデータの抽出
        /// http://dsas.blog.klab.org/archives/52123322.html
        /// </summary>
        /// <param name="item">Iten</param>
        /// <returns>データ文字列</returns>
        private string getExifData(System.Drawing.Imaging.PropertyItem item)
        {
            string val = "";
            if (item.Type == 1) {           //  BYTE      8 ビット符号なし整数
                val = YLib.binary2HexString(item.Value, 0, item.Len);
            } else if (item.Type == 2) {    //  ASCII NULL 文字で終端する ASCII 文字列。ASCII のカウントは NULL 文字分を含む
                val = System.Text.Encoding.ASCII.GetString(item.Value);
                val = val.Trim(new char[] { '\0' });
            } else if (item.Type == 3) {    //  SHORT 16 ビット符号なし整数
                val = BitConverter.ToUInt16(item.Value, 0).ToString();
            } else if (item.Type == 4) {    //  LONG  32 ビット符号なし整数
                val = BitConverter.ToUInt32(item.Value, 0).ToString();
            } else if (item.Type == 5) {    //  RATIONAL LONG 2 個で表現する値。ひとつめの LONG は分子、ふたつめは分母を表す
                val = convExifData(item.Value, item.Len);
            } else if (item.Type == 7) {    //  UNDEFINED     任意のバイト列
                if (item.Id == 0x927C) {        //  MakerNote
                } else if (item.Id == 0x9286) { //  UserComment
                    if (8 < item.Len) {
                        val = cnvUserComment2String(item.Value);
                    } else {
                        val = YLib.binary2AsciiString(item.Value, 0, item.Len);
                    }
                } else if (item.Len == 1) {
                    val = item.Value[0].ToString();
                } else if (item.Value[1] != 0) { 
                    val = YLib.binary2AsciiString(item.Value, 0, item.Len);
                } else {
                    val = System.Text.Encoding.Unicode.GetString(item.Value);
                }
            } else if (item.Type == 9) {    //  SLONG     32 ビット符号つき整数
                val = BitConverter.ToInt32(item.Value, 0).ToString();
            } else if (item.Type == 10) {   //  SRATIONAL SLONG 2 個で表現する値。ひとつめの LONG は分子、ふたつめは分母を表す
                val = convExifData(item.Value, item.Len);
            } else {
                val = YLib.binary2HexString(item.Value, 0, item.Len);
            }
            return val;
        }

        /// <summary>
        /// 度分秒を度に変換
        /// 例 緯度  "35/1,32/1,48000/1000" → 35.5466667
        /// </summary>
        /// <param name="dms">度分秒文字列</param>
        /// <returns>度</returns>
        private double dms2Double(string dms)
        {
            dms = dms.Replace(" ", "");
            string[] dmsArray = dms.Split(',');
            double val = 0;
            if (2 < dmsArray.Length) {
                val = ycalc.expression(dmsArray[0]);
                val += ycalc.expression(dmsArray[1]) / 60.0;
                val += ycalc.expression(dmsArray[2]) / 3600.0;
            }
            return val;
        }

        /// <summary>
        /// 度を度分秒に変換
        /// </summary>
        /// <param name="dms">度</param>
        /// <returns>度分秒文字列</returns>
        private string double2DmsStr(double dms)
        {
            int digit = (int)Math.Floor(dms);
            int min = (int)(dms % 1.0 * 60);
            int sec = (int)((dms * 60) % 1.0 * 60 * 100);
            return string.Format("{0} / 1 , {1} / 1 , {2} / 100", digit, min, sec);
        }

        /// <summary>
        /// 度を度分秒の整数リストに変換
        /// </summary>
        /// <param name="dms">度</param>
        /// <returns>整数リスト</returns>
        private List<int> double2DmsIntList(double dms)
        {
            int digit = (int)Math.Floor(dms);
            int min = (int)(dms % 1.0 * 60);
            int sec = (int)((dms * 60) % 1.0 * 60 * 100);
            List<int> dmsList = new List<int>() {
                digit, 1, min, 1, sec, 100
            };
            return dmsList;
        }

        /// <summary>
        /// bute配列から2値データ(Type=5)を取り出す
        /// 例 緯度  35/1,32/1,48000/1000
        /// </summary>
        /// <param name="value">byte配列</param>
        /// <param name="length">配列の長さ</param>
        /// <returns>2値データ文字列</returns>
        private string convExifData(byte[] value, int length)
        {
            string buf = "";
            for (int i = 0; i < length; i += 8) {
                int val0 = BitConverter.ToInt32(value, i);
                int val1 = BitConverter.ToInt32(value, i + 4);
                buf += (i == 0 ? "" : " , ") + val0 + " / " + val1;
            }
            return buf;
        }

        /// <summary>
        /// 文字列をExifのUserCommentのbyte配列に変換
        /// </summary>
        /// <param name="comment">文字列</param>
        /// <param name="code">変換コード(Unicode,Shift_JIS...)</param>
        /// <returns>ExifのUserComment</returns>
        private byte[] cnvString2UserComment(string comment, string code)
        {
            byte[] codeBytes = new byte[8];
            codeBytes = YLib.ByteOverWrite(codeBytes, 0, System.Text.Encoding.GetEncoding("ASCII").GetBytes(code));
            byte[] commentBytes = YLib.ByteCat(codeBytes, System.Text.Encoding.GetEncoding(code).GetBytes(comment));
            return commentBytes;
        }

        /// <summary>
        /// ExifのUserCommentから文字列に変換
        /// </summary>
        /// <param name="userComment">ExifのUserComment</param>
        /// <returns>文字列</returns>
        private string cnvUserComment2String(byte[] userComment)
        {
            try {
                if (8 < userComment.Length) {
                    string code = YLib.binary2AsciiString(userComment, 0, 8);
                    return System.Text.Encoding.GetEncoding(code).GetString(YLib.ByteCopy(userComment, 8, userComment.Length - 8));
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine("cnvUserComment2String: " + e.Message);
            }
            return "";
        }

    }
}

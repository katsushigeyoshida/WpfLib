using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace WpfLib
{
    /// <summary>
    /// GPXファイルから日時、緯度経度、高度データを取得する
    /// </summary>
    public class GpxReader
    {
        private Encoding[] mEncoding = { Encoding.UTF8, Encoding.GetEncoding("shift_jis"), Encoding.GetEncoding("euc-jp") };
        private int mEncordingType = 0;     //  UTF8
        private List<string> mListData = new List<string>();
        public enum DATATYPE { gpxData, gpxSimpleData };
        public DATATYPE mDataType = DATATYPE.gpxSimpleData;
        public List<GpsData> mListGpsData = new List<GpsData>();    //  GPSデータ(時間/座標/標高)[DATATYPE.gpsData]
        public List<Point> mListGpsPointData;                       //  GPS座標データリスト[DATATYPE.gpsSImpleData]
        public GpsInfoData mGpsInfoData;                            //  gpsデータ情報

        YLib ylib = new YLib();

        public GpxReader()
        {

        }

        /// <summary>
        /// GPXファイルを読み込んで位置情報データを取得(時間/緯度経度/高度)
        /// </summary>
        /// <param name="path">GPXファイルパス</param>
        /// <param name="dataType">データタイプ(2フルデータ/座標データのみ)</param>
        public GpxReader(string path, DATATYPE dataType)
        {
            mDataType = dataType;
            StringBuilder fileData = loadData(path);
            if (fileData != null) {
                setListData(fileData.ToString());   //  ファイルからデータの読み込み
                getGpxData();                       //  ファイルデータから位置情報データリストに変換
            }
        }

        /// <summary>
        /// XMLファイルを読み込んで項目単位でListに格納する(解析の前処理)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public int getFile(string path)
        {
            StringBuilder fileData = loadData(path);
            if (fileData != null) {
                setListData(fileData.ToString());          //  ファイルからデータの読み込み
                return mListData.Count;
            }
            return -1;
        }

        /// <summary>
        /// 解析前のListデータ
        /// </summary>
        /// <returns></returns>
        public List<string> getListData()
        {
            return mListData;
        }

        public int getPointDataCOunt()
        {
            return mListGpsPointData.Count;
        }

        /// <summary>
        /// Listに格納されたGPXデータからGPSの位置情報を取得(現状トラックデータのみ)
        /// </summary>
        private void getGpxData()
        {
            GpsData data = null;
            mGpsInfoData = new GpsInfoData();
            mListGpsPointData = new List<Point>();
            bool gpxOn = false;
            bool trkOn = false;
            bool trksegOn = false;
            //  トラックデータの取得
            for (int i = 0; i < mListData.Count; i++) {
                if (mListData[i].IndexOf("<gpx ") == 0 || mListData[i].IndexOf("<gpx>") == 0) {
                    gpxOn = true;
                } else if (mListData[i].IndexOf("</gpx>") == 0) {
                    gpxOn = false;
                }
                if (gpxOn) {
                    //  TRACKデータの取得開始・終了
                    if (mListData[i].IndexOf("<trk>") == 0) {
                        trkOn = true;
                    } else if (mListData[i].IndexOf("</trk>") == 0) {
                        trkOn = false;
                    }
                }
                if (trkOn) {
                    //  TRACKデータの取得開始・終了
                    if (mListData[i].IndexOf("<trkseg>") == 0) {
                        trksegOn = true;
                    } else if (mListData[i].IndexOf("</trkseg>") == 0) {
                        trksegOn = false;
                    }
                }
                if (trksegOn) {
                    //  TRACKポイントデータの取得開始・終了
                    if (mListData[i].IndexOf("<trkpt ") == 0) {
                        Point loc = new Point(getParameeorDataValue("lon", mListData[i]), getParameeorDataValue("lat", mListData[i]));
                        if (mDataType == DATATYPE.gpxData) {
                            data = new GpsData();
                            data.setLatitude(getParameeorData("lat", mListData[i]));    //  緯度
                            data.setLongitude(getParameeorData("lon", mListData[i]));   //  経度
                        } else if (mDataType == DATATYPE.gpxSimpleData) {
                            mListGpsPointData.Add(loc);                         //  座標データ格納
                        }
                        mGpsInfoData.addDistance(loc);                          //  累積距離
                        mGpsInfoData.extentArea(loc);                              //  領域
                    } else if (mListData[i].IndexOf("<trkpt>") == 0) {
                        if (mDataType == DATATYPE.gpxData) {
                            data = new GpsData();
                        }
                    } else if (mListData[i].IndexOf("<ele>") == 0) {
                        i++;
                        if (mDataType == DATATYPE.gpxData) {
                            data.setElevator(mListData[i].Trim());
                        }
                        mGpsInfoData.setMinMaxElevator(mListData[i].Trim());    //  最小最大標高
                    } else if (mListData[i].IndexOf("<time>") == 0) {
                        i++;
                        if (mDataType == DATATYPE.gpxData) {
                            data.setDateTime(mListData[i].Trim());
                        }
                        mGpsInfoData.setDateTime(mListData[i].Trim());          //  開始・終了時刻
                    } else if (mListData[i].IndexOf("</trkpt>") == 0) {
                        if (mDataType == DATATYPE.gpxData) {
                            if (data != null)
                                mListGpsData.Add(data);                         //  GPSデータ格納
                        }
                    } else if (mListData[i].IndexOf("</trkseg>") == 0) {
                        break;
                    } else if (mListData[i].IndexOf("</trk>") == 0) {
                        break;
                    } else if (mListData[i].IndexOf("</gpx>") == 0) {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// エラーデータをチェックし、あれば削除する
        /// データ間で1km以上離れている点を除外、mGpsInfoも再取得
        /// </summary>
        public void dataChk()
        {
            if (mDataType == DATATYPE.gpxData) {
                if (2 < mListGpsData.Count) {
                    List<GpsData> gpsDataList = new List<GpsData>();
                    double dis0 = mListGpsData[0].distance(mListGpsData[1]);
                    double dis1 = mListGpsData[0].distance(mListGpsData[2]);
                    if (dis0 < 1.0 || dis1 < 1.0)
                        gpsDataList.Add(mListGpsData[0]);
                    for (int i = 1; i < mListGpsData.Count - 1; i++) {
                        dis1 = mListGpsData[i].distance(mListGpsData[i + 1]);
                        if (dis0 < 1.0 || dis1 < 1.0)
                            gpsDataList.Add(mListGpsData[i]);
                        dis0 = dis1;
                    }
                    int n = mListGpsData.Count - 1;
                    dis0 = mListGpsData[n].distance(mListGpsData[n - 1]);
                    dis1 = mListGpsData[n].distance(mListGpsData[n - 2]);
                    if (dis0 < 1.0 || dis1 < 1.0)
                        gpsDataList.Add(mListGpsData[n]);
                    mListGpsData = new List<GpsData>(gpsDataList);
                    mGpsInfoData.setData(mListGpsData);
                }
            }  else if (mDataType == DATATYPE.gpxSimpleData) {
                if (2 < mListGpsPointData.Count) {
                    List<Point> gpsPointList = new List<Point>();
                    int n = 1;
                    double dis0 = ylib.coordinateDistance(mListGpsPointData[0], mListGpsPointData[1]);
                    double dis1 = ylib.coordinateDistance(mListGpsPointData[1], mListGpsPointData[2]);
                    if (dis0 < 1.0 || dis1 < 1.0)
                        gpsPointList.Add(mListGpsPointData[0]);
                    for (int i = 1; i < mListGpsPointData.Count - 1; i++) {
                        dis1 = ylib.coordinateDistance(mListGpsPointData[i], mListGpsPointData[i + 1]);
                        if (dis0 < 1.0 || dis1 < 1.0)
                            gpsPointList.Add(mListGpsPointData[i]);
                        dis0 = dis1;
                    }
                    n = mListGpsPointData.Count - 1;
                    dis0 = ylib.coordinateDistance(mListGpsPointData[n], mListGpsPointData[n - 1]);
                    dis1 = ylib.coordinateDistance(mListGpsPointData[n], mListGpsPointData[n - 2]);
                    if (dis0 < 1.0 || dis1 < 1.0)
                        gpsPointList.Add(mListGpsPointData[n]);
                    mListGpsPointData = new List<Point>(gpsPointList);
                    mGpsInfoData.setTotalDistance(mListGpsPointData);
                    mGpsInfoData.setArea(mListGpsPointData);

                }
            }
        }

        /// <summary>
        /// 時刻から座標を取得する
        /// </summary>
        /// <param name="datetime">日時</param>
        /// <returns>座標</returns>
        public Point getCoordinate(DateTime datetime)
        {
            Point pos = new Point();
            if (datetime < mListGpsData[0].mDateTime ||
                mListGpsData[mListGpsData.Count - 1].mDateTime < datetime)
                return pos;
            for (int i = 0; i < mListGpsData.Count - 1; i++) {
                if (datetime < mListGpsData[i].mDateTime) {
                    double scale = (double)datetime.Subtract(mListGpsData[i-1].mDateTime).Ticks
                        / (double)mListGpsData[i].mDateTime.Subtract(mListGpsData[i-1].mDateTime).Ticks;
                    Point v = mListGpsData[i-1].getCoordinate().vector(mListGpsData[i].getCoordinate());
                    v.scale(scale);
                    pos = mListGpsData[i].getCoordinate().add(v);
                    break;
                }
            }
            return pos;
        }

        /// <summary>
        /// XMLファイルから読み込んだデータをアイテム単位にListに格納
        /// </summary>
        /// <param name="fileData">XMLファイルデータ文字列</param>
        private void setListData(String fileData)
        {
            String buffer = "";
            bool itemOn = false;
            for (int i = 0; i < fileData.Length; i++) {
                if (fileData[i] == '<') {
                    if (0 < buffer.Length) {
                        mListData.Add(buffer);
                        buffer = "";
                        itemOn = true;
                    }
                    buffer += fileData[i];
                } else if (fileData[i] == '>') {
                    buffer += fileData[i];
                    if (0 < buffer.Length)
                        mListData.Add(buffer.ToString());
                    buffer = "";
                    itemOn = false;
                } else if (!itemOn && fileData[i] == '\r') {
                    if (0 < buffer.Length)
                        mListData.Add(buffer.ToString());
                    buffer = "";
                } else if (itemOn && fileData[i] == '\r') {
                    continue;
                } else if (fileData[i] < 0x20 || (buffer.Length == 0 && fileData[i] == 0x20)) {
                    continue;
                } else if (1 < (fileData.Length - i) &&  fileData[i] == 0x20 && fileData[i+1] == 0x20) {
                    continue;
                } else {
                    buffer += fileData[i];
                }
            }
        }

        /// <summary>
        /// XMLデータをファイルから取り込む
        /// </summary>
        /// <param name="filePath">GPXファイル名</param>
        /// <returns>データ文字列</returns>
        private StringBuilder loadData(string filePath)
        {
            StringBuilder buffer = new StringBuilder();

            if (File.Exists(filePath)) {
                using (System.IO.StreamReader dataFile = new StreamReader(filePath, mEncoding[mEncordingType])) {
                    String line;
                    while ((line = dataFile.ReadLine()) != null) {
                        buffer.Append(line);
                        buffer.Append('\r');    //  改行位置を取得するために追加
                    }
                    //      dataFile.Close(); //  usingの場合は不要 Disposeを含んでいる
                }
                return buffer;
            }
            return null;
        }

        /// <summary>
        /// 指定のアイテムの実数データの取得
        /// </summary>
        /// <param name="para">アイテム名</param>
        /// <param name="data">データ</param>
        /// <returns>抽出実数値</returns>
        private double getParameeorDataValue(string para, string data)
        {
            double v;
            if (double.TryParse(getParameeorData(para, data), out v)) {
                return v;
            } else {
                return 0;
            }
        }

        /// <summary>
        /// アイテム項目からデータの取得
        /// </summary>
        /// <param name="para">パラメータ名</param>
        /// <param name="data">アイテム文字列</param>
        /// <returns>抽出したデータ</returns>
        private string getParameeorData(string para, string data)
        {
            int n = data.IndexOf(para);
            if (0 <= n) {
                int m = data.IndexOf('"', n);
                int l = data.IndexOf('"', m + 1);
                return data.Substring(m + 1, l - m - 2);
            }
            return "";
        }

        /// <summary>
        /// 取得したGPSデータの概要
        /// </summary>
        /// <returns></returns>
        public GpsInfoData getGpsInfoData()
        {
            if (mListGpsData != null) {
                GpsInfoData gpsInfoData = new GpsInfoData();
                gpsInfoData.setData(mListGpsData);
                return gpsInfoData;
            }
            return null;
        }
    }

    /// <summary>
    /// GPSデータ情報
    /// </summary>
    public class GpsInfoData
    {
        public DateTime mFirstTime = new DateTime(0);       //  開始時刻
        public DateTime mLastTime = new DateTime(0);        //  終了時刻
        public double mDistance = 0;                        //  移動距離(km)
        public double mMinElevator = double.MaxValue;       //  最小標高(m)
        public double mMaxElevator = double.MinValue;       //  最大標高
        public Rect mArea = new Rect();                     //  座標の領域

        private bool mAreaOn = false;
        private Point mPrevPoint = new Point(double.NaN, double.NaN);
        YLib ylib = new YLib();

        /// <summary>
        /// データをクリアする
        /// </summary>
        public void Clear()
        {
            mFirstTime = new DateTime(0);           //  開始時刻
            mLastTime = new DateTime(0);            //  終了時刻
            mDistance = 0;                          //  移動距離(km)
            mMinElevator = double.MaxValue;         //  最小標高(m)
            mMaxElevator = double.MinValue;         //  最大標高
            mArea = new Rect();                     //  座標の領域
            mAreaOn = false;
            mPrevPoint = new Point(double.NaN, double.NaN);
        }

        /// <summary>
        /// GPSデータリストから情報を設定する
        /// </summary>
        /// <param name="gpsDataList"></param>
        public void setData(List<GpsData> gpsDataList)
        {
            Clear();
            foreach(GpsData gpsData in gpsDataList) {
                Point p = new Point(gpsData.mLongitude, gpsData.mLatitude);
                setDateTime(gpsData.mDateTime);
                addDistance(p);
                setMinMaxElevator(gpsData.mElevator);
                extentArea(p);
            }
        }

        /// <summary>
        /// データを文字列に変換
        /// </summary>
        /// <returns></returns>
        public string toString()
        {
            string buffer = "";
            TimeSpan spanTime = mLastTime - mFirstTime;
            buffer += "開始時間: " + mFirstTime.ToString("yyyy/MM/dd HH:mm:ss") + 
                    " 終了時間: " + mLastTime.ToString("yyyy/MM/dd HH:mm:ss") +
                    " 経過時間: " + ((spanTime.TotalMinutes < 60.0 * 24.0) ? spanTime.ToString(@"hh\:mm\:ss") : spanTime.ToString(@"d\d\a\y\ hh\:mm\:ss"));
            buffer += "\n移動距離: " + mDistance.ToString("#,##0.## km") + 
                        " 速度: " + (mDistance / spanTime.TotalHours).ToString("##0.# km/s");
            buffer += "\n最大標高: " + mMaxElevator.ToString("#,##0 m") + 
                        " 最小標高: " + mMinElevator.ToString("#,##0 m") +
                        " 標高差: " + (mMaxElevator - mMinElevator).ToString("#,##0 m");
            return buffer;
        }

        /// <summary>
        /// 累積距離を求める
        /// </summary>
        /// <param name="locList">座標点リスト</param>
        public void setTotalDistance(List<Point> locList)
        {
            mDistance = 0.0;
            mPrevPoint = new Point(double.NaN, double.NaN);
            foreach (Point loc in locList) {
                addDistance(loc);
            }
        }

        /// <summary>
        /// 領域を求める
        /// </summary>
        /// <param name="locList">座標点リスト</param>
        public void setArea(List<Point> locList)
        {
            mAreaOn = false;
            foreach (Point loc in locList) {
                extentArea(loc);
            }
        }


        /// <summary>
        /// 時刻の取得
        /// </summary>
        /// <param name="dateTime"></param>
        public void setDateTime(string dateTime)
        {
            DateTime dt = new DateTime();
            if (DateTime.TryParse(dateTime, out dt)) {
                setDateTime(dt);
            }
        }

        /// <summary>
        /// 開始時刻と終了時刻の設定
        /// </summary>
        /// <param name="dateTime"></param>
        private void setDateTime(DateTime dateTime)
        {
            if (mFirstTime.Ticks == 0) {
                mFirstTime = dateTime;
                mLastTime = dateTime;
            } else {
                mFirstTime = mFirstTime.Ticks > dateTime.Ticks ? dateTime : mFirstTime;
                mLastTime = mLastTime.Ticks < dateTime.Ticks ? dateTime : mLastTime;
            }
        }

        /// <summary>
        /// 距離の累積(km)
        /// </summary>
        /// <param name="loc">緯度経度座標</param>
        public void addDistance(Point loc)
        {
            if (!double.IsNaN(mPrevPoint.X) && !double.IsNaN(mPrevPoint.Y)) {
                mDistance += ylib.coordinateDistance(mPrevPoint.X, mPrevPoint.Y, loc.X, loc.Y);
            }
            mPrevPoint = loc;
        }

        /// <summary>
        /// 最小最大標高(m)
        /// </summary>
        /// <param name="elevator">標高</param>
        public void setMinMaxElevator(string elevator)
        {
            double ele;
            if (double.TryParse(elevator, out ele)) {
                setMinMaxElevator(ele);
            }
        }

        /// <summary>
        /// 最小最大標高(m)の設定
        /// </summary>
        /// <param name="elevator"></param>
        private void setMinMaxElevator(double elevator)
        {
            mMinElevator = Math.Min(mMinElevator, elevator);
            mMaxElevator = Math.Max(mMaxElevator, elevator);
        }

        /// <summary>
        /// トレース領域を設定する(座標データ)
        /// </summary>
        /// <param name="cp"></param>
        public void extentArea(Point cp)
        {
            if (!mAreaOn) {
                mArea = new Rect(cp, cp);
                mAreaOn = true;
            } else {
                mArea = rectExtension(cp, mArea);
            }
        }
        /// <summary>
        /// トレース領域を拡張する
        /// </summary>
        /// <param name="pos">拡張座標(BaseMap)</param>
        /// <param name="rect">領域</param>
        /// <returns></returns>
        private Rect rectExtension(Point pos, Rect rect)
        {
            Point sp = new Point(Math.Min(pos.X, rect.X), Math.Min(pos.Y, rect.Y));
            Point ep = new Point(Math.Max(pos.X, rect.Right), Math.Max(pos.Y, rect.Bottom));
            return new Rect(sp, ep);
        }
    }

    /// <summary>
    /// GPSの位置情報データクラス
    /// </summary>
    public class GpsData
    {
        public DateTime mDateTime;      //  測定時間
        public double mLatitude;        //  緯度(deg)
        public double mLongitude;       //  経度(deg)
        public double mElevator;        //  高度(m)

        /// <summary>
        /// GPSデータを文字列に変換(日時、座標、標高))
        /// </summary>
        /// <returns>文字列</returns>
        public string toString()
        {
           return $"{mDateTime}({mLatitude},{mLongitude}){mElevator}";
        }

        /// <summary>
        /// GPSデータをCSV形式文字列に変換(日時、座標、標高))
        /// </summary>
        /// <returns></returns>
        public string toCsvString()
        {
            return $"{mDateTime},{mLatitude},{mLongitude},{mElevator}";
        }

        /// <summary>
        /// 測定時間を設定
        /// </summary>
        /// <param name="dateTime">文字列</param>
        public void setDateTime(string dateTime)
        {
            if (!DateTime.TryParse(dateTime, out mDateTime)) {
                mDateTime = new DateTime();
            }
        }

        /// <summary>
        /// 緯度(deg)の設定
        /// </summary>
        /// <param name="latitide">文字列</param>
        public void setLatitude(string latitide)
        {
            if (!double.TryParse(latitide, out mLatitude)) {
                mLatitude = 0.0;
            }
        }

        /// <summary>
        /// 経度(deg)の設定
        /// </summary>
        /// <param name="longitide">文字列</param>
        public void setLongitude(string longitide)
        {
            if (!double.TryParse(longitide, out mLongitude)) {
                mLongitude = 0.0;
            }
        }

        /// <summary>
        /// 標高(m)の設定
        /// </summary>
        /// <param name="elevator">文字列</param>
        public void setElevator(string elevator)
        {
            if (!double.TryParse(elevator, out mElevator)) {
                mElevator = 0.0;
            }
        }

        /// <summary>
        /// 座標をPointで返す
        /// </summary>
        /// <returns></returns>
        public Point getCoordinate()
        {
            return new Point(mLongitude, mLatitude);
        }

        /// <summary>
        /// 距離を求めるkm)
        /// </summary>
        /// <param name="p">対象データ</param>
        /// <returns>距離</returns>
        public double distance(GpsData p)
        {
            return YLib.CoordinateDistance(mLongitude, mLatitude, p.mLongitude, p.mLatitude);
        }

        /// <summary>
        /// 経過時間を求める(秒)
        /// </summary>
        /// <param name="p">対象データ</param>
        /// <returns>経過時間(s)</returns>
        public double laptime(GpsData p)
        {
            return p.mDateTime.Subtract(mDateTime).TotalSeconds;
        }

        /// <summary>
        /// 速度(km/h)を求める
        /// </summary>
        /// <param name="p">対象データ</param>
        /// <returns>速度(km/h)</returns>
        public double speed(GpsData p)
        {
            double lap = laptime(p);
            return lap == 0.0 ? 0.0 :  distance(p) / lap * 3600.0;
        }

    }
}

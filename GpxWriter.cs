using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace WpfLib
{
    public class GpxWriter
    {
        private string mGpxHeaderCreater = "GPS Logger for MapApp";
        private List<GpsData> mGpsDataList;
        private string mGpxFilePath;
        YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="gpsDataList">GPSデータリスト</param>
        /// <param name="filePath">保存ファイル名</param>
        public GpxWriter(List<GpsData> gpsDataList, string filePath) { 
            mGpsDataList = gpsDataList;
            mGpxFilePath = filePath;
        }

        /// <summary>
        /// GPXデータのヘッダ部作成
        /// </summary>
        /// <param name="creater">作成者</param>
        /// <param name="name">名前</param>
        /// <returns>文字列</returns>
        private string initData(string creater = "", string name = "")
        {
            // GPXヘッダ作成
            mGpxHeaderCreater = 0 < creater.Length ? creater : mGpxHeaderCreater;
            var buffer = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n";
            //          buffer += "<gpx xmlns=\"http://www.topografix.com/GPX/1/1\">\n"
            buffer += "<gpx version=\"1.0\" creator=\"" + mGpxHeaderCreater + "\">\n";
            buffer += "<trk>\n";
            if (0 < name.Length)
                buffer += "<name>" + name + "</name>\n";
            buffer += "<trkseg>\n";
            return buffer;
        }

        /// <summary>
        /// 座標データのGPXデータ作成
        /// </summary>
        /// <param name="gpsData">GPSデータ</param>
        /// <returns>文字列</returns>
        private string locationData(GpsData gpsData)
        {
            // 位置データ
            var buffer = "<trkpt lat=\"" + gpsData.mLatitude.ToString() +
                    "\" lon=\"" + gpsData.mLongitude.ToString() + "\">";
            buffer += "<ele>" + gpsData.mElevator.ToString() + "</ele>";
            buffer += "<time>" + gpsData.mDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") + "</time>";
            buffer += "</trkpt>";
            buffer += "\n";
            return buffer;
        }

        /// <summary>
        /// GPX終了部のデータ作成
        /// </summary>
        /// <returns>GPX終了部のデータ</returns>
        private string closeData()
        {
            // 終了コード出力
            string buffer = "</trkseg>\n";
            buffer += "</trk>\n";
            buffer += "</gpx>";
            buffer += "\n";
            return buffer;
        }

        /// <summary>
        /// GPSデータの一括書き込み
        /// </summary>
        /// <returns></returns>
        public bool writeDataAll()
        {
            try {
                string buf = initData();
                foreach (var data in mGpsDataList)
                    buf += locationData(data);
                buf += closeData();
                ylib.saveTextFile(mGpxFilePath, buf);

                return true;
            } catch (Exception e) {
                return false;
            }
        }
    }
}

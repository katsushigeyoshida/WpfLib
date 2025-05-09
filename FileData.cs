using System;
using System.IO;

namespace WpfLib
{
    public class FileData
    {
        //  ファイル属性
        public string FileName { get; set; }    //  ファイル名
        public string Folder { get; set; }      //  フォルダ名
        public DateTime Date { get; set; }      //  ファイルの日時
        public long Size { get; set; }          //  ファイルサイズ

        public FileData()
        {
            FileName = "";
            Folder = "";
            Date = DateTime.Now;
            Size = 0;
        }

        /// <summary>
        /// ファイルデータの設定
        /// </summary>
        /// <param name="fileData">ファイルデータ</param>
        public FileData(FileData fileData)
        {
            FileName = fileData.FileName;
            Folder = fileData.Folder;
            Date = fileData.Date;
            Size = fileData.Size;
        }

        /// <summary>
        /// ファイルデータの設定
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="folder">フォルダ名</param>
        /// <param name="date">ファイル日付(DateTime)</param>
        /// <param name="size">ファイルサイズ</param>
        public FileData(string fileName, string folder, DateTime date, long size)
        {
            FileName = fileName;
            Folder = folder;
            Date = date;
            Size = size;
        }

        /// <summary>
        /// ファイルデータの設定
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="date">ファイル日付(DateTime)</param>
        /// <param name="size">ファイルサイズ</param>
        public FileData(string path, DateTime date, long size)
        {
            FileName = Path.GetFileName(path);
            Folder = Path.GetDirectoryName(path);
            Date = date;
            Size = size;
        }

        /// <summary>
        /// ファイルデータの設定
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="folder">フォルダ名</param>
        /// <param name="date">ファイル日付("yyyy/MM/dd HH:mm:ss")</param>
        /// <param name="size">ファイルサイズ</param>
        public FileData(string fileName, string folder, string date, string size)
        {
            DateTime datetime;
            long filesize;
            FileName = fileName;
            Folder = folder;
            Date = DateTime.TryParse(date, out datetime) ? datetime : DateTime.Now;
            Size = long.TryParse(size, out filesize) ? filesize : 0;
        }

        /// <summary>
        /// ファイルデータの設定
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="date">ファイル日付("yyyy/MM/dd HH:mm:ss")</param>
        /// <param name="size">ファイルサイズ</param>
        public FileData(string path, string date, string size)
        {
            DateTime datetime;
            long filesize;
            FileName = Path.GetFileName(path);
            Folder = Path.GetDirectoryName(path);
            Date = DateTime.TryParse(date, out datetime) ? datetime : DateTime.Now;
            Size = long.TryParse(size, out filesize) ? filesize : 0;
        }

        /// <summary>
        /// ファイルパスの取得
        /// </summary>
        /// <returns></returns>
        public string getPath()
        {
            return Path.Combine(Folder, FileName);
        }

        /// <summary>
        /// ファイル名とフォルダをパスで設定
        /// </summary>
        /// <param name="path"></param>
        public void setPath(string path)
        {
            FileName = Path.GetFileName(path);
            Folder = Path.GetDirectoryName(path);
        }

        /// <summary>
        /// ファイルの日付を文字列でとりだす
        /// </summary>
        /// <returns>日付("yyyy/MM/dd HH:mm:ss")</returns>
        public string getDate()
        {
            return Date.ToString("yyyy/MM/dd HH:mm:ss");
        }

        /// <summary>
        /// ファイルの日付を文字列で設定する
        /// </summary>
        /// <param name="date">日付("yyyy/MM/dd HH:mm:ss")</param>
        public void setDate(String date)
        {
            DateTime datetime;
            Date = DateTime.TryParse(date, out datetime) ? datetime : DateTime.Now;
        }

        /// <summary>
        /// ファイルサイズを文字列で取得
        /// </summary>
        /// <returns>ファイルサイズ</returns>
        public string getSize()
        {
            return Size.ToString();
        }

        /// <summary>
        /// ファイルサイズを文字列で設定
        /// </summary>
        /// <param name="size">ファイルサイズ</param>
        public void setSize(string size)
        {
            long filesize;
            Size = long.TryParse(size, out filesize) ? filesize : 0;
        }

        /// <summary>
        /// ファイルの種別の取得(.を含まない大文字拡張子)
        /// </summary>
        /// <returns></returns>
        public string getFileType()
        {
            return Path.GetExtension(FileName).Substring(1).ToUpper();
        }
    }
}

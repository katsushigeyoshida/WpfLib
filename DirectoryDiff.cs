using System;
using System.Collections.Generic;
using System.IO;

namespace WpfLib
{
    /// <summary>
    /// フォルダの比較用データ
    /// </summary>
    public class FilesData
    {
        public string mRelPath;
        public FileInfo mSrcFile;
        public FileInfo mDstFile;
        public ulong mSrcCrc;
        public ulong mDstCrc;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="relPath">相対パス</param>
        /// <param name="srcFile">比較元ファイル情報</param>
        /// <param name="dstFile">比較先ファイル情報</param>
        public FilesData(string relPath, FileInfo srcFile, FileInfo dstFile)
        {
            mRelPath = relPath;
            mSrcFile = srcFile;
            mDstFile = dstFile;
        }
    }

    /// <summary>
    /// ディレクトリ比較
    /// </summary>
    public class DirectoryDiff
    {
        private List<FileInfo> mSrcFiles = new List<FileInfo>();
        private List<FileInfo> mDestFiles = new List<FileInfo>();
        private string mSrcDir;
        private string mDestDir;
        public List<FilesData> mFiles = new List<FilesData>();
        public bool mHashChk = true;
        private HashCode mHash = new HashCode(HashCode.HashType.CRC32L);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DirectoryDiff()
        {
        }

        /// <summary>
        /// コンストラクタ
        /// 比較元と比較先のファイルリストの登録
        /// </summary>
        /// <param name="srcDir"></param>
        /// <param name="destDir"></param>
        public DirectoryDiff(string srcDir, string destDir, bool hashChk = true)
        {
            char[] trimChar = {'\\'};
            mSrcDir = srcDir.TrimEnd(trimChar);
            mDestDir = destDir.TrimEnd(trimChar);
            mSrcFiles = getDirectories(mSrcDir);
            mDestFiles = getDirectories(mDestDir);
            mHashChk = hashChk;

            foreach (FileInfo srcFile in mSrcFiles) {
                mFiles.Add(new FilesData(srcFile.FullName.Substring(mSrcDir.Length + 1), srcFile, null));
                if (mHashChk)
                    mFiles[mFiles.Count - 1].mSrcCrc = mHash.GetCrc32L(srcFile.FullName);
            }
            foreach (FileInfo destFile in mDestFiles) {
                string destRelPath = destFile.FullName.Substring(mDestDir.Length + 1);
                int n = mFiles.FindIndex(x => x.mRelPath == destRelPath);
                if (0 <= n) {
                    mFiles[n].mDstFile = destFile;
                    if (mHashChk)
                        mFiles[n].mDstCrc = mHash.GetCrc32L(destFile.FullName);
                } else {
                    mFiles.Add(new FilesData(destRelPath, null, destFile));
                    if (mHashChk)
                        mFiles[mFiles.Count - 1].mDstCrc = mHash.GetCrc32L(destFile.FullName);
                }
            }
        }

        /// <summary>
        /// ファイルリストから同一ファイルを除く
        /// 日付とサイズをチェック
        /// </summary>
        public void stripSameFile()
        {
            for (int i = mFiles.Count - 1; i >= 0; i--) {
                if (mHashChk) {
                    if (mFiles[i].mSrcCrc == mFiles[i].mDstCrc)
                        mFiles.RemoveAt(i);
                } else {
                    if (sameFile(mFiles[i].mSrcFile, mFiles[i].mDstFile))
                        mFiles.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// ファイル同士の同一性をチェック
        /// </summary>
        /// <param name="srcFile"></param>
        /// <param name="dstFile"></param>
        /// <returns></returns>
        private bool sameFile(FileInfo srcFile, FileInfo dstFile)
        {
            if (srcFile == null || dstFile == null)
                return false;
            //  日時の比較は1秒以下の誤差がある場合があるので文字列に変換して比較
            if (srcFile.LastWriteTime.ToString().CompareTo(dstFile.LastWriteTime.ToString()) != 0) 
                return false;
            if (srcFile.Length != dstFile.Length)
                return false;
            return true;
        }

        /// <summary>
        /// 比較リストへの変換
        /// </summary>
        /// <returns></returns>
        public List<List<string>> getStringList()
        {
            List<List<string>> fileStringLiset = new List<List<string>>();
            foreach (FilesData fileData in mFiles) {
                List<string> buf = new List<string>();
                buf.Add(Path.GetFileName(fileData.mRelPath));
                buf.Add(Path.GetDirectoryName(fileData.mRelPath));
                buf.Add(fileData.mSrcFile == null ? "" : fileData.mSrcFile.LastWriteTime.ToString());
                buf.Add(fileData.mSrcFile == null ? "" : fileData.mSrcFile.Length.ToString());
                buf.Add(fileData.mDstFile == null ? "" : fileData.mDstFile.LastWriteTime.ToString());
                buf.Add(fileData.mDstFile == null ? "" : fileData.mDstFile.Length.ToString());
                fileStringLiset.Add(buf);
            }
            return fileStringLiset;
        }

        /// <summary>
        /// 更新ファイルリスト
        /// </summary>
        /// <returns></returns>
        public List<FilesData> getUpdateFile()
        {
            List<FilesData> files = new List<FilesData>();
            foreach (var file in mFiles) {
                if (file.mSrcFile != null && file.mDstFile != null) {
                    //  両方に存在するファイル
                    if (file.mSrcFile.LastWriteTime > file.mDstFile.LastWriteTime)
                        files.Add(file);
                } else if (file.mSrcFile != null && file.mDstFile == null) {
                    //  コピー先にないファイル
                    files.Add(file);
                }
            }
            return files;
        }

        /// <summary>
        /// 比較元にないファイル
        /// </summary>
        /// <returns></returns>
        public List<FilesData> getNoExistFile()
        {
            List<FilesData> files = new List<FilesData>();
            foreach (var file in mFiles) {
                if (file.mSrcFile == null) {
                    files.Add(file);
                }
            }
            return files;
        }

        /// <summary>
        /// ファイルの更新コピー
        /// </summary>
        /// <returns>更新ファイル数</returns>
        public int updateCopy()
        {
            List<FilesData> fileList = getUpdateFile();
            int count = 0;
            foreach (var file in fileList) {
                if (file.mSrcFile == null)
                    continue;
                string dstPath;
                if (file.mDstFile == null) {
                    //  コピー先にないファイル
                    dstPath = Path.Combine(mDestDir, file.mRelPath);
                } else {
                    dstPath = file.mDstFile.FullName;
                }
                if (fileCopy(file.mSrcFile.FullName, dstPath))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// コピー元にないファイルの削除
        /// </summary>
        /// <returns>削除ファイル数</returns>
        public int noExistDestFileRemove()
        {
            List<FilesData> fileList = getNoExistFile();
            int count = 0;
            foreach (var file in fileList) {
                File.Delete(file.mDstFile.FullName);
                if (!File.Exists(file.mDstFile.FullName))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// コピー元にコピー先を同期させる
        /// コピー元にないファイルは削除する
        /// </summary>
        /// <returns>コピーファイル数+削除ファイル数</returns>
        public int syncFolder()
        {
            int count = 0;
            count += updateCopy();
            count += noExistDestFileRemove();
            return count;
        }

        /// <summary>
        /// ファイルコピー
        /// </summary>
        /// <param name="srcPath">コピー元ファイル名</param>
        /// <param name="dstPath"><コピー先ファイル名/param>
        /// <returns>コピーの可否</returns>
        public bool fileCopy(string srcPath, string dstPath)
        {
            try {
                string dstDir = Path.GetDirectoryName(dstPath);
                if (!Directory.Exists(dstDir)) {
                    Directory.CreateDirectory(dstDir);
                }
                File.Copy(srcPath, dstPath, true);
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.ToString() + srcPath);
                return false;
            }
            return true;
        }

        /// <summary>
        /// ファイル検索
        /// </summary>
        /// <param name="folder">検索フォルダ</param>
        /// <returns>ファイルリスト</returns>
        public List<FileInfo> getDirectories(string folder)
        {
            List<FileInfo> fileList = new List<FileInfo>();
            try {
                DirectoryInfo di = new DirectoryInfo(folder);
                foreach (DirectoryInfo dir in di.GetDirectories()) {
                    List<FileInfo> filesInfo = getDirectories(dir.FullName);
                    if (filesInfo != null) {
                        fileList.AddRange(filesInfo);
                    }
                }
                string[] files = Directory.GetFiles(folder);
                foreach (var file in files) {
                    FileInfo fi = new FileInfo(file);
                    if (fi != null)
                        fileList.Add(fi);
                }
                return fileList;
            } catch (Exception e) {
                return null;
            }
        }

    }
}

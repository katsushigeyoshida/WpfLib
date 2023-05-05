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
        public DirectoryDiff(string srcDir, string destDir)
        { 
            mSrcDir = srcDir;
            mDestDir = destDir;
            mSrcFiles = getDirectories(srcDir);
            mDestFiles = getDirectories(destDir);

            foreach (FileInfo srcFile in mSrcFiles) {
                mFiles.Add(new FilesData(srcFile.FullName.Substring(srcDir.Length), srcFile, null));
            }
            foreach (FileInfo destFile in mDestFiles) {
                string destPath = destFile.FullName.Substring(destDir.Length);
                int n = mFiles.FindIndex(x => x.mRelPath == destPath);
                if (0 <= n) {
                    mFiles[n].mDstFile = destFile;
                } else {
                    mFiles.Add(new FilesData(destPath, null, destFile));
                }
            }
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
                    if (file.mSrcFile.LastAccessTime > file.mDstFile.LastAccessTime)
                        files.Add(file);
                } else if (file.mSrcFile != null) {
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
            foreach(var file in fileList) {
                if (file.mSrcFile != null && file.mDstFile != null) {
                    if (fileCopy(file.mSrcFile.FullName, file.mDstFile.FullName))
                        count++;
                } else if (file.mSrcFile != null) {
                    string dstPath = Path.Combine(mDestDir, file.mSrcFile.Name);
                    if (fileCopy(file.mSrcFile.FullName, dstPath))
                        count++;
                }
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
        public bool fileCopy(string srcPath,  string dstPath)
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

using Shell32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfLib
{
    public class FilesData
    {
        public string mRelPath;
        public FileInfo mSrcFile;
        public FileInfo mDstFile;

        public FilesData(string relPath, FileInfo srcFile, FileInfo dstFile)
        {
            mRelPath = relPath;
            mSrcFile = srcFile;
            mDstFile = dstFile;
        }
    }

    public class DirectoryDiff
    {
        private List<FileInfo> mSrcFiles = new List<FileInfo>();
        private List<FileInfo> mDestFiles = new List<FileInfo>();
        private string mSrcPath;
        private string mDestPath;
        public List<FilesData> mFiles = new List<FilesData>();

        public DirectoryDiff()
        {

        }

        public DirectoryDiff(string srcDir, string destDir)
        { 
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

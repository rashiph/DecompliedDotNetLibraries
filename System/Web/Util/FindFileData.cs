namespace System.Web.Util
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Web;

    internal sealed class FindFileData
    {
        private System.Web.Util.FileAttributesData _fileAttributesData;
        private string _fileNameLong;
        private string _fileNameShort;

        internal FindFileData(ref UnsafeNativeMethods.WIN32_FIND_DATA wfd)
        {
            this._fileAttributesData = new System.Web.Util.FileAttributesData(ref wfd);
            this._fileNameLong = wfd.cFileName;
            if (((wfd.cAlternateFileName != null) && (wfd.cAlternateFileName.Length > 0)) && !StringUtil.EqualsIgnoreCase(wfd.cFileName, wfd.cAlternateFileName))
            {
                this._fileNameShort = wfd.cAlternateFileName;
            }
        }

        internal static int FindFile(string path, out FindFileData data)
        {
            UnsafeNativeMethods.WIN32_FIND_DATA win_find_data;
            data = null;
            path = FileUtil.RemoveTrailingDirectoryBackSlash(path);
            IntPtr hndFindFile = UnsafeNativeMethods.FindFirstFile(path, out win_find_data);
            int lastError = Marshal.GetLastWin32Error();
            if (hndFindFile == UnsafeNativeMethods.INVALID_HANDLE_VALUE)
            {
                return HttpException.HResultFromLastError(lastError);
            }
            UnsafeNativeMethods.FindClose(hndFindFile);
            data = new FindFileData(ref win_find_data);
            return 0;
        }

        internal static int FindFile(string fullPath, string rootDirectoryPath, out FindFileData data)
        {
            int num = FindFile(fullPath, out data);
            if ((num == 0) && !string.IsNullOrEmpty(rootDirectoryPath))
            {
                rootDirectoryPath = FileUtil.RemoveTrailingDirectoryBackSlash(rootDirectoryPath);
                string str = string.Empty;
                string relativePathShort = string.Empty;
                for (string str3 = Path.GetDirectoryName(fullPath); ((str3 != null) && (str3.Length > (rootDirectoryPath.Length + 1))) && (str3.IndexOf(rootDirectoryPath, StringComparison.OrdinalIgnoreCase) == 0); str3 = Path.GetDirectoryName(str3))
                {
                    UnsafeNativeMethods.WIN32_FIND_DATA win_find_data;
                    IntPtr hndFindFile = UnsafeNativeMethods.FindFirstFile(str3, out win_find_data);
                    int lastError = Marshal.GetLastWin32Error();
                    if (hndFindFile == UnsafeNativeMethods.INVALID_HANDLE_VALUE)
                    {
                        return HttpException.HResultFromLastError(lastError);
                    }
                    UnsafeNativeMethods.FindClose(hndFindFile);
                    str = win_find_data.cFileName + Path.DirectorySeparatorChar + str;
                    if (!string.IsNullOrEmpty(win_find_data.cAlternateFileName))
                    {
                        relativePathShort = win_find_data.cAlternateFileName + Path.DirectorySeparatorChar + relativePathShort;
                    }
                    else
                    {
                        relativePathShort = win_find_data.cFileName + Path.DirectorySeparatorChar + relativePathShort;
                    }
                }
                if (!string.IsNullOrEmpty(str))
                {
                    data.PrependRelativePath(str, relativePathShort);
                }
            }
            return num;
        }

        private void PrependRelativePath(string relativePathLong, string relativePathShort)
        {
            this._fileNameLong = relativePathLong + this._fileNameLong;
            string str = string.IsNullOrEmpty(this._fileNameShort) ? this._fileNameLong : this._fileNameShort;
            this._fileNameShort = relativePathShort + str;
            if (StringUtil.EqualsIgnoreCase(this._fileNameShort, this._fileNameLong))
            {
                this._fileNameShort = null;
            }
        }

        internal System.Web.Util.FileAttributesData FileAttributesData
        {
            get
            {
                return this._fileAttributesData;
            }
        }

        internal string FileNameLong
        {
            get
            {
                return this._fileNameLong;
            }
        }

        internal string FileNameShort
        {
            get
            {
                return this._fileNameShort;
            }
        }
    }
}


namespace System.Web.Util
{
    using System;
    using System.Web;

    internal abstract class FileData
    {
        protected string _path;
        protected UnsafeNativeMethods.WIN32_FIND_DATA _wfd;

        protected FileData()
        {
        }

        internal FindFileData GetFindFileData()
        {
            return new FindFileData(ref this._wfd);
        }

        internal string FullName
        {
            get
            {
                return (this._path + @"\" + this._wfd.cFileName);
            }
        }

        internal bool IsDirectory
        {
            get
            {
                return ((this._wfd.dwFileAttributes & 0x10) != 0);
            }
        }

        internal bool IsHidden
        {
            get
            {
                return ((this._wfd.dwFileAttributes & 2) != 0);
            }
        }

        internal string Name
        {
            get
            {
                return this._wfd.cFileName;
            }
        }
    }
}


namespace System.Windows.Forms
{
    using System;
    using System.Globalization;
    using System.Text;

    public class FileDialogCustomPlace
    {
        private Guid _knownFolderGuid;
        private string _path;

        public FileDialogCustomPlace(Guid knownFolderGuid)
        {
            this._path = "";
            this._knownFolderGuid = Guid.Empty;
            this.KnownFolderGuid = knownFolderGuid;
        }

        public FileDialogCustomPlace(string path)
        {
            this._path = "";
            this._knownFolderGuid = Guid.Empty;
            this.Path = path;
        }

        private static string GetFolderLocation(Guid folderGuid)
        {
            if (UnsafeNativeMethods.IsVista)
            {
                StringBuilder pszPath = new StringBuilder(260);
                if (UnsafeNativeMethods.Shell32.SHGetFolderPathEx(ref folderGuid, 0, IntPtr.Zero, pszPath, (uint) pszPath.Capacity) == 0)
                {
                    return pszPath.ToString();
                }
            }
            return null;
        }

        internal FileDialogNative.IShellItem GetNativePath()
        {
            string folderLocation = "";
            if (!string.IsNullOrEmpty(this._path))
            {
                folderLocation = this._path;
            }
            else
            {
                folderLocation = GetFolderLocation(this._knownFolderGuid);
            }
            if (string.IsNullOrEmpty(folderLocation))
            {
                return null;
            }
            return FileDialog.GetShellItemForPath(folderLocation);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} Path: {1} KnownFolderGuid: {2}", new object[] { base.ToString(), this.Path, this.KnownFolderGuid });
        }

        public Guid KnownFolderGuid
        {
            get
            {
                return this._knownFolderGuid;
            }
            set
            {
                this._path = string.Empty;
                this._knownFolderGuid = value;
            }
        }

        public string Path
        {
            get
            {
                if (string.IsNullOrEmpty(this._path))
                {
                    return string.Empty;
                }
                return this._path;
            }
            set
            {
                this._path = value ?? "";
                this._knownFolderGuid = Guid.Empty;
            }
        }
    }
}


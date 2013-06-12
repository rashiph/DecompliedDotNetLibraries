namespace System.Web.Caching
{
    using System;
    using System.Security.Permissions;
    using System.Web;

    [Serializable, AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Unrestricted)]
    public class FileResponseElement : ResponseElement
    {
        private long _length;
        private long _offset;
        private string _path;

        private FileResponseElement()
        {
        }

        public FileResponseElement(string path, long offset, long length)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (offset < 0L)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (length < 0L)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            this._path = path;
            this._offset = offset;
            this._length = length;
        }

        public long Length
        {
            get
            {
                return this._length;
            }
        }

        public long Offset
        {
            get
            {
                return this._offset;
            }
        }

        public string Path
        {
            get
            {
                return this._path;
            }
        }
    }
}


namespace System.Web.Hosting
{
    using System;
    using System.Web;

    [Serializable]
    public sealed class ApplicationInfo
    {
        private string _id;
        private string _physicalPath;
        private System.Web.VirtualPath _virtualPath;

        internal ApplicationInfo(string id, System.Web.VirtualPath virtualPath, string physicalPath)
        {
            this._id = id;
            this._virtualPath = virtualPath;
            this._physicalPath = physicalPath;
        }

        public string ID
        {
            get
            {
                return this._id;
            }
        }

        public string PhysicalPath
        {
            get
            {
                return this._physicalPath;
            }
        }

        public string VirtualPath
        {
            get
            {
                return this._virtualPath.VirtualPathString;
            }
        }
    }
}


namespace System.Web.Hosting
{
    using System;

    public class AppDomainInfo : IAppDomainInfo
    {
        private string _id;
        private bool _isIdle;
        private string _physicalPath;
        private int _siteId;
        private string _virtualPath;

        internal AppDomainInfo(string id, string vpath, string physPath, int siteId, bool isIdle)
        {
            this._id = id;
            this._virtualPath = vpath;
            this._physicalPath = physPath;
            this._siteId = siteId;
            this._isIdle = isIdle;
        }

        public string GetId()
        {
            return this._id;
        }

        public string GetPhysicalPath()
        {
            return this._physicalPath;
        }

        public int GetSiteId()
        {
            return this._siteId;
        }

        public string GetVirtualPath()
        {
            return this._virtualPath;
        }

        public bool IsIdle()
        {
            return this._isIdle;
        }
    }
}


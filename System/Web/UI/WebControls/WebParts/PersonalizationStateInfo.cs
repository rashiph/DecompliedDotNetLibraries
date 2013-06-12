namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Web.Util;

    [Serializable]
    public abstract class PersonalizationStateInfo
    {
        private DateTime _lastUpdatedDate;
        private string _path;
        private int _size;

        internal PersonalizationStateInfo(string path, DateTime lastUpdatedDate, int size)
        {
            this._path = StringUtil.CheckAndTrimString(path, "path");
            PersonalizationProviderHelper.CheckNegativeInteger(size, "size");
            this._lastUpdatedDate = lastUpdatedDate.ToUniversalTime();
            this._size = size;
        }

        public DateTime LastUpdatedDate
        {
            get
            {
                return this._lastUpdatedDate.ToLocalTime();
            }
        }

        public string Path
        {
            get
            {
                return this._path;
            }
        }

        public int Size
        {
            get
            {
                return this._size;
            }
        }
    }
}


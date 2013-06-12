namespace System.Web.Profile
{
    using System;

    [Serializable]
    public class ProfileInfo
    {
        private bool _IsAnonymous;
        private DateTime _LastActivityDate;
        private DateTime _LastUpdatedDate;
        private int _Size;
        private string _UserName;

        protected ProfileInfo()
        {
        }

        public ProfileInfo(string username, bool isAnonymous, DateTime lastActivityDate, DateTime lastUpdatedDate, int size)
        {
            if (username != null)
            {
                username = username.Trim();
            }
            this._UserName = username;
            if (lastActivityDate.Kind == DateTimeKind.Local)
            {
                lastActivityDate = lastActivityDate.ToUniversalTime();
            }
            this._LastActivityDate = lastActivityDate;
            if (lastUpdatedDate.Kind == DateTimeKind.Local)
            {
                lastUpdatedDate = lastUpdatedDate.ToUniversalTime();
            }
            this._LastUpdatedDate = lastUpdatedDate;
            this._IsAnonymous = isAnonymous;
            this._Size = size;
        }

        public virtual bool IsAnonymous
        {
            get
            {
                return this._IsAnonymous;
            }
        }

        public virtual DateTime LastActivityDate
        {
            get
            {
                return this._LastActivityDate.ToLocalTime();
            }
        }

        public virtual DateTime LastUpdatedDate
        {
            get
            {
                return this._LastUpdatedDate.ToLocalTime();
            }
        }

        public virtual int Size
        {
            get
            {
                return this._Size;
            }
        }

        public virtual string UserName
        {
            get
            {
                return this._UserName;
            }
        }
    }
}


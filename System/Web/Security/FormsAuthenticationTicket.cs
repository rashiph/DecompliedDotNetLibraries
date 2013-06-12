namespace System.Web.Security
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class FormsAuthenticationTicket
    {
        private string _CookiePath;
        private DateTime _Expiration;
        [NonSerialized]
        private DateTime _ExpirationUtc;
        [NonSerialized]
        private bool _ExpirationUtcHasValue;
        [OptionalField(VersionAdded=2)]
        private byte[] _InternalData;
        [OptionalField(VersionAdded=2)]
        private int _InternalVersion;
        private bool _IsPersistent;
        private DateTime _IssueDate;
        [NonSerialized]
        private DateTime _IssueDateUtc;
        [NonSerialized]
        private bool _IssueDateUtcHasValue;
        private string _Name;
        private string _UserData;
        private int _Version;

        public FormsAuthenticationTicket(string name, bool isPersistent, int timeout)
        {
            this._Version = 2;
            this._Name = name;
            this._IssueDateUtcHasValue = true;
            this._IssueDateUtc = DateTime.UtcNow;
            this._IssueDate = DateTime.Now;
            this._IsPersistent = isPersistent;
            this._UserData = "";
            this._ExpirationUtcHasValue = true;
            this._ExpirationUtc = this._IssueDateUtc.AddMinutes((double) timeout);
            this._Expiration = this._IssueDate.AddMinutes((double) timeout);
            this._CookiePath = FormsAuthentication.FormsCookiePath;
        }

        public FormsAuthenticationTicket(int version, string name, DateTime issueDate, DateTime expiration, bool isPersistent, string userData)
        {
            this._Version = version;
            this._Name = name;
            this._Expiration = expiration;
            this._IssueDate = issueDate;
            this._IsPersistent = isPersistent;
            this._UserData = userData;
            this._CookiePath = FormsAuthentication.FormsCookiePath;
        }

        public FormsAuthenticationTicket(int version, string name, DateTime issueDate, DateTime expiration, bool isPersistent, string userData, string cookiePath)
        {
            this._Version = version;
            this._Name = name;
            this._Expiration = expiration;
            this._IssueDate = issueDate;
            this._IsPersistent = isPersistent;
            this._UserData = userData;
            this._CookiePath = cookiePath;
        }

        internal static FormsAuthenticationTicket FromUtc(int version, string name, DateTime issueDateUtc, DateTime expirationUtc, bool isPersistent, string userData, string cookiePath)
        {
            return new FormsAuthenticationTicket(version, name, issueDateUtc.ToLocalTime(), expirationUtc.ToLocalTime(), isPersistent, userData, cookiePath) { _IssueDateUtcHasValue = true, _IssueDateUtc = issueDateUtc, _ExpirationUtcHasValue = true, _ExpirationUtc = expirationUtc };
        }

        public string CookiePath
        {
            get
            {
                return this._CookiePath;
            }
        }

        public DateTime Expiration
        {
            get
            {
                return this._Expiration;
            }
        }

        internal DateTime ExpirationUtc
        {
            get
            {
                if (!this._ExpirationUtcHasValue)
                {
                    return this.Expiration.ToUniversalTime();
                }
                return this._ExpirationUtc;
            }
        }

        public bool Expired
        {
            get
            {
                return (this.ExpirationUtc < DateTime.UtcNow);
            }
        }

        public bool IsPersistent
        {
            get
            {
                return this._IsPersistent;
            }
        }

        public DateTime IssueDate
        {
            get
            {
                return this._IssueDate;
            }
        }

        internal DateTime IssueDateUtc
        {
            get
            {
                if (!this._IssueDateUtcHasValue)
                {
                    return this.IssueDate.ToUniversalTime();
                }
                return this._IssueDateUtc;
            }
        }

        public string Name
        {
            get
            {
                return this._Name;
            }
        }

        public string UserData
        {
            get
            {
                return this._UserData;
            }
        }

        public int Version
        {
            get
            {
                return this._Version;
            }
        }
    }
}


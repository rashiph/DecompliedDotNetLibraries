namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Web.Util;

    [Serializable]
    public sealed class UserPersonalizationStateInfo : PersonalizationStateInfo
    {
        private DateTime _lastActivityDate;
        private string _username;

        public UserPersonalizationStateInfo(string path, DateTime lastUpdatedDate, int size, string username, DateTime lastActivityDate) : base(path, lastUpdatedDate, size)
        {
            this._username = StringUtil.CheckAndTrimString(username, "username");
            this._lastActivityDate = lastActivityDate.ToUniversalTime();
        }

        public DateTime LastActivityDate
        {
            get
            {
                return this._lastActivityDate.ToLocalTime();
            }
        }

        public string Username
        {
            get
            {
                return this._username;
            }
        }
    }
}


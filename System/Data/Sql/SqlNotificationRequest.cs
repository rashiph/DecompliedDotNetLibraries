namespace System.Data.Sql
{
    using System;
    using System.Data.Common;

    public sealed class SqlNotificationRequest
    {
        private string _options;
        private int _timeout;
        private string _userData;

        public SqlNotificationRequest() : this(null, null, 0)
        {
        }

        public SqlNotificationRequest(string userData, string options, int timeout)
        {
            this.UserData = userData;
            this.Timeout = timeout;
            this.Options = options;
        }

        public string Options
        {
            get
            {
                return this._options;
            }
            set
            {
                if ((value != null) && (0xffff < value.Length))
                {
                    throw ADP.ArgumentOutOfRange(string.Empty, "Service");
                }
                this._options = value;
            }
        }

        public int Timeout
        {
            get
            {
                return this._timeout;
            }
            set
            {
                if (0 > value)
                {
                    throw ADP.ArgumentOutOfRange(string.Empty, "Timeout");
                }
                this._timeout = value;
            }
        }

        public string UserData
        {
            get
            {
                return this._userData;
            }
            set
            {
                if ((value != null) && (0xffff < value.Length))
                {
                    throw ADP.ArgumentOutOfRange(string.Empty, "UserData");
                }
                this._userData = value;
            }
        }
    }
}


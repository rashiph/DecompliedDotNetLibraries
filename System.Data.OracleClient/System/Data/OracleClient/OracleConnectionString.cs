namespace System.Data.OracleClient
{
    using System;
    using System.Collections;
    using System.Data.Common;
    using System.Security;
    using System.Security.Permissions;

    internal sealed class OracleConnectionString : System.Data.Common.DbConnectionOptions
    {
        private readonly string _dataSource;
        private readonly bool _enlist;
        private readonly bool _integratedSecurity;
        private readonly int _loadBalanceTimeout;
        private readonly int _maxPoolSize;
        private readonly int _minPoolSize;
        private readonly bool _omitOracleConnectionName;
        private readonly string _password;
        private readonly bool _persistSecurityInfo;
        private readonly bool _pooling;
        private readonly bool _unicode;
        private readonly string _userId;
        private static Hashtable _validKeyNamesAndSynonyms;

        public OracleConnectionString(string connectionString) : base(connectionString, GetParseSynonyms(), false)
        {
            this._integratedSecurity = base.ConvertValueToIntegratedSecurity();
            this._enlist = base.ConvertValueToBoolean("enlist", System.Data.Common.ADP.IsWindowsNT);
            this._persistSecurityInfo = base.ConvertValueToBoolean("persist security info", false);
            this._pooling = base.ConvertValueToBoolean("pooling", true);
            this._unicode = base.ConvertValueToBoolean("unicode", false);
            this._omitOracleConnectionName = base.ConvertValueToBoolean("omit oracle connection name", false);
            this._loadBalanceTimeout = base.ConvertValueToInt32("load balance timeout", 0);
            this._maxPoolSize = base.ConvertValueToInt32("max pool size", 100);
            this._minPoolSize = base.ConvertValueToInt32("min pool size", 0);
            this._dataSource = base.ConvertValueToString("data source", "");
            this._userId = base.ConvertValueToString("user id", "");
            this._password = base.ConvertValueToString("password", "");
            if (this._userId.Length > 30)
            {
                throw System.Data.Common.ADP.InvalidConnectionOptionLength("user id", 30);
            }
            if (this._password.Length > 30)
            {
                throw System.Data.Common.ADP.InvalidConnectionOptionLength("password", 30);
            }
            if (this._loadBalanceTimeout < 0)
            {
                throw System.Data.Common.ADP.InvalidConnectionOptionValue("load balance timeout");
            }
            if (this._maxPoolSize < 1)
            {
                throw System.Data.Common.ADP.InvalidConnectionOptionValue("max pool size");
            }
            if (this._minPoolSize < 0)
            {
                throw System.Data.Common.ADP.InvalidConnectionOptionValue("min pool size");
            }
            if (this._maxPoolSize < this._minPoolSize)
            {
                throw System.Data.Common.ADP.InvalidMinMaxPoolSizeValues();
            }
        }

        protected internal override PermissionSet CreatePermissionSet()
        {
            PermissionSet set = new PermissionSet(PermissionState.None);
            set.AddPermission(new OraclePermission(this));
            return set;
        }

        internal static Hashtable GetParseSynonyms()
        {
            Hashtable hashtable = _validKeyNamesAndSynonyms;
            if (hashtable == null)
            {
                hashtable = new Hashtable(0x13);
                hashtable.Add("data source", "data source");
                hashtable.Add("enlist", "enlist");
                hashtable.Add("integrated security", "integrated security");
                hashtable.Add("load balance timeout", "load balance timeout");
                hashtable.Add("max pool size", "max pool size");
                hashtable.Add("min pool size", "min pool size");
                hashtable.Add("omit oracle connection name", "omit oracle connection name");
                hashtable.Add("password", "password");
                hashtable.Add("persist security info", "persist security info");
                hashtable.Add("pooling", "pooling");
                hashtable.Add("unicode", "unicode");
                hashtable.Add("user id", "user id");
                hashtable.Add("server", "data source");
                hashtable.Add("pwd", "password");
                hashtable.Add("persistsecurityinfo", "persist security info");
                hashtable.Add("uid", "user id");
                hashtable.Add("user", "user id");
                hashtable.Add("connection lifetime", "load balance timeout");
                hashtable.Add("workaround oracle bug 914652", "omit oracle connection name");
                _validKeyNamesAndSynonyms = hashtable;
            }
            return hashtable;
        }

        internal string DataSource
        {
            get
            {
                return this._dataSource;
            }
        }

        internal bool Enlist
        {
            get
            {
                return this._enlist;
            }
        }

        internal bool IntegratedSecurity
        {
            get
            {
                return this._integratedSecurity;
            }
        }

        internal int LoadBalanceTimeout
        {
            get
            {
                return this._loadBalanceTimeout;
            }
        }

        internal int MaxPoolSize
        {
            get
            {
                return this._maxPoolSize;
            }
        }

        internal int MinPoolSize
        {
            get
            {
                return this._minPoolSize;
            }
        }

        internal bool OmitOracleConnectionName
        {
            get
            {
                return this._omitOracleConnectionName;
            }
        }

        internal string Password
        {
            get
            {
                return this._password;
            }
        }

        internal bool Pooling
        {
            get
            {
                return this._pooling;
            }
        }

        internal bool Unicode
        {
            get
            {
                return this._unicode;
            }
        }

        internal string UserId
        {
            get
            {
                return this._userId;
            }
        }
    }
}


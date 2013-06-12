namespace System.Data.SqlClient
{
    using System;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Security;

    internal sealed class SqlConnectionPoolGroupProviderInfo : DbConnectionPoolGroupProviderInfo
    {
        private string _alias;
        private string _failoverPartner;
        private PermissionSet _failoverPermissionSet;
        private bool _useFailoverPartner;

        internal SqlConnectionPoolGroupProviderInfo(SqlConnectionString connectionOptions)
        {
            this._failoverPartner = connectionOptions.FailoverPartner;
            if (ADP.IsEmpty(this._failoverPartner))
            {
                this._failoverPartner = null;
            }
        }

        internal void AliasCheck(string server)
        {
            if (this._alias != server)
            {
                lock (this)
                {
                    if (this._alias == null)
                    {
                        this._alias = server;
                    }
                    else if (this._alias != server)
                    {
                        Bid.Trace("<sc.SqlConnectionPoolGroupProviderInfo|INFO> alias change detected. Clearing PoolGroup\n");
                        base.PoolGroup.Clear();
                        this._alias = server;
                    }
                }
            }
        }

        private PermissionSet CreateFailoverPermission(SqlConnectionString userConnectionOptions, string actualFailoverPartner)
        {
            string str;
            if (userConnectionOptions["failover partner"] == null)
            {
                str = "data source";
            }
            else
            {
                str = "failover partner";
            }
            return new SqlConnectionString(userConnectionOptions.ExpandKeyword(str, actualFailoverPartner)).CreatePermissionSet();
        }

        internal void FailoverCheck(SqlInternalConnection connection, bool actualUseFailoverPartner, SqlConnectionString userConnectionOptions, string actualFailoverPartner)
        {
            if (this.UseFailoverPartner != actualUseFailoverPartner)
            {
                Bid.Trace("<sc.SqlConnectionPoolGroupProviderInfo|INFO> Failover detected. failover partner='%ls'. Clearing PoolGroup\n", actualFailoverPartner);
                base.PoolGroup.Clear();
                this._useFailoverPartner = actualUseFailoverPartner;
            }
            if (!this._useFailoverPartner && (this._failoverPartner != actualFailoverPartner))
            {
                PermissionSet set = this.CreateFailoverPermission(userConnectionOptions, actualFailoverPartner);
                lock (this)
                {
                    if (this._failoverPartner != actualFailoverPartner)
                    {
                        this._failoverPartner = actualFailoverPartner;
                        this._failoverPermissionSet = set;
                    }
                }
            }
        }

        internal void FailoverPermissionDemand()
        {
            if (this._useFailoverPartner)
            {
                PermissionSet set = this._failoverPermissionSet;
                if (set != null)
                {
                    set.Demand();
                }
            }
        }

        internal string FailoverPartner
        {
            get
            {
                return this._failoverPartner;
            }
        }

        internal bool UseFailoverPartner
        {
            get
            {
                return this._useFailoverPartner;
            }
        }
    }
}


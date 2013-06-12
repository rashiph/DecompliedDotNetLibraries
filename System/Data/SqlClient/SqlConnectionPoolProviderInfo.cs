namespace System.Data.SqlClient
{
    using System;
    using System.Data.ProviderBase;

    internal sealed class SqlConnectionPoolProviderInfo : DbConnectionPoolProviderInfo
    {
        private string _instanceName;

        internal string InstanceName
        {
            get
            {
                return this._instanceName;
            }
            set
            {
                this._instanceName = value;
            }
        }
    }
}


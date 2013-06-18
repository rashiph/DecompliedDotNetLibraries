namespace System.Data.ProviderBase
{
    using System;

    internal class DbConnectionPoolGroupProviderInfo
    {
        private System.Data.ProviderBase.DbConnectionPoolGroup _poolGroup;

        internal System.Data.ProviderBase.DbConnectionPoolGroup PoolGroup
        {
            get
            {
                return this._poolGroup;
            }
            set
            {
                this._poolGroup = value;
            }
        }
    }
}


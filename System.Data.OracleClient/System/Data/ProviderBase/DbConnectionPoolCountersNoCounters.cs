namespace System.Data.ProviderBase
{
    using System;

    internal sealed class DbConnectionPoolCountersNoCounters : System.Data.ProviderBase.DbConnectionPoolCounters
    {
        public static readonly System.Data.ProviderBase.DbConnectionPoolCountersNoCounters SingletonInstance = new System.Data.ProviderBase.DbConnectionPoolCountersNoCounters();

        private DbConnectionPoolCountersNoCounters()
        {
        }
    }
}


namespace System.Data.ProviderBase
{
    using System;

    internal sealed class DbConnectionPoolCountersNoCounters : DbConnectionPoolCounters
    {
        public static readonly DbConnectionPoolCountersNoCounters SingletonInstance = new DbConnectionPoolCountersNoCounters();

        private DbConnectionPoolCountersNoCounters()
        {
        }
    }
}


namespace System.Data.ProviderBase
{
    using System;

    internal sealed class DbConnectionClosedNeverOpened : DbConnectionClosed
    {
        internal static readonly DbConnectionInternal SingletonInstance = new DbConnectionClosedNeverOpened();

        private DbConnectionClosedNeverOpened() : base(ConnectionState.Closed, false, true)
        {
        }
    }
}


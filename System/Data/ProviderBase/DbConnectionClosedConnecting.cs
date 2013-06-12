namespace System.Data.ProviderBase
{
    using System;

    internal sealed class DbConnectionClosedConnecting : DbConnectionBusy
    {
        internal static readonly DbConnectionInternal SingletonInstance = new DbConnectionClosedConnecting();

        private DbConnectionClosedConnecting() : base(ConnectionState.Connecting)
        {
        }
    }
}


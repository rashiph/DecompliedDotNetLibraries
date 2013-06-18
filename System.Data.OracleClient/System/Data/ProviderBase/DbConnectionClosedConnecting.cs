namespace System.Data.ProviderBase
{
    using System;

    internal sealed class DbConnectionClosedConnecting : System.Data.ProviderBase.DbConnectionBusy
    {
        internal static readonly System.Data.ProviderBase.DbConnectionInternal SingletonInstance = new System.Data.ProviderBase.DbConnectionClosedConnecting();

        private DbConnectionClosedConnecting() : base(ConnectionState.Connecting)
        {
        }
    }
}


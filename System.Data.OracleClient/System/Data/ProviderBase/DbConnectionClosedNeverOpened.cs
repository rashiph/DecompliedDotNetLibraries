namespace System.Data.ProviderBase
{
    using System;

    internal sealed class DbConnectionClosedNeverOpened : System.Data.ProviderBase.DbConnectionClosed
    {
        internal static readonly System.Data.ProviderBase.DbConnectionInternal SingletonInstance = new System.Data.ProviderBase.DbConnectionClosedNeverOpened();

        private DbConnectionClosedNeverOpened() : base(ConnectionState.Closed, false, true)
        {
        }
    }
}


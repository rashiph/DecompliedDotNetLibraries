namespace System.Data.ProviderBase
{
    using System;

    internal sealed class DbConnectionClosedBusy : System.Data.ProviderBase.DbConnectionBusy
    {
        internal static readonly System.Data.ProviderBase.DbConnectionInternal SingletonInstance = new System.Data.ProviderBase.DbConnectionClosedBusy();

        private DbConnectionClosedBusy() : base(ConnectionState.Closed)
        {
        }
    }
}


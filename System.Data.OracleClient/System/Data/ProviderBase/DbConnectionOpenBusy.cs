namespace System.Data.ProviderBase
{
    using System;

    internal sealed class DbConnectionOpenBusy : System.Data.ProviderBase.DbConnectionBusy
    {
        internal static readonly System.Data.ProviderBase.DbConnectionInternal SingletonInstance = new System.Data.ProviderBase.DbConnectionOpenBusy();

        private DbConnectionOpenBusy() : base(ConnectionState.Open)
        {
        }
    }
}


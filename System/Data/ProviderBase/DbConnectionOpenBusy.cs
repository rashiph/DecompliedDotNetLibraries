namespace System.Data.ProviderBase
{
    using System;

    internal sealed class DbConnectionOpenBusy : DbConnectionBusy
    {
        internal static readonly DbConnectionInternal SingletonInstance = new DbConnectionOpenBusy();

        private DbConnectionOpenBusy() : base(ConnectionState.Open)
        {
        }
    }
}


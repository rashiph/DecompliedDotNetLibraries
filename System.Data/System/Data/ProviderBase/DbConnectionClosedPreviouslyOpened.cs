namespace System.Data.ProviderBase
{
    using System;

    internal sealed class DbConnectionClosedPreviouslyOpened : DbConnectionClosed
    {
        internal static readonly DbConnectionInternal SingletonInstance = new DbConnectionClosedPreviouslyOpened();

        private DbConnectionClosedPreviouslyOpened() : base(ConnectionState.Closed, true, true)
        {
        }
    }
}


namespace System.Data.ProviderBase
{
    using System;

    internal sealed class DbConnectionClosedPreviouslyOpened : System.Data.ProviderBase.DbConnectionClosed
    {
        internal static readonly System.Data.ProviderBase.DbConnectionInternal SingletonInstance = new System.Data.ProviderBase.DbConnectionClosedPreviouslyOpened();

        private DbConnectionClosedPreviouslyOpened() : base(ConnectionState.Closed, true, true)
        {
        }
    }
}


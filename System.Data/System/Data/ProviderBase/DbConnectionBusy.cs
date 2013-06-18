namespace System.Data.ProviderBase
{
    using System;
    using System.Data;
    using System.Data.Common;

    internal abstract class DbConnectionBusy : DbConnectionClosed
    {
        protected DbConnectionBusy(ConnectionState state) : base(state, true, false)
        {
        }

        internal override void OpenConnection(DbConnection outerConnection, DbConnectionFactory connectionFactory)
        {
            throw ADP.ConnectionAlreadyOpen(base.State);
        }
    }
}


namespace System.Data.ProviderBase
{
    using System;
    using System.Data;
    using System.Data.Common;

    internal abstract class DbConnectionBusy : System.Data.ProviderBase.DbConnectionClosed
    {
        protected DbConnectionBusy(ConnectionState state) : base(state, true, false)
        {
        }

        internal override void OpenConnection(DbConnection outerConnection, System.Data.ProviderBase.DbConnectionFactory connectionFactory)
        {
            throw System.Data.Common.ADP.ConnectionAlreadyOpen(base.State);
        }
    }
}


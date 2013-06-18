namespace System.Data.ProviderBase
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Transactions;

    internal abstract class DbConnectionClosed : System.Data.ProviderBase.DbConnectionInternal
    {
        protected DbConnectionClosed(ConnectionState state, bool hidePassword, bool allowSetConnectionString) : base(state, hidePassword, allowSetConnectionString)
        {
        }

        protected override void Activate(Transaction transaction)
        {
            throw System.Data.Common.ADP.ClosedConnectionError();
        }

        public override DbTransaction BeginTransaction(System.Data.IsolationLevel il)
        {
            throw System.Data.Common.ADP.ClosedConnectionError();
        }

        public override void ChangeDatabase(string database)
        {
            throw System.Data.Common.ADP.ClosedConnectionError();
        }

        internal override void CloseConnection(DbConnection owningObject, System.Data.ProviderBase.DbConnectionFactory connectionFactory)
        {
        }

        protected override void Deactivate()
        {
            throw System.Data.Common.ADP.ClosedConnectionError();
        }

        public override void EnlistTransaction(Transaction transaction)
        {
            throw System.Data.Common.ADP.ClosedConnectionError();
        }

        protected internal override DataTable GetSchema(System.Data.ProviderBase.DbConnectionFactory factory, System.Data.ProviderBase.DbConnectionPoolGroup poolGroup, DbConnection outerConnection, string collectionName, string[] restrictions)
        {
            throw System.Data.Common.ADP.ClosedConnectionError();
        }

        internal override void OpenConnection(DbConnection outerConnection, System.Data.ProviderBase.DbConnectionFactory connectionFactory)
        {
            if (connectionFactory.SetInnerConnectionFrom(outerConnection, System.Data.ProviderBase.DbConnectionClosedConnecting.SingletonInstance, this))
            {
                System.Data.ProviderBase.DbConnectionInternal to = null;
                try
                {
                    connectionFactory.PermissionDemand(outerConnection);
                    to = connectionFactory.GetConnection(outerConnection);
                }
                catch
                {
                    connectionFactory.SetInnerConnectionTo(outerConnection, this);
                    throw;
                }
                if (to == null)
                {
                    connectionFactory.SetInnerConnectionTo(outerConnection, this);
                    throw System.Data.Common.ADP.InternalConnectionError(System.Data.Common.ADP.ConnectionError.GetConnectionReturnsNull);
                }
                connectionFactory.SetInnerConnectionEvent(outerConnection, to);
            }
        }

        public override string ServerVersion
        {
            get
            {
                throw System.Data.Common.ADP.ClosedConnectionError();
            }
        }
    }
}


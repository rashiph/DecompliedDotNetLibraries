namespace System.Data.ProviderBase
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Transactions;

    internal abstract class DbConnectionClosed : DbConnectionInternal
    {
        protected DbConnectionClosed(ConnectionState state, bool hidePassword, bool allowSetConnectionString) : base(state, hidePassword, allowSetConnectionString)
        {
        }

        protected override void Activate(Transaction transaction)
        {
            throw ADP.ClosedConnectionError();
        }

        public override DbTransaction BeginTransaction(System.Data.IsolationLevel il)
        {
            throw ADP.ClosedConnectionError();
        }

        public override void ChangeDatabase(string database)
        {
            throw ADP.ClosedConnectionError();
        }

        internal override void CloseConnection(DbConnection owningObject, DbConnectionFactory connectionFactory)
        {
        }

        protected override void Deactivate()
        {
            throw ADP.ClosedConnectionError();
        }

        public override void EnlistTransaction(Transaction transaction)
        {
            throw ADP.ClosedConnectionError();
        }

        protected internal override DataTable GetSchema(DbConnectionFactory factory, DbConnectionPoolGroup poolGroup, DbConnection outerConnection, string collectionName, string[] restrictions)
        {
            throw ADP.ClosedConnectionError();
        }

        internal override void OpenConnection(DbConnection outerConnection, DbConnectionFactory connectionFactory)
        {
            if (connectionFactory.SetInnerConnectionFrom(outerConnection, DbConnectionClosedConnecting.SingletonInstance, this))
            {
                DbConnectionInternal to = null;
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
                    throw ADP.InternalConnectionError(ADP.ConnectionError.GetConnectionReturnsNull);
                }
                connectionFactory.SetInnerConnectionEvent(outerConnection, to);
            }
        }

        public override string ServerVersion
        {
            get
            {
                throw ADP.ClosedConnectionError();
            }
        }
    }
}


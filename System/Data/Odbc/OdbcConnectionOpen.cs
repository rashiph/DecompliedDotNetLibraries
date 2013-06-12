namespace System.Data.Odbc
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Transactions;

    internal sealed class OdbcConnectionOpen : DbConnectionInternal
    {
        internal OdbcConnectionOpen(OdbcConnection outerConnection, OdbcConnectionString connectionOptions)
        {
            OdbcEnvironmentHandle globalEnvironmentHandle = OdbcEnvironment.GetGlobalEnvironmentHandle();
            outerConnection.ConnectionHandle = new OdbcConnectionHandle(outerConnection, connectionOptions, globalEnvironmentHandle);
        }

        protected override void Activate(Transaction transaction)
        {
            OdbcConnection.ExecutePermission.Demand();
        }

        internal OdbcTransaction BeginOdbcTransaction(System.Data.IsolationLevel isolevel)
        {
            return this.OuterConnection.Open_BeginTransaction(isolevel);
        }

        public override DbTransaction BeginTransaction(System.Data.IsolationLevel isolevel)
        {
            return this.BeginOdbcTransaction(isolevel);
        }

        public override void ChangeDatabase(string value)
        {
            this.OuterConnection.Open_ChangeDatabase(value);
        }

        protected override DbReferenceCollection CreateReferenceCollection()
        {
            return new OdbcReferenceCollection();
        }

        protected override void Deactivate()
        {
            base.NotifyWeakReference(0);
        }

        public override void EnlistTransaction(Transaction transaction)
        {
            this.OuterConnection.Open_EnlistTransaction(transaction);
        }

        internal OdbcConnection OuterConnection
        {
            get
            {
                OdbcConnection owner = (OdbcConnection) base.Owner;
                if (owner == null)
                {
                    throw ADP.InvalidOperation("internal connection without an outer connection?");
                }
                return owner;
            }
        }

        public override string ServerVersion
        {
            get
            {
                return this.OuterConnection.Open_GetServerVersion();
            }
        }
    }
}


namespace System.Data.OracleClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading;

    public sealed class OracleTransaction : DbTransaction
    {
        private OracleConnection _connection;
        private int _connectionCloseCount;
        private System.Data.IsolationLevel _isolationLevel;
        internal readonly int _objectID;
        private static int _objectTypeCount;

        internal OracleTransaction(OracleConnection connection) : this(connection, System.Data.IsolationLevel.Unspecified)
        {
        }

        internal OracleTransaction(OracleConnection connection, System.Data.IsolationLevel isolationLevel)
        {
            this._isolationLevel = System.Data.IsolationLevel.ReadCommitted;
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            TransactionState transactionState = connection.TransactionState;
            if (TransactionState.GlobalStarted == transactionState)
            {
                throw System.Data.Common.ADP.NoLocalTransactionInDistributedContext();
            }
            this._connection = connection;
            this._connectionCloseCount = connection.CloseCount;
            this._isolationLevel = isolationLevel;
            this._connection.TransactionState = TransactionState.LocalStarted;
            try
            {
                System.Data.IsolationLevel level = isolationLevel;
                if (level == System.Data.IsolationLevel.Unspecified)
                {
                    return;
                }
                if (level != System.Data.IsolationLevel.ReadCommitted)
                {
                    if (level == System.Data.IsolationLevel.Serializable)
                    {
                        goto Label_009A;
                    }
                    goto Label_00C4;
                }
                using (OracleCommand command2 = this.Connection.CreateCommand())
                {
                    command2.CommandText = "set transaction isolation level read committed";
                    command2.ExecuteNonQuery();
                    return;
                }
            Label_009A:
                using (OracleCommand command = this.Connection.CreateCommand())
                {
                    command.CommandText = "set transaction isolation level serializable";
                    command.ExecuteNonQuery();
                    return;
                }
            Label_00C4:
                throw System.Data.Common.ADP.UnsupportedIsolationLevel();
            }
            catch
            {
                this._connection.TransactionState = transactionState;
                throw;
            }
        }

        private void AssertNotCompleted()
        {
            if ((this.Connection == null) || (this._connectionCloseCount != this.Connection.CloseCount))
            {
                throw System.Data.Common.ADP.TransactionCompleted();
            }
        }

        public override void Commit()
        {
            IntPtr ptr;
            OracleConnection.ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<ora.OracleTransaction.Commit|API> %d#\n", this.ObjectID);
            try
            {
                this.AssertNotCompleted();
                this.Connection.Commit();
                this._connection = null;
                this.Dispose(true);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Connection != null)
                {
                    this.Connection.Rollback();
                }
                this._connection = null;
            }
            base.Dispose(disposing);
        }

        public override void Rollback()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ora.OracleTransaction.Rollback|API> %d#\n", this.ObjectID);
            try
            {
                this.AssertNotCompleted();
                this.Dispose(true);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public OracleConnection Connection
        {
            get
            {
                return this._connection;
            }
        }

        protected override System.Data.Common.DbConnection DbConnection
        {
            get
            {
                return this.Connection;
            }
        }

        public override System.Data.IsolationLevel IsolationLevel
        {
            get
            {
                this.AssertNotCompleted();
                if (System.Data.IsolationLevel.Unspecified == this._isolationLevel)
                {
                    using (OracleCommand command = this.Connection.CreateCommand())
                    {
                        command.Transaction = this;
                        command.CommandText = "select decode(value,'FALSE',0,1) from V$SYSTEM_PARAMETER where name = 'serializable'";
                        decimal num = (decimal) command.ExecuteScalar();
                        if (0M == num)
                        {
                            this._isolationLevel = System.Data.IsolationLevel.ReadCommitted;
                        }
                        else
                        {
                            this._isolationLevel = System.Data.IsolationLevel.Serializable;
                        }
                    }
                }
                return this._isolationLevel;
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }
    }
}


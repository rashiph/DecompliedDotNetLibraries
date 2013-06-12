namespace System.Data.SqlClient
{
    using Microsoft.SqlServer.Server;
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Transactions;

    internal sealed class SqlInternalConnectionSmi : SqlInternalConnection
    {
        private SqlInternalTransaction _currentTransaction;
        private int _isInUse;
        private SqlInternalTransaction _pendingTransaction;
        private Microsoft.SqlServer.Server.SmiConnection _smiConnection;
        private SmiContext _smiContext;
        private SmiEventSink_Default _smiEventSink;

        internal SqlInternalConnectionSmi(SqlConnectionString connectionOptions, SmiContext smiContext) : base(connectionOptions)
        {
            this._smiContext = smiContext;
            this._smiContext.OutOfScope += new EventHandler(this.OnOutOfScope);
            this._smiConnection = this._smiContext.ContextConnection;
            this._smiEventSink = new EventSink(this);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlInternalConnectionSmi.ctor|ADV> %d#, constructed new SMI internal connection\n", base.ObjectID);
            }
        }

        internal void Activate()
        {
            if (Interlocked.Exchange(ref this._isInUse, 1) != 0)
            {
                throw SQL.ContextConnectionIsInUse();
            }
            base.CurrentDatabase = this._smiConnection.GetCurrentDatabase(this._smiEventSink);
            this._smiEventSink.ProcessMessagesAndThrow();
        }

        protected override void Activate(Transaction transaction)
        {
        }

        internal override void AddPreparedCommand(SqlCommand cmd)
        {
        }

        internal void AutomaticEnlistment()
        {
            Transaction currentTransaction = ADP.GetCurrentTransaction();
            Transaction contextTransaction = this._smiContext.ContextTransaction;
            long contextTransactionId = this._smiContext.ContextTransactionId;
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlInternalConnectionSmi.AutomaticEnlistment|ADV> %d#, contextTransactionId=0x%I64x, contextTransaction=%d#, currentSystemTransaction=%d#.\n", base.ObjectID, contextTransactionId, (null != contextTransaction) ? contextTransaction.GetHashCode() : 0, (null != currentTransaction) ? currentTransaction.GetHashCode() : 0);
            }
            if (0L != contextTransactionId)
            {
                if ((null != currentTransaction) && (contextTransaction != currentTransaction))
                {
                    throw SQL.NestedTransactionScopesNotSupported();
                }
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.AutomaticEnlistment|ADV> %d#, using context transaction with transactionId=0x%I64x\n", base.ObjectID, contextTransactionId);
                }
                this._currentTransaction = new SqlInternalTransaction(this, TransactionType.Context, null, contextTransactionId);
                this.ContextTransaction = contextTransaction;
            }
            else if (null == currentTransaction)
            {
                this._currentTransaction = null;
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.AutomaticEnlistment|ADV> %d#, no transaction.\n", base.ObjectID);
                }
            }
            else
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.AutomaticEnlistment|ADV> %d#, using current System.Transaction.\n", base.ObjectID);
                }
                base.Enlist(currentTransaction);
            }
        }

        protected override void ChangeDatabaseInternal(string database)
        {
            this._smiConnection.SetCurrentDatabase(database, this._smiEventSink);
            this._smiEventSink.ProcessMessagesAndThrow();
        }

        internal override void ClearPreparedCommands()
        {
        }

        internal override void DelegatedTransactionEnded()
        {
            base.DelegatedTransactionEnded();
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlInternalConnectionSmi.DelegatedTransactionEnded|ADV> %d#, cleaning up after Delegated Transaction Completion\n", base.ObjectID);
            }
            this._currentTransaction = null;
        }

        internal override void DisconnectTransaction(SqlInternalTransaction internalTransaction)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlInternalConnectionSmi.DisconnectTransaction|ADV> %d#, Disconnecting Transaction %d#.\n", base.ObjectID, internalTransaction.ObjectID);
            }
            if ((this._currentTransaction != null) && (this._currentTransaction == internalTransaction))
            {
                this._currentTransaction = null;
            }
        }

        public override void Dispose()
        {
            this._smiContext.OutOfScope -= new EventHandler(this.OnOutOfScope);
            base.Dispose();
        }

        internal override void ExecuteTransaction(SqlInternalConnection.TransactionRequest transactionRequest, string transactionName, System.Data.IsolationLevel iso, SqlInternalTransaction internalTransaction, bool isDelegateControlRequest)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlInternalConnectionSmi.ExecuteTransaction|ADV> %d#, transactionRequest=%ls, transactionName='%ls', isolationLevel=%ls, internalTransaction=#%d transactionId=0x%I64x.\n", base.ObjectID, transactionRequest.ToString(), (transactionName != null) ? transactionName : "null", iso.ToString(), (internalTransaction != null) ? internalTransaction.ObjectID : 0, (internalTransaction != null) ? internalTransaction.TransactionId : 0L);
            }
            switch (transactionRequest)
            {
                case SqlInternalConnection.TransactionRequest.Begin:
                    try
                    {
                        this._pendingTransaction = internalTransaction;
                        this._smiConnection.BeginTransaction(transactionName, iso, this._smiEventSink);
                        goto Label_0121;
                    }
                    finally
                    {
                        this._pendingTransaction = null;
                    }
                    break;

                case SqlInternalConnection.TransactionRequest.Promote:
                    base.PromotedDTCToken = this._smiConnection.PromoteTransaction(this._currentTransaction.TransactionId, this._smiEventSink);
                    goto Label_0121;

                case SqlInternalConnection.TransactionRequest.Commit:
                    break;

                case SqlInternalConnection.TransactionRequest.Rollback:
                case SqlInternalConnection.TransactionRequest.IfRollback:
                    this._smiConnection.RollbackTransaction(this._currentTransaction.TransactionId, transactionName, this._smiEventSink);
                    goto Label_0121;

                case SqlInternalConnection.TransactionRequest.Save:
                    this._smiConnection.CreateTransactionSavePoint(this._currentTransaction.TransactionId, transactionName, this._smiEventSink);
                    goto Label_0121;

                default:
                    goto Label_0121;
            }
            this._smiConnection.CommitTransaction(this._currentTransaction.TransactionId, this._smiEventSink);
        Label_0121:
            this._smiEventSink.ProcessMessagesAndThrow();
        }

        internal void GetCurrentTransactionPair(out long transactionId, out Transaction transaction)
        {
            lock (this)
            {
                transactionId = (this.CurrentTransaction != null) ? this.CurrentTransaction.TransactionId : 0L;
                transaction = null;
                if (0L != transactionId)
                {
                    transaction = this.InternalEnlistedTransaction;
                }
            }
        }

        protected override byte[] GetDTCAddress()
        {
            byte[] dTCAddress = this._smiConnection.GetDTCAddress(this._smiEventSink);
            this._smiEventSink.ProcessMessagesAndThrow();
            if (Bid.AdvancedOn)
            {
                if (dTCAddress != null)
                {
                    Bid.TraceBin("<sc.SqlInternalConnectionSmi.GetDTCAddress|ADV> whereAbouts", dTCAddress, (ushort) dTCAddress.Length);
                    return dTCAddress;
                }
                Bid.Trace("<sc.SqlInternalConnectionSmi.GetDTCAddress|ADV> whereAbouts=null\n");
            }
            return dTCAddress;
        }

        protected override void InternalDeactivate()
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlInternalConnectionSmi.Deactivate|ADV> %d#, Deactivating.\n", base.ObjectID);
            }
            if (!this.IsNonPoolableTransactionRoot)
            {
                base.Enlist(null);
            }
            if (this._currentTransaction != null)
            {
                if (this._currentTransaction.IsContext)
                {
                    this._currentTransaction = null;
                }
                else if (this._currentTransaction.IsLocal)
                {
                    this._currentTransaction.CloseFromConnection();
                }
            }
            this.ContextTransaction = null;
            this._isInUse = 0;
        }

        private void OnOutOfScope(object s, EventArgs e)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlInternalConnectionSmi.OutOfScope|ADV> %d# context is out of scope\n", base.ObjectID);
            }
            base.DelegatedTransaction = null;
            DbConnection owner = (DbConnection) base.Owner;
            try
            {
                if ((owner != null) && (1 == this._isInUse))
                {
                    owner.Close();
                }
            }
            finally
            {
                this.ContextTransaction = null;
                this._isInUse = 0;
            }
        }

        protected override void PropagateTransactionCookie(byte[] transactionCookie)
        {
            if (Bid.AdvancedOn)
            {
                if (transactionCookie != null)
                {
                    Bid.TraceBin("<sc.SqlInternalConnectionSmi.PropagateTransactionCookie|ADV> transactionCookie", transactionCookie, (ushort) transactionCookie.Length);
                }
                else
                {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.PropagateTransactionCookie|ADV> null\n");
                }
            }
            this._smiConnection.EnlistTransaction(transactionCookie, this._smiEventSink);
            this._smiEventSink.ProcessMessagesAndThrow();
        }

        internal override void RemovePreparedCommand(SqlCommand cmd)
        {
        }

        private void TransactionEnded(long transactionId, System.Data.SqlClient.TransactionState transactionState)
        {
            if (this._currentTransaction != null)
            {
                this._currentTransaction.Completed(transactionState);
                this._currentTransaction = null;
            }
        }

        private void TransactionEndedByServer(long transactionId)
        {
            SqlDelegatedTransaction delegatedTransaction = base.DelegatedTransaction;
            if (delegatedTransaction != null)
            {
                delegatedTransaction.Transaction.Rollback();
                base.DelegatedTransaction = null;
            }
            this.TransactionEnded(transactionId, System.Data.SqlClient.TransactionState.Unknown);
        }

        private void TransactionStarted(long transactionId, bool isDistributed)
        {
            this._currentTransaction = this._pendingTransaction;
            this._pendingTransaction = null;
            if (this._currentTransaction != null)
            {
                this._currentTransaction.TransactionId = transactionId;
            }
            else
            {
                TransactionType type = isDistributed ? TransactionType.Distributed : TransactionType.LocalFromTSQL;
                this._currentTransaction = new SqlInternalTransaction(this, type, null, transactionId);
            }
            this._currentTransaction.Activate();
        }

        internal override void ValidateConnectionForExecute(SqlCommand command)
        {
            if (base.FindLiveReader(null) != null)
            {
                throw ADP.OpenReaderExists();
            }
        }

        private Transaction ContextTransaction { get; set; }

        internal SmiEventSink CurrentEventSink
        {
            get
            {
                return this._smiEventSink;
            }
        }

        internal override SqlInternalTransaction CurrentTransaction
        {
            get
            {
                return this._currentTransaction;
            }
        }

        internal SmiContext InternalContext
        {
            get
            {
                return this._smiContext;
            }
        }

        private Transaction InternalEnlistedTransaction
        {
            get
            {
                Transaction enlistedTransaction = base.EnlistedTransaction;
                if (null == enlistedTransaction)
                {
                    enlistedTransaction = this.ContextTransaction;
                }
                return enlistedTransaction;
            }
        }

        internal override bool IsKatmaiOrNewer
        {
            get
            {
                return (SmiContextFactory.Instance.NegotiatedSmiVersion >= 210L);
            }
        }

        internal override bool IsLockedForBulkCopy
        {
            get
            {
                return false;
            }
        }

        internal override bool IsShiloh
        {
            get
            {
                return false;
            }
        }

        internal override bool IsYukonOrNewer
        {
            get
            {
                return true;
            }
        }

        internal override SqlInternalTransaction PendingTransaction
        {
            get
            {
                return this.CurrentTransaction;
            }
        }

        public override string ServerVersion
        {
            get
            {
                return SmiContextFactory.Instance.ServerVersion;
            }
        }

        internal Microsoft.SqlServer.Server.SmiConnection SmiConnection
        {
            get
            {
                return this._smiConnection;
            }
        }

        protected override bool UnbindOnTransactionCompletion
        {
            get
            {
                return (base.ConnectionOptions.TransactionBinding == SqlConnectionString.TransactionBindingEnum.ImplicitUnbind);
            }
        }

        private sealed class EventSink : SmiEventSink_Default
        {
            private SqlInternalConnectionSmi _connection;

            internal EventSink(SqlInternalConnectionSmi connection)
            {
                this._connection = connection;
            }

            internal override void DefaultDatabaseChanged(string databaseName)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.EventSink.DefaultDatabaseChanged|ADV> %d#, databaseName='%ls'.\n", this._connection.ObjectID, databaseName);
                }
                this._connection.CurrentDatabase = databaseName;
            }

            protected override void DispatchMessages(bool ignoreNonFatalMessages)
            {
                SqlException exception = base.ProcessMessages(false, ignoreNonFatalMessages);
                if (exception != null)
                {
                    SqlConnection connection = this._connection.Connection;
                    if ((connection != null) && connection.FireInfoMessageEventOnUserErrors)
                    {
                        connection.OnInfoMessage(new SqlInfoMessageEventArgs(exception));
                    }
                    else
                    {
                        this._connection.OnError(exception, false);
                    }
                }
            }

            internal override void TransactionCommitted(long transactionId)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.EventSink.TransactionCommitted|ADV> %d#, transactionId=0x%I64x.\n", this._connection.ObjectID, transactionId);
                }
                this._connection.TransactionEnded(transactionId, System.Data.SqlClient.TransactionState.Committed);
            }

            internal override void TransactionDefected(long transactionId)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.EventSink.TransactionDefected|ADV> %d#, transactionId=0x%I64x.\n", this._connection.ObjectID, transactionId);
                }
                this._connection.TransactionEnded(transactionId, System.Data.SqlClient.TransactionState.Unknown);
            }

            internal override void TransactionEnded(long transactionId)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.EventSink.TransactionEnded|ADV> %d#, transactionId=0x%I64x.\n", this._connection.ObjectID, transactionId);
                }
                this._connection.TransactionEndedByServer(transactionId);
            }

            internal override void TransactionEnlisted(long transactionId)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.EventSink.TransactionEnlisted|ADV> %d#, transactionId=0x%I64x.\n", this._connection.ObjectID, transactionId);
                }
                this._connection.TransactionStarted(transactionId, true);
            }

            internal override void TransactionRolledBack(long transactionId)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.EventSink.TransactionRolledBack|ADV> %d#, transactionId=0x%I64x.\n", this._connection.ObjectID, transactionId);
                }
                this._connection.TransactionEnded(transactionId, System.Data.SqlClient.TransactionState.Aborted);
            }

            internal override void TransactionStarted(long transactionId)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.EventSink.TransactionStarted|ADV> %d#, transactionId=0x%I64x.\n", this._connection.ObjectID, transactionId);
                }
                this._connection.TransactionStarted(transactionId, false);
            }

            internal override string ServerVersion
            {
                get
                {
                    return SmiContextFactory.Instance.ServerVersion;
                }
            }
        }
    }
}


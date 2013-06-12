namespace System.Data.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Threading;
    using System.Transactions;

    internal abstract class SqlInternalConnection : DbConnectionInternal
    {
        private readonly SqlConnectionString _connectionOptions;
        private bool _isEnlistedInTransaction;
        private byte[] _promotedDTCToken;
        private byte[] _whereAbouts;

        internal SqlInternalConnection(SqlConnectionString connectionOptions)
        {
            this._connectionOptions = connectionOptions;
        }

        internal abstract void AddPreparedCommand(SqlCommand cmd);
        internal virtual SqlTransaction BeginSqlTransaction(System.Data.IsolationLevel iso, string transactionName)
        {
            SNIHandle target = null;
            SqlStatistics statistics = null;
            SqlTransaction transaction2;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target = GetBestEffortCleanupTarget(this.Connection);
                statistics = SqlStatistics.StartTimer(this.Connection.Statistics);
                SqlConnection.ExecutePermission.Demand();
                this.ValidateConnectionForExecute(null);
                if (this.HasLocalTransactionFromAPI)
                {
                    throw ADP.ParallelTransactionsNotSupported(this.Connection);
                }
                if (iso == System.Data.IsolationLevel.Unspecified)
                {
                    iso = System.Data.IsolationLevel.ReadCommitted;
                }
                SqlTransaction transaction = new SqlTransaction(this, this.Connection, iso, this.AvailableInternalTransaction);
                this.ExecuteTransaction(TransactionRequest.Begin, transactionName, iso, transaction.InternalTransaction, false);
                return transaction;
            }
            catch (OutOfMemoryException exception3)
            {
                this.Connection.Abort(exception3);
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this.Connection.Abort(exception2);
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this.Connection.Abort(exception);
                BestEffortCleanup(target);
                throw;
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return transaction2;
        }

        public override DbTransaction BeginTransaction(System.Data.IsolationLevel iso)
        {
            return this.BeginSqlTransaction(iso, null);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static void BestEffortCleanup(SNIHandle target)
        {
            if (target != null)
            {
                target.Dispose();
            }
        }

        public override void ChangeDatabase(string database)
        {
            SqlConnection.ExecutePermission.Demand();
            if (ADP.IsEmpty(database))
            {
                throw ADP.EmptyDatabaseName();
            }
            this.ValidateConnectionForExecute(null);
            this.ChangeDatabaseInternal(database);
        }

        protected abstract void ChangeDatabaseInternal(string database);
        protected override void CleanupTransactionOnCompletion(Transaction transaction)
        {
            SqlDelegatedTransaction delegatedTransaction = this.DelegatedTransaction;
            if (delegatedTransaction != null)
            {
                delegatedTransaction.TransactionEnded(transaction);
            }
        }

        internal abstract void ClearPreparedCommands();
        internal override void CloseConnection(DbConnection owningObject, DbConnectionFactory connectionFactory)
        {
            if (!base.IsConnectionDoomed)
            {
                this.ClearPreparedCommands();
            }
            base.CloseConnection(owningObject, connectionFactory);
        }

        protected override DbReferenceCollection CreateReferenceCollection()
        {
            return new SqlReferenceCollection();
        }

        protected override void Deactivate()
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlInternalConnection.Deactivate|ADV> %d# deactivating\n", base.ObjectID);
            }
            SNIHandle target = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target = GetBestEffortCleanupTarget(this.Connection);
                SqlReferenceCollection referenceCollection = (SqlReferenceCollection) base.ReferenceCollection;
                if (referenceCollection != null)
                {
                    referenceCollection.Deactivate();
                }
                this.InternalDeactivate();
            }
            catch (OutOfMemoryException)
            {
                base.DoomThisConnection();
                throw;
            }
            catch (StackOverflowException)
            {
                base.DoomThisConnection();
                throw;
            }
            catch (ThreadAbortException)
            {
                base.DoomThisConnection();
                BestEffortCleanup(target);
                throw;
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                base.DoomThisConnection();
                ADP.TraceExceptionWithoutRethrow(exception);
            }
        }

        internal abstract void DisconnectTransaction(SqlInternalTransaction internalTransaction);
        public override void Dispose()
        {
            this._whereAbouts = null;
            base.Dispose();
        }

        protected void Enlist(Transaction tx)
        {
            if (null == tx)
            {
                if (this.IsEnlistedInTransaction)
                {
                    this.EnlistNull();
                }
                else
                {
                    Transaction enlistedTransaction = base.EnlistedTransaction;
                    if ((enlistedTransaction != null) && (enlistedTransaction.TransactionInformation.Status != TransactionStatus.Active))
                    {
                        this.EnlistNull();
                    }
                }
            }
            else if (!tx.Equals(base.EnlistedTransaction))
            {
                this.EnlistNonNull(tx);
            }
        }

        private void EnlistNonNull(Transaction tx)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlInternalConnection.EnlistNonNull|ADV> %d#, transaction %d#.\n", base.ObjectID, tx.GetHashCode());
            }
            bool flag = false;
            if (this.IsYukonOrNewer)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlInternalConnection.EnlistNonNull|ADV> %d#, attempting to delegate\n", base.ObjectID);
                }
                SqlDelegatedTransaction promotableSinglePhaseNotification = new SqlDelegatedTransaction(this, tx);
                try
                {
                    if (tx.EnlistPromotableSinglePhase(promotableSinglePhaseNotification))
                    {
                        flag = true;
                        this.DelegatedTransaction = promotableSinglePhaseNotification;
                        if (Bid.AdvancedOn)
                        {
                            long transactionId = 0L;
                            int objectID = 0;
                            if (this.CurrentTransaction != null)
                            {
                                transactionId = this.CurrentTransaction.TransactionId;
                                objectID = this.CurrentTransaction.ObjectID;
                            }
                            Bid.Trace("<sc.SqlInternalConnection.EnlistNonNull|ADV> %d#, delegated to transaction %d# with transactionId=0x%I64x\n", base.ObjectID, objectID, transactionId);
                        }
                    }
                }
                catch (SqlException exception)
                {
                    if (exception.Class >= 20)
                    {
                        throw;
                    }
                    SqlInternalConnectionTds tds = this as SqlInternalConnectionTds;
                    if (tds != null)
                    {
                        TdsParser parser = tds.Parser;
                        if ((parser == null) || (parser.State != TdsParserState.OpenLoggedIn))
                        {
                            throw;
                        }
                    }
                    ADP.TraceExceptionWithoutRethrow(exception);
                }
            }
            if (!flag)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlInternalConnection.EnlistNonNull|ADV> %d#, delegation not possible, enlisting.\n", base.ObjectID);
                }
                byte[] transactionCookie = null;
                if (this._whereAbouts == null)
                {
                    byte[] dTCAddress = this.GetDTCAddress();
                    if (dTCAddress == null)
                    {
                        throw SQL.CannotGetDTCAddress();
                    }
                    this._whereAbouts = dTCAddress;
                }
                transactionCookie = GetTransactionCookie(tx, this._whereAbouts);
                this.PropagateTransactionCookie(transactionCookie);
                this._isEnlistedInTransaction = true;
                if (Bid.AdvancedOn)
                {
                    long num2 = 0L;
                    int num = 0;
                    if (this.CurrentTransaction != null)
                    {
                        num2 = this.CurrentTransaction.TransactionId;
                        num = this.CurrentTransaction.ObjectID;
                    }
                    Bid.Trace("<sc.SqlInternalConnection.EnlistNonNull|ADV> %d#, enlisted with transaction %d# with transactionId=0x%I64x\n", base.ObjectID, num, num2);
                }
            }
            base.EnlistedTransaction = tx;
        }

        internal void EnlistNull()
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlInternalConnection.EnlistNull|ADV> %d#, unenlisting.\n", base.ObjectID);
            }
            this.PropagateTransactionCookie(null);
            this._isEnlistedInTransaction = false;
            base.EnlistedTransaction = null;
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlInternalConnection.EnlistNull|ADV> %d#, unenlisted.\n", base.ObjectID);
            }
        }

        public override void EnlistTransaction(Transaction transaction)
        {
            this.ValidateConnectionForExecute(null);
            if (this.HasLocalTransaction)
            {
                throw ADP.LocalTransactionPresent();
            }
            if ((null == transaction) || !transaction.Equals(base.EnlistedTransaction))
            {
                SNIHandle target = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    target = GetBestEffortCleanupTarget(this.Connection);
                    this.Enlist(transaction);
                }
                catch (OutOfMemoryException exception3)
                {
                    this.Connection.Abort(exception3);
                    throw;
                }
                catch (StackOverflowException exception2)
                {
                    this.Connection.Abort(exception2);
                    throw;
                }
                catch (ThreadAbortException exception)
                {
                    this.Connection.Abort(exception);
                    BestEffortCleanup(target);
                    throw;
                }
            }
        }

        internal abstract void ExecuteTransaction(TransactionRequest transactionRequest, string name, System.Data.IsolationLevel iso, SqlInternalTransaction internalTransaction, bool isDelegateControlRequest);
        internal SqlDataReader FindLiveReader(SqlCommand command)
        {
            SqlDataReader reader = null;
            SqlReferenceCollection referenceCollection = (SqlReferenceCollection) base.ReferenceCollection;
            if (referenceCollection != null)
            {
                reader = referenceCollection.FindLiveReader(command);
            }
            return reader;
        }

        internal static SNIHandle GetBestEffortCleanupTarget(SqlConnection connection)
        {
            if (connection != null)
            {
                SqlInternalConnectionTds innerConnection = connection.InnerConnection as SqlInternalConnectionTds;
                if (innerConnection != null)
                {
                    TdsParser parser = innerConnection.Parser;
                    if (parser != null)
                    {
                        return parser.GetBestEffortCleanupTarget();
                    }
                }
            }
            return null;
        }

        protected abstract byte[] GetDTCAddress();
        private static byte[] GetTransactionCookie(Transaction transaction, byte[] whereAbouts)
        {
            byte[] exportCookie = null;
            if (null != transaction)
            {
                exportCookie = TransactionInterop.GetExportCookie(transaction, whereAbouts);
            }
            return exportCookie;
        }

        protected virtual void InternalDeactivate()
        {
        }

        internal void OnError(SqlException exception, bool breakConnection)
        {
            if (breakConnection)
            {
                base.DoomThisConnection();
            }
            if (this.Connection != null)
            {
                this.Connection.OnError(exception, breakConnection);
            }
            else if (exception.Class >= 11)
            {
                throw exception;
            }
        }

        protected abstract void PropagateTransactionCookie(byte[] transactionCookie);
        internal abstract void RemovePreparedCommand(SqlCommand cmd);
        internal abstract void ValidateConnectionForExecute(SqlCommand command);

        internal virtual SqlInternalTransaction AvailableInternalTransaction
        {
            get
            {
                return this.CurrentTransaction;
            }
        }

        internal SqlConnection Connection
        {
            get
            {
                return (SqlConnection) base.Owner;
            }
        }

        internal SqlConnectionString ConnectionOptions
        {
            get
            {
                return this._connectionOptions;
            }
        }

        internal string CurrentDatabase { get; set; }

        internal string CurrentDataSource { get; set; }

        internal abstract SqlInternalTransaction CurrentTransaction { get; }

        internal SqlDelegatedTransaction DelegatedTransaction { get; set; }

        internal bool HasLocalTransaction
        {
            get
            {
                SqlInternalTransaction currentTransaction = this.CurrentTransaction;
                return ((currentTransaction != null) && currentTransaction.IsLocal);
            }
        }

        internal bool HasLocalTransactionFromAPI
        {
            get
            {
                SqlInternalTransaction currentTransaction = this.CurrentTransaction;
                return ((currentTransaction != null) && currentTransaction.HasParentTransaction);
            }
        }

        internal bool IsEnlistedInTransaction
        {
            get
            {
                return this._isEnlistedInTransaction;
            }
        }

        internal abstract bool IsKatmaiOrNewer { get; }

        internal abstract bool IsLockedForBulkCopy { get; }

        protected internal override bool IsNonPoolableTransactionRoot
        {
            get
            {
                return this.IsTransactionRoot;
            }
        }

        internal abstract bool IsShiloh { get; }

        internal override bool IsTransactionRoot
        {
            get
            {
                if (this.DelegatedTransaction == null)
                {
                    return false;
                }
                lock (this)
                {
                    return ((this.DelegatedTransaction != null) && this.DelegatedTransaction.IsActive);
                }
            }
        }

        internal abstract bool IsYukonOrNewer { get; }

        internal abstract SqlInternalTransaction PendingTransaction { get; }

        internal byte[] PromotedDTCToken
        {
            get
            {
                return this._promotedDTCToken;
            }
            set
            {
                this._promotedDTCToken = value;
            }
        }

        internal enum TransactionRequest
        {
            Begin,
            Promote,
            Commit,
            Rollback,
            IfRollback,
            Save
        }
    }
}


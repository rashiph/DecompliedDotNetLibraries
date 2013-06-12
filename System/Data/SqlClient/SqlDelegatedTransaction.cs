namespace System.Data.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Transactions;

    internal sealed class SqlDelegatedTransaction : IPromotableSinglePhaseNotification, ITransactionPromoter
    {
        private bool _active;
        private System.Transactions.Transaction _atomicTransaction;
        private SqlInternalConnection _connection;
        private SqlInternalTransaction _internalTransaction;
        private System.Data.IsolationLevel _isolationLevel;
        private readonly int _objectID = Interlocked.Increment(ref _objectTypeCount);
        private static int _objectTypeCount;

        internal SqlDelegatedTransaction(SqlInternalConnection connection, System.Transactions.Transaction tx)
        {
            this._connection = connection;
            this._atomicTransaction = tx;
            this._active = false;
            System.Transactions.IsolationLevel isolationLevel = tx.IsolationLevel;
            switch (isolationLevel)
            {
                case System.Transactions.IsolationLevel.Serializable:
                    this._isolationLevel = System.Data.IsolationLevel.Serializable;
                    return;

                case System.Transactions.IsolationLevel.RepeatableRead:
                    this._isolationLevel = System.Data.IsolationLevel.RepeatableRead;
                    return;

                case System.Transactions.IsolationLevel.ReadCommitted:
                    this._isolationLevel = System.Data.IsolationLevel.ReadCommitted;
                    return;

                case System.Transactions.IsolationLevel.ReadUncommitted:
                    this._isolationLevel = System.Data.IsolationLevel.ReadUncommitted;
                    return;

                case System.Transactions.IsolationLevel.Snapshot:
                    this._isolationLevel = System.Data.IsolationLevel.Snapshot;
                    return;
            }
            throw SQL.UnknownSysTxIsolationLevel(isolationLevel);
        }

        private SqlInternalConnection GetValidConnection()
        {
            SqlInternalConnection connection = this._connection;
            if (connection == null)
            {
                throw ADP.ObjectDisposed(this);
            }
            return connection;
        }

        public void Initialize()
        {
            SqlInternalConnection innerConnection = this._connection;
            SqlConnection connection = innerConnection.Connection;
            Bid.Trace("<sc.SqlDelegatedTransaction.Initialize|RES|CPOOL> %d#, Connection %d#, delegating transaction.\n", this.ObjectID, innerConnection.ObjectID);
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (innerConnection.IsEnlistedInTransaction)
                {
                    Bid.Trace("<sc.SqlDelegatedTransaction.Initialize|RES|CPOOL> %d#, Connection %d#, was enlisted, now defecting.\n", this.ObjectID, innerConnection.ObjectID);
                    innerConnection.EnlistNull();
                }
                this._internalTransaction = new SqlInternalTransaction(innerConnection, TransactionType.Delegated, null);
                innerConnection.ExecuteTransaction(SqlInternalConnection.TransactionRequest.Begin, null, this._isolationLevel, this._internalTransaction, true);
                if (innerConnection.CurrentTransaction == null)
                {
                    innerConnection.DoomThisConnection();
                    throw ADP.InternalError(ADP.InternalErrorCode.UnknownTransactionFailure);
                }
                this._active = true;
            }
            catch (OutOfMemoryException exception3)
            {
                connection.Abort(exception3);
                throw;
            }
            catch (StackOverflowException exception2)
            {
                connection.Abort(exception2);
                throw;
            }
            catch (ThreadAbortException exception)
            {
                connection.Abort(exception);
                throw;
            }
        }

        public byte[] Promote()
        {
            Exception exception;
            SqlInternalConnection validConnection = this.GetValidConnection();
            byte[] promotedDTCToken = null;
            SqlConnection connection = validConnection.Connection;
            Bid.Trace("<sc.SqlDelegatedTransaction.Promote|RES|CPOOL> %d#, Connection %d#, promoting transaction.\n", this.ObjectID, validConnection.ObjectID);
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                lock (validConnection)
                {
                    try
                    {
                        this.ValidateActiveOnConnection(validConnection);
                        validConnection.ExecuteTransaction(SqlInternalConnection.TransactionRequest.Promote, null, System.Data.IsolationLevel.Unspecified, this._internalTransaction, true);
                        promotedDTCToken = this._connection.PromotedDTCToken;
                        exception = null;
                    }
                    catch (SqlException exception3)
                    {
                        exception = exception3;
                        ADP.TraceExceptionWithoutRethrow(exception3);
                        validConnection.DoomThisConnection();
                    }
                    catch (InvalidOperationException exception2)
                    {
                        exception = exception2;
                        ADP.TraceExceptionWithoutRethrow(exception2);
                        validConnection.DoomThisConnection();
                    }
                }
            }
            catch (OutOfMemoryException exception6)
            {
                connection.Abort(exception6);
                throw;
            }
            catch (StackOverflowException exception5)
            {
                connection.Abort(exception5);
                throw;
            }
            catch (ThreadAbortException exception4)
            {
                connection.Abort(exception4);
                throw;
            }
            if (exception != null)
            {
                throw SQL.PromotionFailed(exception);
            }
            return promotedDTCToken;
        }

        public void Rollback(SinglePhaseEnlistment enlistment)
        {
            SqlInternalConnection validConnection = this.GetValidConnection();
            SqlConnection connection = validConnection.Connection;
            Bid.Trace("<sc.SqlDelegatedTransaction.Rollback|RES|CPOOL> %d#, Connection %d#, aborting transaction.\n", this.ObjectID, validConnection.ObjectID);
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                lock (validConnection)
                {
                    try
                    {
                        this.ValidateActiveOnConnection(validConnection);
                        this._active = false;
                        this._connection = null;
                        validConnection.ExecuteTransaction(SqlInternalConnection.TransactionRequest.Rollback, null, System.Data.IsolationLevel.Unspecified, this._internalTransaction, true);
                    }
                    catch (SqlException exception5)
                    {
                        ADP.TraceExceptionWithoutRethrow(exception5);
                        validConnection.DoomThisConnection();
                    }
                    catch (InvalidOperationException exception4)
                    {
                        ADP.TraceExceptionWithoutRethrow(exception4);
                        validConnection.DoomThisConnection();
                    }
                }
                validConnection.CleanupConnectionOnTransactionCompletion(this._atomicTransaction);
                enlistment.Aborted();
            }
            catch (OutOfMemoryException exception3)
            {
                connection.Abort(exception3);
                throw;
            }
            catch (StackOverflowException exception2)
            {
                connection.Abort(exception2);
                throw;
            }
            catch (ThreadAbortException exception)
            {
                connection.Abort(exception);
                throw;
            }
        }

        public void SinglePhaseCommit(SinglePhaseEnlistment enlistment)
        {
            SqlInternalConnection validConnection = this.GetValidConnection();
            SqlConnection connection = validConnection.Connection;
            Bid.Trace("<sc.SqlDelegatedTransaction.SinglePhaseCommit|RES|CPOOL> %d#, Connection %d#, committing transaction.\n", this.ObjectID, validConnection.ObjectID);
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (validConnection.IsConnectionDoomed)
                {
                    lock (validConnection)
                    {
                        this._active = false;
                        this._connection = null;
                    }
                    enlistment.Aborted(SQL.ConnectionDoomed());
                }
                else
                {
                    Exception exception;
                    lock (validConnection)
                    {
                        try
                        {
                            this.ValidateActiveOnConnection(validConnection);
                            this._active = false;
                            this._connection = null;
                            validConnection.ExecuteTransaction(SqlInternalConnection.TransactionRequest.Commit, null, System.Data.IsolationLevel.Unspecified, this._internalTransaction, true);
                            exception = null;
                        }
                        catch (SqlException exception3)
                        {
                            exception = exception3;
                            ADP.TraceExceptionWithoutRethrow(exception3);
                            validConnection.DoomThisConnection();
                        }
                        catch (InvalidOperationException exception2)
                        {
                            exception = exception2;
                            ADP.TraceExceptionWithoutRethrow(exception2);
                            validConnection.DoomThisConnection();
                        }
                    }
                    if (exception != null)
                    {
                        if (this._internalTransaction.IsCommitted)
                        {
                            enlistment.Committed();
                        }
                        else if (this._internalTransaction.IsAborted)
                        {
                            enlistment.Aborted(exception);
                        }
                        else
                        {
                            enlistment.InDoubt(exception);
                        }
                    }
                    validConnection.CleanupConnectionOnTransactionCompletion(this._atomicTransaction);
                    if (exception == null)
                    {
                        enlistment.Committed();
                    }
                }
            }
            catch (OutOfMemoryException exception6)
            {
                connection.Abort(exception6);
                throw;
            }
            catch (StackOverflowException exception5)
            {
                connection.Abort(exception5);
                throw;
            }
            catch (ThreadAbortException exception4)
            {
                connection.Abort(exception4);
                throw;
            }
        }

        internal void TransactionEnded(System.Transactions.Transaction transaction)
        {
            SqlInternalConnection connection = this._connection;
            if (connection != null)
            {
                Bid.Trace("<sc.SqlDelegatedTransaction.TransactionEnded|RES|CPOOL> %d#, Connection %d#, transaction completed externally.\n", this.ObjectID, connection.ObjectID);
                lock (connection)
                {
                    if (this._atomicTransaction.Equals(transaction))
                    {
                        this._active = false;
                        this._connection = null;
                    }
                }
            }
        }

        private void ValidateActiveOnConnection(SqlInternalConnection connection)
        {
            if ((!this._active || (connection != this._connection)) || (connection.DelegatedTransaction != this))
            {
                if (connection != null)
                {
                    connection.DoomThisConnection();
                }
                if ((connection != this._connection) && (this._connection != null))
                {
                    this._connection.DoomThisConnection();
                }
                throw ADP.InternalError(ADP.InternalErrorCode.UnpooledObjectHasWrongOwner);
            }
        }

        internal bool IsActive
        {
            get
            {
                return this._active;
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        internal System.Transactions.Transaction Transaction
        {
            get
            {
                return this._atomicTransaction;
            }
        }
    }
}


namespace System.Data.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading;

    internal sealed class SqlInternalTransaction
    {
        private bool _disposing;
        private SqlInternalConnection _innerConnection;
        internal readonly int _objectID;
        private static int _objectTypeCount;
        private int _openResultCount;
        private WeakReference _parent;
        private long _transactionId;
        private TransactionState _transactionState;
        private TransactionType _transactionType;
        internal const long NullTransactionId = 0L;

        internal SqlInternalTransaction(SqlInternalConnection innerConnection, TransactionType type, SqlTransaction outerTransaction) : this(innerConnection, type, outerTransaction, 0L)
        {
        }

        internal SqlInternalTransaction(SqlInternalConnection innerConnection, TransactionType type, SqlTransaction outerTransaction, long transactionId)
        {
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            Bid.PoolerTrace("<sc.SqlInternalTransaction.ctor|RES|CPOOL> %d#, Created for connection %d#, outer transaction %d#, Type %d\n", this.ObjectID, innerConnection.ObjectID, (outerTransaction != null) ? outerTransaction.ObjectID : -1, (int) type);
            this._innerConnection = innerConnection;
            this._transactionType = type;
            if (outerTransaction != null)
            {
                this._parent = new WeakReference(outerTransaction);
            }
            this._transactionId = transactionId;
        }

        internal void Activate()
        {
            this._transactionState = TransactionState.Active;
        }

        private void CheckTransactionLevelAndZombie()
        {
            try
            {
                if (!this.IsZombied && (this.GetServerTransactionLevel() == 0))
                {
                    this.Zombie();
                }
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                ADP.TraceExceptionWithoutRethrow(exception);
                this.Zombie();
            }
        }

        internal void CloseFromConnection()
        {
            SqlInternalConnection connection = this._innerConnection;
            Bid.PoolerTrace("<sc.SqlInteralTransaction.CloseFromConnection|RES|CPOOL> %d#, Closing\n", this.ObjectID);
            bool flag = true;
            try
            {
                connection.ExecuteTransaction(SqlInternalConnection.TransactionRequest.IfRollback, null, IsolationLevel.Unspecified, null, false);
            }
            catch (Exception exception)
            {
                flag = ADP.IsCatchableExceptionType(exception);
                throw;
            }
            finally
            {
                if (flag)
                {
                    this.Zombie();
                }
            }
        }

        internal void Commit()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<sc.SqlInternalTransaction.Commit|API> %d#", this.ObjectID);
            if (this._innerConnection.IsLockedForBulkCopy)
            {
                throw SQL.ConnectionLockedForBcpEvent();
            }
            this._innerConnection.ValidateConnectionForExecute(null);
            try
            {
                this._innerConnection.ExecuteTransaction(SqlInternalConnection.TransactionRequest.Commit, null, IsolationLevel.Unspecified, null, false);
                if (!this.IsZombied && !this._innerConnection.IsYukonOrNewer)
                {
                    this.Zombie();
                }
                else
                {
                    this.ZombieParent();
                }
            }
            catch (Exception exception)
            {
                if (ADP.IsCatchableExceptionType(exception))
                {
                    this.CheckTransactionLevelAndZombie();
                }
                throw;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal void Completed(TransactionState transactionState)
        {
            this._transactionState = transactionState;
            this.Zombie();
        }

        internal int DecrementAndObtainOpenResultCount()
        {
            int num = Interlocked.Decrement(ref this._openResultCount);
            if (num < 0)
            {
                throw ADP.InvalidOperation("Internal Error: Open Result Count Exceeded");
            }
            return num;
        }

        internal void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            Bid.PoolerTrace("<sc.SqlInteralTransaction.Dispose|RES|CPOOL> %d#, Disposing\n", this.ObjectID);
            if (disposing && (this._innerConnection != null))
            {
                this._disposing = true;
                this.Rollback();
            }
        }

        private int GetServerTransactionLevel()
        {
            using (SqlCommand command = new SqlCommand("set @out = @@trancount", (SqlConnection) this._innerConnection.Owner))
            {
                command.Transaction = this.Parent;
                SqlParameter parameter = new SqlParameter("@out", SqlDbType.Int) {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(parameter);
                command.RunExecuteReader(CommandBehavior.Default, RunBehavior.UntilDone, false, "GetServerTransactionLevel");
                return (int) parameter.Value;
            }
        }

        internal int IncrementAndObtainOpenResultCount()
        {
            int num = Interlocked.Increment(ref this._openResultCount);
            if (num < 0)
            {
                throw ADP.InvalidOperation("Internal Error: Open Result Count Exceeded");
            }
            return num;
        }

        internal void InitParent(SqlTransaction transaction)
        {
            this._parent = new WeakReference(transaction);
        }

        internal void Rollback()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<sc.SqlInternalTransaction.Rollback|API> %d#", this.ObjectID);
            if (this._innerConnection.IsLockedForBulkCopy)
            {
                throw SQL.ConnectionLockedForBcpEvent();
            }
            this._innerConnection.ValidateConnectionForExecute(null);
            try
            {
                this._innerConnection.ExecuteTransaction(SqlInternalConnection.TransactionRequest.IfRollback, null, IsolationLevel.Unspecified, null, false);
                this.Zombie();
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                this.CheckTransactionLevelAndZombie();
                if (!this._disposing)
                {
                    throw;
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal void Rollback(string transactionName)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<sc.SqlInternalTransaction.Rollback|API> %d#, transactionName='%ls'", this.ObjectID, transactionName);
            if (this._innerConnection.IsLockedForBulkCopy)
            {
                throw SQL.ConnectionLockedForBcpEvent();
            }
            this._innerConnection.ValidateConnectionForExecute(null);
            try
            {
                if (ADP.IsEmpty(transactionName))
                {
                    throw SQL.NullEmptyTransactionName();
                }
                try
                {
                    this._innerConnection.ExecuteTransaction(SqlInternalConnection.TransactionRequest.Rollback, transactionName, IsolationLevel.Unspecified, null, false);
                    if (!this.IsZombied && !this._innerConnection.IsYukonOrNewer)
                    {
                        this.CheckTransactionLevelAndZombie();
                    }
                }
                catch (Exception exception)
                {
                    if (ADP.IsCatchableExceptionType(exception))
                    {
                        this.CheckTransactionLevelAndZombie();
                    }
                    throw;
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal void Save(string savePointName)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<sc.SqlInternalTransaction.Save|API> %d#, savePointName='%ls'", this.ObjectID, savePointName);
            this._innerConnection.ValidateConnectionForExecute(null);
            try
            {
                if (ADP.IsEmpty(savePointName))
                {
                    throw SQL.NullEmptyTransactionName();
                }
                try
                {
                    this._innerConnection.ExecuteTransaction(SqlInternalConnection.TransactionRequest.Save, savePointName, IsolationLevel.Unspecified, null, false);
                }
                catch (Exception exception)
                {
                    if (ADP.IsCatchableExceptionType(exception))
                    {
                        this.CheckTransactionLevelAndZombie();
                    }
                    throw;
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal string TraceString()
        {
            return string.Format(null, "(ObjId={0}, tranId={1}, state={2}, type={3}, open={4}, disp={5}", new object[] { this.ObjectID, this._transactionId, this._transactionState, this._transactionType, this._openResultCount, this._disposing });
        }

        internal void Zombie()
        {
            this.ZombieParent();
            SqlInternalConnection connection = this._innerConnection;
            this._innerConnection = null;
            if (connection != null)
            {
                connection.DisconnectTransaction(this);
            }
        }

        private void ZombieParent()
        {
            if (this._parent != null)
            {
                SqlTransaction target = (SqlTransaction) this._parent.Target;
                if (target != null)
                {
                    target.Zombie();
                }
                this._parent = null;
            }
        }

        internal bool HasParentTransaction
        {
            get
            {
                return ((TransactionType.LocalFromAPI == this._transactionType) || ((TransactionType.LocalFromTSQL == this._transactionType) && (this._parent != null)));
            }
        }

        internal bool IsAborted
        {
            get
            {
                return (TransactionState.Aborted == this._transactionState);
            }
        }

        internal bool IsActive
        {
            get
            {
                return (TransactionState.Active == this._transactionState);
            }
        }

        internal bool IsCommitted
        {
            get
            {
                return (TransactionState.Committed == this._transactionState);
            }
        }

        internal bool IsCompleted
        {
            get
            {
                if ((TransactionState.Aborted != this._transactionState) && (TransactionState.Committed != this._transactionState))
                {
                    return (TransactionState.Unknown == this._transactionState);
                }
                return true;
            }
        }

        internal bool IsContext
        {
            get
            {
                return (TransactionType.Context == this._transactionType);
            }
        }

        internal bool IsDelegated
        {
            get
            {
                return (TransactionType.Delegated == this._transactionType);
            }
        }

        internal bool IsDistributed
        {
            get
            {
                return (TransactionType.Distributed == this._transactionType);
            }
        }

        internal bool IsLocal
        {
            get
            {
                return (((TransactionType.LocalFromTSQL == this._transactionType) || (TransactionType.LocalFromAPI == this._transactionType)) || (TransactionType.Context == this._transactionType));
            }
        }

        internal bool IsOrphaned
        {
            get
            {
                if (this._parent == null)
                {
                    return false;
                }
                return (this._parent.Target == null);
            }
        }

        internal bool IsZombied
        {
            get
            {
                return (null == this._innerConnection);
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        internal int OpenResultsCount
        {
            get
            {
                return this._openResultCount;
            }
        }

        internal SqlTransaction Parent
        {
            get
            {
                SqlTransaction target = null;
                if (this._parent != null)
                {
                    target = (SqlTransaction) this._parent.Target;
                }
                return target;
            }
        }

        internal long TransactionId
        {
            get
            {
                return this._transactionId;
            }
            set
            {
                this._transactionId = value;
            }
        }
    }
}


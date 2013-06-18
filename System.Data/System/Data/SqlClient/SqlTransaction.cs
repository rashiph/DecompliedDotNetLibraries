namespace System.Data.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public sealed class SqlTransaction : DbTransaction
    {
        private SqlConnection _connection;
        private SqlInternalTransaction _internalTransaction;
        private bool _isFromAPI;
        internal readonly System.Data.IsolationLevel _isolationLevel = System.Data.IsolationLevel.ReadCommitted;
        internal readonly int _objectID = Interlocked.Increment(ref _objectTypeCount);
        private static int _objectTypeCount;

        internal SqlTransaction(SqlInternalConnection internalConnection, SqlConnection con, System.Data.IsolationLevel iso, SqlInternalTransaction internalTransaction)
        {
            this._isolationLevel = iso;
            this._connection = con;
            if (internalTransaction == null)
            {
                this._internalTransaction = new SqlInternalTransaction(internalConnection, TransactionType.LocalFromAPI, this);
            }
            else
            {
                this._internalTransaction = internalTransaction;
                this._internalTransaction.InitParent(this);
            }
        }

        public override void Commit()
        {
            IntPtr ptr;
            SqlConnection.ExecutePermission.Demand();
            this.ZombieCheck();
            SqlStatistics statistics = null;
            Bid.ScopeEnter(out ptr, "<sc.SqlTransaction.Commit|API> %d#", this.ObjectID);
            SNIHandle target = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target = SqlInternalConnection.GetBestEffortCleanupTarget(this._connection);
                statistics = SqlStatistics.StartTimer(this.Statistics);
                this._isFromAPI = true;
                this._internalTransaction.Commit();
            }
            catch (OutOfMemoryException exception3)
            {
                this._connection.Abort(exception3);
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this._connection.Abort(exception2);
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this._connection.Abort(exception);
                SqlInternalConnection.BestEffortCleanup(target);
                throw;
            }
            finally
            {
                this._isFromAPI = false;
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref ptr);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SNIHandle target = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    target = SqlInternalConnection.GetBestEffortCleanupTarget(this._connection);
                    if (!this.IsZombied && !this.IsYukonPartialZombie)
                    {
                        this._internalTransaction.Dispose();
                    }
                }
                catch (OutOfMemoryException exception3)
                {
                    this._connection.Abort(exception3);
                    throw;
                }
                catch (StackOverflowException exception2)
                {
                    this._connection.Abort(exception2);
                    throw;
                }
                catch (ThreadAbortException exception)
                {
                    this._connection.Abort(exception);
                    SqlInternalConnection.BestEffortCleanup(target);
                    throw;
                }
            }
            base.Dispose(disposing);
        }

        public override void Rollback()
        {
            if (this.IsYukonPartialZombie)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlTransaction.Rollback|ADV> %d# partial zombie no rollback required\n", this.ObjectID);
                }
                this._internalTransaction = null;
            }
            else
            {
                IntPtr ptr;
                this.ZombieCheck();
                SqlStatistics statistics = null;
                Bid.ScopeEnter(out ptr, "<sc.SqlTransaction.Rollback|API> %d#", this.ObjectID);
                SNIHandle target = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    target = SqlInternalConnection.GetBestEffortCleanupTarget(this._connection);
                    statistics = SqlStatistics.StartTimer(this.Statistics);
                    this._isFromAPI = true;
                    this._internalTransaction.Rollback();
                }
                catch (OutOfMemoryException exception3)
                {
                    this._connection.Abort(exception3);
                    throw;
                }
                catch (StackOverflowException exception2)
                {
                    this._connection.Abort(exception2);
                    throw;
                }
                catch (ThreadAbortException exception)
                {
                    this._connection.Abort(exception);
                    SqlInternalConnection.BestEffortCleanup(target);
                    throw;
                }
                finally
                {
                    this._isFromAPI = false;
                    SqlStatistics.StopTimer(statistics);
                    Bid.ScopeLeave(ref ptr);
                }
            }
        }

        public void Rollback(string transactionName)
        {
            IntPtr ptr;
            SqlConnection.ExecutePermission.Demand();
            this.ZombieCheck();
            SqlStatistics statistics = null;
            Bid.ScopeEnter(out ptr, "<sc.SqlTransaction.Rollback|API> %d# transactionName='%ls'", this.ObjectID, transactionName);
            SNIHandle target = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target = SqlInternalConnection.GetBestEffortCleanupTarget(this._connection);
                statistics = SqlStatistics.StartTimer(this.Statistics);
                this._isFromAPI = true;
                this._internalTransaction.Rollback(transactionName);
            }
            catch (OutOfMemoryException exception3)
            {
                this._connection.Abort(exception3);
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this._connection.Abort(exception2);
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this._connection.Abort(exception);
                SqlInternalConnection.BestEffortCleanup(target);
                throw;
            }
            finally
            {
                this._isFromAPI = false;
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref ptr);
            }
        }

        public void Save(string savePointName)
        {
            IntPtr ptr;
            SqlConnection.ExecutePermission.Demand();
            this.ZombieCheck();
            SqlStatistics statistics = null;
            Bid.ScopeEnter(out ptr, "<sc.SqlTransaction.Save|API> %d# savePointName='%ls'", this.ObjectID, savePointName);
            SNIHandle target = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target = SqlInternalConnection.GetBestEffortCleanupTarget(this._connection);
                statistics = SqlStatistics.StartTimer(this.Statistics);
                this._internalTransaction.Save(savePointName);
            }
            catch (OutOfMemoryException exception3)
            {
                this._connection.Abort(exception3);
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this._connection.Abort(exception2);
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this._connection.Abort(exception);
                SqlInternalConnection.BestEffortCleanup(target);
                throw;
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal void Zombie()
        {
            SqlInternalConnection innerConnection = this._connection.InnerConnection as SqlInternalConnection;
            if (((innerConnection != null) && innerConnection.IsYukonOrNewer) && !this._isFromAPI)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlTransaction.Zombie|ADV> %d# yukon deferred zombie\n", this.ObjectID);
                }
            }
            else
            {
                this._internalTransaction = null;
            }
        }

        private void ZombieCheck()
        {
            if (this.IsZombied)
            {
                if (this.IsYukonPartialZombie)
                {
                    this._internalTransaction = null;
                }
                throw ADP.TransactionZombied(this);
            }
        }

        public SqlConnection Connection
        {
            get
            {
                if (this.IsZombied)
                {
                    return null;
                }
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

        internal SqlInternalTransaction InternalTransaction
        {
            get
            {
                return this._internalTransaction;
            }
        }

        public override System.Data.IsolationLevel IsolationLevel
        {
            get
            {
                this.ZombieCheck();
                return this._isolationLevel;
            }
        }

        private bool IsYukonPartialZombie
        {
            get
            {
                return ((this._internalTransaction != null) && this._internalTransaction.IsCompleted);
            }
        }

        internal bool IsZombied
        {
            get
            {
                if (this._internalTransaction != null)
                {
                    return this._internalTransaction.IsCompleted;
                }
                return true;
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        internal SqlStatistics Statistics
        {
            get
            {
                if ((this._connection != null) && this._connection.StatisticsEnabled)
                {
                    return this._connection.Statistics;
                }
                return null;
            }
        }
    }
}


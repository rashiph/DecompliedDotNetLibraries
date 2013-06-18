namespace System.Activities.DurableInstancing
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.Transactions;

    internal abstract class SqlWorkflowInstanceStoreAsyncResult : AsyncResult
    {
        private static Action<AsyncResult, Exception> finallyCallback = new Action<AsyncResult, Exception>(SqlWorkflowInstanceStoreAsyncResult.Finally);
        private static AsyncResult.AsyncCompletion onBindReclaimed = new AsyncResult.AsyncCompletion(SqlWorkflowInstanceStoreAsyncResult.OnBindReclaimed);
        private static AsyncResult.AsyncCompletion onSqlCommandAsyncResultCallback = new AsyncResult.AsyncCompletion(SqlWorkflowInstanceStoreAsyncResult.SqlCommandAsyncResultCallback);
        private SqlCommand sqlCommand;

        protected SqlWorkflowInstanceStoreAsyncResult(System.Runtime.DurableInstancing.InstancePersistenceContext context, System.Runtime.DurableInstancing.InstancePersistenceCommand command, SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, Transaction currentTransaction, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
        {
            this.DependentTransaction = (currentTransaction != null) ? currentTransaction.DependentClone(DependentCloneOption.BlockCommitUntilComplete) : null;
            this.InstancePersistenceContext = context;
            this.InstancePersistenceCommand = command;
            this.Store = store;
            this.StoreLock = storeLock;
            this.TimeoutHelper = new System.Runtime.TimeoutHelper(timeout);
            base.OnCompleting = (Action<AsyncResult, Exception>) Delegate.Combine(base.OnCompleting, finallyCallback);
        }

        private void ClearMembers()
        {
            this.InstancePersistenceCommand = null;
            this.InstancePersistenceContext = null;
            this.Store = null;
            this.StoreLock = null;
        }

        public static bool End(IAsyncResult result)
        {
            if (result == null)
            {
                throw FxTrace.Exception.ArgumentNull("result");
            }
            AsyncResult.End<SqlWorkflowInstanceStoreAsyncResult>(result);
            return true;
        }

        private static void Finally(AsyncResult result, Exception currentException)
        {
            SqlWorkflowInstanceStoreAsyncResult result2 = result as SqlWorkflowInstanceStoreAsyncResult;
            try
            {
                if (result2.DependentTransaction != null)
                {
                    using (result2.DependentTransaction)
                    {
                        result2.DependentTransaction.Complete();
                    }
                }
            }
            catch (TransactionException)
            {
                if (currentException == null)
                {
                    throw;
                }
            }
            finally
            {
                result2.OnCommandCompletion();
                result2.ClearMembers();
                StoreUtilities.TraceSqlCommand(result2.sqlCommand, false);
            }
        }

        protected abstract void GenerateSqlCommand(SqlCommand sqlCommand);
        protected abstract string GetSqlCommandText();
        protected abstract CommandType GetSqlCommandType();
        private static bool OnBindReclaimed(IAsyncResult result)
        {
            SqlWorkflowInstanceStoreAsyncResult asyncState = (SqlWorkflowInstanceStoreAsyncResult) result.AsyncState;
            asyncState.InstancePersistenceContext.EndBindReclaimedLock(result);
            Guid instanceId = asyncState.InstancePersistenceContext.InstanceView.InstanceId;
            long instanceVersion = asyncState.InstancePersistenceContext.InstanceVersion;
            ((InstanceLockTracking) asyncState.InstancePersistenceContext.UserContext).TrackStoreLock(instanceId, instanceVersion, null);
            asyncState.InstancePersistenceContext.InstanceHandle.Free();
            throw FxTrace.Exception.AsError(new InstanceLockLostException(asyncState.InstancePersistenceCommand.Name, instanceId));
        }

        protected virtual void OnCommandCompletion()
        {
        }

        protected abstract Exception ProcessSqlResult(SqlDataReader reader);
        public void ScheduleCallback()
        {
            ActionItem.Schedule(new Action<object>(SqlWorkflowInstanceStoreAsyncResult.StartOperationCallback), this);
        }

        private static bool SqlCommandAsyncResultCallback(IAsyncResult result)
        {
            SqlWorkflowInstanceStoreAsyncResult asyncState = (SqlWorkflowInstanceStoreAsyncResult) result.AsyncState;
            Exception exception = null;
            bool flag = true;
            try
            {
                using (asyncState.sqlCommand)
                {
                    using (SqlDataReader reader = SqlCommandAsyncResult.End(result))
                    {
                        exception = asyncState.ProcessSqlResult(reader);
                    }
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                Guid instanceId = (asyncState.InstancePersistenceContext != null) ? asyncState.InstancePersistenceContext.InstanceView.InstanceId : Guid.Empty;
                exception = new InstancePersistenceCommandException(asyncState.InstancePersistenceCommand.Name, instanceId, exception2);
            }
            if (exception is InstanceAlreadyLockedToOwnerException)
            {
                InstanceAlreadyLockedToOwnerException exception3 = (InstanceAlreadyLockedToOwnerException) exception;
                long instanceVersion = exception3.InstanceVersion;
                if (!asyncState.InstancePersistenceContext.InstanceView.IsBoundToInstance)
                {
                    asyncState.InstancePersistenceContext.BindInstance(exception3.InstanceId);
                }
                IAsyncResult result3 = asyncState.InstancePersistenceContext.BeginBindReclaimedLock(instanceVersion, asyncState.TimeoutHelper.RemainingTime(), asyncState.PrepareAsyncCompletion(onBindReclaimed), asyncState);
                if (!asyncState.SyncContinue(result3))
                {
                    flag = false;
                }
                return flag;
            }
            if (exception == null)
            {
                return flag;
            }
            if (asyncState.sqlCommand.Connection != null)
            {
                asyncState.sqlCommand.Connection.Close();
            }
            asyncState.TraceException(exception);
            throw FxTrace.Exception.AsError(exception);
        }

        private void StartOperation()
        {
            Guid instanceId = (this.InstancePersistenceContext != null) ? this.InstancePersistenceContext.InstanceView.InstanceId : Guid.Empty;
            Exception exception = null;
            try
            {
                this.sqlCommand = new SqlCommand();
                this.GenerateSqlCommand(this.sqlCommand);
                this.sqlCommand.CommandText = this.GetSqlCommandText();
                this.sqlCommand.CommandType = this.GetSqlCommandType();
                StoreUtilities.TraceSqlCommand(this.sqlCommand, true);
                SqlCommandAsyncResult result = new SqlCommandAsyncResult(this.sqlCommand, this.ConnectionString, this.DependentTransaction, this.TimeoutHelper.RemainingTime(), 0, this.Store.MaxConnectionRetries, base.PrepareAsyncCompletion(onSqlCommandAsyncResultCallback), this);
                result.StartCommand();
                if (!base.SyncContinue(result))
                {
                    return;
                }
            }
            catch (InstancePersistenceException exception2)
            {
                exception = exception2;
            }
            catch (Exception exception3)
            {
                if (Fx.IsFatal(exception3))
                {
                    throw;
                }
                exception = new InstancePersistenceCommandException(this.InstancePersistenceCommand.Name, instanceId, exception3);
            }
            if (exception != null)
            {
                if (this.sqlCommand.Connection != null)
                {
                    this.sqlCommand.Connection.Close();
                }
                this.sqlCommand.Dispose();
                this.TraceException(exception);
            }
            base.Complete(false, exception);
        }

        private static void StartOperationCallback(object state)
        {
            ((SqlWorkflowInstanceStoreAsyncResult) state).StartOperation();
        }

        private void TraceException(Exception exception)
        {
            bool flag = true;
            if (this.Store.IsLockRetryEnabled() && (exception is InstanceLockedException))
            {
                flag = false;
            }
            if (flag && TD.FoundProcessingErrorIsEnabled())
            {
                TD.FoundProcessingError(exception.Message, exception);
            }
        }

        protected virtual string ConnectionString
        {
            get
            {
                return this.Store.CachedConnectionString;
            }
        }

        protected System.Transactions.DependentTransaction DependentTransaction { get; set; }

        protected System.Runtime.DurableInstancing.InstancePersistenceCommand InstancePersistenceCommand { get; private set; }

        protected System.Runtime.DurableInstancing.InstancePersistenceContext InstancePersistenceContext { get; private set; }

        protected SqlWorkflowInstanceStore Store { get; private set; }

        protected SqlWorkflowInstanceStoreLock StoreLock { get; private set; }

        protected System.Runtime.TimeoutHelper TimeoutHelper { get; set; }
    }
}


namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;

    internal class LoadRetryAsyncResult : AsyncResult
    {
        private bool commandSuccess;
        private TimeoutHelper commandTimeout;
        private InstanceLockedException lastInstanceLockedException;
        private static AsyncCallback onTryCommandCallback = Fx.ThunkCallback(new AsyncCallback(LoadRetryAsyncResult.OnTryCommandCallback));
        private int retryCount;

        public LoadRetryAsyncResult(SqlWorkflowInstanceStore store, System.Runtime.DurableInstancing.InstancePersistenceContext context, System.Runtime.DurableInstancing.InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
        {
            this.InstanceStore = store;
            this.InstancePersistenceContext = context;
            this.InstancePersistenceCommand = command;
            this.commandTimeout = new TimeoutHelper(timeout);
            this.InstanceStore.BeginTryCommandInternal(this.InstancePersistenceContext, this.InstancePersistenceCommand, this.commandTimeout.RemainingTime(), onTryCommandCallback, this);
        }

        public void AbortRetry()
        {
            base.Complete(false, this.lastInstanceLockedException);
        }

        private void CompleteTryCommand(IAsyncResult result)
        {
            this.commandSuccess = this.InstanceStore.EndTryCommand(result);
        }

        public static bool End(IAsyncResult result)
        {
            return AsyncResult.End<LoadRetryAsyncResult>(result).commandSuccess;
        }

        private static void OnTryCommandCallback(IAsyncResult result)
        {
            LoadRetryAsyncResult asyncState = (LoadRetryAsyncResult) result.AsyncState;
            Exception exception = null;
            bool flag = true;
            try
            {
                asyncState.CompleteTryCommand(result);
            }
            catch (InstanceLockedException exception2)
            {
                TimeSpan nextRetryDelay = asyncState.InstanceStore.GetNextRetryDelay(++asyncState.retryCount);
                if (nextRetryDelay < asyncState.commandTimeout.RemainingTime())
                {
                    asyncState.RetryTimeout = nextRetryDelay;
                    if (asyncState.InstanceStore.EnqueueRetry(asyncState))
                    {
                        asyncState.lastInstanceLockedException = exception2;
                        flag = false;
                    }
                }
                else if (TD.LockRetryTimeoutIsEnabled())
                {
                    TD.LockRetryTimeout(asyncState.commandTimeout.OriginalTimeout.ToString());
                }
                if (flag)
                {
                    exception = exception2;
                }
            }
            catch (Exception exception3)
            {
                if (Fx.IsFatal(exception3))
                {
                    throw;
                }
                exception = exception3;
            }
            if (flag)
            {
                asyncState.Complete(false, exception);
            }
        }

        public void Retry()
        {
            this.InstanceStore.BeginTryCommandInternal(this.InstancePersistenceContext, this.InstancePersistenceCommand, this.commandTimeout.RemainingTime(), onTryCommandCallback, this);
        }

        private System.Runtime.DurableInstancing.InstancePersistenceCommand InstancePersistenceCommand { get; set; }

        private System.Runtime.DurableInstancing.InstancePersistenceContext InstancePersistenceContext { get; set; }

        public SqlWorkflowInstanceStore InstanceStore { get; private set; }

        public TimeSpan RetryTimeout { get; private set; }
    }
}


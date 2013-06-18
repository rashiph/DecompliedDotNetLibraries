namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;

    internal abstract class PersistenceTask
    {
        private bool automaticallyResetTimer;
        private AsyncCallback commandCompletedCallback;
        private InstancePersistenceCommand instancePersistenceCommand;
        private TimeSpan taskInterval;
        private IOThreadTimer taskTimer;
        private object thisLock;
        private bool timerCancelled;

        public PersistenceTask(SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, InstancePersistenceCommand instancePersistenceCommand, TimeSpan taskInterval, bool automaticallyResetTimer)
        {
            this.automaticallyResetTimer = automaticallyResetTimer;
            this.commandCompletedCallback = Fx.ThunkCallback(new AsyncCallback(this.CommandCompletedCallback));
            this.instancePersistenceCommand = instancePersistenceCommand;
            this.Store = store;
            this.StoreLock = storeLock;
            this.SurrogateLockOwnerId = this.StoreLock.SurrogateLockOwnerId;
            this.taskInterval = taskInterval;
            this.thisLock = new object();
        }

        public void CancelTimer()
        {
            lock (this.ThisLock)
            {
                this.timerCancelled = true;
                if (this.taskTimer != null)
                {
                    this.taskTimer.Cancel();
                    this.taskTimer = null;
                }
            }
        }

        private void CommandCompletedCallback(IAsyncResult result)
        {
            SqlWorkflowInstanceStoreAsyncResult result1 = (SqlWorkflowInstanceStoreAsyncResult) result;
            try
            {
                this.Store.EndTryCommand(result);
                if (this.automaticallyResetTimer)
                {
                    this.ResetTimer(false);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.HandleError(exception);
            }
        }

        protected abstract void HandleError(Exception exception);
        private void OnTimerFired(object state)
        {
            if (this.StoreLock.IsLockOwnerValid(this.SurrogateLockOwnerId))
            {
                try
                {
                    this.Store.BeginTryCommandInternal(null, this.instancePersistenceCommand, SqlWorkflowInstanceStoreConstants.LockOwnerTimeoutBuffer, this.commandCompletedCallback, null);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.HandleError(exception);
                }
            }
        }

        public void ResetTimer(bool fireImmediately)
        {
            this.ResetTimer(fireImmediately, null);
        }

        public virtual void ResetTimer(bool fireImmediately, TimeSpan? taskIntervalOverride)
        {
            TimeSpan taskInterval = this.taskInterval;
            if (taskIntervalOverride.HasValue && (taskIntervalOverride.Value < this.taskInterval))
            {
                taskInterval = taskIntervalOverride.Value;
            }
            lock (this.ThisLock)
            {
                if (!this.timerCancelled)
                {
                    if (this.taskTimer == null)
                    {
                        this.taskTimer = new IOThreadTimer(new Action<object>(this.OnTimerFired), null, false);
                    }
                    this.taskTimer.Set(fireImmediately ? TimeSpan.Zero : taskInterval);
                }
            }
        }

        protected SqlWorkflowInstanceStore Store { get; set; }

        protected SqlWorkflowInstanceStoreLock StoreLock { get; set; }

        protected long SurrogateLockOwnerId { get; set; }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }
    }
}


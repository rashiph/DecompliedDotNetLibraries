namespace System.Web
{
    using System;
    using System.Threading;

    internal class AspNetSynchronizationContext : SynchronizationContext
    {
        private HttpApplication _application;
        private bool _disabled;
        private Exception _error;
        private bool _invalidOperationEncountered;
        private WaitCallback _lastCompletionWorkItemCallback;
        private int _pendingCount;
        private bool _syncCaller;

        internal AspNetSynchronizationContext(HttpApplication app)
        {
            this._application = app;
        }

        private void CallCallback(SendOrPostCallback callback, object state)
        {
            if (this._syncCaller)
            {
                this.CallCallbackPossiblyUnderLock(callback, state);
            }
            else
            {
                lock (this._application)
                {
                    this.CallCallbackPossiblyUnderLock(callback, state);
                }
            }
        }

        private void CallCallbackPossiblyUnderLock(SendOrPostCallback callback, object state)
        {
            HttpApplication.ThreadContext context = null;
            try
            {
                context = this._application.OnThreadEnter();
                try
                {
                    callback(state);
                }
                catch (Exception exception)
                {
                    this._error = exception;
                }
            }
            finally
            {
                if (context != null)
                {
                    context.Leave();
                }
            }
        }

        internal void ClearError()
        {
            this._error = null;
        }

        public override SynchronizationContext CreateCopy()
        {
            return new AspNetSynchronizationContext(this._application) { _disabled = this._disabled, _syncCaller = this._syncCaller };
        }

        internal void Disable()
        {
            this._disabled = true;
        }

        internal void Enable()
        {
            this._disabled = false;
        }

        public override void OperationCompleted()
        {
            if ((!this._invalidOperationEncountered && (!this._disabled || (this._pendingCount != 0))) && ((Interlocked.Decrement(ref this._pendingCount) == 0) && (this._lastCompletionWorkItemCallback != null)))
            {
                WaitCallback callBack = this._lastCompletionWorkItemCallback;
                this._lastCompletionWorkItemCallback = null;
                ThreadPool.QueueUserWorkItem(callBack);
            }
        }

        public override void OperationStarted()
        {
            if (this._invalidOperationEncountered || (this._disabled && (this._pendingCount == 0)))
            {
                this._invalidOperationEncountered = true;
                throw new InvalidOperationException(System.Web.SR.GetString("Async_operation_disabled"));
            }
            Interlocked.Increment(ref this._pendingCount);
        }

        public override void Post(SendOrPostCallback callback, object state)
        {
            this.CallCallback(callback, state);
        }

        internal void ResetSyncCaller()
        {
            this._syncCaller = false;
        }

        public override void Send(SendOrPostCallback callback, object state)
        {
            this.CallCallback(callback, state);
        }

        internal void SetLastCompletionWorkItem(WaitCallback callback)
        {
            this._lastCompletionWorkItemCallback = callback;
        }

        internal void SetSyncCaller()
        {
            this._syncCaller = true;
        }

        internal bool Enabled
        {
            get
            {
                return !this._disabled;
            }
        }

        internal Exception Error
        {
            get
            {
                return this._error;
            }
        }

        internal int PendingOperationsCount
        {
            get
            {
                return this._pendingCount;
            }
        }
    }
}


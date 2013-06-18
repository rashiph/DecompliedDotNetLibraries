namespace System.Activities
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class AsyncInvokeOperation
    {
        private object thisLock;

        public AsyncInvokeOperation(SynchronizationContext syncContext)
        {
            this.SyncContext = syncContext;
            this.thisLock = new object();
        }

        public void OperationCompleted()
        {
            lock (this.thisLock)
            {
                Fx.AssertAndThrowFatal(!this.Completed, "Async operation has already been completed");
                this.Completed = true;
            }
            this.SyncContext.OperationCompleted();
        }

        public void OperationStarted()
        {
            this.SyncContext.OperationStarted();
        }

        public void PostOperationCompleted(SendOrPostCallback callback, object arg)
        {
            lock (this.thisLock)
            {
                Fx.AssertAndThrowFatal(!this.Completed, "Async operation has already been completed");
                this.Completed = true;
            }
            this.SyncContext.Post(callback, arg);
            this.SyncContext.OperationCompleted();
        }

        private bool Completed { get; set; }

        private SynchronizationContext SyncContext { get; set; }
    }
}


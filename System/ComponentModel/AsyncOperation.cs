namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public sealed class AsyncOperation
    {
        private bool alreadyCompleted;
        private System.Threading.SynchronizationContext syncContext;
        private object userSuppliedState;

        private AsyncOperation(object userSuppliedState, System.Threading.SynchronizationContext syncContext)
        {
            this.userSuppliedState = userSuppliedState;
            this.syncContext = syncContext;
            this.alreadyCompleted = false;
            this.syncContext.OperationStarted();
        }

        internal static AsyncOperation CreateOperation(object userSuppliedState, System.Threading.SynchronizationContext syncContext)
        {
            return new AsyncOperation(userSuppliedState, syncContext);
        }

        ~AsyncOperation()
        {
            if (!this.alreadyCompleted && (this.syncContext != null))
            {
                this.syncContext.OperationCompleted();
            }
        }

        public void OperationCompleted()
        {
            this.VerifyNotCompleted();
            this.OperationCompletedCore();
        }

        private void OperationCompletedCore()
        {
            try
            {
                this.syncContext.OperationCompleted();
            }
            finally
            {
                this.alreadyCompleted = true;
                GC.SuppressFinalize(this);
            }
        }

        public void Post(SendOrPostCallback d, object arg)
        {
            this.VerifyNotCompleted();
            this.VerifyDelegateNotNull(d);
            this.syncContext.Post(d, arg);
        }

        public void PostOperationCompleted(SendOrPostCallback d, object arg)
        {
            this.Post(d, arg);
            this.OperationCompletedCore();
        }

        private void VerifyDelegateNotNull(SendOrPostCallback d)
        {
            if (d == null)
            {
                throw new ArgumentNullException(SR.GetString("Async_NullDelegate"), "d");
            }
        }

        private void VerifyNotCompleted()
        {
            if (this.alreadyCompleted)
            {
                throw new InvalidOperationException(SR.GetString("Async_OperationAlreadyCompleted"));
            }
        }

        public System.Threading.SynchronizationContext SynchronizationContext
        {
            get
            {
                return this.syncContext;
            }
        }

        public object UserSuppliedState
        {
            get
            {
                return this.userSuppliedState;
            }
        }
    }
}


namespace System.Activities
{
    using System;
    using System.Activities.Runtime;

    public sealed class NativeActivityFaultContext : NativeActivityContext
    {
        private Exception exception;
        private bool isFaultHandled;
        private ActivityInstanceReference source;

        internal NativeActivityFaultContext(System.Activities.ActivityInstance executingActivityInstance, ActivityExecutor executor, BookmarkManager bookmarkManager, Exception exception, ActivityInstanceReference source) : base(executingActivityInstance, executor, bookmarkManager)
        {
            this.exception = exception;
            this.source = source;
        }

        internal FaultContext CreateFaultContext()
        {
            return new FaultContext(this.exception, this.source);
        }

        public void HandleFault()
        {
            base.ThrowIfDisposed();
            this.isFaultHandled = true;
        }

        internal bool IsFaultHandled
        {
            get
            {
                return this.isFaultHandled;
            }
        }
    }
}


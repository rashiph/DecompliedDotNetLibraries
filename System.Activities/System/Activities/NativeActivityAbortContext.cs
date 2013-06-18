namespace System.Activities
{
    using System;
    using System.Activities.Runtime;

    public sealed class NativeActivityAbortContext : ActivityContext
    {
        private Exception reason;

        internal NativeActivityAbortContext(System.Activities.ActivityInstance instance, ActivityExecutor executor, Exception reason) : base(instance, executor)
        {
            this.reason = reason;
        }

        public Exception Reason
        {
            get
            {
                base.ThrowIfDisposed();
                return this.reason;
            }
        }
    }
}


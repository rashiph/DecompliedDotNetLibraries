namespace System.Activities
{
    using System;
    using System.Runtime.CompilerServices;

    public class WorkflowApplicationAbortedEventArgs : WorkflowApplicationEventArgs
    {
        internal WorkflowApplicationAbortedEventArgs(System.Activities.WorkflowApplication application, Exception reason) : base(application)
        {
            this.Reason = reason;
        }

        public Exception Reason { get; private set; }
    }
}


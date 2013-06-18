namespace System.Activities
{
    using System;
    using System.Runtime.CompilerServices;

    public class WorkflowApplicationUnhandledExceptionEventArgs : WorkflowApplicationEventArgs
    {
        internal WorkflowApplicationUnhandledExceptionEventArgs(System.Activities.WorkflowApplication application, Exception exception, Activity exceptionSource, string exceptionSourceInstanceId) : base(application)
        {
            this.UnhandledException = exception;
            this.ExceptionSource = exceptionSource;
            this.ExceptionSourceInstanceId = exceptionSourceInstanceId;
        }

        public Activity ExceptionSource { get; private set; }

        public string ExceptionSourceInstanceId { get; private set; }

        public Exception UnhandledException { get; private set; }
    }
}


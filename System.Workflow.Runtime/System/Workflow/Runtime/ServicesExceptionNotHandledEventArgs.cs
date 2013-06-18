namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime;

    public sealed class ServicesExceptionNotHandledEventArgs : EventArgs
    {
        private System.Exception exception;
        private Guid instanceId;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ServicesExceptionNotHandledEventArgs(System.Exception exception, Guid instanceId)
        {
            this.exception = exception;
            this.instanceId = instanceId;
        }

        public System.Exception Exception
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.exception;
            }
        }

        public Guid WorkflowInstanceId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.instanceId;
            }
        }
    }
}


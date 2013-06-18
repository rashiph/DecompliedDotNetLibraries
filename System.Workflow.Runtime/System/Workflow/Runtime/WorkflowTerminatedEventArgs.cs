namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public class WorkflowTerminatedEventArgs : WorkflowEventArgs
    {
        private System.Exception exception;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal WorkflowTerminatedEventArgs(WorkflowInstance instance, System.Exception e) : base(instance)
        {
            this.exception = e;
        }

        internal WorkflowTerminatedEventArgs(WorkflowInstance instance, string error) : base(instance)
        {
            this.exception = new WorkflowTerminatedException(error);
        }

        public System.Exception Exception
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.exception;
            }
        }
    }
}


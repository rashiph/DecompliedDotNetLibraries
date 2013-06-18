namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime;

    public class WorkflowSuspendedEventArgs : WorkflowEventArgs
    {
        private string _error;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal WorkflowSuspendedEventArgs(WorkflowInstance instance, string error) : base(instance)
        {
            this._error = error;
        }

        public string Error
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._error;
            }
        }
    }
}


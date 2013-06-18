namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [Serializable]
    public class TrackingWorkflowTerminatedEventArgs : EventArgs
    {
        private System.Exception _e;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal TrackingWorkflowTerminatedEventArgs(System.Exception exception)
        {
            this._e = exception;
        }

        internal TrackingWorkflowTerminatedEventArgs(string error)
        {
            this._e = new WorkflowTerminatedException(error);
        }

        public System.Exception Exception
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._e;
            }
        }
    }
}


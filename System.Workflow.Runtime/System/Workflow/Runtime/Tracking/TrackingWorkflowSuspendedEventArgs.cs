namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;

    [Serializable]
    public class TrackingWorkflowSuspendedEventArgs : EventArgs
    {
        private string _error;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal TrackingWorkflowSuspendedEventArgs(string error)
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


namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime;

    public sealed class WorkflowRuntimeEventArgs : EventArgs
    {
        private bool _isStarted;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal WorkflowRuntimeEventArgs(bool isStarted)
        {
            this._isStarted = isStarted;
        }

        public bool IsStarted
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._isStarted;
            }
        }
    }
}


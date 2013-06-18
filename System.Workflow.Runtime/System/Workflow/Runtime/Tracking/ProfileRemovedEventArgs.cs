namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;

    public sealed class ProfileRemovedEventArgs : EventArgs
    {
        private Type _workflowType;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ProfileRemovedEventArgs()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ProfileRemovedEventArgs(Type workflowType)
        {
            this._workflowType = workflowType;
        }

        public Type WorkflowType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._workflowType;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._workflowType = value;
            }
        }
    }
}


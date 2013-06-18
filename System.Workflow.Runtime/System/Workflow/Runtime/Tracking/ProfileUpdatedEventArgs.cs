namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;

    public sealed class ProfileUpdatedEventArgs : EventArgs
    {
        private System.Workflow.Runtime.Tracking.TrackingProfile _profile;
        private Type _workflowType;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ProfileUpdatedEventArgs()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ProfileUpdatedEventArgs(Type workflowType, System.Workflow.Runtime.Tracking.TrackingProfile profile)
        {
            this._workflowType = workflowType;
            this._profile = profile;
        }

        public System.Workflow.Runtime.Tracking.TrackingProfile TrackingProfile
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._profile;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._profile = value;
            }
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


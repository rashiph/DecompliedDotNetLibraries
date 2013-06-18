namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;

    [Serializable]
    public class TrackingProfile
    {
        private ActivityTrackPointCollection _activities = new ActivityTrackPointCollection();
        private UserTrackPointCollection _code = new UserTrackPointCollection();
        private WorkflowTrackPointCollection _instance = new WorkflowTrackPointCollection();
        private System.Version _version;

        public ActivityTrackPointCollection ActivityTrackPoints
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._activities;
            }
        }

        public UserTrackPointCollection UserTrackPoints
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._code;
            }
        }

        public System.Version Version
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._version;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._version = value;
            }
        }

        public WorkflowTrackPointCollection WorkflowTrackPoints
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._instance;
            }
        }
    }
}


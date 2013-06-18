namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public sealed class ReplicatorChildEventArgs : EventArgs
    {
        private System.Workflow.ComponentModel.Activity activity;
        private object instanceData;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ReplicatorChildEventArgs(object instanceData, System.Workflow.ComponentModel.Activity activity)
        {
            this.instanceData = instanceData;
            this.activity = activity;
        }

        public System.Workflow.ComponentModel.Activity Activity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activity;
            }
        }

        public object InstanceData
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.instanceData;
            }
        }
    }
}


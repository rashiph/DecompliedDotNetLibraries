namespace System.Workflow.Runtime.DebugEngine
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    internal sealed class InstanceData : ICloneable
    {
        private Activity rootActivity;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceData(Activity rootActivity)
        {
            this.rootActivity = rootActivity;
        }

        object ICloneable.Clone()
        {
            return new InstanceData(this.rootActivity);
        }

        public Activity RootActivity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.rootActivity;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.rootActivity = value;
            }
        }
    }
}


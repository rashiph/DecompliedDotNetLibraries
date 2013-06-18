namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [Serializable]
    internal sealed class ConditionedActivityGroupStateInfo
    {
        private Dictionary<string, CAGChildStats> childActivityStats;
        private bool completed;
        private bool testing;

        internal ConditionedActivityGroupStateInfo(ConditionedActivityGroup cag)
        {
            int count = cag.EnabledActivities.Count;
            this.childActivityStats = new Dictionary<string, CAGChildStats>(count);
            foreach (Activity activity in cag.EnabledActivities)
            {
                this.childActivityStats[activity.QualifiedName] = new CAGChildStats();
            }
        }

        internal Dictionary<string, CAGChildStats> ChildrenStats
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.childActivityStats;
            }
        }

        internal bool Completed
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.completed;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.completed = value;
            }
        }

        internal bool Testing
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.testing;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.testing = value;
            }
        }
    }
}


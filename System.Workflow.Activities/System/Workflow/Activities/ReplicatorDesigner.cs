namespace System.Workflow.Activities
{
    using System;
    using System.Collections.ObjectModel;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    internal sealed class ReplicatorDesigner : System.Workflow.Activities.SequenceDesigner
    {
        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            CompositeActivity activity = base.Activity as CompositeActivity;
            if ((activity != null) && (activity.EnabledActivities.Count > 0))
            {
                return false;
            }
            if (activitiesToInsert.Count > 1)
            {
                return false;
            }
            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }
    }
}


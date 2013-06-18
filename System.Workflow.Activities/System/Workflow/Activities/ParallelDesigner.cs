namespace System.Workflow.Activities
{
    using System;
    using System.Collections.ObjectModel;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(ParallelDesignerTheme))]
    internal sealed class ParallelDesigner : ParallelActivityDesigner
    {
        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            foreach (Activity activity in activitiesToInsert)
            {
                if (activity.GetType() != typeof(SequenceActivity))
                {
                    return false;
                }
            }
            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }

        protected override CompositeActivity OnCreateNewBranch()
        {
            return new SequenceActivity();
        }
    }
}


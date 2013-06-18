namespace System.Workflow.Activities
{
    using System;
    using System.Collections.ObjectModel;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(StateInitializationDesignerTheme))]
    internal sealed class StateInitializationDesigner : System.Workflow.Activities.SequenceDesigner
    {
        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            if (parentActivityDesigner == null)
            {
                throw new ArgumentNullException("parentActivityDesigner");
            }
            return ((parentActivityDesigner.Activity is StateActivity) && base.CanBeParentedTo(parentActivityDesigner));
        }

        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            foreach (Activity activity in activitiesToInsert)
            {
                if (activity is IEventActivity)
                {
                    return false;
                }
            }
            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }

        protected override void DoDefaultAction()
        {
            base.DoDefaultAction();
            base.EnsureVisible();
        }

        public override bool CanExpandCollapse
        {
            get
            {
                return false;
            }
        }
    }
}


namespace System.Workflow.Activities
{
    using System;
    using System.Collections.ObjectModel;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(EventHandlingScopeActivityDesignerTheme))]
    internal sealed class EventHandlingScopeDesigner : SequentialActivityDesigner
    {
        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            int num = 0;
            foreach (Activity activity in ((EventHandlingScopeActivity) base.Activity).Activities)
            {
                if (!System.Workflow.Activities.Common.Helpers.IsFrameworkActivity(activity) && !(activity is EventHandlersActivity))
                {
                    num++;
                }
            }
            if (num > 0)
            {
                return false;
            }
            return base.CanInsertActivities(insertLocation, activitiesToInsert);
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


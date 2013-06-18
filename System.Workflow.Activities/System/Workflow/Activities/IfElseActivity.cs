namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    [SRDescription("ConditionalActivityDescription"), ToolboxItem(typeof(IfElseToolboxItem)), ActivityValidator(typeof(IfElseValidator)), Designer(typeof(IfElseDesigner), typeof(IDesigner)), SRCategory("Standard"), ToolboxBitmap(typeof(IfElseActivity), "Resources.Decision.png")]
    public sealed class IfElseActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IfElseActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IfElseActivity(string name) : base(name)
        {
        }

        public IfElseBranchActivity AddBranch(ICollection<Activity> activities)
        {
            if (activities == null)
            {
                throw new ArgumentNullException("activities");
            }
            return this.AddBranch(activities, null);
        }

        public IfElseBranchActivity AddBranch(ICollection<Activity> activities, ActivityCondition branchCondition)
        {
            if (activities == null)
            {
                throw new ArgumentNullException("activities");
            }
            if (!base.DesignMode)
            {
                throw new InvalidOperationException(SR.GetString("Error_ConditionalBranchUpdateAtRuntime"));
            }
            IfElseBranchActivity item = new IfElseBranchActivity();
            foreach (Activity activity2 in activities)
            {
                item.Activities.Add(activity2);
            }
            item.Condition = branchCondition;
            base.Activities.Add(item);
            return item;
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            bool flag = true;
            for (int i = 0; i < base.EnabledActivities.Count; i++)
            {
                Activity activity = base.EnabledActivities[i];
                if (activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    flag = false;
                    executionContext.CancelActivity(activity);
                    break;
                }
                if ((activity.ExecutionStatus == ActivityExecutionStatus.Canceling) || (activity.ExecutionStatus == ActivityExecutionStatus.Faulting))
                {
                    flag = false;
                    break;
                }
            }
            if (!flag)
            {
                return ActivityExecutionStatus.Canceling;
            }
            return ActivityExecutionStatus.Closed;
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            bool flag = true;
            for (int i = 0; i < base.EnabledActivities.Count; i++)
            {
                IfElseBranchActivity activity = base.EnabledActivities[i] as IfElseBranchActivity;
                if ((activity.Condition == null) || activity.Condition.Evaluate(activity, executionContext))
                {
                    flag = false;
                    activity.RegisterForStatusChange(Activity.ClosedEvent, this);
                    executionContext.ExecuteActivity(activity);
                    break;
                }
            }
            if (!flag)
            {
                return ActivityExecutionStatus.Executing;
            }
            return ActivityExecutionStatus.Closed;
        }

        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
            {
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");
            }
            context.CloseActivity();
        }
    }
}


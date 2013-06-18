namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Runtime.DebugEngine;

    [SRCategory("Standard"), ActivityValidator(typeof(ParallelValidator)), WorkflowDebuggerStepping(WorkflowDebuggerSteppingOption.Concurrent), Designer(typeof(ParallelDesigner), typeof(IDesigner)), SRDescription("ParallelActivityDescription"), ToolboxItem(typeof(ParallelToolboxItem)), ToolboxBitmap(typeof(ParallelActivity), "Resources.Parallel.png")]
    public sealed class ParallelActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        private static DependencyProperty IsExecutingProperty = DependencyProperty.Register("IsExecuting", typeof(bool), typeof(ParallelActivity), new PropertyMetadata(false));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ParallelActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ParallelActivity(string name) : base(name)
        {
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            bool flag = true;
            for (int i = 0; i < base.EnabledActivities.Count; i++)
            {
                Activity activity = base.EnabledActivities[i];
                if (activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    executionContext.CancelActivity(activity);
                    flag = false;
                }
                else if ((activity.ExecutionStatus == ActivityExecutionStatus.Canceling) || (activity.ExecutionStatus == ActivityExecutionStatus.Faulting))
                {
                    flag = false;
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
            this.IsExecuting = true;
            for (int i = 0; i < base.EnabledActivities.Count; i++)
            {
                Activity activity = base.EnabledActivities[i];
                activity.RegisterForStatusChange(Activity.ClosedEvent, this);
                executionContext.ExecuteActivity(activity);
            }
            if (base.EnabledActivities.Count != 0)
            {
                return ActivityExecutionStatus.Executing;
            }
            return ActivityExecutionStatus.Closed;
        }

        protected override void OnActivityChangeAdd(ActivityExecutionContext executionContext, Activity addedActivity)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (addedActivity == null)
            {
                throw new ArgumentNullException("addedActivity");
            }
            ParallelActivity activity = executionContext.Activity as ParallelActivity;
            if ((activity.ExecutionStatus == ActivityExecutionStatus.Executing) && activity.IsExecuting)
            {
                addedActivity.RegisterForStatusChange(Activity.ClosedEvent, this);
                executionContext.ExecuteActivity(addedActivity);
            }
        }

        protected override void OnActivityChangeRemove(ActivityExecutionContext rootExecutionContext, Activity removedActivity)
        {
        }

        protected override void OnClosed(IServiceProvider provider)
        {
            base.RemoveProperty(IsExecutingProperty);
        }

        protected override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            base.OnWorkflowChangesCompleted(executionContext);
            if (this.IsExecuting)
            {
                bool flag = true;
                for (int i = 0; i < base.EnabledActivities.Count; i++)
                {
                    Activity activity = base.EnabledActivities[i];
                    if (activity.ExecutionStatus != ActivityExecutionStatus.Closed)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    executionContext.CloseActivity();
                }
            }
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
            ParallelActivity activity = context.Activity as ParallelActivity;
            e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
            bool flag = true;
            for (int i = 0; i < activity.EnabledActivities.Count; i++)
            {
                Activity activity2 = activity.EnabledActivities[i];
                if ((activity2.ExecutionStatus != ActivityExecutionStatus.Initialized) && (activity2.ExecutionStatus != ActivityExecutionStatus.Closed))
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                context.CloseActivity();
            }
        }

        private bool IsExecuting
        {
            get
            {
                return (bool) base.GetValue(IsExecutingProperty);
            }
            set
            {
                base.SetValue(IsExecutingProperty, value);
            }
        }
    }
}


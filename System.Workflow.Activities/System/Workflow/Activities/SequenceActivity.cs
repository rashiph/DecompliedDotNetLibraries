namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [SRCategory("Standard"), SRDescription("SequenceActivityDescription"), ToolboxItem(typeof(ActivityToolboxItem)), Designer(typeof(System.Workflow.Activities.SequenceDesigner), typeof(IDesigner)), ToolboxBitmap(typeof(SequenceActivity), "Resources.Sequence.png")]
    public class SequenceActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        private static readonly DependencyProperty ActiveChildQualifiedNameProperty = DependencyProperty.Register("ActiveChildQualifiedName", typeof(string), typeof(SequenceActivity));
        [NonSerialized]
        private bool activeChildRemovedInDynamicUpdate;
        private static readonly DependencyProperty SequenceFaultingProperty = DependencyProperty.Register("SequenceFaulting", typeof(bool), typeof(SequenceActivity));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SequenceActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SequenceActivity(string name) : base(name)
        {
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            for (int i = base.EnabledActivities.Count - 1; i >= 0; i--)
            {
                Activity activity = base.EnabledActivities[i];
                if (activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    executionContext.CancelActivity(activity);
                    return ActivityExecutionStatus.Canceling;
                }
                if ((activity.ExecutionStatus == ActivityExecutionStatus.Canceling) || (activity.ExecutionStatus == ActivityExecutionStatus.Faulting))
                {
                    return ActivityExecutionStatus.Canceling;
                }
                if (activity.ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    base.RemoveProperty(ActiveChildQualifiedNameProperty);
                    return ActivityExecutionStatus.Closed;
                }
            }
            return ActivityExecutionStatus.Closed;
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (base.EnabledActivities.Count == 0)
            {
                this.OnSequenceComplete(executionContext);
                return ActivityExecutionStatus.Closed;
            }
            base.EnabledActivities[0].RegisterForStatusChange(Activity.ClosedEvent, this);
            executionContext.ExecuteActivity(base.EnabledActivities[0]);
            base.SetValue(ActiveChildQualifiedNameProperty, base.EnabledActivities[0].QualifiedName);
            return ActivityExecutionStatus.Executing;
        }

        protected override ActivityExecutionStatus HandleFault(ActivityExecutionContext executionContext, Exception exception)
        {
            base.SetValue(SequenceFaultingProperty, true);
            ActivityExecutionStatus status = base.HandleFault(executionContext, exception);
            if (status == ActivityExecutionStatus.Closed)
            {
                base.RemoveProperty(SequenceFaultingProperty);
            }
            return status;
        }

        protected override void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            string str = base.GetValue(ActiveChildQualifiedNameProperty) as string;
            if (removedActivity.QualifiedName.Equals(str))
            {
                this.activeChildRemovedInDynamicUpdate = true;
            }
            base.OnActivityChangeRemove(executionContext, removedActivity);
        }

        protected virtual void OnSequenceComplete(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            base.RemoveProperty(ActiveChildQualifiedNameProperty);
        }

        protected override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            if ((base.GetValue(ActiveChildQualifiedNameProperty) is string) && this.activeChildRemovedInDynamicUpdate)
            {
                if ((base.ExecutionStatus == ActivityExecutionStatus.Canceling) || ((base.ExecutionStatus == ActivityExecutionStatus.Faulting) && ((bool) base.GetValue(SequenceFaultingProperty))))
                {
                    if (base.ExecutionStatus == ActivityExecutionStatus.Faulting)
                    {
                        base.RemoveProperty(SequenceFaultingProperty);
                    }
                    base.RemoveProperty(ActiveChildQualifiedNameProperty);
                    executionContext.CloseActivity();
                }
                else if ((base.ExecutionStatus == ActivityExecutionStatus.Executing) && !this.TryScheduleNextChild(executionContext))
                {
                    this.OnSequenceComplete(executionContext);
                    executionContext.CloseActivity();
                }
            }
            this.activeChildRemovedInDynamicUpdate = false;
            base.OnWorkflowChangesCompleted(executionContext);
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
            ActivityExecutionContext executionContext = sender as ActivityExecutionContext;
            if (executionContext == null)
            {
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");
            }
            e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
            SequenceActivity activity = executionContext.Activity as SequenceActivity;
            if (activity == null)
            {
                throw new ArgumentException("sender");
            }
            if ((activity.ExecutionStatus == ActivityExecutionStatus.Canceling) || ((activity.ExecutionStatus == ActivityExecutionStatus.Faulting) && ((bool) base.GetValue(SequenceFaultingProperty))))
            {
                if (activity.ExecutionStatus == ActivityExecutionStatus.Faulting)
                {
                    base.RemoveProperty(SequenceFaultingProperty);
                }
                base.RemoveProperty(ActiveChildQualifiedNameProperty);
                executionContext.CloseActivity();
            }
            else if ((activity.ExecutionStatus == ActivityExecutionStatus.Executing) && !this.TryScheduleNextChild(executionContext))
            {
                this.OnSequenceComplete(executionContext);
                executionContext.CloseActivity();
            }
        }

        private bool TryScheduleNextChild(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            IList<Activity> enabledActivities = base.EnabledActivities;
            if (enabledActivities.Count == 0)
            {
                return false;
            }
            int num = 0;
            for (int i = enabledActivities.Count - 1; i >= 0; i--)
            {
                if (enabledActivities[i].ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    if (i == (enabledActivities.Count - 1))
                    {
                        return false;
                    }
                    num = i + 1;
                    break;
                }
            }
            enabledActivities[num].RegisterForStatusChange(Activity.ClosedEvent, this);
            executionContext.ExecuteActivity(enabledActivities[num]);
            base.SetValue(ActiveChildQualifiedNameProperty, enabledActivities[num].QualifiedName);
            return true;
        }
    }
}


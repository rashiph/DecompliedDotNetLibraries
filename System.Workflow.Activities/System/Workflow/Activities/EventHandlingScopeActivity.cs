namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    [ActivityValidator(typeof(EventHandlingScopeValidator)), ToolboxBitmap(typeof(EventHandlingScopeActivity), "Resources.Sequence.png"), Designer(typeof(EventHandlingScopeDesigner), typeof(IDesigner)), SRDescription("EventHandlingScopeActivityDescription"), ToolboxItem(typeof(ActivityToolboxItem))]
    public sealed class EventHandlingScopeActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        [NonSerialized]
        private bool bodyActivityRemovedInDynamicUpdate;
        [NonSerialized]
        private bool eventHandlersRemovedInDynamicUpdate;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventHandlingScopeActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventHandlingScopeActivity(string name) : base(name)
        {
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            Activity bodyActivity = this.BodyActivity;
            System.Workflow.Activities.EventHandlersActivity eventHandlersActivity = this.EventHandlersActivity;
            if ((bodyActivity == null) && (eventHandlersActivity == null))
            {
                return ActivityExecutionStatus.Closed;
            }
            bool flag = false;
            if ((bodyActivity != null) && (bodyActivity.ExecutionStatus == ActivityExecutionStatus.Executing))
            {
                executionContext.CancelActivity(bodyActivity);
                flag = true;
            }
            if ((eventHandlersActivity != null) && (eventHandlersActivity.ExecutionStatus == ActivityExecutionStatus.Executing))
            {
                executionContext.CancelActivity(eventHandlersActivity);
                flag = true;
            }
            if ((!flag && ((bodyActivity == null) || ((bodyActivity.ExecutionStatus != ActivityExecutionStatus.Faulting) && (bodyActivity.ExecutionStatus != ActivityExecutionStatus.Canceling)))) && ((eventHandlersActivity == null) || ((eventHandlersActivity.ExecutionStatus != ActivityExecutionStatus.Faulting) && (eventHandlersActivity.ExecutionStatus != ActivityExecutionStatus.Canceling))))
            {
                return ActivityExecutionStatus.Closed;
            }
            return base.ExecutionStatus;
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            Activity bodyActivity = this.BodyActivity;
            if (bodyActivity == null)
            {
                return ActivityExecutionStatus.Closed;
            }
            System.Workflow.Activities.EventHandlersActivity eventHandlersActivity = this.EventHandlersActivity;
            if (eventHandlersActivity != null)
            {
                eventHandlersActivity.RegisterForStatusChange(Activity.ClosedEvent, this);
                executionContext.ExecuteActivity(eventHandlersActivity);
            }
            bodyActivity.RegisterForStatusChange(Activity.ClosedEvent, this);
            executionContext.ExecuteActivity(bodyActivity);
            return base.ExecutionStatus;
        }

        protected override void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            base.OnActivityChangeRemove(executionContext, removedActivity);
            if (removedActivity is System.Workflow.Activities.EventHandlersActivity)
            {
                this.eventHandlersRemovedInDynamicUpdate = true;
            }
            else
            {
                this.bodyActivityRemovedInDynamicUpdate = true;
            }
        }

        protected override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            base.OnWorkflowChangesCompleted(executionContext);
            if (base.ExecutionStatus == ActivityExecutionStatus.Executing)
            {
                if (this.bodyActivityRemovedInDynamicUpdate)
                {
                    if ((this.EventHandlersActivity == null) || (this.EventHandlersActivity.ExecutionStatus == ActivityExecutionStatus.Closed))
                    {
                        executionContext.CloseActivity();
                    }
                    else if (this.EventHandlersActivity.ExecutionStatus == ActivityExecutionStatus.Executing)
                    {
                        this.EventHandlersActivity.UnsubscribeAndClose();
                    }
                }
                if (this.eventHandlersRemovedInDynamicUpdate && ((this.BodyActivity == null) || (this.BodyActivity.ExecutionStatus == ActivityExecutionStatus.Closed)))
                {
                    executionContext.CloseActivity();
                }
            }
            this.eventHandlersRemovedInDynamicUpdate = false;
            this.bodyActivityRemovedInDynamicUpdate = false;
        }

        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
            {
                throw new ArgumentException();
            }
            e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
            if (e.Activity is System.Workflow.Activities.EventHandlersActivity)
            {
                if (this.BodyActivity.ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    context.CloseActivity();
                }
            }
            else
            {
                System.Workflow.Activities.EventHandlersActivity eventHandlersActivity = this.EventHandlersActivity;
                if ((eventHandlersActivity == null) || (eventHandlersActivity.ExecutionStatus == ActivityExecutionStatus.Closed))
                {
                    context.CloseActivity();
                }
                else
                {
                    eventHandlersActivity.UnsubscribeAndClose();
                }
            }
        }

        internal Activity BodyActivity
        {
            get
            {
                Activity activity = null;
                foreach (Activity activity2 in base.EnabledActivities)
                {
                    if (!(activity2 is System.Workflow.Activities.EventHandlersActivity))
                    {
                        activity = activity2;
                    }
                }
                return activity;
            }
        }

        internal System.Workflow.Activities.EventHandlersActivity EventHandlersActivity
        {
            get
            {
                System.Workflow.Activities.EventHandlersActivity activity = null;
                foreach (Activity activity2 in base.EnabledActivities)
                {
                    if (activity2 is System.Workflow.Activities.EventHandlersActivity)
                    {
                        activity = activity2 as System.Workflow.Activities.EventHandlersActivity;
                    }
                }
                return activity;
            }
        }
    }
}


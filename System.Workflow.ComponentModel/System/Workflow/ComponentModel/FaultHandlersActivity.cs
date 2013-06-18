namespace System.Workflow.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    [SRCategory("Standard"), ToolboxItem(false), Designer(typeof(FaultHandlersActivityDesigner), typeof(IDesigner)), ToolboxBitmap(typeof(FaultHandlersActivity), "Resources.Exceptions.png"), ActivityValidator(typeof(FaultHandlersActivityValidator)), AlternateFlowActivity]
    public sealed class FaultHandlersActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        [NonSerialized]
        private bool activeChildRemoved;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public FaultHandlersActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public FaultHandlersActivity(string name) : base(name)
        {
        }

        protected internal override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            for (int i = 0; i < base.EnabledActivities.Count; i++)
            {
                Activity activity = base.EnabledActivities[i];
                if (activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    executionContext.CancelActivity(activity);
                }
                if ((activity.ExecutionStatus == ActivityExecutionStatus.Canceling) || (activity.ExecutionStatus == ActivityExecutionStatus.Faulting))
                {
                    return base.ExecutionStatus;
                }
            }
            return ActivityExecutionStatus.Closed;
        }

        private bool CanHandleException(FaultHandlerActivity exceptionHandler, Type et)
        {
            Type faultType = exceptionHandler.FaultType;
            if (!(et == faultType))
            {
                return et.IsSubclassOf(faultType);
            }
            return true;
        }

        protected internal override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            Exception e = base.Parent.GetValue(ActivityExecutionContext.CurrentExceptionProperty) as Exception;
            if (e != null)
            {
                Type et = e.GetType();
                foreach (FaultHandlerActivity activity in base.EnabledActivities)
                {
                    if (this.CanHandleException(activity, et))
                    {
                        base.Parent.RemoveProperty(ActivityExecutionContext.CurrentExceptionProperty);
                        activity.SetException(e);
                        activity.RegisterForStatusChange(Activity.ClosedEvent, this);
                        executionContext.ExecuteActivity(activity);
                        return ActivityExecutionStatus.Executing;
                    }
                }
            }
            return ActivityExecutionStatus.Closed;
        }

        protected internal override void Initialize(IServiceProvider provider)
        {
            if (base.Parent == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_MustHaveParent"));
            }
            base.Initialize(provider);
        }

        protected internal override void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            if (removedActivity == null)
            {
                throw new ArgumentNullException("removedActivity");
            }
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if ((removedActivity.ExecutionStatus == ActivityExecutionStatus.Closed) && (base.ExecutionStatus != ActivityExecutionStatus.Closed))
            {
                this.activeChildRemoved = true;
            }
            base.OnActivityChangeRemove(executionContext, removedActivity);
        }

        protected override void OnClosed(IServiceProvider provider)
        {
        }

        protected internal override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (this.activeChildRemoved)
            {
                executionContext.CloseActivity();
                this.activeChildRemoved = false;
            }
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
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
            {
                throw new ArgumentException("Error_SenderMustBeActivityExecutionContext", "sender");
            }
            e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
            context.CloseActivity();
        }
    }
}


namespace System.Workflow.ComponentModel
{
    using System;

    internal sealed class SynchronizationFilter : ActivityExecutionFilter, IActivityEventListener<EventArgs>, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        public override ActivityExecutionStatus Execute(Activity activity, ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            activity.RegisterForStatusChange(Activity.LockCountOnStatusChangeChangedEvent, this);
            activity.HoldLockOnStatusChange(this);
            if (executionContext.AcquireLocks(this))
            {
                return this.ExecuteActivityNow(executionContext);
            }
            return activity.ExecutionStatus;
        }

        private ActivityExecutionStatus ExecuteActivityNow(ActivityExecutionContext context)
        {
            return base.NextActivityExecutorInChain(context.Activity).Execute(context.Activity, context);
        }

        public void OnEvent(object sender, EventArgs e)
        {
            ActivityExecutionContext context = (ActivityExecutionContext) sender;
            if ((context.Activity.ExecutionStatus == ActivityExecutionStatus.Executing) && (this.ExecuteActivityNow(context) == ActivityExecutionStatus.Closed))
            {
                context.CloseActivity();
            }
        }

        public void OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context.Activity.HasPrimaryClosed && (context.Activity.LockCountOnStatusChange == 1))
            {
                context.ReleaseLocks(false);
                context.Activity.UnregisterForStatusChange(Activity.LockCountOnStatusChangeChangedEvent, this);
                context.Activity.ReleaseLockOnStatusChange(this);
            }
        }
    }
}


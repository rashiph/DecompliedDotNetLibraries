namespace System.Workflow.ComponentModel
{
    using System;

    internal class CompensationHandlingFilter : ActivityExecutionFilter, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        public static DependencyProperty CompensateProcessedProperty = DependencyProperty.RegisterAttached("CompensateProcessed", typeof(bool), typeof(CompensationHandlingFilter), new PropertyMetadata(false));
        internal static DependencyProperty LastCompensatedOrderIdProperty = DependencyProperty.RegisterAttached("LastCompensatedOrderId", typeof(int), typeof(CompensationHandlingFilter), new PropertyMetadata(false));

        public override ActivityExecutionStatus Compensate(Activity activity, ActivityExecutionContext executionContext)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            executionContext.Activity.HoldLockOnStatusChange(this);
            return base.NextActivityExecutorInChain(activity).Compensate(activity, executionContext);
        }

        internal static Activity GetCompensationHandler(Activity activityWithCompensation)
        {
            CompositeActivity activity2 = activityWithCompensation as CompositeActivity;
            if (activity2 != null)
            {
                foreach (Activity activity3 in ((ISupportAlternateFlow) activity2).AlternateFlowActivities)
                {
                    if (activity3 is CompensationHandlerActivity)
                    {
                        return activity3;
                    }
                }
            }
            return null;
        }

        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
            {
                throw new ArgumentException("sender");
            }
            if (e.Activity == context.Activity)
            {
                if (context.Activity.HasPrimaryClosed && !((bool) context.Activity.GetValue(CompensateProcessedProperty)))
                {
                    context.Activity.SetValue(CompensateProcessedProperty, true);
                    if (context.Activity.ExecutionResult != ActivityExecutionResult.Compensated)
                    {
                        context.Activity.ReleaseLockOnStatusChange(this);
                    }
                    else
                    {
                        Activity compensationHandler = GetCompensationHandler(context.Activity);
                        if (compensationHandler != null)
                        {
                            compensationHandler.RegisterForStatusChange(Activity.ClosedEvent, this);
                            context.ExecuteActivity(compensationHandler);
                        }
                        else if (!CompensationUtils.TryCompensateLastCompletedChildActivity(context, context.Activity, this))
                        {
                            context.Activity.ReleaseLockOnStatusChange(this);
                        }
                    }
                }
            }
            else if ((e.Activity is CompensationHandlerActivity) && (e.ExecutionStatus == ActivityExecutionStatus.Closed))
            {
                e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
                context.Activity.ReleaseLockOnStatusChange(this);
            }
            else if (e.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
                if (!CompensationUtils.TryCompensateLastCompletedChildActivity(context, context.Activity, this))
                {
                    context.Activity.ReleaseLockOnStatusChange(this);
                }
            }
        }
    }
}


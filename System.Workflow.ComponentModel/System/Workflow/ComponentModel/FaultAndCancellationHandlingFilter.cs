namespace System.Workflow.ComponentModel
{
    using System;

    internal sealed class FaultAndCancellationHandlingFilter : ActivityExecutionFilter, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        public static DependencyProperty FaultProcessedProperty = DependencyProperty.RegisterAttached("FaultProcessed", typeof(bool), typeof(FaultAndCancellationHandlingFilter), new PropertyMetadata(false));

        public override ActivityExecutionStatus Execute(Activity activity, ActivityExecutionContext executionContext)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (!(activity is CompositeActivity))
            {
                throw new InvalidOperationException("activity");
            }
            executionContext.Activity.HoldLockOnStatusChange(this);
            return base.Execute(activity, executionContext);
        }

        internal static Activity GetCancellationHandler(Activity activityWithCancelHandler)
        {
            CompositeActivity activity2 = activityWithCancelHandler as CompositeActivity;
            if (activity2 != null)
            {
                foreach (Activity activity3 in ((ISupportAlternateFlow) activity2).AlternateFlowActivities)
                {
                    if (activity3 is CancellationHandlerActivity)
                    {
                        return activity3;
                    }
                }
            }
            return null;
        }

        internal static CompositeActivity GetFaultHandlers(Activity activityWithExceptionHandlers)
        {
            CompositeActivity activity2 = activityWithExceptionHandlers as CompositeActivity;
            if (activity2 != null)
            {
                foreach (Activity activity3 in ((ISupportAlternateFlow) activity2).AlternateFlowActivities)
                {
                    if (activity3 is FaultHandlersActivity)
                    {
                        return (activity3 as CompositeActivity);
                    }
                }
            }
            return null;
        }

        public override ActivityExecutionStatus HandleFault(Activity activity, ActivityExecutionContext executionContext, Exception exception)
        {
            if (activity.HasPrimaryClosed)
            {
                Activity faultHandlers = GetFaultHandlers(executionContext.Activity);
                if (((faultHandlers != null) && (faultHandlers.ExecutionStatus != ActivityExecutionStatus.Closed)) && (faultHandlers.ExecutionStatus != ActivityExecutionStatus.Initialized))
                {
                    if (faultHandlers.ExecutionStatus == ActivityExecutionStatus.Executing)
                    {
                        executionContext.CancelActivity(faultHandlers);
                    }
                    return ActivityExecutionStatus.Faulting;
                }
                faultHandlers = GetCancellationHandler(executionContext.Activity);
                if (((faultHandlers != null) && (faultHandlers.ExecutionStatus != ActivityExecutionStatus.Closed)) && (faultHandlers.ExecutionStatus != ActivityExecutionStatus.Initialized))
                {
                    if (faultHandlers.ExecutionStatus == ActivityExecutionStatus.Executing)
                    {
                        executionContext.CancelActivity(faultHandlers);
                    }
                    return ActivityExecutionStatus.Faulting;
                }
                if ((bool) activity.GetValue(FaultProcessedProperty))
                {
                    this.SafeReleaseLockOnStatusChange(executionContext);
                }
            }
            return base.HandleFault(activity, executionContext, exception);
        }

        public void OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
            {
                throw new ArgumentException("sender");
            }
            if (e.Activity == context.Activity)
            {
                if (context.Activity.HasPrimaryClosed && !((bool) context.Activity.GetValue(FaultProcessedProperty)))
                {
                    context.Activity.SetValue(FaultProcessedProperty, true);
                    if ((!context.Activity.WasExecuting || (context.Activity.ExecutionResult != ActivityExecutionResult.Faulted)) || (context.Activity.GetValue(ActivityExecutionContext.CurrentExceptionProperty) == null))
                    {
                        if (context.Activity.ExecutionResult != ActivityExecutionResult.Canceled)
                        {
                            this.SafeReleaseLockOnStatusChange(context);
                        }
                        else
                        {
                            Activity cancellationHandler = GetCancellationHandler(context.Activity);
                            if (cancellationHandler != null)
                            {
                                cancellationHandler.RegisterForStatusChange(Activity.ClosedEvent, this);
                                context.ExecuteActivity(cancellationHandler);
                            }
                            else if (!CompensationUtils.TryCompensateLastCompletedChildActivity(context, context.Activity, this))
                            {
                                this.SafeReleaseLockOnStatusChange(context);
                            }
                        }
                    }
                    else
                    {
                        CompositeActivity faultHandlers = GetFaultHandlers(context.Activity);
                        if (faultHandlers != null)
                        {
                            faultHandlers.RegisterForStatusChange(Activity.ClosedEvent, this);
                            context.ExecuteActivity(faultHandlers);
                        }
                        else if (!CompensationUtils.TryCompensateLastCompletedChildActivity(context, context.Activity, this))
                        {
                            this.SafeReleaseLockOnStatusChange(context);
                        }
                    }
                }
            }
            else if (((e.Activity is FaultHandlersActivity) || (e.Activity is CancellationHandlerActivity)) && (e.ExecutionStatus == ActivityExecutionStatus.Closed))
            {
                e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
                if (context.Activity.GetValue(ActivityExecutionContext.CurrentExceptionProperty) == null)
                {
                    this.SafeReleaseLockOnStatusChange(context);
                }
                else if (!CompensationUtils.TryCompensateLastCompletedChildActivity(context, context.Activity, this))
                {
                    this.SafeReleaseLockOnStatusChange(context);
                }
            }
            else if (e.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
                if (!CompensationUtils.TryCompensateLastCompletedChildActivity(context, context.Activity, this))
                {
                    this.SafeReleaseLockOnStatusChange(context);
                }
            }
        }

        private void SafeReleaseLockOnStatusChange(ActivityExecutionContext context)
        {
            try
            {
                context.Activity.ReleaseLockOnStatusChange(this);
            }
            catch (Exception)
            {
                context.Activity.RemoveProperty(FaultProcessedProperty);
                throw;
            }
        }
    }
}


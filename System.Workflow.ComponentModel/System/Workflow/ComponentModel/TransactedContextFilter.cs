namespace System.Workflow.ComponentModel
{
    using System;

    internal sealed class TransactedContextFilter : ActivityExecutionFilter, IActivityEventListener<EventArgs>, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
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
            if (!activity.SupportsTransaction)
            {
                throw new ArgumentException("activity");
            }
            activity.RegisterForStatusChange(Activity.LockCountOnStatusChangeChangedEvent, this);
            activity.HoldLockOnStatusChange(this);
            return ExecuteActivity(activity, executionContext, false);
        }

        private static ActivityExecutionStatus ExecuteActivity(Activity activity, ActivityExecutionContext context, bool locksAcquired)
        {
            TransactedContextFilter activityExecutorFromType = (TransactedContextFilter) ActivityExecutors.GetActivityExecutorFromType(typeof(TransactedContextFilter));
            if (!locksAcquired && !context.AcquireLocks(activityExecutorFromType))
            {
                return activity.ExecutionStatus;
            }
            if (GetTransactionOptions(activity) != null)
            {
                context.CheckpointInstanceState();
            }
            return activityExecutorFromType.NextActivityExecutorInChain(activity).Execute(activity, context);
        }

        internal static WorkflowTransactionOptions GetTransactionOptions(Activity activity)
        {
            return (activity.GetValue((activity is TransactionScopeActivity) ? TransactionScopeActivity.TransactionOptionsProperty : CompensatableTransactionScopeActivity.TransactionOptionsProperty) as WorkflowTransactionOptions);
        }

        private void OnRevertInstanceState(object sender, EventArgs e)
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
            StateRevertedEventArgs args = e as StateRevertedEventArgs;
            context.Activity.SetValueCommon(ActivityExecutionContext.CurrentExceptionProperty, args.Exception, ActivityExecutionContext.CurrentExceptionProperty.DefaultMetadata, false);
            context.ReleaseLocks(false);
            context.Activity.UnregisterForStatusChange(Activity.LockCountOnStatusChangeChangedEvent, this);
            context.Activity.ReleaseLockOnStatusChange(this);
        }

        void IActivityEventListener<EventArgs>.OnEvent(object sender, EventArgs e)
        {
            ActivityExecutionContext context = (ActivityExecutionContext) sender;
            if ((context.Activity.ExecutionStatus == ActivityExecutionStatus.Executing) && (ExecuteActivity(context.Activity, context, true) == ActivityExecutionStatus.Closed))
            {
                context.CloseActivity();
            }
        }

        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
            {
                throw new ArgumentException("sender");
            }
            if (context.Activity.HasPrimaryClosed && (context.Activity.LockCountOnStatusChange == 1))
            {
                Exception exception = (Exception) context.Activity.GetValue(ActivityExecutionContext.CurrentExceptionProperty);
                if (exception != null)
                {
                    if (GetTransactionOptions(context.Activity) != null)
                    {
                        context.RequestRevertToCheckpointState(new EventHandler<EventArgs>(this.OnRevertInstanceState), new StateRevertedEventArgs(exception), false, null);
                    }
                    else
                    {
                        context.ReleaseLocks(false);
                        context.Activity.UnregisterForStatusChange(Activity.LockCountOnStatusChangeChangedEvent, this);
                        context.Activity.ReleaseLockOnStatusChange(this);
                    }
                }
                else
                {
                    try
                    {
                        context.ReleaseLocks(true);
                        context.Activity.UnregisterForStatusChange(Activity.LockCountOnStatusChangeChangedEvent, this);
                        context.Activity.ReleaseLockOnStatusChange(this);
                        context.DisposeCheckpointState();
                    }
                    catch
                    {
                        context.Activity.RegisterForStatusChange(Activity.LockCountOnStatusChangeChangedEvent, this);
                        throw;
                    }
                }
            }
        }
    }
}


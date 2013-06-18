namespace System.Workflow.ComponentModel
{
    using System;

    internal class CompositeActivityExecutor<T> : ActivityExecutor<T>, ISupportWorkflowChanges where T: CompositeActivity
    {
        protected override ActivityExecutionStatus Cancel(T activity, ActivityExecutionContext executionContext)
        {
            return base.Cancel(activity, executionContext);
        }

        protected override ActivityExecutionStatus Execute(T activity, ActivityExecutionContext executionContext)
        {
            return base.Execute(activity, executionContext);
        }

        void ISupportWorkflowChanges.OnActivityAdded(ActivityExecutionContext executionContext, Activity addedActivity)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (addedActivity == null)
            {
                throw new ArgumentNullException("addedActivity");
            }
            CompositeActivity activity = executionContext.Activity as CompositeActivity;
            if (activity == null)
            {
                throw new ArgumentException("Error_InvalidActivityExecutionContext", "executionContext");
            }
            activity.OnActivityChangeAdd(executionContext, addedActivity);
        }

        void ISupportWorkflowChanges.OnActivityRemoved(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (removedActivity == null)
            {
                throw new ArgumentNullException("removedActivity");
            }
            CompositeActivity activity = executionContext.Activity as CompositeActivity;
            if (activity == null)
            {
                throw new ArgumentException("Error_InvalidActivityExecutionContext", "executionContext");
            }
            activity.OnActivityChangeRemove(executionContext, removedActivity);
        }

        void ISupportWorkflowChanges.OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            CompositeActivity activity = executionContext.Activity as CompositeActivity;
            if (activity == null)
            {
                throw new ArgumentException("Error_InvalidActivityExecutionContext", "executionContext");
            }
            activity.OnWorkflowChangesCompleted(executionContext);
        }
    }
}


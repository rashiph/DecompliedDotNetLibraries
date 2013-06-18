namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Runtime;

    internal abstract class ActivityExecutionFilter : ActivityExecutor, ISupportWorkflowChanges
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ActivityExecutionFilter()
        {
        }

        public override ActivityExecutionStatus Cancel(Activity activity, ActivityExecutionContext executionContext)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            ActivityExecutor executor = this.NextActivityExecutorInChain(executionContext.Activity);
            if (!(executor is ActivityExecutionFilter) && executionContext.Activity.HasPrimaryClosed)
            {
                return ActivityExecutionStatus.Closed;
            }
            return executor.Cancel(activity, executionContext);
        }

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
            return this.NextActivityExecutorInChain(executionContext.Activity).Compensate(activity, executionContext);
        }

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
            return this.NextActivityExecutorInChain(executionContext.Activity).Execute(activity, executionContext);
        }

        public override ActivityExecutionStatus HandleFault(Activity activity, ActivityExecutionContext executionContext, Exception exception)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            ActivityExecutor executor = this.NextActivityExecutorInChain(executionContext.Activity);
            if (!(executor is ActivityExecutionFilter) && executionContext.Activity.HasPrimaryClosed)
            {
                return ActivityExecutionStatus.Closed;
            }
            return executor.HandleFault(activity, executionContext, exception);
        }

        protected ActivityExecutor NextActivityExecutorInChain(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            ActivityExecutor executor = null;
            IList activityExecutors = ActivityExecutors.GetActivityExecutors(activity);
            int index = activityExecutors.IndexOf(this);
            if (index < (activityExecutors.Count - 1))
            {
                executor = (ActivityExecutor) activityExecutors[index + 1];
            }
            return executor;
        }

        protected ISupportWorkflowChanges NextDynamicChangeExecutorInChain(Activity activity)
        {
            return (this.NextActivityExecutorInChain(activity) as ISupportWorkflowChanges);
        }

        public virtual void OnActivityAdded(ActivityExecutionContext executionContext, Activity addedActivity)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (addedActivity == null)
            {
                throw new ArgumentNullException("addedActivity");
            }
            this.NextDynamicChangeExecutorInChain(executionContext.Activity).OnActivityAdded(executionContext, addedActivity);
        }

        public virtual void OnActivityRemoved(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (removedActivity == null)
            {
                throw new ArgumentNullException("removedActivity");
            }
            this.NextDynamicChangeExecutorInChain(executionContext.Activity).OnActivityRemoved(executionContext, removedActivity);
        }

        public virtual void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            this.NextDynamicChangeExecutorInChain(executionContext.Activity).OnWorkflowChangesCompleted(executionContext);
        }
    }
}


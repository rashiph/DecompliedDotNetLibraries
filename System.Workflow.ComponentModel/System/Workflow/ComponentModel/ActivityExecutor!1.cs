namespace System.Workflow.ComponentModel
{
    using System;

    internal class ActivityExecutor<T> : ActivityExecutor where T: Activity
    {
        public sealed override ActivityExecutionStatus Cancel(Activity activity, ActivityExecutionContext executionContext)
        {
            return this.Cancel((T) activity, executionContext);
        }

        protected virtual ActivityExecutionStatus Cancel(T activity, ActivityExecutionContext executionContext)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            return activity.Cancel(executionContext);
        }

        public sealed override ActivityExecutionStatus Compensate(Activity activity, ActivityExecutionContext executionContext)
        {
            return this.Compensate((T) activity, executionContext);
        }

        protected virtual ActivityExecutionStatus Compensate(T activity, ActivityExecutionContext executionContext)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            return ((ICompensatableActivity) activity).Compensate(executionContext);
        }

        public sealed override ActivityExecutionStatus Execute(Activity activity, ActivityExecutionContext executionContext)
        {
            return this.Execute((T) activity, executionContext);
        }

        protected virtual ActivityExecutionStatus Execute(T activity, ActivityExecutionContext executionContext)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            return activity.Execute(executionContext);
        }

        public sealed override ActivityExecutionStatus HandleFault(Activity activity, ActivityExecutionContext executionContext, Exception exception)
        {
            return this.HandleFault((T) activity, executionContext, exception);
        }

        protected virtual ActivityExecutionStatus HandleFault(T activity, ActivityExecutionContext executionContext, Exception exception)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            return activity.HandleFault(executionContext, exception);
        }
    }
}


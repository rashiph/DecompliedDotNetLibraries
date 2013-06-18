namespace System.Workflow.ComponentModel
{
    using System;
    using System.Runtime;

    internal abstract class ActivityExecutor
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ActivityExecutor()
        {
        }

        public abstract ActivityExecutionStatus Cancel(Activity activity, ActivityExecutionContext executionContext);
        public abstract ActivityExecutionStatus Compensate(Activity activity, ActivityExecutionContext executionContext);
        public abstract ActivityExecutionStatus Execute(Activity activity, ActivityExecutionContext executionContext);
        public abstract ActivityExecutionStatus HandleFault(Activity activity, ActivityExecutionContext executionContext, Exception exception);
    }
}


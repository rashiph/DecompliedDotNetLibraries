namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Runtime;
    using System.Threading;

    public abstract class WorkflowSchedulerService : WorkflowRuntimeService
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected WorkflowSchedulerService()
        {
        }

        protected internal abstract void Cancel(Guid timerId);
        protected internal abstract void Schedule(WaitCallback callback, Guid workflowInstanceId);
        protected internal abstract void Schedule(WaitCallback callback, Guid workflowInstanceId, DateTime whenUtc, Guid timerId);
    }
}


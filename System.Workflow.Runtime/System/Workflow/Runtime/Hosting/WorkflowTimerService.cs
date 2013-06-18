namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Threading;
    using System.Workflow.ComponentModel;

    internal class WorkflowTimerService : WorkflowRuntimeService, ITimerService
    {
        public void CancelTimer(Guid timerId)
        {
            (base.Runtime.GetService(typeof(WorkflowSchedulerService)) as WorkflowSchedulerService).Cancel(timerId);
        }

        public void ScheduleTimer(WaitCallback callback, Guid workflowInstanceId, DateTime whenUtc, Guid timerId)
        {
            (base.Runtime.GetService(typeof(WorkflowSchedulerService)) as WorkflowSchedulerService).Schedule(callback, workflowInstanceId, whenUtc, timerId);
        }
    }
}


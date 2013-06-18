namespace System.Workflow.ComponentModel
{
    using System;
    using System.Threading;

    internal interface ITimerService
    {
        void CancelTimer(Guid timerId);
        void ScheduleTimer(WaitCallback callback, Guid workflowInstanceId, DateTime whenUtc, Guid timerId);
    }
}


namespace System.Workflow.Runtime
{
    using System;

    [Serializable]
    internal enum WorkflowEventInternal
    {
        Created,
        Completing,
        Completed,
        SchedulerEmpty,
        Idle,
        Suspending,
        Suspended,
        Resuming,
        Resumed,
        Persisting,
        Persisted,
        Unloading,
        Unloaded,
        Loaded,
        Exception,
        Terminating,
        Terminated,
        Aborting,
        Aborted,
        Runnable,
        Executing,
        NotExecuting,
        UserTrackPoint,
        ActivityStatusChange,
        ActivityStateCreated,
        HandlerEntered,
        HandlerExited,
        DynamicChangeBegin,
        DynamicChangeRollback,
        DynamicChangeCommit,
        Creating,
        Starting,
        Started,
        Changed,
        HandlerInvoking,
        HandlerInvoked,
        ActivityExecuting,
        Loading
    }
}


namespace System.Workflow.Runtime.Tracking
{
    using System;

    [Serializable]
    public enum TrackingWorkflowEvent
    {
        Created,
        Completed,
        Idle,
        Suspended,
        Resumed,
        Persisted,
        Unloaded,
        Loaded,
        Exception,
        Terminated,
        Aborted,
        Changed,
        Started
    }
}


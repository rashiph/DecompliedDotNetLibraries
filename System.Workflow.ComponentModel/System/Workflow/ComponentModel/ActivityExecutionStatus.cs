namespace System.Workflow.ComponentModel
{
    using System;

    public enum ActivityExecutionStatus : byte
    {
        Canceling = 2,
        Closed = 3,
        Compensating = 4,
        Executing = 1,
        Faulting = 5,
        Initialized = 0
    }
}


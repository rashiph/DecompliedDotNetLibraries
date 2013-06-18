namespace System.Workflow.ComponentModel
{
    using System;

    public enum ActivityExecutionResult : byte
    {
        Canceled = 2,
        Compensated = 3,
        Faulted = 4,
        None = 0,
        Succeeded = 1,
        Uninitialized = 5
    }
}


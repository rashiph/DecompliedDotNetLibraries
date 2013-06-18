namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct PerformanceCounterActionMapping
    {
        internal PerformanceCounterOperation Operation;
        internal PerformanceCounterAction Action;
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal PerformanceCounterActionMapping(PerformanceCounterAction action, PerformanceCounterOperation operation)
        {
            this.Operation = operation;
            this.Action = action;
        }
    }
}


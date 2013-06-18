namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct PerformanceCounterStatement
    {
        internal List<PerformanceCounter> Counters;
        internal PerformanceCounterOperation Operation;
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal PerformanceCounterStatement(List<PerformanceCounter> counters, PerformanceCounterOperation operation)
        {
            this.Counters = counters;
            this.Operation = operation;
        }
    }
}


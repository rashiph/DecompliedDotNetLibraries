namespace System.Workflow.Runtime
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct PerformanceCounterData
    {
        internal string Name;
        internal string Description;
        internal PerformanceCounterType CounterType;
        internal PerformanceCounterActionMapping[] Mappings;
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal PerformanceCounterData(string name, string description, PerformanceCounterType counterType, PerformanceCounterActionMapping[] mappings)
        {
            this.Name = name;
            this.Description = description;
            this.CounterType = counterType;
            this.Mappings = mappings;
        }
    }
}


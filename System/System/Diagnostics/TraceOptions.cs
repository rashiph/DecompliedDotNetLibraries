namespace System.Diagnostics
{
    using System;

    [Flags]
    public enum TraceOptions
    {
        Callstack = 0x20,
        DateTime = 2,
        LogicalOperationStack = 1,
        None = 0,
        ProcessId = 8,
        ThreadId = 0x10,
        Timestamp = 4
    }
}


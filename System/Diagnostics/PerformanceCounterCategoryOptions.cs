namespace System.Diagnostics
{
    using System;

    [Flags]
    internal enum PerformanceCounterCategoryOptions
    {
        EnableReuse = 1,
        UseUniqueSharedMemory = 2
    }
}


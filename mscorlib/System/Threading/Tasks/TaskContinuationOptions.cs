namespace System.Threading.Tasks
{
    using System;

    [Serializable, Flags]
    public enum TaskContinuationOptions
    {
        AttachedToParent = 4,
        ExecuteSynchronously = 0x80000,
        LongRunning = 2,
        None = 0,
        NotOnCanceled = 0x40000,
        NotOnFaulted = 0x20000,
        NotOnRanToCompletion = 0x10000,
        OnlyOnCanceled = 0x30000,
        OnlyOnFaulted = 0x50000,
        OnlyOnRanToCompletion = 0x60000,
        PreferFairness = 1
    }
}


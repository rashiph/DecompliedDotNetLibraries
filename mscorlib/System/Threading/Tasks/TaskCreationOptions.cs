namespace System.Threading.Tasks
{
    using System;

    [Serializable, Flags]
    public enum TaskCreationOptions
    {
        AttachedToParent = 4,
        LongRunning = 2,
        None = 0,
        PreferFairness = 1
    }
}


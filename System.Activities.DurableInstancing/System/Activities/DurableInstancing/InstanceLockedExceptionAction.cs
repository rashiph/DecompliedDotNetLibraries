namespace System.Activities.DurableInstancing
{
    using System;

    public enum InstanceLockedExceptionAction
    {
        NoRetry,
        BasicRetry,
        AggressiveRetry
    }
}


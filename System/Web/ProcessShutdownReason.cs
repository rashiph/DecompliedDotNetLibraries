namespace System.Web
{
    using System;

    public enum ProcessShutdownReason
    {
        None,
        Unexpected,
        RequestsLimit,
        RequestQueueLimit,
        Timeout,
        IdleTimeout,
        MemoryLimitExceeded,
        PingFailed,
        DeadlockSuspected
    }
}


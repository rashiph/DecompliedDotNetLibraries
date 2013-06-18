namespace System.Activities.DurableInstancing
{
    using System;

    internal interface ILoadRetryStrategy
    {
        TimeSpan RetryDelay(int retryAttempt);
    }
}


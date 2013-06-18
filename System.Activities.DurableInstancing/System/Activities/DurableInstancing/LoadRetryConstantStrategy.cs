namespace System.Activities.DurableInstancing
{
    using System;

    internal class LoadRetryConstantStrategy : ILoadRetryStrategy
    {
        private static readonly TimeSpan defaultRetryDelay = TimeSpan.FromSeconds(5.0);

        public TimeSpan RetryDelay(int retryAttempt)
        {
            return defaultRetryDelay;
        }
    }
}


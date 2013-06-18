namespace System.Activities.DurableInstancing
{
    using System;

    internal static class LoadRetryStrategyFactory
    {
        public static ILoadRetryStrategy CreateRetryStrategy(InstanceLockedExceptionAction instanceLockedExceptionAction)
        {
            switch (instanceLockedExceptionAction)
            {
                case InstanceLockedExceptionAction.BasicRetry:
                    return new LoadRetryConstantStrategy();

                case InstanceLockedExceptionAction.AggressiveRetry:
                    return new LoadRetryExponentialBackoffStrategy();
            }
            return null;
        }
    }
}


namespace System.Activities.DurableInstancing
{
    using System;

    internal class LoadRetryExponentialBackoffStrategy : ILoadRetryStrategy
    {
        private readonly TimeSpan DefaultBackoffLimit = TimeSpan.FromSeconds(10.0);
        private readonly TimeSpan DefaultBackoffMultiplier = TimeSpan.FromMilliseconds(100.0);
        private readonly int expLimit = (((int) Math.Log(2147483647.0, 2.0)) - 1);
        private readonly TimeSpan maxDelay;
        private readonly TimeSpan multiplier;
        private Random random = new Random(DateTime.Now.Millisecond);

        public LoadRetryExponentialBackoffStrategy()
        {
            this.multiplier = this.DefaultBackoffMultiplier;
            this.maxDelay = this.DefaultBackoffLimit;
        }

        public TimeSpan RetryDelay(int retryAttempt)
        {
            int num = Math.Min(retryAttempt, this.expLimit);
            return TimeSpan.FromMilliseconds(Math.Min(this.maxDelay.TotalMilliseconds, this.multiplier.TotalMilliseconds * this.random.Next(1, (((int) 2) << num) - 1)));
        }
    }
}


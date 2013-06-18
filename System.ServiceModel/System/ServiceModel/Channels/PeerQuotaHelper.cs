namespace System.ServiceModel.Channels
{
    using System;
    using System.Threading;

    internal class PeerQuotaHelper
    {
        private int enqueuedCount;
        private int quota = 0x40;
        private AutoResetEvent waiter = new AutoResetEvent(false);

        public PeerQuotaHelper(int limit)
        {
            this.quota = limit;
        }

        public void ItemDequeued()
        {
            if (Interlocked.Decrement(ref this.enqueuedCount) >= this.quota)
            {
                this.waiter.Set();
            }
        }

        public void ReadyToEnqueueItem()
        {
            if (Interlocked.Increment(ref this.enqueuedCount) > this.quota)
            {
                this.waiter.WaitOne();
            }
        }
    }
}


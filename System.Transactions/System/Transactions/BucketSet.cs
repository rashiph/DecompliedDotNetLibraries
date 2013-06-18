namespace System.Transactions
{
    using System;

    internal class BucketSet
    {
        private long absoluteTimeout;
        internal Bucket headBucket;
        internal object nextSetWeak;
        internal BucketSet prevSet;
        private TransactionTable table;

        internal BucketSet(TransactionTable table, long absoluteTimeout)
        {
            this.headBucket = new Bucket(this);
            this.table = table;
            this.absoluteTimeout = absoluteTimeout;
        }

        internal void Add(InternalTransaction newTx)
        {
            while (!this.headBucket.Add(newTx))
            {
            }
        }

        internal void TimeoutTransactions()
        {
            Bucket headBucket = this.headBucket;
            do
            {
                headBucket.TimeoutTransactions();
                WeakReference nextBucketWeak = headBucket.nextBucketWeak;
                if (nextBucketWeak != null)
                {
                    headBucket = (Bucket) nextBucketWeak.Target;
                }
                else
                {
                    headBucket = null;
                }
            }
            while (headBucket != null);
        }

        internal long AbsoluteTimeout
        {
            get
            {
                return this.absoluteTimeout;
            }
        }
    }
}


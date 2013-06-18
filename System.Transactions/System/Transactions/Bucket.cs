namespace System.Transactions
{
    using System;
    using System.Threading;

    internal class Bucket
    {
        private int index = -1;
        internal WeakReference nextBucketWeak;
        private BucketSet owningSet;
        private Bucket previous;
        private int size = 0x400;
        private bool timedOut = false;
        private InternalTransaction[] transactions;

        internal Bucket(BucketSet owningSet)
        {
            this.transactions = new InternalTransaction[this.size];
            this.owningSet = owningSet;
        }

        internal bool Add(InternalTransaction tx)
        {
            int index = Interlocked.Increment(ref this.index);
            if (index < this.size)
            {
                tx.tableBucket = this;
                tx.bucketIndex = index;
                Thread.MemoryBarrier();
                this.transactions[index] = tx;
                if (!this.timedOut)
                {
                    goto Label_0097;
                }
                lock (tx)
                {
                    tx.State.Timeout(tx);
                    goto Label_0097;
                }
            }
            Bucket bucket = new Bucket(this.owningSet) {
                nextBucketWeak = new WeakReference(this)
            };
            if (Interlocked.CompareExchange<Bucket>(ref this.owningSet.headBucket, bucket, this) == this)
            {
                this.previous = bucket;
            }
            return false;
        Label_0097:
            return true;
        }

        internal void Remove(InternalTransaction tx)
        {
            this.transactions[tx.bucketIndex] = null;
        }

        internal void TimeoutTransactions()
        {
            int index = this.index;
            this.timedOut = true;
            Thread.MemoryBarrier();
            for (int i = 0; (i <= index) && (i < this.size); i++)
            {
                InternalTransaction tx = this.transactions[i];
                if (tx != null)
                {
                    lock (tx)
                    {
                        tx.State.Timeout(tx);
                    }
                }
            }
        }
    }
}


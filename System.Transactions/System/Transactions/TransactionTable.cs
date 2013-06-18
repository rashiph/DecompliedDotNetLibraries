namespace System.Transactions
{
    using System;
    using System.Threading;

    internal class TransactionTable
    {
        private BucketSet headBucketSet;
        private long lastTimerTime;
        private CheapUnfairReaderWriterLock rwLock;
        private long ticks;
        private const long TicksPerMillisecond = 0x2710L;
        private Timer timer;
        private bool timerEnabled;
        private const int timerInternalExponent = 9;
        private int timerInterval;

        internal TransactionTable()
        {
            this.timer = new Timer(new TimerCallback(this.ThreadTimer), null, -1, this.timerInterval);
            this.timerEnabled = false;
            this.timerInterval = 0x200;
            this.ticks = 0L;
            this.headBucketSet = new BucketSet(this, 0x7fffffffffffffffL);
            this.rwLock = new CheapUnfairReaderWriterLock();
        }

        internal int Add(InternalTransaction txNew)
        {
            Thread.BeginCriticalRegion();
            int num = 0;
            try
            {
                num = this.rwLock.AcquireReaderLock();
                try
                {
                    if ((txNew.AbsoluteTimeout != 0x7fffffffffffffffL) && !this.timerEnabled)
                    {
                        if (!this.timer.Change(this.timerInterval, this.timerInterval))
                        {
                            throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("UnexpectedTimerFailure"), null);
                        }
                        this.lastTimerTime = DateTime.UtcNow.Ticks;
                        this.timerEnabled = true;
                    }
                    txNew.CreationTime = this.CurrentTime;
                    this.AddIter(txNew);
                }
                finally
                {
                    this.rwLock.ReleaseReaderLock();
                }
            }
            finally
            {
                Thread.EndCriticalRegion();
            }
            return num;
        }

        private void AddIter(InternalTransaction txNew)
        {
            BucketSet headBucketSet = this.headBucketSet;
            while (headBucketSet.AbsoluteTimeout != txNew.AbsoluteTimeout)
            {
                BucketSet set3 = null;
                do
                {
                    WeakReference nextSetWeak = (WeakReference) headBucketSet.nextSetWeak;
                    BucketSet target = null;
                    if (nextSetWeak != null)
                    {
                        target = (BucketSet) nextSetWeak.Target;
                    }
                    if (target == null)
                    {
                        BucketSet set6 = new BucketSet(this, txNew.AbsoluteTimeout);
                        WeakReference reference5 = new WeakReference(set6);
                        WeakReference reference4 = (WeakReference) Interlocked.CompareExchange(ref headBucketSet.nextSetWeak, reference5, nextSetWeak);
                        if (reference4 == nextSetWeak)
                        {
                            set6.prevSet = headBucketSet;
                        }
                    }
                    else
                    {
                        set3 = headBucketSet;
                        headBucketSet = target;
                    }
                }
                while (headBucketSet.AbsoluteTimeout > txNew.AbsoluteTimeout);
                if (headBucketSet.AbsoluteTimeout != txNew.AbsoluteTimeout)
                {
                    BucketSet set2 = new BucketSet(this, txNew.AbsoluteTimeout);
                    WeakReference reference3 = new WeakReference(set2);
                    set2.nextSetWeak = set3.nextSetWeak;
                    WeakReference reference2 = (WeakReference) Interlocked.CompareExchange(ref set3.nextSetWeak, reference3, set2.nextSetWeak);
                    if (reference2 == set2.nextSetWeak)
                    {
                        if (reference2 != null)
                        {
                            BucketSet set5 = (BucketSet) reference2.Target;
                            if (set5 != null)
                            {
                                set5.prevSet = set2;
                            }
                        }
                        set2.prevSet = headBucketSet;
                    }
                    headBucketSet = set3;
                    set3 = null;
                }
            }
            headBucketSet.Add(txNew);
        }

        internal TimeSpan RecalcTimeout(InternalTransaction tx)
        {
            return TimeSpan.FromMilliseconds((double) ((tx.AbsoluteTimeout - this.ticks) * this.timerInterval));
        }

        internal void Remove(InternalTransaction tx)
        {
            tx.tableBucket.Remove(tx);
            tx.tableBucket = null;
        }

        private void ThreadTimer(object state)
        {
            if (!this.timerEnabled)
            {
                return;
            }
            this.ticks += 1L;
            this.lastTimerTime = DateTime.UtcNow.Ticks;
            BucketSet set4 = null;
            BucketSet headBucketSet = this.headBucketSet;
            WeakReference nextSetWeak = (WeakReference) headBucketSet.nextSetWeak;
            BucketSet target = null;
            if (nextSetWeak != null)
            {
                target = (BucketSet) nextSetWeak.Target;
            }
            if (target == null)
            {
                this.rwLock.AcquireWriterLock();
                try
                {
                    if (!this.timer.Change(-1, -1))
                    {
                        throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("UnexpectedTimerFailure"), null);
                    }
                    this.timerEnabled = false;
                    return;
                }
                finally
                {
                    this.rwLock.ReleaseWriterLock();
                }
            }
        Label_00A0:
            nextSetWeak = (WeakReference) headBucketSet.nextSetWeak;
            if (nextSetWeak != null)
            {
                target = (BucketSet) nextSetWeak.Target;
                if (target != null)
                {
                    set4 = headBucketSet;
                    headBucketSet = target;
                    if (headBucketSet.AbsoluteTimeout <= this.ticks)
                    {
                        Thread.BeginCriticalRegion();
                        try
                        {
                            WeakReference reference2 = (WeakReference) Interlocked.CompareExchange(ref set4.nextSetWeak, null, nextSetWeak);
                            if (reference2 == nextSetWeak)
                            {
                                BucketSet set = null;
                                do
                                {
                                    if (reference2 != null)
                                    {
                                        set = (BucketSet) reference2.Target;
                                    }
                                    else
                                    {
                                        set = null;
                                    }
                                    if (set != null)
                                    {
                                        set.TimeoutTransactions();
                                        reference2 = (WeakReference) set.nextSetWeak;
                                    }
                                }
                                while (set != null);
                                return;
                            }
                        }
                        finally
                        {
                            Thread.EndCriticalRegion();
                        }
                        headBucketSet = set4;
                    }
                    goto Label_00A0;
                }
            }
        }

        internal long TimeoutTicks(TimeSpan timeout)
        {
            if (timeout != TimeSpan.Zero)
            {
                return (((timeout.Ticks / 0x2710L) >> 9) + this.ticks);
            }
            return 0x7fffffffffffffffL;
        }

        private long CurrentTime
        {
            get
            {
                if (this.timerEnabled)
                {
                    return this.lastTimerTime;
                }
                return DateTime.UtcNow.Ticks;
            }
        }
    }
}


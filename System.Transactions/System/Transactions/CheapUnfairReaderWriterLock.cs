namespace System.Transactions
{
    using System;
    using System.Threading;

    internal class CheapUnfairReaderWriterLock
    {
        private const int MAX_SPIN_COUNT = 100;
        private int readersIn;
        private int readersOut;
        private const int SLEEP_TIME = 500;
        private object syncRoot;
        private object writerFinishedEvent;
        private bool writerPresent;

        public int AcquireReaderLock()
        {
            int num = 0;
        Label_0002:
            if (this.writerPresent)
            {
                this.WriterFinishedEvent.WaitOne();
            }
            num = Interlocked.Increment(ref this.readersIn);
            if (this.writerPresent)
            {
                Interlocked.Decrement(ref this.readersIn);
                goto Label_0002;
            }
            return num;
        }

        public void AcquireWriterLock()
        {
            Monitor.Enter(this.SyncRoot);
            this.writerPresent = true;
            this.WriterFinishedEvent.Reset();
            do
            {
                for (int i = 0; this.ReadersPresent && (i < 100); i++)
                {
                    Thread.Sleep(0);
                }
                if (this.ReadersPresent)
                {
                    Thread.Sleep(500);
                }
            }
            while (this.ReadersPresent);
        }

        public void ReleaseReaderLock()
        {
            Interlocked.Increment(ref this.readersOut);
        }

        public void ReleaseWriterLock()
        {
            try
            {
                this.writerPresent = false;
                this.WriterFinishedEvent.Set();
            }
            finally
            {
                Monitor.Exit(this.SyncRoot);
            }
        }

        private bool ReadersPresent
        {
            get
            {
                return (this.readersIn != this.readersOut);
            }
        }

        private object SyncRoot
        {
            get
            {
                if (this.syncRoot == null)
                {
                    Interlocked.CompareExchange(ref this.syncRoot, new object(), null);
                }
                return this.syncRoot;
            }
        }

        private ManualResetEvent WriterFinishedEvent
        {
            get
            {
                if (this.writerFinishedEvent == null)
                {
                    Interlocked.CompareExchange(ref this.writerFinishedEvent, new ManualResetEvent(true), null);
                }
                return (ManualResetEvent) this.writerFinishedEvent;
            }
        }
    }
}


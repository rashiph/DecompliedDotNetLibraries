namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Threading;

    internal class SyncCommunicationWaiter : ICommunicationWaiter, IDisposable
    {
        private bool closed;
        private object mutex;
        private CommunicationWaitResult result;
        private ManualResetEvent waitHandle;

        public SyncCommunicationWaiter(object mutex)
        {
            this.mutex = mutex;
            this.waitHandle = new ManualResetEvent(false);
        }

        public void Dispose()
        {
            lock (this.ThisLock)
            {
                if (!this.closed)
                {
                    this.closed = true;
                    this.waitHandle.Close();
                }
            }
        }

        public void Signal()
        {
            lock (this.ThisLock)
            {
                if (!this.closed)
                {
                    this.waitHandle.Set();
                }
            }
        }

        public CommunicationWaitResult Wait(TimeSpan timeout, bool aborting)
        {
            if (this.closed)
            {
                return CommunicationWaitResult.Aborted;
            }
            if (timeout < TimeSpan.Zero)
            {
                return CommunicationWaitResult.Expired;
            }
            if (aborting)
            {
                this.result = CommunicationWaitResult.Aborted;
            }
            bool flag = !TimeoutHelper.WaitOne(this.waitHandle, timeout);
            lock (this.ThisLock)
            {
                if (this.result == CommunicationWaitResult.Waiting)
                {
                    this.result = flag ? CommunicationWaitResult.Expired : CommunicationWaitResult.Succeeded;
                }
            }
            lock (this.ThisLock)
            {
                if (!this.closed)
                {
                    this.waitHandle.Set();
                }
            }
            return this.result;
        }

        private object ThisLock
        {
            get
            {
                return this.mutex;
            }
        }
    }
}


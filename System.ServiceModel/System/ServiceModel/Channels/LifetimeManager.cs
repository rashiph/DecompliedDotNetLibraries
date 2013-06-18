namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    internal class LifetimeManager
    {
        private bool aborted;
        private int busyCount;
        private ICommunicationWaiter busyWaiter;
        private int busyWaiterCount;
        private object mutex;
        private LifetimeState state;

        public LifetimeManager(object mutex)
        {
            this.mutex = mutex;
            this.state = LifetimeState.Opened;
        }

        public void Abort()
        {
            lock (this.ThisLock)
            {
                if ((this.State == LifetimeState.Closed) || this.aborted)
                {
                    return;
                }
                this.aborted = true;
                this.state = LifetimeState.Closing;
            }
            this.OnAbort();
            this.state = LifetimeState.Closed;
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            lock (this.ThisLock)
            {
                this.ThrowIfNotOpened();
                this.state = LifetimeState.Closing;
            }
            return this.OnBeginClose(timeout, callback, state);
        }

        public void Close(TimeSpan timeout)
        {
            lock (this.ThisLock)
            {
                this.ThrowIfNotOpened();
                this.state = LifetimeState.Closing;
            }
            this.OnClose(timeout);
            this.state = LifetimeState.Closed;
        }

        private CommunicationWaitResult CloseCore(TimeSpan timeout, bool aborting)
        {
            ICommunicationWaiter busyWaiter = null;
            CommunicationWaitResult succeeded = CommunicationWaitResult.Succeeded;
            lock (this.ThisLock)
            {
                if (this.busyCount > 0)
                {
                    if (this.busyWaiter != null)
                    {
                        if (!aborting && this.aborted)
                        {
                            return CommunicationWaitResult.Aborted;
                        }
                        busyWaiter = this.busyWaiter;
                    }
                    else
                    {
                        busyWaiter = new SyncCommunicationWaiter(this.ThisLock);
                        this.busyWaiter = busyWaiter;
                    }
                    Interlocked.Increment(ref this.busyWaiterCount);
                }
            }
            if (busyWaiter != null)
            {
                succeeded = busyWaiter.Wait(timeout, aborting);
                if (Interlocked.Decrement(ref this.busyWaiterCount) == 0)
                {
                    busyWaiter.Dispose();
                    this.busyWaiter = null;
                }
            }
            return succeeded;
        }

        protected void DecrementBusyCount()
        {
            ICommunicationWaiter busyWaiter = null;
            bool flag = false;
            lock (this.ThisLock)
            {
                if (this.busyCount <= 0)
                {
                    throw Fx.AssertAndThrow("LifetimeManager.DecrementBusyCount: (this.busyCount > 0)");
                }
                if (--this.busyCount == 0)
                {
                    if (this.busyWaiter != null)
                    {
                        busyWaiter = this.busyWaiter;
                        Interlocked.Increment(ref this.busyWaiterCount);
                    }
                    flag = true;
                }
            }
            if (busyWaiter != null)
            {
                busyWaiter.Signal();
                if (Interlocked.Decrement(ref this.busyWaiterCount) == 0)
                {
                    busyWaiter.Dispose();
                    this.busyWaiter = null;
                }
            }
            if (flag && (this.State == LifetimeState.Opened))
            {
                this.OnEmpty();
            }
        }

        public void EndClose(IAsyncResult result)
        {
            this.OnEndClose(result);
            this.state = LifetimeState.Closed;
        }

        protected virtual void IncrementBusyCount()
        {
            lock (this.ThisLock)
            {
                this.busyCount++;
            }
        }

        protected virtual void IncrementBusyCountWithoutLock()
        {
            this.busyCount++;
        }

        protected virtual void OnAbort()
        {
            this.CloseCore(TimeSpan.FromSeconds(1.0), true);
        }

        protected virtual IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            CloseCommunicationAsyncResult result = null;
            lock (this.ThisLock)
            {
                if (this.busyCount > 0)
                {
                    if (this.busyWaiter != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().ToString()));
                    }
                    result = new CloseCommunicationAsyncResult(timeout, callback, state, this.ThisLock);
                    this.busyWaiter = result;
                    Interlocked.Increment(ref this.busyWaiterCount);
                }
            }
            if (result != null)
            {
                return result;
            }
            return new CompletedAsyncResult(callback, state);
        }

        protected virtual void OnClose(TimeSpan timeout)
        {
            switch (this.CloseCore(timeout, false))
            {
                case CommunicationWaitResult.Expired:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("SFxCloseTimedOut1", new object[] { timeout })));

                case CommunicationWaitResult.Aborted:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().ToString()));
            }
        }

        protected virtual void OnEmpty()
        {
        }

        protected virtual void OnEndClose(IAsyncResult result)
        {
            if (result is CloseCommunicationAsyncResult)
            {
                CloseCommunicationAsyncResult.End(result);
                if (Interlocked.Decrement(ref this.busyWaiterCount) == 0)
                {
                    this.busyWaiter.Dispose();
                    this.busyWaiter = null;
                }
            }
            else
            {
                CompletedAsyncResult.End(result);
            }
        }

        private void ThrowIfNotOpened()
        {
            if (!this.aborted && (this.state != LifetimeState.Opened))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().ToString()));
            }
        }

        public int BusyCount
        {
            get
            {
                return this.busyCount;
            }
        }

        protected LifetimeState State
        {
            get
            {
                return this.state;
            }
        }

        protected object ThisLock
        {
            get
            {
                return this.mutex;
            }
        }
    }
}


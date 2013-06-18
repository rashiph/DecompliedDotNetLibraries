namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    internal sealed class Guard
    {
        private bool closed;
        private ManualResetEvent closeEvent;
        private int currentCount;
        private int maxCount;
        private object thisLock;

        private event WaitAsyncResult.SignaledHandler Signaled;

        public Guard() : this(1)
        {
        }

        public Guard(int maxCount)
        {
            this.thisLock = new object();
            this.maxCount = maxCount;
        }

        public void Abort()
        {
            this.closed = true;
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool flag = false;
            WaitAsyncResult result = null;
            lock (this.thisLock)
            {
                if (this.closed || (this.currentCount == 0))
                {
                    flag = true;
                }
                else
                {
                    result = new WaitAsyncResult(timeout, true, callback, state);
                    this.Signaled += new WaitAsyncResult.SignaledHandler(result.OnSignaled);
                }
                this.closed = true;
            }
            if (flag)
            {
                return new CompletedAsyncResult(callback, state);
            }
            result.Begin();
            return result;
        }

        public void Close(TimeSpan timeout)
        {
            lock (this.thisLock)
            {
                if (this.closed)
                {
                    return;
                }
                this.closed = true;
                if (this.currentCount > 0)
                {
                    this.closeEvent = new ManualResetEvent(false);
                }
            }
            if (this.closeEvent != null)
            {
                try
                {
                    if (!TimeoutHelper.WaitOne(this.closeEvent, timeout))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("TimeoutOnOperation", new object[] { timeout })));
                    }
                }
                finally
                {
                    lock (this.thisLock)
                    {
                        this.closeEvent.Close();
                        this.closeEvent = null;
                    }
                }
            }
        }

        public void EndClose(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                WaitAsyncResult.End(result);
            }
        }

        public bool Enter()
        {
            lock (this.thisLock)
            {
                if (this.closed)
                {
                    return false;
                }
                if (this.currentCount == this.maxCount)
                {
                    return false;
                }
                this.currentCount++;
                return true;
            }
        }

        public void Exit()
        {
            WaitAsyncResult.SignaledHandler signaled = null;
            lock (this.thisLock)
            {
                this.currentCount--;
                if (this.currentCount < 0)
                {
                    throw Fx.AssertAndThrow("Exit can only be called after Enter.");
                }
                if (this.currentCount == 0)
                {
                    if (this.closeEvent != null)
                    {
                        this.closeEvent.Set();
                    }
                    signaled = this.Signaled;
                }
            }
            if (signaled != null)
            {
                signaled();
            }
        }
    }
}


namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal class CloseCommunicationAsyncResult : AsyncResult, ICommunicationWaiter, IDisposable
    {
        private object mutex;
        private CommunicationWaitResult result;
        private TimeSpan timeout;
        private TimeoutHelper timeoutHelper;
        private IOThreadTimer timer;

        public CloseCommunicationAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, object mutex) : base(callback, state)
        {
            this.timeout = timeout;
            this.timeoutHelper = new TimeoutHelper(timeout);
            this.mutex = mutex;
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("SFxCloseTimedOut1", new object[] { timeout })));
            }
            this.timer = new IOThreadTimer(new Action<object>(CloseCommunicationAsyncResult.TimeoutCallback), this, true);
            this.timer.Set(timeout);
        }

        public void Dispose()
        {
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<CloseCommunicationAsyncResult>(result);
        }

        public void Signal()
        {
            lock (this.ThisLock)
            {
                if (this.result != CommunicationWaitResult.Waiting)
                {
                    return;
                }
                this.result = CommunicationWaitResult.Succeeded;
            }
            this.timer.Cancel();
            base.Complete(false);
        }

        private void Timeout()
        {
            lock (this.ThisLock)
            {
                if (this.result != CommunicationWaitResult.Waiting)
                {
                    return;
                }
                this.result = CommunicationWaitResult.Expired;
            }
            base.Complete(false, new TimeoutException(System.ServiceModel.SR.GetString("SFxCloseTimedOut1", new object[] { this.timeout })));
        }

        private static void TimeoutCallback(object state)
        {
            ((CloseCommunicationAsyncResult) state).Timeout();
        }

        public CommunicationWaitResult Wait(TimeSpan timeout, bool aborting)
        {
            if (timeout < TimeSpan.Zero)
            {
                return CommunicationWaitResult.Expired;
            }
            lock (this.ThisLock)
            {
                if (this.result != CommunicationWaitResult.Waiting)
                {
                    return this.result;
                }
                this.result = CommunicationWaitResult.Aborted;
            }
            this.timer.Cancel();
            TimeoutHelper.WaitOne(base.AsyncWaitHandle, timeout);
            base.Complete(false, new ObjectDisposedException(base.GetType().ToString()));
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


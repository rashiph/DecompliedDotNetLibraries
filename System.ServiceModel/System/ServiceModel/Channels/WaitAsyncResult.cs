namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    internal class WaitAsyncResult : AsyncResult
    {
        private bool completed;
        private object thisLock;
        private bool throwTimeoutException;
        private bool timedOut;
        private TimeSpan timeout;
        private IOThreadTimer timer;

        public WaitAsyncResult(TimeSpan timeout, bool throwTimeoutException, AsyncCallback callback, object state) : base(callback, state)
        {
            this.thisLock = new object();
            this.timeout = timeout;
            this.throwTimeoutException = throwTimeoutException;
        }

        public void Begin()
        {
            lock (this.thisLock)
            {
                if (!this.completed && (this.timeout != TimeSpan.MaxValue))
                {
                    this.timer = new IOThreadTimer(new Action<object>(this.OnTimerElapsed), null, true);
                    this.timer.Set(this.timeout);
                }
            }
        }

        public static bool End(IAsyncResult result)
        {
            return !AsyncResult.End<WaitAsyncResult>(result).timedOut;
        }

        protected virtual string GetTimeoutString(TimeSpan timeout)
        {
            return System.ServiceModel.SR.GetString("TimeoutOnOperation", new object[] { timeout });
        }

        public void OnAborted(CommunicationObject communicationObject)
        {
            if (this.ShouldComplete(false))
            {
                base.Complete(false, communicationObject.CreateClosedException());
            }
        }

        public void OnFaulted(CommunicationObject communicationObject)
        {
            if (this.ShouldComplete(false))
            {
                base.Complete(false, communicationObject.GetTerminalException());
            }
        }

        public void OnSignaled()
        {
            if (this.ShouldComplete(false))
            {
                base.Complete(false);
            }
        }

        protected virtual void OnTimerElapsed(object state)
        {
            if (this.ShouldComplete(true))
            {
                if (this.throwTimeoutException)
                {
                    base.Complete(false, new TimeoutException(this.GetTimeoutString(this.timeout)));
                }
                else
                {
                    base.Complete(false);
                }
            }
        }

        private bool ShouldComplete(bool timedOut)
        {
            lock (this.thisLock)
            {
                if (!this.completed)
                {
                    this.completed = true;
                    this.timedOut = timedOut;
                    if (!timedOut && (this.timer != null))
                    {
                        this.timer.Cancel();
                    }
                    return true;
                }
            }
            return false;
        }

        public delegate void AbortHandler(CommunicationObject communicationObject);

        public delegate void SignaledHandler();
    }
}


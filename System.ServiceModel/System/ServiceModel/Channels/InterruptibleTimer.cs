namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Threading;

    internal class InterruptibleTimer
    {
        private bool aborted;
        private WaitCallback callback;
        private TimeSpan defaultInterval;
        private static Action<object> onTimerElapsed = new Action<object>(InterruptibleTimer.OnTimerElapsed);
        private bool set;
        private object state;
        private object thisLock = new object();
        private IOThreadTimer timer;

        public InterruptibleTimer(TimeSpan defaultInterval, WaitCallback callback, object state)
        {
            if (callback == null)
            {
                throw Fx.AssertAndThrow("Argument callback cannot be null.");
            }
            this.defaultInterval = defaultInterval;
            this.callback = callback;
            this.state = state;
        }

        public void Abort()
        {
            lock (this.ThisLock)
            {
                this.aborted = true;
                if (this.set)
                {
                    this.timer.Cancel();
                    this.set = false;
                }
            }
        }

        public bool Cancel()
        {
            lock (this.ThisLock)
            {
                if (!this.aborted && this.set)
                {
                    this.timer.Cancel();
                    this.set = false;
                    return true;
                }
                return false;
            }
        }

        private void InternalSet(TimeSpan interval, bool ifNotSet)
        {
            lock (this.ThisLock)
            {
                if (!this.aborted && (!ifNotSet || !this.set))
                {
                    if (this.timer == null)
                    {
                        this.timer = new IOThreadTimer(onTimerElapsed, this, true);
                    }
                    this.timer.Set(interval);
                    this.set = true;
                }
            }
        }

        private void OnTimerElapsed()
        {
            lock (this.ThisLock)
            {
                if (this.aborted)
                {
                    return;
                }
                this.set = false;
            }
            this.callback(this.state);
        }

        private static void OnTimerElapsed(object state)
        {
            ((InterruptibleTimer) state).OnTimerElapsed();
        }

        public void Set()
        {
            this.Set(this.defaultInterval);
        }

        public void Set(TimeSpan interval)
        {
            this.InternalSet(interval, false);
        }

        public void SetIfNotSet()
        {
            this.InternalSet(this.defaultInterval, true);
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }
    }
}


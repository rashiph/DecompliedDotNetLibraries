namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Threading;

    internal class SmartTimer : IDisposable
    {
        private TimerCallback callback;
        private TimeSpan infinite = new TimeSpan(-1L);
        private object locker = new object();
        private TimeSpan minUpdate = new TimeSpan(0, 0, 5);
        private DateTime next;
        private bool nextChanged;
        private TimeSpan period;
        private Timer timer;

        public SmartTimer(TimerCallback callback, object state, TimeSpan due, TimeSpan period)
        {
            this.period = period;
            this.callback = callback;
            this.next = DateTime.UtcNow + due;
            this.timer = new Timer(new TimerCallback(this.HandleCallback), state, due, this.infinite);
        }

        public void Dispose()
        {
            lock (this.locker)
            {
                if (this.timer != null)
                {
                    this.timer.Dispose();
                    this.timer = null;
                }
            }
        }

        private void HandleCallback(object state)
        {
            try
            {
                this.callback(state);
            }
            finally
            {
                lock (this.locker)
                {
                    if (this.timer != null)
                    {
                        if (!this.nextChanged)
                        {
                            this.next = DateTime.UtcNow + this.period;
                        }
                        else
                        {
                            this.nextChanged = false;
                        }
                        TimeSpan dueTime = (TimeSpan) (this.next - DateTime.UtcNow);
                        if (dueTime < TimeSpan.Zero)
                        {
                            dueTime = TimeSpan.Zero;
                        }
                        this.timer.Change(dueTime, this.infinite);
                    }
                }
            }
        }

        public void Update(DateTime newNext)
        {
            if ((newNext < this.next) && ((this.next - DateTime.UtcNow) > this.minUpdate))
            {
                lock (this.locker)
                {
                    if (((newNext < this.next) && ((this.next - DateTime.UtcNow) > this.minUpdate)) && (this.timer != null))
                    {
                        this.next = newNext;
                        this.nextChanged = true;
                        TimeSpan dueTime = (TimeSpan) (this.next - DateTime.UtcNow);
                        if (dueTime < TimeSpan.Zero)
                        {
                            dueTime = TimeSpan.Zero;
                        }
                        this.timer.Change(dueTime, this.infinite);
                    }
                }
            }
        }
    }
}


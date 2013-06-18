namespace System.Web
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Web.Hosting;

    internal class IdleTimeoutMonitor
    {
        private TimeSpan _idleTimeout;
        private DateTime _lastEvent;
        private Timer _timer;
        private readonly TimeSpan _timerPeriod = new TimeSpan(0, 0, 30);

        internal IdleTimeoutMonitor(TimeSpan timeout)
        {
            this._idleTimeout = timeout;
            this._timer = new Timer(new TimerCallback(this.TimerCompletionCallback), null, this._timerPeriod, this._timerPeriod);
            this._lastEvent = DateTime.UtcNow;
        }

        internal void Stop()
        {
            if (this._timer != null)
            {
                lock (this)
                {
                    if (this._timer != null)
                    {
                        this._timer.Dispose();
                        this._timer = null;
                    }
                }
            }
        }

        private void TimerCompletionCallback(object state)
        {
            HttpApplicationFactory.TrimApplicationInstances();
            if (((((this._idleTimeout != TimeSpan.MaxValue) && !HostingEnvironment.ShutdownInitiated) && (HostingEnvironment.BusyCount == 0)) && (DateTime.UtcNow > this.LastEvent.Add(this._idleTimeout))) && !Debugger.IsAttached)
            {
                HttpRuntime.SetShutdownReason(ApplicationShutdownReason.IdleTimeout, System.Web.SR.GetString("Hosting_Env_IdleTimeout"));
                HostingEnvironment.InitiateShutdownWithoutDemand();
            }
        }

        internal DateTime LastEvent
        {
            get
            {
                lock (this)
                {
                    return this._lastEvent;
                }
            }
            set
            {
                lock (this)
                {
                    this._lastEvent = value;
                }
            }
        }
    }
}


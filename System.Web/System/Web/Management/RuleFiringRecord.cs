namespace System.Web.Management
{
    using System;
    using System.Threading;
    using System.Web.Configuration;

    public sealed class RuleFiringRecord
    {
        internal DateTime _lastFired;
        internal HealthMonitoringSectionHelper.RuleInfo _ruleInfo;
        internal int _timesRaised;
        internal int _updatingLastFired;
        private static TimeSpan TS_ONE_SECOND = new TimeSpan(0, 0, 1);

        internal RuleFiringRecord(HealthMonitoringSectionHelper.RuleInfo ruleInfo)
        {
            this._ruleInfo = ruleInfo;
            this._lastFired = DateTime.MinValue;
            this._timesRaised = 0;
            this._updatingLastFired = 0;
        }

        internal bool CheckAndUpdate(WebBaseEvent eventRaised)
        {
            DateTime now = DateTime.Now;
            HealthMonitoringManager manager = HealthMonitoringManager.Manager();
            int num = Interlocked.Increment(ref this._timesRaised);
            if (manager == null)
            {
                return false;
            }
            if (this._ruleInfo._customEvaluatorType != null)
            {
                IWebEventCustomEvaluator evaluator = (IWebEventCustomEvaluator) manager._sectionHelper._customEvaluatorInstances[this._ruleInfo._customEvaluatorType];
                try
                {
                    eventRaised.PreProcessEventInit();
                    if (!evaluator.CanFire(eventRaised, this))
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
            if (num < this._ruleInfo._minInstances)
            {
                return false;
            }
            if (num > this._ruleInfo._maxLimit)
            {
                return false;
            }
            if (this._ruleInfo._minInterval == TimeSpan.Zero)
            {
                this.UpdateLastFired(now, false);
                return true;
            }
            if ((now - this._lastFired) <= this._ruleInfo._minInterval)
            {
                return false;
            }
            lock (this)
            {
                if ((now - this._lastFired) <= this._ruleInfo._minInterval)
                {
                    return false;
                }
                this.UpdateLastFired(now, true);
                return true;
            }
        }

        private void UpdateLastFired(DateTime now, bool alreadyLocked)
        {
            TimeSpan span = (TimeSpan) (now - this._lastFired);
            if (span >= TS_ONE_SECOND)
            {
                if (!alreadyLocked)
                {
                    if (Interlocked.CompareExchange(ref this._updatingLastFired, 1, 0) == 0)
                    {
                        try
                        {
                            this._lastFired = now;
                        }
                        finally
                        {
                            Interlocked.Exchange(ref this._updatingLastFired, 0);
                        }
                    }
                }
                else
                {
                    this._lastFired = now;
                }
            }
        }

        public DateTime LastFired
        {
            get
            {
                return this._lastFired;
            }
        }

        public int TimesRaised
        {
            get
            {
                return this._timesRaised;
            }
        }
    }
}


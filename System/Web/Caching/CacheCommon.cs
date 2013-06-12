namespace System.Web.Caching
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Web.Configuration;

    internal class CacheCommon
    {
        internal CacheInternal _cacheInternal;
        protected internal CacheMemoryStats _cacheMemoryStats;
        internal Cache _cachePublic = new Cache(0);
        private int _currentPollInterval = 0x7530;
        internal bool _enableExpiration;
        internal bool _enableMemoryCollection;
        internal int _inCacheManagerThread;
        internal bool _internalConfigRead;
        private Timer _timer;
        private object _timerLock = new object();
        private const int MEMORYSTATUS_INTERVAL_30_SECONDS = 0x7530;
        private const int MEMORYSTATUS_INTERVAL_5_SECONDS = 0x1388;

        internal CacheCommon()
        {
            this._cacheMemoryStats = new CacheMemoryStats(new SRef(this));
            this._enableMemoryCollection = true;
            this._enableExpiration = true;
        }

        private void AdjustTimer()
        {
            lock (this._timerLock)
            {
                if (this._timer != null)
                {
                    if (this._cacheMemoryStats.IsAboveHighPressure())
                    {
                        if (this._currentPollInterval > 0x1388)
                        {
                            this._currentPollInterval = 0x1388;
                            this._timer.Change(this._currentPollInterval, this._currentPollInterval);
                        }
                    }
                    else if ((this._cacheMemoryStats.CacheSizePressure.PressureLast > (this._cacheMemoryStats.CacheSizePressure.PressureLow / 2)) || (this._cacheMemoryStats.TotalMemoryPressure.PressureLast > (this._cacheMemoryStats.TotalMemoryPressure.PressureLow / 2)))
                    {
                        int num = Math.Min(CacheMemorySizePressure.PollInterval, 0x7530);
                        if (this._currentPollInterval != num)
                        {
                            this._currentPollInterval = num;
                            this._timer.Change(this._currentPollInterval, this._currentPollInterval);
                        }
                    }
                    else if (this._currentPollInterval != CacheMemorySizePressure.PollInterval)
                    {
                        this._currentPollInterval = CacheMemorySizePressure.PollInterval;
                        this._timer.Change(this._currentPollInterval, this._currentPollInterval);
                    }
                }
            }
        }

        internal long CacheManagerThread(int minPercent)
        {
            long num4;
            if (Interlocked.Exchange(ref this._inCacheManagerThread, 1) != 0)
            {
                return 0L;
            }
            try
            {
                if (this._timer == null)
                {
                    return 0L;
                }
                this._cacheMemoryStats.Update();
                this.AdjustTimer();
                int percent = Math.Max(minPercent, this._cacheMemoryStats.GetPercentToTrim());
                long totalCount = this._cacheInternal.TotalCount;
                Stopwatch stopwatch = Stopwatch.StartNew();
                long trimCount = this._cacheInternal.TrimIfNecessary(percent);
                stopwatch.Stop();
                if ((percent > 0) && (trimCount > 0L))
                {
                    this._cacheMemoryStats.SetTrimStats(stopwatch.Elapsed.Ticks, totalCount, trimCount);
                }
                num4 = trimCount;
            }
            finally
            {
                Interlocked.Exchange(ref this._inCacheManagerThread, 0);
            }
            return num4;
        }

        private void CacheManagerTimerCallback(object state)
        {
            this.CacheManagerThread(0);
        }

        internal void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.EnableCacheMemoryTimer(false);
                this._cacheMemoryStats.Dispose();
            }
        }

        internal void EnableCacheMemoryTimer(bool enable)
        {
            lock (this._timerLock)
            {
                if (enable)
                {
                    if (this._timer == null)
                    {
                        this._timer = new Timer(new TimerCallback(this.CacheManagerTimerCallback), null, this._currentPollInterval, this._currentPollInterval);
                    }
                    else
                    {
                        this._timer.Change(this._currentPollInterval, this._currentPollInterval);
                    }
                }
                else
                {
                    Timer comparand = this._timer;
                    if ((comparand != null) && (Interlocked.CompareExchange<Timer>(ref this._timer, null, comparand) == comparand))
                    {
                        comparand.Dispose();
                    }
                }
            }
            if (!enable)
            {
                while (this._inCacheManagerThread != 0)
                {
                    Thread.Sleep(100);
                }
            }
        }

        internal void ReadCacheInternalConfig(CacheSection cacheSection)
        {
            if (!this._internalConfigRead)
            {
                lock (this)
                {
                    if (!this._internalConfigRead)
                    {
                        this._internalConfigRead = true;
                        if (cacheSection != null)
                        {
                            this._enableMemoryCollection = !cacheSection.DisableMemoryCollection;
                            this._enableExpiration = !cacheSection.DisableExpiration;
                            this._cacheMemoryStats.ReadConfig(cacheSection);
                            this._currentPollInterval = CacheMemorySizePressure.PollInterval;
                            this.ResetFromConfigSettings();
                        }
                    }
                }
            }
        }

        internal void ResetFromConfigSettings()
        {
            this.EnableCacheMemoryTimer(this._enableMemoryCollection);
            this._cacheInternal.EnableExpirationTimer(this._enableExpiration);
        }

        internal void SetCacheInternal(CacheInternal cacheInternal)
        {
            this._cacheInternal = cacheInternal;
            this._cachePublic.SetCacheInternal(cacheInternal);
        }
    }
}


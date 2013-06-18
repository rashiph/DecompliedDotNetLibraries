namespace System.Runtime.Caching
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics;
    using System.Runtime.Caching.Configuration;
    using System.Security;
    using System.Threading;

    internal sealed class MemoryCacheStatistics : IDisposable
    {
        private CacheMemoryMonitor _cacheMemoryMonitor;
        private int _configCacheMemoryLimitMegabytes;
        private int _configPhysicalMemoryLimitPercentage;
        private int _configPollingInterval;
        private int _disposed;
        private int _inCacheManagerThread;
        private long _lastTrimCount;
        private long _lastTrimDurationTicks;
        private int _lastTrimGen2Count;
        private int _lastTrimPercent;
        private DateTime _lastTrimTime;
        private MemoryCache _memoryCache;
        private PhysicalMemoryMonitor _physicalMemoryMonitor;
        private int _pollingInterval;
        private Timer _timer;
        private object _timerLock;
        private long _totalCountBeforeTrim;
        private const int MEMORYSTATUS_INTERVAL_30_SECONDS = 0x7530;
        private const int MEMORYSTATUS_INTERVAL_5_SECONDS = 0x1388;

        private MemoryCacheStatistics()
        {
        }

        internal MemoryCacheStatistics(MemoryCache memoryCache, NameValueCollection config)
        {
            this._memoryCache = memoryCache;
            this._lastTrimGen2Count = -1;
            this._lastTrimTime = DateTime.MinValue;
            this._timerLock = new object();
            this.InitializeConfiguration(config);
            this._pollingInterval = this._configPollingInterval;
            this._physicalMemoryMonitor = new PhysicalMemoryMonitor(this._configPhysicalMemoryLimitPercentage);
            this.InitDisposableMembers();
        }

        private void AdjustTimer()
        {
            lock (this._timerLock)
            {
                if (this._timer != null)
                {
                    if (this._physicalMemoryMonitor.IsAboveHighPressure() || this._cacheMemoryMonitor.IsAboveHighPressure())
                    {
                        if (this._pollingInterval > 0x1388)
                        {
                            this._pollingInterval = 0x1388;
                            this._timer.Change(this._pollingInterval, this._pollingInterval);
                        }
                    }
                    else if ((this._cacheMemoryMonitor.PressureLast > (this._cacheMemoryMonitor.PressureLow / 2)) || (this._physicalMemoryMonitor.PressureLast > (this._physicalMemoryMonitor.PressureLow / 2)))
                    {
                        int num = Math.Min(this._configPollingInterval, 0x7530);
                        if (this._pollingInterval != num)
                        {
                            this._pollingInterval = num;
                            this._timer.Change(this._pollingInterval, this._pollingInterval);
                        }
                    }
                    else if (this._pollingInterval != this._configPollingInterval)
                    {
                        this._pollingInterval = this._configPollingInterval;
                        this._timer.Change(this._pollingInterval, this._pollingInterval);
                    }
                }
            }
        }

        [SecuritySafeCritical]
        internal long CacheManagerThread(int minPercent)
        {
            long num4;
            if (Interlocked.Exchange(ref this._inCacheManagerThread, 1) != 0)
            {
                return 0L;
            }
            try
            {
                if (this._disposed == 1)
                {
                    return 0L;
                }
                this.Update();
                this.AdjustTimer();
                int percent = Math.Max(minPercent, this.GetPercentToTrim());
                long count = this._memoryCache.GetCount(null);
                Stopwatch stopwatch = Stopwatch.StartNew();
                long trimCount = this._memoryCache.Trim(percent);
                stopwatch.Stop();
                if ((percent > 0) && (trimCount > 0L))
                {
                    this.SetTrimStats(stopwatch.Elapsed.Ticks, count, trimCount);
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

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this._disposed, 1) != 0)
            {
                return;
            }
            lock (this._timerLock)
            {
                Timer comparand = this._timer;
                if ((comparand != null) && (Interlocked.CompareExchange<Timer>(ref this._timer, null, comparand) == comparand))
                {
                    comparand.Dispose();
                }
                goto Label_0052;
            }
        Label_004B:
            Thread.Sleep(100);
        Label_0052:
            if (this._inCacheManagerThread != 0)
            {
                goto Label_004B;
            }
            if (this._cacheMemoryMonitor != null)
            {
                this._cacheMemoryMonitor.Dispose();
            }
        }

        private int GetPercentToTrim()
        {
            if (GC.CollectionCount(2) != this._lastTrimGen2Count)
            {
                return Math.Max(this._physicalMemoryMonitor.GetPercentToTrim(this._lastTrimTime, this._lastTrimPercent), this._cacheMemoryMonitor.GetPercentToTrim(this._lastTrimTime, this._lastTrimPercent));
            }
            return 0;
        }

        private void InitDisposableMembers()
        {
            bool flag = true;
            try
            {
                this._cacheMemoryMonitor = new CacheMemoryMonitor(this._memoryCache, this._configCacheMemoryLimitMegabytes);
                this._timer = new Timer(new TimerCallback(this.CacheManagerTimerCallback), null, this._configPollingInterval, this._configPollingInterval);
                flag = false;
            }
            finally
            {
                if (flag)
                {
                    this.Dispose();
                }
            }
        }

        private void InitializeConfiguration(NameValueCollection config)
        {
            MemoryCacheElement element = null;
            if (!this._memoryCache.ConfigLess)
            {
                MemoryCacheSection section = ConfigurationManager.GetSection("system.runtime.caching/memoryCache") as MemoryCacheSection;
                if (section != null)
                {
                    element = section.NamedCaches[this._memoryCache.Name];
                }
            }
            if (element != null)
            {
                this._configCacheMemoryLimitMegabytes = element.CacheMemoryLimitMegabytes;
                this._configPhysicalMemoryLimitPercentage = element.PhysicalMemoryLimitPercentage;
                double totalMilliseconds = element.PollingInterval.TotalMilliseconds;
                this._configPollingInterval = (totalMilliseconds < 2147483647.0) ? ((int) totalMilliseconds) : 0x7fffffff;
            }
            else
            {
                this._configPollingInterval = 0x1d4c0;
                this._configCacheMemoryLimitMegabytes = 0;
                this._configPhysicalMemoryLimitPercentage = 0;
            }
            if (config != null)
            {
                this._configPollingInterval = ConfigUtil.GetIntValueFromTimeSpan(config, "pollingInterval", this._configPollingInterval);
                this._configCacheMemoryLimitMegabytes = ConfigUtil.GetIntValue(config, "cacheMemoryLimitMegabytes", this._configCacheMemoryLimitMegabytes, true, 0x7fffffff);
                this._configPhysicalMemoryLimitPercentage = ConfigUtil.GetIntValue(config, "physicalMemoryLimitPercentage", this._configPhysicalMemoryLimitPercentage, true, 100);
            }
        }

        private void SetTrimStats(long trimDurationTicks, long totalCountBeforeTrim, long trimCount)
        {
            this._lastTrimDurationTicks = trimDurationTicks;
            int num = GC.CollectionCount(2);
            if (num != this._lastTrimGen2Count)
            {
                this._lastTrimTime = DateTime.UtcNow;
                this._totalCountBeforeTrim = totalCountBeforeTrim;
                this._lastTrimCount = trimCount;
            }
            else
            {
                this._lastTrimCount += trimCount;
            }
            this._lastTrimGen2Count = num;
            this._lastTrimPercent = (int) ((this._lastTrimCount * 100L) / this._totalCountBeforeTrim);
        }

        private void Update()
        {
            this._physicalMemoryMonitor.Update();
            this._cacheMemoryMonitor.Update();
        }

        internal void UpdateConfig(NameValueCollection config)
        {
            int num = ConfigUtil.GetIntValueFromTimeSpan(config, "pollingInterval", this._configPollingInterval);
            int cacheMemoryLimitMegabytes = ConfigUtil.GetIntValue(config, "cacheMemoryLimitMegabytes", this._configCacheMemoryLimitMegabytes, true, 0x7fffffff);
            int physicalMemoryLimitPercentage = ConfigUtil.GetIntValue(config, "physicalMemoryLimitPercentage", this._configPhysicalMemoryLimitPercentage, true, 100);
            if (num != this._configPollingInterval)
            {
                lock (this._timerLock)
                {
                    this._configPollingInterval = num;
                }
            }
            if ((cacheMemoryLimitMegabytes != this._configCacheMemoryLimitMegabytes) || (physicalMemoryLimitPercentage != this._configPhysicalMemoryLimitPercentage))
            {
                try
                {
                    try
                    {
                    }
                    finally
                    {
                        while (Interlocked.Exchange(ref this._inCacheManagerThread, 1) != 0)
                        {
                            Thread.Sleep(100);
                        }
                    }
                    if (this._disposed == 0)
                    {
                        if (cacheMemoryLimitMegabytes != this._configCacheMemoryLimitMegabytes)
                        {
                            this._cacheMemoryMonitor.SetLimit(cacheMemoryLimitMegabytes);
                            this._configCacheMemoryLimitMegabytes = cacheMemoryLimitMegabytes;
                        }
                        if (physicalMemoryLimitPercentage != this._configPhysicalMemoryLimitPercentage)
                        {
                            this._physicalMemoryMonitor.SetLimit(physicalMemoryLimitPercentage);
                            this._configPhysicalMemoryLimitPercentage = physicalMemoryLimitPercentage;
                        }
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref this._inCacheManagerThread, 0);
                }
            }
        }

        internal long CacheMemoryLimit
        {
            get
            {
                return this._cacheMemoryMonitor.MemoryLimit;
            }
        }

        internal long PhysicalMemoryLimit
        {
            get
            {
                return this._physicalMemoryMonitor.MemoryLimit;
            }
        }

        internal TimeSpan PollingInterval
        {
            get
            {
                return TimeSpan.FromMilliseconds((double) this._configPollingInterval);
            }
        }
    }
}


namespace System.Web.Hosting
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Web;
    using System.Web.Util;

    internal class CacheManager : IDisposable
    {
        private ApplicationManager _appManager;
        private int _currentPollInterval;
        private long[] _deltaSamples;
        private bool _disposed;
        private long _highPressureMark;
        private int _idx;
        private int _idxDeltaSamples;
        private int _inducedGCCount;
        private long _inducedGCDurationTicks;
        private DateTime _inducedGCFinishTime;
        private long _inducedGCMinInterval;
        private long _inducedGCPostPrivateBytes;
        private long _inducedGCPrivateBytesChange;
        private int _inPBytesMonitorThread;
        private int _lastTrimPercent;
        private long _limit;
        private long _lowPressureMark;
        private long _maxDelta;
        private long _mediumPressureMark;
        private long _minMaxDelta;
        private uint _pid;
        private long[] _samples;
        private DateTime[] _sampleTimes;
        private Timer _timer;
        private object _timerLock;
        private DateTime _timerSuspendTime;
        private long _totalCacheSize;
        private long _trimDurationTicks;
        private bool _useGetProcessMemoryInfo;
        private const int DELTA_SAMPLE_COUNT = 10;
        private const int HIGH_FREQ_INTERVAL_MS = 0x1388;
        private const int HIGH_FREQ_INTERVAL_S = 5;
        private const int LOW_FREQ_INTERVAL_MS = 0x1d4c0;
        private const int LOW_FREQ_INTERVAL_S = 120;
        private const int MEDIUM_FREQ_INTERVAL_MS = 0x7530;
        private const int MEDIUM_FREQ_INTERVAL_S = 30;
        private const long MEGABYTE = 0x100000L;
        private const int MEGABYTE_SHIFT = 20;
        private const int SAMPLE_COUNT = 2;

        private CacheManager()
        {
            this._lastTrimPercent = 10;
            this._inducedGCMinInterval = 0x2faf080L;
            this._inducedGCFinishTime = DateTime.MinValue;
            this._currentPollInterval = 0x7530;
            this._timerSuspendTime = DateTime.MinValue;
            this._timerLock = new object();
        }

        internal CacheManager(ApplicationManager appManager, long privateBytesLimit)
        {
            this._lastTrimPercent = 10;
            this._inducedGCMinInterval = 0x2faf080L;
            this._inducedGCFinishTime = DateTime.MinValue;
            this._currentPollInterval = 0x7530;
            this._timerSuspendTime = DateTime.MinValue;
            this._timerLock = new object();
            if (privateBytesLimit > 0L)
            {
                this._appManager = appManager;
                this._limit = privateBytesLimit;
                this._pid = (uint) SafeNativeMethods.GetCurrentProcessId();
                this._minMaxDelta = 0x200000L * SystemInfo.GetNumProcessCPUs();
                this.AdjustMaxDeltaAndPressureMarks(this._minMaxDelta);
                this._samples = new long[2];
                this._sampleTimes = new DateTime[2];
                this._useGetProcessMemoryInfo = VersionInfo.ExeName == "w3wp";
                this._deltaSamples = new long[10];
                this._timer = new Timer(new TimerCallback(this.PBytesMonitorThread), null, this._currentPollInterval, this._currentPollInterval);
            }
        }

        private void Adjust()
        {
            long num = this._samples[this._idx];
            long num2 = this._samples[this._idx ^ 1];
            if ((num > num2) && (num2 > 0L))
            {
                DateTime time = this._sampleTimes[this._idx];
                DateTime time2 = this._sampleTimes[this._idx ^ 1];
                long num3 = num - num2;
                long num4 = (long) Math.Round(time.Subtract(time2).TotalSeconds);
                if (num4 > 0L)
                {
                    long delta = num3 / num4;
                    this._deltaSamples[this._idxDeltaSamples] = delta;
                    this._idxDeltaSamples = (this._idxDeltaSamples + 1) % 10;
                    this.AdjustMaxDeltaAndPressureMarks(delta);
                }
            }
            lock (this._timerLock)
            {
                if (this._timer != null)
                {
                    if (num > this._mediumPressureMark)
                    {
                        if (this._currentPollInterval > 0x1388)
                        {
                            this._currentPollInterval = 0x1388;
                            this._timer.Change(this._currentPollInterval, this._currentPollInterval);
                        }
                    }
                    else if (num > this._lowPressureMark)
                    {
                        if (this._currentPollInterval > 0x7530)
                        {
                            this._currentPollInterval = 0x7530;
                            this._timer.Change(this._currentPollInterval, this._currentPollInterval);
                        }
                    }
                    else if (this._currentPollInterval != 0x1d4c0)
                    {
                        this._currentPollInterval = 0x1d4c0;
                        this._timer.Change(this._currentPollInterval, this._currentPollInterval);
                    }
                }
            }
        }

        private void AdjustMaxDeltaAndPressureMarks(long delta)
        {
            long num = this._maxDelta;
            if (delta > num)
            {
                num = delta;
            }
            else
            {
                bool flag = true;
                long num2 = this._maxDelta / 4L;
                foreach (long num3 in this._deltaSamples)
                {
                    if (num3 > num2)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    num = num2 * 2L;
                }
            }
            num = Math.Max(num, this._minMaxDelta);
            if (this._maxDelta != num)
            {
                this._maxDelta = num;
                this._highPressureMark = Math.Max((long) ((this._limit * 9L) / 10L), (long) (this._limit - ((this._maxDelta * 2L) * 5L)));
                this._lowPressureMark = Math.Max((long) ((this._limit * 6L) / 10L), (long) (this._limit - ((this._maxDelta * 2L) * 120L)));
                this._mediumPressureMark = Math.Max((long) ((this._highPressureMark + this._lowPressureMark) / 2L), (long) (this._limit - ((this._maxDelta * 2L) * 30L)));
                this._mediumPressureMark = Math.Min(this._highPressureMark, this._mediumPressureMark);
            }
        }

        private void CollectInfrequently(long privateBytes)
        {
            long ticks = DateTime.UtcNow.Subtract(this._inducedGCFinishTime).Ticks;
            bool flag = ticks > this._inducedGCMinInterval;
            if (flag || (this._lastTrimPercent < 50))
            {
                if (!flag)
                {
                    this._lastTrimPercent = Math.Min(50, this._lastTrimPercent + 10);
                }
                else if ((this._lastTrimPercent > 10) && (ticks > (2L * this._inducedGCMinInterval)))
                {
                    this._lastTrimPercent = Math.Max(10, this._lastTrimPercent - 10);
                }
                int percent = (this._totalCacheSize > 0L) ? this._lastTrimPercent : 0;
                long num3 = 0L;
                if (percent > 0)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    num3 = this._appManager.TrimCaches(percent);
                    stopwatch.Stop();
                    this._trimDurationTicks = stopwatch.Elapsed.Ticks;
                }
                if ((num3 != 0L) && !this._appManager.ShutdownInProgress)
                {
                    Stopwatch stopwatch2 = Stopwatch.StartNew();
                    GC.Collect();
                    stopwatch2.Stop();
                    this._inducedGCCount++;
                    this._inducedGCFinishTime = DateTime.UtcNow;
                    this._inducedGCDurationTicks = stopwatch2.Elapsed.Ticks;
                    this._inducedGCPostPrivateBytes = this.NextSample();
                    this._inducedGCPrivateBytesChange = privateBytes - this._inducedGCPostPrivateBytes;
                    this._inducedGCMinInterval = Math.Max((long) ((this._inducedGCDurationTicks * 0x3e8L) / 0x21L), (long) 0x2faf080L);
                    if ((this._inducedGCPrivateBytesChange * 100L) <= privateBytes)
                    {
                        this._inducedGCMinInterval = Math.Max(this._inducedGCMinInterval, 0x23c34600L);
                    }
                }
            }
        }

        public void Dispose()
        {
            this._disposed = true;
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DisposeTimer();
            }
        }

        private void DisposeTimer()
        {
            lock (this._timerLock)
            {
                if (this._timer != null)
                {
                    this._timer.Dispose();
                    this._timer = null;
                }
            }
        }

        internal long GetUpdatedTotalCacheSize(long sizeUpdate)
        {
            if (sizeUpdate != 0L)
            {
                return Interlocked.Add(ref this._totalCacheSize, sizeUpdate);
            }
            return this._totalCacheSize;
        }

        private long NextSample()
        {
            long num;
            if (this._useGetProcessMemoryInfo)
            {
                long num2;
                UnsafeNativeMethods.GetPrivateBytesIIS6(out num2, true);
                num = num2;
            }
            else
            {
                uint num3;
                uint privatePageCount = 0;
                UnsafeNativeMethods.GetProcessMemoryInformation(this._pid, out privatePageCount, out num3, true);
                num = privatePageCount << 20;
            }
            this._idx ^= 1;
            this._sampleTimes[this._idx] = DateTime.UtcNow;
            this._samples[this._idx] = num;
            return num;
        }

        private void PBytesMonitorThread(object state)
        {
            if (Interlocked.Exchange(ref this._inPBytesMonitorThread, 1) == 0)
            {
                try
                {
                    if (!this._disposed)
                    {
                        long privateBytes = this.NextSample();
                        this.Adjust();
                        if (privateBytes > this._highPressureMark)
                        {
                            this.CollectInfrequently(privateBytes);
                        }
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref this._inPBytesMonitorThread, 0);
                }
            }
        }
    }
}


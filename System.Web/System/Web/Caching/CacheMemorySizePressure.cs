namespace System.Web.Caching
{
    using System;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Hosting;

    internal sealed class CacheMemorySizePressure : CacheMemoryPressure
    {
        private long[] _cacheSizeSamples;
        private DateTime[] _cacheSizeSampleTimes;
        private int _gen2Count;
        private int _idx;
        private long _memoryLimit;
        private SRef _sizedRef;
        private DateTime _startupTime;
        private long _totalCacheSize;
        private const long PRIVATE_BYTES_LIMIT_2GB = 0x32000000L;
        private const long PRIVATE_BYTES_LIMIT_3GB = 0x70800000L;
        private const long PRIVATE_BYTES_LIMIT_64BIT = 0x10000000000L;
        private static long s_autoPrivateBytesLimit = -1L;
        private static long s_effectiveProcessMemoryLimit = -1L;
        private static bool s_isIIS6 = HostingEnvironment.IsUnderIIS6Process;
        private static uint s_pid = 0;
        private static int s_pollInterval;
        private static long s_workerProcessMemoryLimit = -1L;
        private const int SAMPLE_COUNT = 2;

        internal CacheMemorySizePressure(SRef sizedRef)
        {
            this._sizedRef = sizedRef;
            this._gen2Count = GC.CollectionCount(2);
            this._cacheSizeSamples = new long[2];
            this._cacheSizeSampleTimes = new DateTime[2];
            base._pressureHigh = 0x63;
            base._pressureMiddle = 0x62;
            base._pressureLow = 0x61;
            this._startupTime = DateTime.UtcNow;
            base.InitHistory();
        }

        internal void Dispose()
        {
            SRef comparand = this._sizedRef;
            if ((comparand != null) && (Interlocked.CompareExchange<SRef>(ref this._sizedRef, null, comparand) == comparand))
            {
                comparand.Dispose();
            }
            ApplicationManager applicationManager = HostingEnvironment.GetApplicationManager();
            if (applicationManager != null)
            {
                long sizeUpdate = -this._cacheSizeSamples[this._idx];
                applicationManager.GetUpdatedTotalCacheSize(sizeUpdate);
            }
        }

        protected override int GetCurrentPressure()
        {
            int num = GC.CollectionCount(2);
            SRef ref2 = this._sizedRef;
            if ((num != this._gen2Count) && (ref2 != null))
            {
                this._gen2Count = num;
                this._idx ^= 1;
                this._cacheSizeSampleTimes[this._idx] = DateTime.UtcNow;
                this._cacheSizeSamples[this._idx] = ref2.ApproximateSize;
                ApplicationManager applicationManager = HostingEnvironment.GetApplicationManager();
                if (applicationManager != null)
                {
                    long sizeUpdate = this._cacheSizeSamples[this._idx] - this._cacheSizeSamples[this._idx ^ 1];
                    this._totalCacheSize = applicationManager.GetUpdatedTotalCacheSize(sizeUpdate);
                }
                else
                {
                    this._totalCacheSize = this._cacheSizeSamples[this._idx];
                }
            }
            if (this._memoryLimit <= 0L)
            {
                return 0;
            }
            long num3 = this._cacheSizeSamples[this._idx];
            if (num3 > this._memoryLimit)
            {
                num3 = this._memoryLimit;
            }
            PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_PROC_MEM_LIMIT_USED, (int) (num3 >> 10));
            return (int) ((num3 * 100L) / this._memoryLimit);
        }

        internal override int GetPercentToTrim(DateTime lastTrimTime, int lastTrimPercent)
        {
            int num = 0;
            if (base.IsAboveHighPressure())
            {
                long num2 = this._cacheSizeSamples[this._idx];
                if (num2 > this._memoryLimit)
                {
                    num = Math.Min(100, (int) (((num2 - this._memoryLimit) * 100L) / num2));
                }
            }
            return num;
        }

        internal bool HasLimit()
        {
            return (this._memoryLimit != 0L);
        }

        internal override void ReadConfig(CacheSection cacheSection)
        {
            long privateBytesLimit = cacheSection.PrivateBytesLimit;
            this._memoryLimit = WorkerProcessMemoryLimit;
            if ((privateBytesLimit == 0L) && (this._memoryLimit == 0L))
            {
                this._memoryLimit = EffectiveProcessMemoryLimit;
            }
            else if ((privateBytesLimit != 0L) && (this._memoryLimit != 0L))
            {
                this._memoryLimit = Math.Min(this._memoryLimit, privateBytesLimit);
            }
            else if (privateBytesLimit != 0L)
            {
                this._memoryLimit = privateBytesLimit;
            }
            if (this._memoryLimit > 0L)
            {
                if (s_pid == 0)
                {
                    s_pid = (uint) SafeNativeMethods.GetCurrentProcessId();
                }
                base._pressureHigh = 100;
                base._pressureMiddle = 90;
                base._pressureLow = 80;
            }
            s_pollInterval = (int) Math.Min(cacheSection.PrivateBytesPollTime.TotalMilliseconds, 2147483647.0);
            PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_PROC_MEM_LIMIT_USED_BASE, (int) (this._memoryLimit >> 10));
        }

        private static long AutoPrivateBytesLimit
        {
            get
            {
                long num = s_autoPrivateBytesLimit;
                if (num == -1L)
                {
                    bool flag = IntPtr.Size == 8;
                    long totalPhysical = CacheMemoryPressure.TotalPhysical;
                    long totalVirtual = CacheMemoryPressure.TotalVirtual;
                    if (totalPhysical != 0L)
                    {
                        long num4;
                        if (flag)
                        {
                            num4 = 0x10000000000L;
                        }
                        else if (totalVirtual > 0x80000000L)
                        {
                            num4 = 0x70800000L;
                        }
                        else
                        {
                            num4 = 0x32000000L;
                        }
                        long num5 = HostingEnvironment.IsHosted ? ((totalPhysical * 3L) / 5L) : totalPhysical;
                        num = Math.Min(num5, num4);
                    }
                    else
                    {
                        num = flag ? 0x10000000000L : 0x32000000L;
                    }
                    Interlocked.Exchange(ref s_autoPrivateBytesLimit, num);
                }
                return num;
            }
        }

        internal static long EffectiveProcessMemoryLimit
        {
            get
            {
                long workerProcessMemoryLimit = s_effectiveProcessMemoryLimit;
                if (workerProcessMemoryLimit == -1L)
                {
                    workerProcessMemoryLimit = WorkerProcessMemoryLimit;
                    if (workerProcessMemoryLimit == 0L)
                    {
                        workerProcessMemoryLimit = AutoPrivateBytesLimit;
                    }
                    Interlocked.Exchange(ref s_effectiveProcessMemoryLimit, workerProcessMemoryLimit);
                }
                return workerProcessMemoryLimit;
            }
        }

        internal long MemoryLimit
        {
            get
            {
                return this._memoryLimit;
            }
        }

        internal static int PollInterval
        {
            get
            {
                return s_pollInterval;
            }
        }

        internal static long WorkerProcessMemoryLimit
        {
            get
            {
                long num = s_workerProcessMemoryLimit;
                if (num == -1L)
                {
                    if (UnsafeNativeMethods.GetModuleHandle("aspnet_wp.exe") != IntPtr.Zero)
                    {
                        num = UnsafeNativeMethods.PMGetMemoryLimitInMB() << 20;
                    }
                    else if (UnsafeNativeMethods.GetModuleHandle("w3wp.exe") != IntPtr.Zero)
                    {
                        num = ServerConfig.GetInstance().GetW3WPMemoryLimitInKB() << 10;
                    }
                    Interlocked.Exchange(ref s_workerProcessMemoryLimit, num);
                }
                return num;
            }
        }
    }
}


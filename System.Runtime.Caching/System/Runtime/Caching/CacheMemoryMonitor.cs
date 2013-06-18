namespace System.Runtime.Caching
{
    using System;
    using System.Runtime.Caching.Hosting;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    internal sealed class CacheMemoryMonitor : MemoryMonitor, IDisposable
    {
        private long[] _cacheSizeSamples;
        private DateTime[] _cacheSizeSampleTimes;
        private int _gen2Count;
        private int _idx;
        private MemoryCache _memoryCache;
        private long _memoryLimit;
        private SRef _sizedRef;
        private const long PRIVATE_BYTES_LIMIT_2GB = 0x32000000L;
        private const long PRIVATE_BYTES_LIMIT_3GB = 0x70800000L;
        private const long PRIVATE_BYTES_LIMIT_64BIT = 0x10000000000L;
        private static long s_autoPrivateBytesLimit = -1L;
        private static long s_effectiveProcessMemoryLimit = -1L;
        private static IMemoryCacheManager s_memoryCacheManager;
        private const int SAMPLE_COUNT = 2;

        private CacheMemoryMonitor()
        {
        }

        internal CacheMemoryMonitor(MemoryCache memoryCache, int cacheMemoryLimitMegabytes)
        {
            this._memoryCache = memoryCache;
            this._gen2Count = GC.CollectionCount(2);
            this._cacheSizeSamples = new long[2];
            this._cacheSizeSampleTimes = new DateTime[2];
            InitMemoryCacheManager();
            this.InitDisposableMembers(cacheMemoryLimitMegabytes);
        }

        public void Dispose()
        {
            SRef comparand = this._sizedRef;
            if ((comparand != null) && (Interlocked.CompareExchange<SRef>(ref this._sizedRef, null, comparand) == comparand))
            {
                comparand.Dispose();
            }
            IMemoryCacheManager manager = s_memoryCacheManager;
            if (manager != null)
            {
                manager.ReleaseCache(this._memoryCache);
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
                IMemoryCacheManager manager = s_memoryCacheManager;
                if (manager != null)
                {
                    manager.UpdateCacheSize(this._cacheSizeSamples[this._idx], this._memoryCache);
                }
            }
            if (this._memoryLimit <= 0L)
            {
                return 0;
            }
            long num2 = this._cacheSizeSamples[this._idx];
            if (num2 > this._memoryLimit)
            {
                num2 = this._memoryLimit;
            }
            return (int) ((num2 * 100L) / this._memoryLimit);
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

        private void InitDisposableMembers(int cacheMemoryLimitMegabytes)
        {
            bool flag = true;
            try
            {
                this._sizedRef = new SRef(this._memoryCache);
                this.SetLimit(cacheMemoryLimitMegabytes);
                base.InitHistory();
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

        [SecuritySafeCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private static void InitMemoryCacheManager()
        {
            if (s_memoryCacheManager == null)
            {
                IMemoryCacheManager service = null;
                IServiceProvider host = ObjectCache.Host;
                if (host != null)
                {
                    service = host.GetService(typeof(IMemoryCacheManager)) as IMemoryCacheManager;
                }
                if (service != null)
                {
                    Interlocked.CompareExchange<IMemoryCacheManager>(ref s_memoryCacheManager, service, null);
                }
            }
        }

        internal void SetLimit(int cacheMemoryLimitMegabytes)
        {
            long num = cacheMemoryLimitMegabytes;
            num = num << 20;
            this._memoryLimit = 0L;
            if ((num == 0L) && (this._memoryLimit == 0L))
            {
                this._memoryLimit = EffectiveProcessMemoryLimit;
            }
            else if ((num != 0L) && (this._memoryLimit != 0L))
            {
                this._memoryLimit = Math.Min(this._memoryLimit, num);
            }
            else if (num != 0L)
            {
                this._memoryLimit = num;
            }
            if (this._memoryLimit > 0L)
            {
                base._pressureHigh = 100;
                base._pressureLow = 80;
            }
            else
            {
                base._pressureHigh = 0x63;
                base._pressureLow = 0x61;
            }
        }

        private static long AutoPrivateBytesLimit
        {
            get
            {
                long num = s_autoPrivateBytesLimit;
                if (num == -1L)
                {
                    bool flag = IntPtr.Size == 8;
                    long totalPhysical = MemoryMonitor.TotalPhysical;
                    long totalVirtual = MemoryMonitor.TotalVirtual;
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
                        long num5 = (totalPhysical * 3L) / 5L;
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
                long autoPrivateBytesLimit = s_effectiveProcessMemoryLimit;
                if (autoPrivateBytesLimit == -1L)
                {
                    autoPrivateBytesLimit = AutoPrivateBytesLimit;
                    Interlocked.Exchange(ref s_effectiveProcessMemoryLimit, autoPrivateBytesLimit);
                }
                return autoPrivateBytesLimit;
            }
        }

        internal long MemoryLimit
        {
            get
            {
                return this._memoryLimit;
            }
        }
    }
}


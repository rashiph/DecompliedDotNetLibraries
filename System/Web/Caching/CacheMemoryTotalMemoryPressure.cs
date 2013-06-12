namespace System.Web.Caching
{
    using System;
    using System.Web;
    using System.Web.Configuration;

    internal sealed class CacheMemoryTotalMemoryPressure : CacheMemoryPressure
    {
        private const int MIN_TOTAL_MEMORY_TRIM_PERCENT = 10;
        private static readonly long TARGET_TOTAL_MEMORY_TRIM_INTERVAL_TICKS = 0xb2d05e00L;

        internal CacheMemoryTotalMemoryPressure()
        {
            long totalPhysical = CacheMemoryPressure.TotalPhysical;
            if (totalPhysical >= 0x100000000L)
            {
                base._pressureHigh = 0x63;
            }
            else if (totalPhysical >= 0x80000000L)
            {
                base._pressureHigh = 0x62;
            }
            else if (totalPhysical >= 0x40000000L)
            {
                base._pressureHigh = 0x61;
            }
            else if (totalPhysical >= 0x30000000L)
            {
                base._pressureHigh = 0x60;
            }
            else
            {
                base._pressureHigh = 0x5f;
            }
            base._pressureMiddle = base._pressureHigh - 2;
            base._pressureLow = base._pressureHigh - 9;
            base.InitHistory();
            PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_MACH_MEM_LIMIT_USED_BASE, base._pressureHigh);
        }

        protected override int GetCurrentPressure()
        {
            UnsafeNativeMethods.MEMORYSTATUSEX memoryStatusEx = new UnsafeNativeMethods.MEMORYSTATUSEX();
            memoryStatusEx.Init();
            if (UnsafeNativeMethods.GlobalMemoryStatusEx(ref memoryStatusEx) == 0)
            {
                return 0;
            }
            int dwMemoryLoad = memoryStatusEx.dwMemoryLoad;
            if (base._pressureHigh != 0)
            {
                PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_MACH_MEM_LIMIT_USED, dwMemoryLoad);
            }
            return dwMemoryLoad;
        }

        internal override int GetPercentToTrim(DateTime lastTrimTime, int lastTrimPercent)
        {
            int num = 0;
            if (base.IsAboveHighPressure())
            {
                long ticks = DateTime.UtcNow.Subtract(lastTrimTime).Ticks;
                if (ticks > 0L)
                {
                    num = Math.Min(50, (int) ((lastTrimPercent * TARGET_TOTAL_MEMORY_TRIM_INTERVAL_TICKS) / ticks));
                    num = Math.Max(10, num);
                }
            }
            return num;
        }

        internal override void ReadConfig(CacheSection cacheSection)
        {
            int percentagePhysicalMemoryUsedLimit = cacheSection.PercentagePhysicalMemoryUsedLimit;
            if (percentagePhysicalMemoryUsedLimit != 0)
            {
                base._pressureHigh = Math.Max(3, percentagePhysicalMemoryUsedLimit);
                base._pressureMiddle = Math.Max(2, base._pressureHigh - 2);
                base._pressureLow = Math.Max(1, base._pressureHigh - 9);
                PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_MACH_MEM_LIMIT_USED_BASE, base._pressureHigh);
            }
        }

        internal long MemoryLimit
        {
            get
            {
                return (long) base._pressureHigh;
            }
        }
    }
}


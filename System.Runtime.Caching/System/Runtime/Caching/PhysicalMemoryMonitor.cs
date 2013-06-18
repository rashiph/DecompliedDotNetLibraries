namespace System.Runtime.Caching
{
    using System;
    using System.Security;

    internal sealed class PhysicalMemoryMonitor : MemoryMonitor
    {
        private const int MIN_TOTAL_MEMORY_TRIM_PERCENT = 10;
        private static readonly long TARGET_TOTAL_MEMORY_TRIM_INTERVAL_TICKS = 0xb2d05e00L;

        private PhysicalMemoryMonitor()
        {
        }

        internal PhysicalMemoryMonitor(int physicalMemoryLimitPercentage)
        {
            long totalPhysical = MemoryMonitor.TotalPhysical;
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
            base._pressureLow = base._pressureHigh - 9;
            this.SetLimit(physicalMemoryLimitPercentage);
            base.InitHistory();
        }

        [SecuritySafeCritical]
        protected override int GetCurrentPressure()
        {
            MEMORYSTATUSEX memoryStatusEx = new MEMORYSTATUSEX();
            memoryStatusEx.Init();
            if (UnsafeNativeMethods.GlobalMemoryStatusEx(ref memoryStatusEx) == 0)
            {
                return 0;
            }
            return memoryStatusEx.dwMemoryLoad;
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

        internal void SetLimit(int physicalMemoryLimitPercentage)
        {
            if (physicalMemoryLimitPercentage != 0)
            {
                base._pressureHigh = Math.Max(3, physicalMemoryLimitPercentage);
                base._pressureLow = Math.Max(1, base._pressureHigh - 9);
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


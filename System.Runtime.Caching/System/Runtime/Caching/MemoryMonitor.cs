namespace System.Runtime.Caching
{
    using System;
    using System.Security;

    internal abstract class MemoryMonitor
    {
        protected int _i0;
        protected int _pressureHigh;
        protected int[] _pressureHist;
        protected int _pressureLow;
        protected int _pressureTotal;
        protected const long GIGABYTE = 0x40000000L;
        protected const int GIGABYTE_SHIFT = 30;
        protected const int HISTORY_COUNT = 6;
        protected const long KILOBYTE = 0x400L;
        protected const int KILOBYTE_SHIFT = 10;
        protected const long MEGABYTE = 0x100000L;
        protected const int MEGABYTE_SHIFT = 20;
        private static long s_totalPhysical;
        private static long s_totalVirtual;
        protected const long TERABYTE = 0x10000000000L;
        protected const int TERABYTE_SHIFT = 40;

        [SecuritySafeCritical]
        static MemoryMonitor()
        {
            MEMORYSTATUSEX memoryStatusEx = new MEMORYSTATUSEX();
            memoryStatusEx.Init();
            if (UnsafeNativeMethods.GlobalMemoryStatusEx(ref memoryStatusEx) != 0)
            {
                s_totalPhysical = memoryStatusEx.ullTotalPhys;
                s_totalVirtual = memoryStatusEx.ullTotalVirtual;
            }
        }

        protected MemoryMonitor()
        {
        }

        protected abstract int GetCurrentPressure();
        internal abstract int GetPercentToTrim(DateTime lastTrimTime, int lastTrimPercent);
        protected void InitHistory()
        {
            int currentPressure = this.GetCurrentPressure();
            this._pressureHist = new int[6];
            for (int i = 0; i < 6; i++)
            {
                this._pressureHist[i] = currentPressure;
                this._pressureTotal += currentPressure;
            }
        }

        internal bool IsAboveHighPressure()
        {
            return (this.PressureLast >= this.PressureHigh);
        }

        internal void Update()
        {
            int currentPressure = this.GetCurrentPressure();
            this._i0 = (this._i0 + 1) % 6;
            this._pressureTotal -= this._pressureHist[this._i0];
            this._pressureTotal += currentPressure;
            this._pressureHist[this._i0] = currentPressure;
        }

        internal int PressureHigh
        {
            get
            {
                return this._pressureHigh;
            }
        }

        internal int PressureLast
        {
            get
            {
                return this._pressureHist[this._i0];
            }
        }

        internal int PressureLow
        {
            get
            {
                return this._pressureLow;
            }
        }

        internal static long TotalPhysical
        {
            get
            {
                return s_totalPhysical;
            }
        }

        internal static long TotalVirtual
        {
            get
            {
                return s_totalVirtual;
            }
        }
    }
}


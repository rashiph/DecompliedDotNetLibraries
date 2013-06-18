namespace System.Web.Util
{
    using System;
    using System.Web;

    internal sealed class Counter
    {
        private Counter()
        {
        }

        internal static long Frequency
        {
            get
            {
                long lpFrequency = 0L;
                SafeNativeMethods.QueryPerformanceFrequency(ref lpFrequency);
                return lpFrequency;
            }
        }

        internal static long Value
        {
            get
            {
                long lpPerformanceCount = 0L;
                SafeNativeMethods.QueryPerformanceCounter(ref lpPerformanceCount);
                return lpPerformanceCount;
            }
        }
    }
}


namespace System.EnterpriseServices
{
    using System;
    using System.Diagnostics;

    internal static class Perf
    {
        private static long _count;
        private static long _freq;

        static Perf()
        {
            Util.QueryPerformanceFrequency(out _freq);
        }

        [Conditional("_DEBUG_PERF")]
        internal static void Tick(string name)
        {
            long num;
            Util.QueryPerformanceCounter(out num);
            if (_count != 0L)
            {
                double num1 = ((double) (num - _count)) / ((double) _freq);
            }
            _count = num;
        }
    }
}


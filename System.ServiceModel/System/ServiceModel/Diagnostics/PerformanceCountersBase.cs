namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Globalization;
    using System.Threading;

    internal abstract class PerformanceCountersBase : IDisposable
    {
        protected int disposed;

        protected PerformanceCountersBase()
        {
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this.disposed, 1) == 0)
            {
                this.Dispose(true);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        protected static string GetHashedString(string str, int startIndex, int count, bool hashAtEnd)
        {
            string str2 = str.Remove(startIndex, count);
            string str3 = ((uint) (str.GetHashCode() % 0x63)).ToString("00", CultureInfo.InvariantCulture);
            if (!hashAtEnd)
            {
                return (str3 + str2);
            }
            return (str2 + str3);
        }

        internal abstract string[] CounterNames { get; }

        internal abstract bool Initialized { get; }

        internal abstract string InstanceName { get; }

        internal abstract int PerfCounterEnd { get; }

        internal abstract int PerfCounterStart { get; }
    }
}


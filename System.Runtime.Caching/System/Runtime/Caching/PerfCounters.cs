namespace System.Runtime.Caching
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Caching.Hosting;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    internal sealed class PerfCounters : IDisposable
    {
        private PerformanceCounter[] _counters;
        private long[] _counterValues;
        private const string CACHE_ENTRIES = "Cache Entries";
        private const string CACHE_HIT_RATIO = "Cache Hit Ratio";
        private const string CACHE_HIT_RATIO_BASE = "Cache Hit Ratio Base";
        private const string CACHE_HITS = "Cache Hits";
        private const string CACHE_MISSES = "Cache Misses";
        private const string CACHE_TRIMS = "Cache Trims";
        private const string CACHE_TURNOVER = "Cache Turnover Rate";
        private const int NUM_COUNTERS = 7;
        private const string PERF_COUNTER_CATEGORY = ".NET Memory Cache 4.0";
        private static string s_appId;

        private PerfCounters()
        {
        }

        internal PerfCounters(string cacheName)
        {
            if (cacheName == null)
            {
                throw new ArgumentNullException("cacheName");
            }
            EnsureAppIdInited();
            this.InitDisposableMembers(cacheName);
        }

        internal void Decrement(PerfCounterName name)
        {
            int index = (int) name;
            this._counters[index].Decrement();
            Interlocked.Decrement(ref this._counterValues[index]);
        }

        public void Dispose()
        {
            PerformanceCounter[] comparand = this._counters;
            if ((comparand != null) && (Interlocked.CompareExchange<PerformanceCounter[]>(ref this._counters, null, comparand) == comparand))
            {
                for (int i = 0; i < 7; i++)
                {
                    PerformanceCounter counter = comparand[i];
                    if (counter != null)
                    {
                        long num2 = Interlocked.Exchange(ref this._counterValues[i], 0L);
                        if (num2 != 0L)
                        {
                            counter.IncrementBy(-num2);
                        }
                        counter.Dispose();
                    }
                }
            }
        }

        [SecuritySafeCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private static void EnsureAppIdInited()
        {
            if (s_appId == null)
            {
                IApplicationIdentifier service = null;
                IServiceProvider host = ObjectCache.Host;
                if (host != null)
                {
                    service = host.GetService(typeof(IApplicationIdentifier)) as IApplicationIdentifier;
                }
                string fileNameWithoutExtension = (service != null) ? service.GetApplicationId() : null;
                if (string.IsNullOrEmpty(fileNameWithoutExtension))
                {
                    StringBuilder filename = new StringBuilder(0x200);
                    if (System.Runtime.Caching.UnsafeNativeMethods.GetModuleFileName(IntPtr.Zero, filename, 0x200) != 0)
                    {
                        fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename.ToString());
                    }
                }
                if (string.IsNullOrEmpty(fileNameWithoutExtension))
                {
                    fileNameWithoutExtension = AppDomain.CurrentDomain.FriendlyName;
                }
                Interlocked.CompareExchange<string>(ref s_appId, fileNameWithoutExtension, null);
            }
        }

        internal void Increment(PerfCounterName name)
        {
            int index = (int) name;
            this._counters[index].Increment();
            Interlocked.Increment(ref this._counterValues[index]);
        }

        internal void IncrementBy(PerfCounterName name, long value)
        {
            int index = (int) name;
            this._counters[index].IncrementBy(value);
            Interlocked.Add(ref this._counterValues[index], value);
        }

        private void InitDisposableMembers(string cacheName)
        {
            bool flag = true;
            try
            {
                StringBuilder builder = (s_appId != null) ? new StringBuilder(s_appId + ":" + cacheName) : new StringBuilder(cacheName);
                for (int i = 0; i < builder.Length; i++)
                {
                    switch (builder[i])
                    {
                        case '/':
                        case '\\':
                        case '#':
                            builder[i] = '_';
                            break;

                        case '(':
                            builder[i] = '[';
                            break;

                        case ')':
                            builder[i] = ']';
                            break;
                    }
                }
                string instanceName = builder.ToString();
                this._counters = new PerformanceCounter[7];
                this._counterValues = new long[7];
                this._counters[0] = new PerformanceCounter(".NET Memory Cache 4.0", "Cache Entries", instanceName, false);
                this._counters[1] = new PerformanceCounter(".NET Memory Cache 4.0", "Cache Hits", instanceName, false);
                this._counters[2] = new PerformanceCounter(".NET Memory Cache 4.0", "Cache Hit Ratio", instanceName, false);
                this._counters[3] = new PerformanceCounter(".NET Memory Cache 4.0", "Cache Hit Ratio Base", instanceName, false);
                this._counters[4] = new PerformanceCounter(".NET Memory Cache 4.0", "Cache Misses", instanceName, false);
                this._counters[5] = new PerformanceCounter(".NET Memory Cache 4.0", "Cache Trims", instanceName, false);
                this._counters[6] = new PerformanceCounter(".NET Memory Cache 4.0", "Cache Turnover Rate", instanceName, false);
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
    }
}


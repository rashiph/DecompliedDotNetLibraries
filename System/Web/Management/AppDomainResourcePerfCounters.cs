namespace System.Web.Management
{
    using System;
    using System.Threading;
    using System.Web;

    internal class AppDomainResourcePerfCounters
    {
        private int _CPUUsageLastReported;
        private static bool _fInit = false;
        private static object _InitLock = new object();
        private static int _inProgressLock = 0;
        private DateTime _LastCollectTime = DateTime.UtcNow;
        private int _MemUsageLastReported;
        private static bool _StopRequested = false;
        private static Timer _Timer = null;
        private TimeSpan _TotalCPUTime = AppDomain.CurrentDomain.MonitoringTotalProcessorTime;
        private const uint NUM_SECONDS_TO_POLL = 5;

        private AppDomainResourcePerfCounters()
        {
        }

        internal static void Init()
        {
            if (!_fInit)
            {
                lock (_InitLock)
                {
                    if (!_fInit)
                    {
                        if (AppDomain.MonitoringIsEnabled)
                        {
                            PerfCounters.SetCounter(AppPerfCounter.APP_CPU_USED_BASE, 100);
                            _Timer = new Timer(new System.Threading.TimerCallback(new AppDomainResourcePerfCounters().TimerCallback), null, 0x1388, 0x1388);
                        }
                        _fInit = true;
                    }
                }
            }
        }

        private void SetPerfCounters()
        {
            long num = AppDomain.CurrentDomain.MonitoringSurvivedMemorySize / 0x400L;
            this._MemUsageLastReported = (int) Math.Min(0x7fffffffL, Math.Max(0L, num));
            PerfCounters.SetCounter(AppPerfCounter.APP_MEMORY_USED, this._MemUsageLastReported);
            DateTime utcNow = DateTime.UtcNow;
            TimeSpan monitoringTotalProcessorTime = AppDomain.CurrentDomain.MonitoringTotalProcessorTime;
            TimeSpan span2 = (TimeSpan) (utcNow - this._LastCollectTime);
            double totalMilliseconds = span2.TotalMilliseconds;
            TimeSpan span3 = monitoringTotalProcessorTime - this._TotalCPUTime;
            int num4 = (int) ((span3.TotalMilliseconds * 100.0) / totalMilliseconds);
            this._CPUUsageLastReported = Math.Min(100, Math.Max(0, num4));
            PerfCounters.SetCounter(AppPerfCounter.APP_CPU_USED, this._CPUUsageLastReported);
            this._TotalCPUTime = monitoringTotalProcessorTime;
            this._LastCollectTime = utcNow;
        }

        internal static void Stop()
        {
            if (_Timer == null)
            {
                return;
            }
            _StopRequested = true;
            lock (_InitLock)
            {
                if (_Timer != null)
                {
                    _Timer.Dispose();
                    _Timer = null;
                }
                goto Label_0048;
            }
        Label_0041:
            Thread.Sleep(100);
        Label_0048:
            if (_inProgressLock != 0)
            {
                goto Label_0041;
            }
        }

        private void TimerCallback(object state)
        {
            if ((!_StopRequested && AppDomain.MonitoringIsEnabled) && (Interlocked.Exchange(ref _inProgressLock, 1) == 0))
            {
                try
                {
                    this.SetPerfCounters();
                }
                catch
                {
                }
                finally
                {
                    Interlocked.Exchange(ref _inProgressLock, 0);
                }
            }
        }
    }
}


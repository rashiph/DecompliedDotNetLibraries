namespace System.Web.Management
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Web;
    using System.Web.Hosting;

    public class WebProcessStatistics
    {
        private static int s_appdomainCount;
        private static bool s_getCurrentProcFailed = false;
        private static DateTime s_lastUpdated = DateTime.MinValue;
        private static object s_lockObject = new object();
        private static long s_managedHeapSize;
        private static long s_peakWorkingSet;
        private static int s_requestsExecuting;
        private static int s_requestsQueued;
        private static int s_requestsRejected;
        private static DateTime s_startTime = DateTime.MinValue;
        private static int s_threadCount;
        private static long s_workingSet;
        private static TimeSpan TS_ONE_SECOND = new TimeSpan(0, 0, 1);

        static WebProcessStatistics()
        {
            try
            {
                s_startTime = Process.GetCurrentProcess().StartTime;
            }
            catch
            {
                s_getCurrentProcFailed = true;
            }
        }

        public virtual void FormatToString(WebEventFormatter formatter)
        {
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_process_start_time", this.ProcessStartTime.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_thread_count", this.ThreadCount.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_working_set", this.WorkingSet.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_peak_working_set", this.PeakWorkingSet.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_managed_heap_size", this.ManagedHeapSize.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_application_domain_count", this.AppDomainCount.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_requests_executing", this.RequestsExecuting.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_request_queued", this.RequestsQueued.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_request_rejected", this.RequestsRejected.ToString(CultureInfo.InstalledUICulture)));
        }

        private void Update()
        {
            DateTime now = DateTime.Now;
            if ((now - s_lastUpdated) >= TS_ONE_SECOND)
            {
                lock (s_lockObject)
                {
                    if ((now - s_lastUpdated) >= TS_ONE_SECOND)
                    {
                        if (!s_getCurrentProcFailed)
                        {
                            Process currentProcess = Process.GetCurrentProcess();
                            s_threadCount = currentProcess.Threads.Count;
                            s_workingSet = currentProcess.WorkingSet64;
                            s_peakWorkingSet = currentProcess.PeakWorkingSet64;
                        }
                        s_managedHeapSize = GC.GetTotalMemory(false);
                        s_appdomainCount = HostingEnvironment.AppDomainsCount;
                        s_requestsExecuting = PerfCounters.GetGlobalCounter(GlobalPerfCounter.REQUESTS_CURRENT);
                        s_requestsQueued = PerfCounters.GetGlobalCounter(GlobalPerfCounter.REQUESTS_QUEUED);
                        s_requestsRejected = PerfCounters.GetGlobalCounter(GlobalPerfCounter.REQUESTS_REJECTED);
                        s_lastUpdated = now;
                    }
                }
            }
        }

        public int AppDomainCount
        {
            get
            {
                this.Update();
                return s_appdomainCount;
            }
        }

        public long ManagedHeapSize
        {
            get
            {
                this.Update();
                return s_managedHeapSize;
            }
        }

        public long PeakWorkingSet
        {
            get
            {
                this.Update();
                return s_peakWorkingSet;
            }
        }

        public DateTime ProcessStartTime
        {
            get
            {
                this.Update();
                return s_startTime;
            }
        }

        public int RequestsExecuting
        {
            get
            {
                this.Update();
                return s_requestsExecuting;
            }
        }

        public int RequestsQueued
        {
            get
            {
                this.Update();
                return s_requestsQueued;
            }
        }

        public int RequestsRejected
        {
            get
            {
                this.Update();
                return s_requestsRejected;
            }
        }

        public int ThreadCount
        {
            get
            {
                this.Update();
                return s_threadCount;
            }
        }

        public long WorkingSet
        {
            get
            {
                this.Update();
                return s_workingSet;
            }
        }
    }
}


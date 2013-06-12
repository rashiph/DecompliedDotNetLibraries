namespace System.Web.SessionState
{
    using System;
    using System.Web;
    using System.Web.Configuration;

    public sealed class StateRuntime : IStateRuntime
    {
        static StateRuntime()
        {
            WebConfigurationFileMap fileMap = new WebConfigurationFileMap();
            UserMapPath configMapPath = new UserMapPath(fileMap);
            HttpConfigurationSystem.EnsureInit(configMapPath, false, true);
            StateApplication customApplication = new StateApplication();
            HttpApplicationFactory.SetCustomApplication(customApplication);
            PerfCounters.OpenStateCounters();
            ResetStateServerCounters();
        }

        public void ProcessRequest(IntPtr tracker, int verb, string uri, int exclusive, int timeout, int lockCookieExists, int lockCookie, int contentLength, IntPtr content)
        {
            this.ProcessRequest(tracker, verb, uri, exclusive, 0, timeout, lockCookieExists, lockCookie, contentLength, content);
        }

        public void ProcessRequest(IntPtr tracker, int verb, string uri, int exclusive, int extraFlags, int timeout, int lockCookieExists, int lockCookie, int contentLength, IntPtr content)
        {
            StateHttpWorkerRequest wr = new StateHttpWorkerRequest(tracker, (UnsafeNativeMethods.StateProtocolVerb) verb, uri, (UnsafeNativeMethods.StateProtocolExclusive) exclusive, extraFlags, timeout, lockCookieExists, lockCookie, contentLength, content);
            HttpRuntime.ProcessRequest(wr);
        }

        private static void ResetStateServerCounters()
        {
            PerfCounters.SetStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_TOTAL, 0);
            PerfCounters.SetStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_ACTIVE, 0);
            PerfCounters.SetStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_TIMED_OUT, 0);
            PerfCounters.SetStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_ABANDONED, 0);
        }

        public void StopProcessing()
        {
            ResetStateServerCounters();
            HttpRuntime.Close();
        }
    }
}


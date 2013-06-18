namespace System.Web.Hosting
{
    using System;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Management;
    using System.Web.Util;

    public sealed class ISAPIRuntime : MarshalByRefObject, IISAPIRuntime, IRegisteredObject
    {
        private static int _isThisAppDomainRemovedFromUnmanagedTable;
        private const int WORKER_REQUEST_TYPE_IN_PROC = 0;
        private const int WORKER_REQUEST_TYPE_IN_PROC_VERSION_2 = 2;
        private const int WORKER_REQUEST_TYPE_OOP = 1;

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public ISAPIRuntime()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public void DoGCCollect()
        {
            for (int i = 10; i > 0; i--)
            {
                GC.Collect();
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public int ProcessRequest(IntPtr ecb, int iWRType)
        {
            IntPtr zero = IntPtr.Zero;
            if (iWRType == 2)
            {
                zero = ecb;
                ecb = System.Web.UnsafeNativeMethods.GetEcb(zero);
            }
            ISAPIWorkerRequest wr = null;
            try
            {
                bool useOOP = iWRType == 1;
                wr = ISAPIWorkerRequest.CreateWorkerRequest(ecb, useOOP);
                wr.Initialize();
                string appPathTranslated = wr.GetAppPathTranslated();
                string appDomainAppPathInternal = HttpRuntime.AppDomainAppPathInternal;
                if ((appDomainAppPathInternal == null) || StringUtil.EqualsIgnoreCase(appPathTranslated, appDomainAppPathInternal))
                {
                    HttpRuntime.ProcessRequestNoDemand(wr);
                    return 0;
                }
                HttpRuntime.ShutdownAppDomain(ApplicationShutdownReason.PhysicalApplicationPathChanged, System.Web.SR.GetString("Hosting_Phys_Path_Changed", new object[] { appDomainAppPathInternal, appPathTranslated }));
                return 1;
            }
            catch (Exception exception)
            {
                try
                {
                    WebBaseEvent.RaiseRuntimeError(exception, this);
                }
                catch
                {
                }
                if ((wr == null) || !(wr.Ecb == IntPtr.Zero))
                {
                    throw;
                }
                if (zero != IntPtr.Zero)
                {
                    System.Web.UnsafeNativeMethods.SetDoneWithSessionCalled(zero);
                }
                if (exception is ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
                return 0;
            }
        }

        internal static void RemoveThisAppDomainFromUnmanagedTable()
        {
            if (Interlocked.Exchange(ref _isThisAppDomainRemovedFromUnmanagedTable, 1) == 0)
            {
                try
                {
                    string appDomainAppIdInternal = HttpRuntime.AppDomainAppIdInternal;
                    if (appDomainAppIdInternal != null)
                    {
                        System.Web.UnsafeNativeMethods.AppDomainRestart(appDomainAppIdInternal);
                    }
                    HttpRuntime.AddAppDomainTraceMessage(System.Web.SR.GetString("App_Domain_Restart"));
                }
                catch
                {
                }
            }
        }

        public void StartProcessing()
        {
        }

        public void StopProcessing()
        {
            HostingEnvironment.UnregisterObject(this);
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            RemoveThisAppDomainFromUnmanagedTable();
            HostingEnvironment.UnregisterObject(this);
        }
    }
}


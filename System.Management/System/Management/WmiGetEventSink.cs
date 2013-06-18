namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    internal class WmiGetEventSink : WmiEventSink
    {
        private static object contextParameter;
        private ManagementObject managementObject;
        private static ManagementObject managementObjectParameter;
        private static ManagementScope scopeParameter;
        private static ManagementOperationObserver watcherParameter;
        private static WmiGetEventSink wmiGetEventSinkNew;

        private WmiGetEventSink(ManagementOperationObserver watcher, object context, ManagementScope scope, ManagementObject managementObject) : base(watcher, context, scope, null, null)
        {
            this.managementObject = managementObject;
        }

        internal static WmiGetEventSink GetWmiGetEventSink(ManagementOperationObserver watcher, object context, ManagementScope scope, ManagementObject managementObject)
        {
            if (MTAHelper.IsNoContextMTA())
            {
                return new WmiGetEventSink(watcher, context, scope, managementObject);
            }
            watcherParameter = watcher;
            contextParameter = context;
            scopeParameter = scope;
            managementObjectParameter = managementObject;
            new ThreadDispatch(new ThreadDispatch.ThreadWorkerMethod(WmiGetEventSink.HackToCreateWmiGetEventSink)).Start();
            return wmiGetEventSinkNew;
        }

        private static void HackToCreateWmiGetEventSink()
        {
            wmiGetEventSinkNew = new WmiGetEventSink(watcherParameter, contextParameter, scopeParameter, managementObjectParameter);
        }

        public override void Indicate(IntPtr pIWbemClassObject)
        {
            Marshal.AddRef(pIWbemClassObject);
            IWbemClassObjectFreeThreaded threaded = new IWbemClassObjectFreeThreaded(pIWbemClassObject);
            if (this.managementObject != null)
            {
                try
                {
                    this.managementObject.wbemObject = threaded;
                }
                catch
                {
                }
            }
        }
    }
}


namespace System.Management
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class SinkForEventQuery : IWmiEventSource
    {
        private object context;
        private ManagementEventWatcher eventWatcher;
        private bool isLocal;
        private IWbemServices services;
        private int status;
        private IWbemObjectSink stub;

        public SinkForEventQuery(ManagementEventWatcher eventWatcher, object context, IWbemServices services)
        {
            this.services = services;
            this.context = context;
            this.eventWatcher = eventWatcher;
            this.status = 0;
            this.isLocal = false;
            if ((string.Compare(eventWatcher.Scope.Path.Server, ".", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(eventWatcher.Scope.Path.Server, Environment.MachineName, StringComparison.OrdinalIgnoreCase) == 0))
            {
                this.isLocal = true;
            }
            if (MTAHelper.IsNoContextMTA())
            {
                this.HackToCreateStubInMTA(this);
            }
            else
            {
                new ThreadDispatch(new ThreadDispatch.ThreadWorkerMethodWithParam(this.HackToCreateStubInMTA)) { Parameter = this }.Start();
            }
        }

        internal void Cancel()
        {
            if (this.stub != null)
            {
                lock (this)
                {
                    if (this.stub != null)
                    {
                        int errorCode = this.services.CancelAsyncCall_(this.stub);
                        this.ReleaseStub();
                        if (errorCode < 0)
                        {
                            if ((errorCode & 0xfffff000L) == 0x80041000L)
                            {
                                ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                            }
                            else
                            {
                                Marshal.ThrowExceptionForHR(errorCode);
                            }
                        }
                    }
                }
            }
        }

        private void Cancel2(object o)
        {
            try
            {
                this.Cancel();
            }
            catch
            {
            }
        }

        private void HackToCreateStubInMTA(object param)
        {
            SinkForEventQuery pIUnknown = (SinkForEventQuery) param;
            object ppIUnknown = null;
            pIUnknown.Status = WmiNetUtilsHelper.GetDemultiplexedStub_f(pIUnknown, pIUnknown.isLocal, out ppIUnknown);
            pIUnknown.stub = (IWbemObjectSink) ppIUnknown;
        }

        public void Indicate(IntPtr pWbemClassObject)
        {
            Marshal.AddRef(pWbemClassObject);
            IWbemClassObjectFreeThreaded wbemObject = new IWbemClassObjectFreeThreaded(pWbemClassObject);
            try
            {
                EventArrivedEventArgs args = new EventArrivedEventArgs(this.context, new ManagementBaseObject(wbemObject));
                this.eventWatcher.FireEventArrived(args);
            }
            catch
            {
            }
        }

        internal void ReleaseStub()
        {
            if (this.stub != null)
            {
                lock (this)
                {
                    if (this.stub != null)
                    {
                        try
                        {
                            Marshal.ReleaseComObject(this.stub);
                            this.stub = null;
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        public void SetStatus(int flags, int hResult, string message, IntPtr pErrObj)
        {
            try
            {
                this.eventWatcher.FireStopped(new StoppedEventArgs(this.context, hResult));
                if ((hResult != -2147217358) && (hResult != 0x40006))
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.Cancel2));
                }
            }
            catch
            {
            }
        }

        public int Status
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.status;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.status = value;
            }
        }

        internal IWbemObjectSink Stub
        {
            get
            {
                return this.stub;
            }
        }
    }
}


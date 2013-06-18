namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class WmiEventSink : IWmiEventSource
    {
        private string className;
        private static string classNameParameter;
        private object context;
        private static object contextParameter;
        private int hash;
        private bool isLocal;
        private ManagementPath path;
        private static string pathParameter;
        private static int s_hash = 0;
        private ManagementScope scope;
        private static ManagementScope scopeParameter;
        private object stub;
        private ManagementOperationObserver watcher;
        private static ManagementOperationObserver watcherParameter;
        private static WmiEventSink wmiEventSinkNew;

        internal event InternalObjectPutEventHandler InternalObjectPut;

        protected WmiEventSink(ManagementOperationObserver watcher, object context, ManagementScope scope, string path, string className)
        {
            try
            {
                this.context = context;
                this.watcher = watcher;
                this.className = className;
                this.isLocal = false;
                if (path != null)
                {
                    this.path = new ManagementPath(path);
                    if ((string.Compare(this.path.Server, ".", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(this.path.Server, Environment.MachineName, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        this.isLocal = true;
                    }
                }
                if (scope != null)
                {
                    this.scope = scope.Clone();
                    if ((path == null) && ((string.Compare(this.scope.Path.Server, ".", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(this.scope.Path.Server, Environment.MachineName, StringComparison.OrdinalIgnoreCase) == 0)))
                    {
                        this.isLocal = true;
                    }
                }
                WmiNetUtilsHelper.GetDemultiplexedStub_f(this, this.isLocal, out this.stub);
                this.hash = Interlocked.Increment(ref s_hash);
            }
            catch
            {
            }
        }

        internal void Cancel()
        {
            try
            {
                this.scope.GetIWbemServices().CancelAsyncCall_((IWbemObjectSink) this.stub);
            }
            catch
            {
            }
        }

        public override int GetHashCode()
        {
            return this.hash;
        }

        internal static WmiEventSink GetWmiEventSink(ManagementOperationObserver watcher, object context, ManagementScope scope, string path, string className)
        {
            if (MTAHelper.IsNoContextMTA())
            {
                return new WmiEventSink(watcher, context, scope, path, className);
            }
            watcherParameter = watcher;
            contextParameter = context;
            scopeParameter = scope;
            pathParameter = path;
            classNameParameter = className;
            new ThreadDispatch(new ThreadDispatch.ThreadWorkerMethod(WmiEventSink.HackToCreateWmiEventSink)).Start();
            return wmiEventSinkNew;
        }

        private static void HackToCreateWmiEventSink()
        {
            wmiEventSinkNew = new WmiEventSink(watcherParameter, contextParameter, scopeParameter, pathParameter, classNameParameter);
        }

        public virtual void Indicate(IntPtr pIWbemClassObject)
        {
            Marshal.AddRef(pIWbemClassObject);
            IWbemClassObjectFreeThreaded wbemObject = new IWbemClassObjectFreeThreaded(pIWbemClassObject);
            try
            {
                ObjectReadyEventArgs args = new ObjectReadyEventArgs(this.context, ManagementBaseObject.GetBaseObject(wbemObject, this.scope));
                this.watcher.FireObjectReady(args);
            }
            catch
            {
            }
        }

        internal void ReleaseStub()
        {
            try
            {
                if (this.stub != null)
                {
                    Marshal.ReleaseComObject(this.stub);
                    this.stub = null;
                }
            }
            catch
            {
            }
        }

        public void SetStatus(int flags, int hResult, string message, IntPtr pErrorObj)
        {
            IWbemClassObjectFreeThreaded wbemObject = null;
            if (pErrorObj != IntPtr.Zero)
            {
                Marshal.AddRef(pErrorObj);
                wbemObject = new IWbemClassObjectFreeThreaded(pErrorObj);
            }
            try
            {
                if (flags == 0)
                {
                    if (this.path != null)
                    {
                        if (this.className == null)
                        {
                            this.path.RelativePath = message;
                        }
                        else
                        {
                            this.path.RelativePath = this.className;
                        }
                        if (this.InternalObjectPut != null)
                        {
                            try
                            {
                                InternalObjectPutEventArgs e = new InternalObjectPutEventArgs(this.path);
                                this.InternalObjectPut(this, e);
                            }
                            catch
                            {
                            }
                        }
                        ObjectPutEventArgs args2 = new ObjectPutEventArgs(this.context, this.path);
                        this.watcher.FireObjectPut(args2);
                    }
                    CompletedEventArgs args3 = null;
                    if (wbemObject != null)
                    {
                        args3 = new CompletedEventArgs(this.context, hResult, new ManagementBaseObject(wbemObject));
                    }
                    else
                    {
                        args3 = new CompletedEventArgs(this.context, hResult, null);
                    }
                    this.watcher.FireCompleted(args3);
                    this.watcher.RemoveSink(this);
                }
                else if ((flags & 2) != 0)
                {
                    ProgressEventArgs args = new ProgressEventArgs(this.context, (hResult & -65536) >> 0x10, hResult & 0xffff, message);
                    this.watcher.FireProgress(args);
                }
            }
            catch
            {
            }
        }

        public IWbemObjectSink Stub
        {
            get
            {
                try
                {
                    return ((this.stub != null) ? ((IWbemObjectSink) this.stub) : null);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}


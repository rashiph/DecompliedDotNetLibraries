namespace System.Management
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    internal class MTAHelper
    {
        private static bool CanCallCoGetObjectContext = IsWindows2000OrHigher();
        private static object critSec = new object();
        private static AutoResetEvent evtGo = new AutoResetEvent(false);
        private static Guid IID_IComThreadingInfo = new Guid("000001ce-0000-0000-C000-000000000046");
        private static Guid IID_IObjectContext = new Guid("51372AE0-CAE7-11CF-BE81-00AA00A2FA25");
        private static ArrayList reqList = new ArrayList(3);
        private static bool workerThreadInitialized = false;

        [SuppressUnmanagedCodeSecurity, DllImport("ole32.dll")]
        private static extern int CoGetObjectContext([In] ref Guid riid, out IntPtr pUnk);
        public static object CreateInMTA(Type type)
        {
            if (IsNoContextMTA())
            {
                return Activator.CreateInstance(type);
            }
            MTARequest request = new MTARequest(type);
            lock (critSec)
            {
                if (!workerThreadInitialized)
                {
                    InitWorkerThread();
                    workerThreadInitialized = true;
                }
                int index = reqList.Add(request);
                if (!evtGo.Set())
                {
                    reqList.RemoveAt(index);
                    throw new ManagementException(RC.GetString("WORKER_THREAD_WAKEUP_FAILED"));
                }
            }
            request.evtDone.WaitOne();
            if (request.exception != null)
            {
                throw request.exception;
            }
            return request.createdObject;
        }

        private static void InitWorkerThread()
        {
            Thread thread = new Thread(new ThreadStart(MTAHelper.WorkerThread));
            thread.SetApartmentState(ApartmentState.MTA);
            thread.IsBackground = true;
            thread.Start();
        }

        public static bool IsNoContextMTA()
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.MTA)
            {
                return false;
            }
            if (CanCallCoGetObjectContext)
            {
                IntPtr zero = IntPtr.Zero;
                IntPtr ppv = IntPtr.Zero;
                try
                {
                    WmiNetUtilsHelper.APTTYPE apttype;
                    if (CoGetObjectContext(ref IID_IComThreadingInfo, out zero) != 0)
                    {
                        return false;
                    }
                    if (WmiNetUtilsHelper.GetCurrentApartmentType_f(3, zero, out apttype) != 0)
                    {
                        return false;
                    }
                    if (apttype != WmiNetUtilsHelper.APTTYPE.APTTYPE_MTA)
                    {
                        return false;
                    }
                    if (Marshal.QueryInterface(zero, ref IID_IObjectContext, out ppv) == 0)
                    {
                        return false;
                    }
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        Marshal.Release(zero);
                    }
                    if (ppv != IntPtr.Zero)
                    {
                        Marshal.Release(ppv);
                    }
                }
            }
            return true;
        }

        private static bool IsWindows2000OrHigher()
        {
            OperatingSystem oSVersion = Environment.OSVersion;
            return ((oSVersion.Platform == PlatformID.Win32NT) && (oSVersion.Version >= new Version(5, 0)));
        }

        private static void WorkerThread()
        {
            MTARequest request;
        Label_0000:
            evtGo.WaitOne();
        Label_000B:
            request = null;
            lock (critSec)
            {
                if (reqList.Count > 0)
                {
                    request = (MTARequest) reqList[0];
                    reqList.RemoveAt(0);
                }
                else
                {
                    goto Label_0000;
                }
            }
            try
            {
                try
                {
                    request.createdObject = Activator.CreateInstance(request.typeToCreate);
                }
                catch (Exception exception)
                {
                    request.exception = exception;
                }
                goto Label_000B;
            }
            finally
            {
                request.evtDone.Set();
            }
        }

        private class MTARequest
        {
            public object createdObject;
            public AutoResetEvent evtDone = new AutoResetEvent(false);
            public Exception exception;
            public Type typeToCreate;

            public MTARequest(Type typeToCreate)
            {
                this.typeToCreate = typeToCreate;
            }
        }
    }
}


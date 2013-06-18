namespace System.Web.Services.Protocols
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Services;
    using System.Web.Services.Diagnostics;
    using System.Web.Services.Interop;

    internal class RemoteDebugger : INotifySource2
    {
        private static INotifyConnection2 connection;
        private static string debuggerHeader = "VsDebuggerCausalityData";
        private static bool getConnection = true;
        private static Guid IID_NotifyConnection2Guid = new Guid("1AF04045-6659-4aaa-9F4B-2741AC56224B");
        private static Guid IID_NotifyConnectionClassGuid = new Guid("12A5B9F0-7A1C-4fcb-8163-160A30F519B5");
        private const int INPROC_SERVER = 1;
        private NotifyFilter notifyFilter;
        private INotifySink2 notifySink;
        private static object s_InternalSyncObject;
        private UserThread userThread;

        [DebuggerStepThrough, DebuggerHidden]
        internal RemoteDebugger()
        {
        }

        private void Close()
        {
            if ((this.notifySink != null) && (connection != null))
            {
                lock (InternalSyncObject)
                {
                    if ((this.notifySink != null) && (connection != null))
                    {
                        TraceMethod caller = Tracing.On ? new TraceMethod(this, "Close", new object[0]) : null;
                        if (Tracing.On)
                        {
                            Tracing.Enter("RemoteDebugger", caller);
                        }
                        try
                        {
                            System.Web.Services.UnsafeNativeMethods.UnregisterNotifySource(connection, this);
                        }
                        catch (Exception exception)
                        {
                            if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                            {
                                throw;
                            }
                            if (Tracing.On)
                            {
                                Tracing.ExceptionCatch(TraceEventType.Warning, caller, exception);
                            }
                        }
                        if (Tracing.On)
                        {
                            Tracing.Exit("RemoteDebugger", caller);
                        }
                        this.notifySink = null;
                    }
                }
            }
        }

        private static void CloseSharedResources()
        {
            if (connection != null)
            {
                lock (InternalSyncObject)
                {
                    if (connection != null)
                    {
                        TraceMethod caller = Tracing.On ? new TraceMethod(typeof(RemoteDebugger), "CloseSharedResources", new object[0]) : null;
                        if (Tracing.On)
                        {
                            Tracing.Enter("RemoteDebugger", caller);
                        }
                        try
                        {
                            Marshal.ReleaseComObject(connection);
                        }
                        catch (Exception exception)
                        {
                            if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                            {
                                throw;
                            }
                            if (Tracing.On)
                            {
                                Tracing.ExceptionCatch(TraceEventType.Warning, typeof(RemoteDebugger), "CloseSharedResources", exception);
                            }
                        }
                        if (Tracing.On)
                        {
                            Tracing.Exit("RemoteDebugger", caller);
                        }
                        connection = null;
                    }
                }
            }
        }

        ~RemoteDebugger()
        {
            this.Close();
        }

        internal static bool IsClientCallOutEnabled()
        {
            bool flag = false;
            try
            {
                flag = (!System.ComponentModel.CompModSwitches.DisableRemoteDebugging.Enabled && Debugger.IsAttached) && (Connection != null);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Warning, typeof(RemoteDebugger), "IsClientCallOutEnabled", exception);
                }
            }
            return flag;
        }

        internal static bool IsServerCallInEnabled(ServerProtocol protocol, out string stringBuffer)
        {
            stringBuffer = null;
            bool flag = false;
            try
            {
                if (System.ComponentModel.CompModSwitches.DisableRemoteDebugging.Enabled)
                {
                    return false;
                }
                flag = protocol.Context.IsDebuggingEnabled && (Connection != null);
                if (flag)
                {
                    stringBuffer = protocol.Request.Headers[debuggerHeader];
                    flag = (stringBuffer != null) && (stringBuffer.Length > 0);
                }
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Warning, typeof(RemoteDebugger), "IsServerCallInEnabled", exception);
                }
                flag = false;
            }
            return flag;
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal void NotifyClientCallOut(WebRequest request)
        {
            try
            {
                if (this.NotifySink != null)
                {
                    IntPtr ptr;
                    int num = 0;
                    CallId callId = new CallId(null, 0, IntPtr.Zero, 0L, null, request.RequestUri.Host);
                    TraceMethod caller = Tracing.On ? new TraceMethod(this, "NotifyClientCallOut", new object[0]) : null;
                    if (Tracing.On)
                    {
                        Tracing.Enter("RemoteDebugger", caller);
                    }
                    System.Web.Services.UnsafeNativeMethods.OnSyncCallOut(this.NotifySink, callId, out ptr, ref num);
                    if (Tracing.On)
                    {
                        Tracing.Exit("RemoteDebugger", caller);
                    }
                    if (ptr != IntPtr.Zero)
                    {
                        byte[] destination = null;
                        try
                        {
                            destination = new byte[num];
                            Marshal.Copy(ptr, destination, 0, num);
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(ptr);
                        }
                        string str = Convert.ToBase64String(destination);
                        request.Headers.Add(debuggerHeader, str);
                    }
                }
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Warning, typeof(RemoteDebugger), "NotifyClientCallOut", exception);
                }
            }
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal void NotifyClientCallReturn(WebResponse response)
        {
            try
            {
                if (this.NotifySink == null)
                {
                    return;
                }
                byte[] buffer = new byte[0];
                if (response != null)
                {
                    string s = response.Headers[debuggerHeader];
                    if ((s != null) && (s.Length != 0))
                    {
                        buffer = Convert.FromBase64String(s);
                    }
                }
                CallId callId = new CallId(null, 0, IntPtr.Zero, 0L, null, null);
                TraceMethod caller = Tracing.On ? new TraceMethod(this, "NotifyClientCallReturn", new object[0]) : null;
                if (Tracing.On)
                {
                    Tracing.Enter("RemoteDebugger", caller);
                }
                System.Web.Services.UnsafeNativeMethods.OnSyncCallReturn(this.NotifySink, callId, buffer, buffer.Length);
                if (Tracing.On)
                {
                    Tracing.Exit("RemoteDebugger", caller);
                }
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Warning, typeof(RemoteDebugger), "NotifyClientCallReturn", exception);
                }
            }
            this.Close();
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal void NotifyServerCallEnter(ServerProtocol protocol, string stringBuffer)
        {
            try
            {
                if (this.NotifySink != null)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(protocol.Type.FullName);
                    builder.Append('.');
                    builder.Append(protocol.MethodInfo.Name);
                    builder.Append('(');
                    ParameterInfo[] parameters = protocol.MethodInfo.Parameters;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (i != 0)
                        {
                            builder.Append(',');
                        }
                        builder.Append(parameters[i].ParameterType.FullName);
                    }
                    builder.Append(')');
                    byte[] buffer = Convert.FromBase64String(stringBuffer);
                    CallId callId = new CallId(null, 0, IntPtr.Zero, 0L, builder.ToString(), null);
                    TraceMethod caller = Tracing.On ? new TraceMethod(this, "NotifyServerCallEnter", new object[0]) : null;
                    if (Tracing.On)
                    {
                        Tracing.Enter("RemoteDebugger", caller);
                    }
                    System.Web.Services.UnsafeNativeMethods.OnSyncCallEnter(this.NotifySink, callId, buffer, buffer.Length);
                    if (Tracing.On)
                    {
                        Tracing.Exit("RemoteDebugger", caller);
                    }
                }
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Warning, typeof(RemoteDebugger), "NotifyServerCallEnter", exception);
                }
            }
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal void NotifyServerCallExit(HttpResponse response)
        {
            try
            {
                IntPtr ptr;
                if (this.NotifySink == null)
                {
                    return;
                }
                int num = 0;
                CallId callId = new CallId(null, 0, IntPtr.Zero, 0L, null, null);
                TraceMethod caller = Tracing.On ? new TraceMethod(this, "NotifyServerCallExit", new object[0]) : null;
                if (Tracing.On)
                {
                    Tracing.Enter("RemoteDebugger", caller);
                }
                System.Web.Services.UnsafeNativeMethods.OnSyncCallExit(this.NotifySink, callId, out ptr, ref num);
                if (Tracing.On)
                {
                    Tracing.Exit("RemoteDebugger", caller);
                }
                if (ptr == IntPtr.Zero)
                {
                    return;
                }
                byte[] destination = null;
                try
                {
                    destination = new byte[num];
                    Marshal.Copy(ptr, destination, 0, num);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(ptr);
                }
                string str = Convert.ToBase64String(destination);
                response.AddHeader(debuggerHeader, str);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Warning, typeof(RemoteDebugger), "NotifyServerCallExit", exception);
                }
            }
            this.Close();
        }

        private static void OnAppDomainUnload(object sender, EventArgs args)
        {
            CloseSharedResources();
        }

        private static void OnProcessExit(object sender, EventArgs args)
        {
            CloseSharedResources();
        }

        void INotifySource2.SetNotifyFilter(NotifyFilter in_NotifyFilter, UserThread in_pUserThreadFilter)
        {
            this.notifyFilter = in_NotifyFilter;
            this.userThread = in_pUserThreadFilter;
        }

        private static INotifyConnection2 Connection
        {
            get
            {
                if ((connection == null) && getConnection)
                {
                    lock (InternalSyncObject)
                    {
                        if (connection == null)
                        {
                            object obj2;
                            AppDomain.CurrentDomain.DomainUnload += new EventHandler(RemoteDebugger.OnAppDomainUnload);
                            AppDomain.CurrentDomain.ProcessExit += new EventHandler(RemoteDebugger.OnProcessExit);
                            TraceMethod caller = Tracing.On ? new TraceMethod(typeof(RemoteDebugger), "get_Connection", new object[0]) : null;
                            if (Tracing.On)
                            {
                                Tracing.Enter("RemoteDebugger", caller);
                            }
                            int num = System.Web.Services.UnsafeNativeMethods.CoCreateInstance(ref IID_NotifyConnectionClassGuid, null, 1, ref IID_NotifyConnection2Guid, out obj2);
                            if (Tracing.On)
                            {
                                Tracing.Exit("RemoteDebugger", caller);
                            }
                            if (num >= 0)
                            {
                                connection = (INotifyConnection2) obj2;
                            }
                            else
                            {
                                connection = null;
                            }
                        }
                        getConnection = false;
                    }
                }
                return connection;
            }
        }

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }

        private INotifySink2 NotifySink
        {
            get
            {
                if ((this.notifySink == null) && (Connection != null))
                {
                    TraceMethod caller = Tracing.On ? new TraceMethod(this, "get_NotifySink", new object[0]) : null;
                    if (Tracing.On)
                    {
                        Tracing.Enter("RemoteDebugger", caller);
                    }
                    this.notifySink = System.Web.Services.UnsafeNativeMethods.RegisterNotifySource(Connection, this);
                    if (Tracing.On)
                    {
                        Tracing.Exit("RemoteDebugger", caller);
                    }
                }
                return this.notifySink;
            }
        }
    }
}


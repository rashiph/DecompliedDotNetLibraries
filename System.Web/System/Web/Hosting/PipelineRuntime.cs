namespace System.Web.Hosting
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Util;

    internal sealed class PipelineRuntime : MarshalByRefObject, IPipelineRuntime, IRegisteredObject
    {
        private static object _delegatelock = new object();
        private static DisposeFunctionDelegate _disposeDelegate = null;
        private static IntPtr _disposeDelegatePointer = IntPtr.Zero;
        private static ExecuteFunctionDelegate _executeDelegate = null;
        private static IntPtr _executeDelegatePointer = IntPtr.Zero;
        private static int _inIndicateCompletionCount;
        private static RoleFunctionDelegate _roleDelegate = null;
        private static IntPtr _roleDelegatePointer = IntPtr.Zero;
        internal const string InitExceptionModuleName = "AspNetInitializationExceptionModule";
        private static IntPtr s_ApplicationContext;
        private const string s_InitExceptionModulePrecondition = "";
        private static bool s_InitializationCompleted;
        private static int s_isThisAppDomainRemovedFromUnmanagedTable;
        private static bool s_StopProcessingCalled;
        private static string s_thisAppDomainsIsapiAppId;

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public PipelineRuntime()
        {
            HostingEnvironment.RegisterObject(this);
        }

        internal static void DisposeHandler(IntPtr managedHttpContext)
        {
            DisposeHandlerPrivate(UnwrapContext(managedHttpContext));
        }

        internal static void DisposeHandler(HttpContext context, IntPtr nativeRequestContext, RequestNotificationStatus status)
        {
            if (UnsafeIISMethods.MgdCanDisposeManagedContext(nativeRequestContext, status))
            {
                DisposeHandlerPrivate(context);
            }
        }

        private static void DisposeHandlerPrivate(HttpContext context)
        {
            try
            {
                context.FinishPipelineRequest();
                IIS7WorkerRequest workerRequest = context.WorkerRequest as IIS7WorkerRequest;
                if (workerRequest != null)
                {
                    workerRequest.Dispose();
                }
                PerfCounters.DecrementCounter(AppPerfCounter.REQUESTS_EXECUTING);
                context.DisposePrincipal();
            }
            finally
            {
                if (context != null)
                {
                    context.Unroot();
                }
                HttpRuntime.DecrementActivePipelineCount();
            }
        }

        private StringBuilder FormatExceptionMessage(Exception e, string[] strings)
        {
            StringBuilder builder = new StringBuilder(0x1000);
            if (strings != null)
            {
                for (int i = 0; i < strings.Length; i++)
                {
                    builder.Append(strings[i]);
                }
            }
            for (Exception exception = e; exception != null; exception = exception.InnerException)
            {
                if (exception == e)
                {
                    builder.Append("\r\n\r\nException: ");
                }
                else
                {
                    builder.Append("\r\n\r\nInnerException: ");
                }
                builder.Append(exception.GetType().FullName);
                builder.Append("\r\nMessage: ");
                builder.Append(exception.Message);
                builder.Append("\r\nStackTrace: ");
                builder.Append(exception.StackTrace);
            }
            return builder;
        }

        public IntPtr GetDisposeDelegate()
        {
            if (IntPtr.Zero == _disposeDelegatePointer)
            {
                lock (_delegatelock)
                {
                    if (IntPtr.Zero == _disposeDelegatePointer)
                    {
                        DisposeFunctionDelegate d = new DisposeFunctionDelegate(PipelineRuntime.DisposeHandler);
                        if (d != null)
                        {
                            IntPtr functionPointerForDelegate = Marshal.GetFunctionPointerForDelegate(d);
                            if (IntPtr.Zero != functionPointerForDelegate)
                            {
                                Thread.MemoryBarrier();
                                _disposeDelegate = d;
                                _disposeDelegatePointer = functionPointerForDelegate;
                            }
                        }
                    }
                }
            }
            return _disposeDelegatePointer;
        }

        public IntPtr GetExecuteDelegate()
        {
            if (IntPtr.Zero == _executeDelegatePointer)
            {
                lock (_delegatelock)
                {
                    if (IntPtr.Zero == _executeDelegatePointer)
                    {
                        ExecuteFunctionDelegate d = new ExecuteFunctionDelegate(PipelineRuntime.ProcessRequestNotification);
                        if (d != null)
                        {
                            IntPtr functionPointerForDelegate = Marshal.GetFunctionPointerForDelegate(d);
                            if (IntPtr.Zero != functionPointerForDelegate)
                            {
                                Thread.MemoryBarrier();
                                _executeDelegate = d;
                                _executeDelegatePointer = functionPointerForDelegate;
                            }
                        }
                    }
                }
            }
            return _executeDelegatePointer;
        }

        public IntPtr GetRoleDelegate()
        {
            if (IntPtr.Zero == _roleDelegatePointer)
            {
                lock (_delegatelock)
                {
                    if (IntPtr.Zero == _roleDelegatePointer)
                    {
                        RoleFunctionDelegate d = new RoleFunctionDelegate(PipelineRuntime.RoleHandler);
                        if (d != null)
                        {
                            IntPtr functionPointerForDelegate = Marshal.GetFunctionPointerForDelegate(d);
                            if (IntPtr.Zero != functionPointerForDelegate)
                            {
                                Thread.MemoryBarrier();
                                _roleDelegate = d;
                                _roleDelegatePointer = functionPointerForDelegate;
                            }
                        }
                    }
                }
            }
            return _roleDelegatePointer;
        }

        public void InitializeApplication(IntPtr appContext)
        {
            s_ApplicationContext = appContext;
            HttpApplication app = null;
            try
            {
                HttpRuntime.UseIntegratedPipeline = true;
                if (!HttpRuntime.HostingInitFailed)
                {
                    HttpWorkerRequest wr = new SimpleWorkerRequest("", "", new StringWriter(CultureInfo.InvariantCulture));
                    HttpContext context = new HttpContext(wr);
                    app = HttpApplicationFactory.GetPipelineApplicationInstance(appContext, context);
                }
            }
            catch (Exception exception)
            {
                if (HttpRuntime.InitializationException == null)
                {
                    HttpRuntime.InitializationException = exception;
                }
            }
            finally
            {
                s_InitializationCompleted = true;
                if (HttpRuntime.InitializationException != null)
                {
                    int errorCode = UnsafeIISMethods.MgdRegisterEventSubscription(appContext, "AspNetInitializationExceptionModule", RequestNotification.BeginRequest, 0, "AspNetInitializationExceptionModule", "", new IntPtr(-1), false);
                    if (errorCode < 0)
                    {
                        throw new COMException(System.Web.SR.GetString("Failed_Pipeline_Subscription", new object[] { "AspNetInitializationExceptionModule" }), errorCode);
                    }
                    errorCode = UnsafeIISMethods.MgdRegisterEventSubscription(appContext, "ManagedPipelineHandler", RequestNotification.ExecuteRequestHandler, 0, string.Empty, "managedHandler", new IntPtr(-1), false);
                    if (errorCode < 0)
                    {
                        throw new COMException(System.Web.SR.GetString("Failed_Pipeline_Subscription", new object[] { "ManagedPipelineHandler" }), errorCode);
                    }
                }
                if (app != null)
                {
                    HttpApplicationFactory.RecyclePipelineApplicationInstance(app);
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        private static void InitializeRequestContext(IntPtr nativeRequestContext, int flags, out IIS7WorkerRequest wr, out HttpContext context)
        {
            wr = null;
            context = null;
            try
            {
                bool etwProviderEnabled = (flags & 0x40) == 0x40;
                wr = IIS7WorkerRequest.CreateWorkerRequest(nativeRequestContext, etwProviderEnabled);
                context = new HttpContext(wr, false);
            }
            catch
            {
                UnsafeIISMethods.MgdSetBadRequestStatus(nativeRequestContext);
            }
        }

        internal static int ProcessRequestNotification(IntPtr managedHttpContext, IntPtr nativeRequestContext, IntPtr moduleData, int flags)
        {
            int num;
            try
            {
                num = ProcessRequestNotificationHelper(managedHttpContext, nativeRequestContext, moduleData, flags);
            }
            catch (Exception exception)
            {
                ApplicationManager.RecordFatalException(exception);
                throw;
            }
            return num;
        }

        internal static int ProcessRequestNotificationHelper(IntPtr managedHttpContext, IntPtr nativeRequestContext, IntPtr moduleData, int flags)
        {
            IIS7WorkerRequest wr = null;
            HttpContext context = null;
            RequestNotificationStatus notificationStatus = RequestNotificationStatus.Continue;
            if (managedHttpContext == IntPtr.Zero)
            {
                InitializeRequestContext(nativeRequestContext, flags, out wr, out context);
                if (context == null)
                {
                    return 2;
                }
                context.Root();
                UnsafeIISMethods.MgdSetManagedHttpContext(nativeRequestContext, context.ContextPtr);
                HttpRuntime.IncrementActivePipelineCount();
            }
            else
            {
                context = UnwrapContext(managedHttpContext);
                wr = context.WorkerRequest as IIS7WorkerRequest;
            }
            if ((context.InIndicateCompletion && (context.CurrentThread != Thread.CurrentThread)) && (0x20000000 != UnsafeIISMethods.MgdGetCurrentNotification(nativeRequestContext)))
            {
                while (context.InIndicateCompletion)
                {
                    Thread.Sleep(10);
                }
            }
            NotificationContext notificationContext = context.NotificationContext;
            bool locked = false;
            try
            {
                bool isReEntry = notificationContext != null;
                if (isReEntry)
                {
                    context.ApplicationInstance.AcquireNotifcationContextLock(ref locked);
                }
                context.NotificationContext = new NotificationContext(flags, isReEntry);
                notificationStatus = HttpRuntime.ProcessRequestNotification(wr, context);
            }
            finally
            {
                if (notificationStatus != RequestNotificationStatus.Pending)
                {
                    context.NotificationContext = notificationContext;
                }
                if (locked)
                {
                    context.ApplicationInstance.ReleaseNotifcationContextLock();
                }
            }
            if (notificationStatus != RequestNotificationStatus.Pending)
            {
                HttpApplication.ThreadContext indicateCompletionContext = context.IndicateCompletionContext;
                if (!context.InIndicateCompletion && (indicateCompletionContext != null))
                {
                    if (notificationStatus == RequestNotificationStatus.Continue)
                    {
                        try
                        {
                            context.InIndicateCompletion = true;
                            Interlocked.Increment(ref _inIndicateCompletionCount);
                            UnsafeIISMethods.MgdIndicateCompletion(nativeRequestContext, ref notificationStatus);
                            goto Label_01C2;
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _inIndicateCompletionCount);
                            if (!indicateCompletionContext.HasLeaveBeenCalled)
                            {
                                lock (indicateCompletionContext)
                                {
                                    if (!indicateCompletionContext.HasLeaveBeenCalled)
                                    {
                                        indicateCompletionContext.Leave();
                                        context.IndicateCompletionContext = null;
                                        context.InIndicateCompletion = false;
                                    }
                                }
                            }
                        }
                    }
                    if (!indicateCompletionContext.HasLeaveBeenCalled)
                    {
                        lock (indicateCompletionContext)
                        {
                            if (!indicateCompletionContext.HasLeaveBeenCalled)
                            {
                                indicateCompletionContext.Leave();
                                context.IndicateCompletionContext = null;
                                context.InIndicateCompletion = false;
                            }
                        }
                    }
                }
            }
        Label_01C2:
            return (int) notificationStatus;
        }

        internal static void RemoveThisAppDomainFromUnmanagedTable()
        {
            if (Interlocked.Exchange(ref s_isThisAppDomainRemovedFromUnmanagedTable, 1) == 0)
            {
                try
                {
                    if ((s_thisAppDomainsIsapiAppId != null) && (s_ApplicationContext != IntPtr.Zero))
                    {
                        UnsafeIISMethods.MgdAppDomainShutdown(s_ApplicationContext);
                    }
                    HttpRuntime.AddAppDomainTraceMessage(System.Web.SR.GetString("App_Domain_Restart"));
                }
                catch (Exception exception)
                {
                    if (ShouldRethrowException(exception))
                    {
                        throw;
                    }
                }
            }
        }

        internal static int RoleHandler(IntPtr pManagedPrincipal, IntPtr pszRole, int cchRole, bool disposing, out bool isInRole)
        {
            isInRole = false;
            GCHandle handle = GCHandle.FromIntPtr(pManagedPrincipal);
            IPrincipal target = (IPrincipal) handle.Target;
            if (target != null)
            {
                if (disposing)
                {
                    if (handle.IsAllocated)
                    {
                        handle.Free();
                    }
                    WindowsIdentity identity = target.Identity as WindowsIdentity;
                    if (identity != null)
                    {
                        identity.Dispose();
                    }
                    return 0;
                }
                try
                {
                    isInRole = target.IsInRole(StringUtil.StringFromWCharPtr(pszRole, cchRole));
                }
                catch (Exception exception)
                {
                    return Marshal.GetHRForException(exception);
                }
            }
            return 0;
        }

        internal void SetThisAppDomainsIsapiAppId(string appId)
        {
            s_thisAppDomainsIsapiAppId = appId;
        }

        internal static bool ShouldRethrowException(Exception ex)
        {
            return ((((ex is NullReferenceException) || (ex is AccessViolationException)) || ((ex is StackOverflowException) || (ex is OutOfMemoryException))) || (ex is ThreadAbortException));
        }

        public void StartProcessing()
        {
        }

        [EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
        public void StopProcessing()
        {
            if (UnsafeIISMethods.MgdHasConfigChanged() && !HostingEnvironment.ShutdownInitiated)
            {
                HttpRuntime.SetShutdownReason(ApplicationShutdownReason.ConfigurationChange, "IIS configuration change");
            }
            s_StopProcessingCalled = true;
            HostingEnvironment.InitiateShutdownWithoutDemand();
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            while (!s_InitializationCompleted && !s_StopProcessingCalled)
            {
                Thread.Sleep(250);
            }
            RemoveThisAppDomainFromUnmanagedTable();
            HostingEnvironment.UnregisterObject(this);
        }

        private static HttpContext UnwrapContext(IntPtr contextPtr)
        {
            return (HttpContext) GCHandle.FromIntPtr(contextPtr).Target;
        }

        internal static void WaitForRequestsToDrain()
        {
            while (!s_StopProcessingCalled || (_inIndicateCompletionCount > 0))
            {
                Thread.Sleep(250);
            }
        }

        internal bool HostingShutdownInitiated
        {
            get
            {
                return HostingEnvironment.ShutdownInitiated;
            }
        }
    }
}


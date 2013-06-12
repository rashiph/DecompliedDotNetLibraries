namespace System.Web.Hosting
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;

    public sealed class ProcessHost : MarshalByRefObject, IProcessHost, IAdphManager, IPphManager, IProcessHostIdleAndHealthCheck, IApplicationPreloadManager
    {
        private ApplicationManager _appManager;
        private IProcessHostSupportFunctions _functions;
        private Semaphore _preloadingThrottle;
        private IApplicationPreloadUtil _preloadUtil;
        private static object _processHostStaticLock = new object();
        private Hashtable _protocolHandlers = new Hashtable();
        private ProtocolsSection _protocolsConfig;
        private static ProcessHost _theProcessHost;

        private ProcessHost(IProcessHostSupportFunctions functions)
        {
            try
            {
                this._functions = functions;
                HostingEnvironment.SupportFunctions = functions;
                this._appManager = ApplicationManager.GetApplicationManager();
                int initialCount = (int) Misc.GetAspNetRegValue(null, "MaxPreloadConcurrency", 0);
                if (initialCount > 0)
                {
                    this._preloadingThrottle = new Semaphore(initialCount, initialCount);
                }
            }
            catch (Exception exception)
            {
                using (new ProcessImpersonationContext())
                {
                    Misc.ReportUnhandledException(exception, new string[] { System.Web.SR.GetString("Cant_Create_Process_Host") });
                }
                throw;
            }
        }

        private ISAPIApplicationHost CreateAppHost(string appId, string appPath)
        {
            if (string.IsNullOrEmpty(appPath))
            {
                string str;
                string str2;
                string str3;
                string str4;
                this._functions.GetApplicationProperties(appId, out str, out str2, out str3, out str4);
                if (!System.Web.Util.StringUtil.StringEndsWith(str2, '\\'))
                {
                    str2 = str2 + @"\";
                }
                appPath = str2;
            }
            return new ISAPIApplicationHost(appId, appPath, false, this._functions, null);
        }

        public void EnumerateAppDomains(out IAppDomainInfoEnum appDomainInfoEnum)
        {
            try
            {
                AppDomainInfo[] appDomainInfos = ApplicationManager.GetApplicationManager().GetAppDomainInfos();
                appDomainInfoEnum = new AppDomainInfoEnum(appDomainInfos);
            }
            catch (Exception exception)
            {
                using (new ProcessImpersonationContext())
                {
                    Misc.ReportUnhandledException(exception, new string[] { System.Web.SR.GetString("Failure_AppDomain_Enum") });
                }
                throw;
            }
        }

        private Type GetAppDomainProtocolHandlerType(string protocolId)
        {
            Type type = null;
            try
            {
                ProtocolElement element = this.ProtocolsConfig.Protocols[protocolId];
                if (element == null)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Unknown_protocol_id", new object[] { protocolId }));
                }
                type = this.ValidateAndGetType(element, element.AppDomainHandlerType, typeof(AppDomainProtocolHandler), "AppDomainHandlerType");
            }
            catch (Exception exception)
            {
                using (new ProcessImpersonationContext())
                {
                    Misc.ReportUnhandledException(exception, new string[] { System.Web.SR.GetString("Invalid_AppDomain_Prot_Type") });
                }
            }
            return type;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private void GetApplicationPreloadInfoWithAssert(string context, out bool enabled, out string startupObjType, out string[] parametersForStartupObj)
        {
            this._preloadUtil.GetApplicationPreloadInfo(context, out enabled, out startupObjType, out parametersForStartupObj);
        }

        private static Exception GetInnerMostException(Exception e)
        {
            if (e != null)
            {
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                }
                return e;
            }
            return null;
        }

        internal static ProcessHost GetProcessHost(IProcessHostSupportFunctions functions)
        {
            if (_theProcessHost == null)
            {
                lock (_processHostStaticLock)
                {
                    if (_theProcessHost == null)
                    {
                        _theProcessHost = new ProcessHost(functions);
                    }
                }
            }
            return _theProcessHost;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public bool IsIdle()
        {
            bool flag = false;
            try
            {
                flag = this._appManager.IsIdle();
            }
            catch (Exception exception)
            {
                using (new ProcessImpersonationContext())
                {
                    Misc.ReportUnhandledException(exception, new string[] { System.Web.SR.GetString("Failure_PMH_Idle") });
                }
                throw;
            }
            return flag;
        }

        public void Ping(IProcessPingCallback callback)
        {
            try
            {
                if (callback != null)
                {
                    this._appManager.Ping(callback);
                }
            }
            catch (Exception exception)
            {
                using (new ProcessImpersonationContext())
                {
                    Misc.ReportUnhandledException(exception, new string[] { System.Web.SR.GetString("Failure_PMH_Ping") });
                }
                throw;
            }
        }

        internal static void PreloadApplicationIfNotShuttingdown(string appId, LockableAppDomainContext ac)
        {
            WaitCallback callBack = null;
            if ((DefaultHost != null) && UnsafeIISMethods.MgdHasConfigChanged())
            {
                if (callBack == null)
                {
                    callBack = delegate (object o) {
                        lock (ac)
                        {
                            try
                            {
                                DefaultHost.PreloadApplicationIfRequired(appId, null, null, ac);
                            }
                            catch (Exception exception)
                            {
                                DefaultHost.ReportApplicationPreloadFailureWithAssert(ac.PreloadContext, -2147467259, Misc.FormatExceptionMessage(exception, new string[] { System.Web.SR.GetString("Failure_Preload_Application_Initialization") }));
                            }
                        }
                    };
                }
                ThreadPool.QueueUserWorkItem(callBack);
            }
        }

        internal void PreloadApplicationIfRequired(string appId, IApplicationHost appHostParameter, HostingEnvironmentParameters hostingParameters, LockableAppDomainContext ac)
        {
            if (((this._preloadUtil != null) && (ac.PreloadContext != null)) && (ac.HostEnv == null))
            {
                string str;
                string[] strArray;
                bool flag;
                this.GetApplicationPreloadInfoWithAssert(ac.PreloadContext, out flag, out str, out strArray);
                if (flag && !string.IsNullOrEmpty(str))
                {
                    if (this._preloadingThrottle != null)
                    {
                        this._preloadingThrottle.WaitOne();
                    }
                    try
                    {
                        IApplicationHost appHost = (appHostParameter == null) ? this.CreateAppHost(appId, null) : appHostParameter;
                        PreloadHost host2 = (PreloadHost) this._appManager.CreateObjectInternal(appId, typeof(PreloadHost), appHost, true, hostingParameters);
                        Exception initializationException = host2.InitializationException;
                        if (GetInnerMostException(initializationException) is IOException)
                        {
                            try
                            {
                                ac.RetryingPreload = true;
                                ac.HostEnv.InitiateShutdownInternal();
                            }
                            finally
                            {
                                ac.RetryingPreload = false;
                            }
                            appHost = (appHostParameter == null) ? this.CreateAppHost(appId, null) : appHostParameter;
                            host2 = (PreloadHost) this._appManager.CreateObjectInternal(appId, typeof(PreloadHost), appHost, true, hostingParameters);
                            initializationException = host2.InitializationException;
                        }
                        if (initializationException != null)
                        {
                            this.ReportApplicationPreloadFailureWithAssert(ac.PreloadContext, -2147467259, Misc.FormatExceptionMessage(initializationException, new string[] { System.Web.SR.GetString("Failure_Preload_Application_Initialization") }));
                            throw initializationException;
                        }
                        try
                        {
                            host2.CreateIProcessHostPreloadClientInstanceAndCallPreload(str, strArray);
                        }
                        catch (Exception exception2)
                        {
                            this.ReportApplicationPreloadFailureWithAssert(ac.PreloadContext, -2147467259, Misc.FormatExceptionMessage(exception2, new string[] { System.Web.SR.GetString("Failure_Calling_Preload_Provider") }).ToString());
                            throw;
                        }
                    }
                    finally
                    {
                        if (this._preloadingThrottle != null)
                        {
                            this._preloadingThrottle.Release();
                        }
                    }
                }
            }
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private void ReportApplicationPreloadFailureWithAssert(string context, int errorCode, string errorMessage)
        {
            this._preloadUtil.ReportApplicationPreloadFailure(context, errorCode, errorMessage);
        }

        public void SetApplicationPreloadState(string context, string appId, bool enabled)
        {
            if (string.IsNullOrEmpty(context))
            {
                throw System.Web.Util.ExceptionUtil.ParameterNullOrEmpty("context");
            }
            if (string.IsNullOrEmpty(appId))
            {
                throw System.Web.Util.ExceptionUtil.ParameterNullOrEmpty("appId");
            }
            if (enabled && (this._preloadUtil == null))
            {
                throw new ArgumentException(System.Web.SR.GetString("Invalid_Enabled_Preload_Parameter"), "enabled");
            }
            LockableAppDomainContext lockableAppDomainContext = this._appManager.GetLockableAppDomainContext(appId);
            lock (lockableAppDomainContext)
            {
                lockableAppDomainContext.PreloadContext = context;
                if (enabled)
                {
                    this.PreloadApplicationIfRequired(appId, null, null, lockableAppDomainContext);
                }
            }
        }

        public void SetApplicationPreloadUtil(IApplicationPreloadUtil applicationPreloadUtil)
        {
            if (this._preloadUtil != null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Failure_ApplicationPreloadUtil_Already_Set"));
            }
            this._preloadUtil = applicationPreloadUtil;
        }

        public void Shutdown()
        {
            try
            {
                ArrayList list = new ArrayList();
                lock (this)
                {
                    foreach (DictionaryEntry entry in this._protocolHandlers)
                    {
                        list.Add(entry.Value);
                    }
                    this._protocolHandlers = new Hashtable();
                }
                foreach (ProcessProtocolHandler handler in list)
                {
                    handler.StopProtocol(true);
                }
                this._appManager.ShutdownAll();
                while (Marshal.ReleaseComObject(this._functions) != 0)
                {
                }
            }
            catch (Exception exception)
            {
                using (new ProcessImpersonationContext())
                {
                    Misc.ReportUnhandledException(exception, new string[] { System.Web.SR.GetString("Failure_Shutdown_ProcessHost"), exception.ToString() });
                }
                throw;
            }
        }

        public void ShutdownApplication(string appId)
        {
            try
            {
                this._appManager.ShutdownApplication(appId);
            }
            catch (Exception exception)
            {
                using (new ProcessImpersonationContext())
                {
                    Misc.ReportUnhandledException(exception, new string[] { System.Web.SR.GetString("Failure_Stop_Integrated_App") });
                }
                throw;
            }
        }

        public void StartAppDomainProtocolListenerChannel(string appId, string protocolId, IListenerChannelCallback listenerChannelCallback)
        {
            try
            {
                if (appId == null)
                {
                    throw new ArgumentNullException("appId");
                }
                if (protocolId == null)
                {
                    throw new ArgumentNullException("protocolId");
                }
                ISAPIApplicationHost appHostParameter = this.CreateAppHost(appId, null);
                Type appDomainProtocolHandlerType = this.GetAppDomainProtocolHandlerType(protocolId);
                AppDomainProtocolHandler handler = null;
                LockableAppDomainContext lockableAppDomainContext = this._appManager.GetLockableAppDomainContext(appId);
                lock (lockableAppDomainContext)
                {
                    HostingEnvironmentParameters hostingParameters = new HostingEnvironmentParameters {
                        HostingFlags = HostingEnvironmentFlags.ThrowHostingInitErrors
                    };
                    this.PreloadApplicationIfRequired(appId, appHostParameter, hostingParameters, lockableAppDomainContext);
                    handler = (AppDomainProtocolHandler) this._appManager.CreateObjectInternal(appId, appDomainProtocolHandlerType, appHostParameter, false, hostingParameters);
                    ListenerAdapterDispatchShim shim = (ListenerAdapterDispatchShim) this._appManager.CreateObjectInternal(appId, typeof(ListenerAdapterDispatchShim), appHostParameter, false, hostingParameters);
                    if (shim == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("Failure_Create_Listener_Shim"));
                    }
                    shim.StartListenerChannel(handler, listenerChannelCallback);
                    ((IRegisteredObject) shim).Stop(true);
                }
            }
            catch (Exception exception)
            {
                using (new ProcessImpersonationContext())
                {
                    Misc.ReportUnhandledException(exception, new string[] { System.Web.SR.GetString("Failure_Start_AppDomain_Listener") });
                }
                throw;
            }
        }

        public void StartApplication(string appId, string appPath, out object runtimeInterface)
        {
            try
            {
                if (appId == null)
                {
                    throw new ArgumentNullException("appId");
                }
                if (appPath == null)
                {
                    throw new ArgumentNullException("appPath");
                }
                runtimeInterface = null;
                PipelineRuntime o = null;
                if (appPath[0] == '.')
                {
                    FileInfo info = new FileInfo(appPath);
                    appPath = info.FullName;
                }
                if (!System.Web.Util.StringUtil.StringEndsWith(appPath, '\\'))
                {
                    appPath = appPath + @"\";
                }
                IApplicationHost appHostParameter = this.CreateAppHost(appId, appPath);
                LockableAppDomainContext lockableAppDomainContext = this._appManager.GetLockableAppDomainContext(appId);
                lock (lockableAppDomainContext)
                {
                    this._appManager.RemoveFromTableIfRuntimeExists(appId, typeof(PipelineRuntime));
                    this.PreloadApplicationIfRequired(appId, appHostParameter, null, lockableAppDomainContext);
                    try
                    {
                        o = (PipelineRuntime) this._appManager.CreateObjectInternal(appId, typeof(PipelineRuntime), appHostParameter, true, null);
                    }
                    catch (AppDomainUnloadedException)
                    {
                    }
                    if (o != null)
                    {
                        o.SetThisAppDomainsIsapiAppId(appId);
                        o.StartProcessing();
                        runtimeInterface = new ObjectHandle(o);
                    }
                }
            }
            catch (Exception exception)
            {
                using (new ProcessImpersonationContext())
                {
                    Misc.ReportUnhandledException(exception, new string[] { System.Web.SR.GetString("Failure_Start_Integrated_App") });
                }
                throw;
            }
        }

        public void StartProcessProtocolListenerChannel(string protocolId, IListenerChannelCallback listenerChannelCallback)
        {
            try
            {
                if (protocolId == null)
                {
                    throw new ArgumentNullException("protocolId");
                }
                ProtocolElement element = this.ProtocolsConfig.Protocols[protocolId];
                if (element == null)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Unknown_protocol_id", new object[] { protocolId }));
                }
                ProcessProtocolHandler handler = null;
                Type type = null;
                type = this.ValidateAndGetType(element, element.ProcessHandlerType, typeof(ProcessProtocolHandler), "ProcessHandlerType");
                lock (this)
                {
                    handler = this._protocolHandlers[protocolId] as ProcessProtocolHandler;
                    if (handler == null)
                    {
                        handler = (ProcessProtocolHandler) Activator.CreateInstance(type);
                        this._protocolHandlers[protocolId] = handler;
                    }
                }
                if (handler != null)
                {
                    handler.StartListenerChannel(listenerChannelCallback, this);
                }
            }
            catch (Exception exception)
            {
                using (new ProcessImpersonationContext())
                {
                    Misc.ReportUnhandledException(exception, new string[] { System.Web.SR.GetString("Invalid_Process_Prot_Type") });
                }
                throw;
            }
        }

        public void StopAppDomainProtocol(string appId, string protocolId, bool immediate)
        {
            try
            {
                if (appId == null)
                {
                    throw new ArgumentNullException("appId");
                }
                if (protocolId == null)
                {
                    throw new ArgumentNullException("protocolId");
                }
                Type appDomainProtocolHandlerType = this.GetAppDomainProtocolHandlerType(protocolId);
                AppDomainProtocolHandler handler = null;
                LockableAppDomainContext lockableAppDomainContext = this._appManager.GetLockableAppDomainContext(appId);
                lock (lockableAppDomainContext)
                {
                    handler = (AppDomainProtocolHandler) this._appManager.GetObject(appId, appDomainProtocolHandlerType);
                }
                if (handler != null)
                {
                    handler.StopProtocol(immediate);
                }
            }
            catch (Exception exception)
            {
                using (new ProcessImpersonationContext())
                {
                    Misc.ReportUnhandledException(exception, new string[] { System.Web.SR.GetString("Failure_Stop_AppDomain_Protocol") });
                }
                throw;
            }
        }

        public void StopAppDomainProtocolListenerChannel(string appId, string protocolId, int listenerChannelId, bool immediate)
        {
            try
            {
                if (appId == null)
                {
                    throw new ArgumentNullException("appId");
                }
                if (protocolId == null)
                {
                    throw new ArgumentNullException("protocolId");
                }
                Type appDomainProtocolHandlerType = this.GetAppDomainProtocolHandlerType(protocolId);
                AppDomainProtocolHandler handler = null;
                LockableAppDomainContext lockableAppDomainContext = this._appManager.GetLockableAppDomainContext(appId);
                lock (lockableAppDomainContext)
                {
                    handler = (AppDomainProtocolHandler) this._appManager.GetObject(appId, appDomainProtocolHandlerType);
                }
                if (handler != null)
                {
                    handler.StopListenerChannel(listenerChannelId, immediate);
                }
            }
            catch (Exception exception)
            {
                using (new ProcessImpersonationContext())
                {
                    Misc.ReportUnhandledException(exception, new string[] { System.Web.SR.GetString("Failure_Stop_AppDomain_Listener") });
                }
                throw;
            }
        }

        public void StopProcessProtocol(string protocolId, bool immediate)
        {
            try
            {
                if (protocolId == null)
                {
                    throw new ArgumentNullException("protocolId");
                }
                ProcessProtocolHandler handler = null;
                lock (this)
                {
                    handler = this._protocolHandlers[protocolId] as ProcessProtocolHandler;
                    if (handler != null)
                    {
                        this._protocolHandlers.Remove(protocolId);
                    }
                }
                if (handler != null)
                {
                    handler.StopProtocol(immediate);
                }
            }
            catch (Exception exception)
            {
                using (new ProcessImpersonationContext())
                {
                    Misc.ReportUnhandledException(exception, new string[] { System.Web.SR.GetString("Failure_Stop_Process_Prot") });
                }
                throw;
            }
        }

        public void StopProcessProtocolListenerChannel(string protocolId, int listenerChannelId, bool immediate)
        {
            try
            {
                if (protocolId == null)
                {
                    throw new ArgumentNullException("protocolId");
                }
                ProcessProtocolHandler handler = null;
                lock (this)
                {
                    handler = this._protocolHandlers[protocolId] as ProcessProtocolHandler;
                }
                if (handler != null)
                {
                    handler.StopListenerChannel(listenerChannelId, immediate);
                }
            }
            catch (Exception exception)
            {
                using (new ProcessImpersonationContext())
                {
                    Misc.ReportUnhandledException(exception, new string[] { System.Web.SR.GetString("Failure_Stop_Listener_Channel") });
                }
                throw;
            }
        }

        private Type ValidateAndGetType(ProtocolElement element, string typeName, Type assignableType, string elementPropertyName)
        {
            Type type;
            try
            {
                type = Type.GetType(typeName, true);
            }
            catch (Exception exception)
            {
                PropertyInformation information = null;
                string filename = string.Empty;
                int line = 0;
                if ((element != null) && (element.ElementInformation != null))
                {
                    information = element.ElementInformation.Properties[elementPropertyName];
                    if (information != null)
                    {
                        filename = information.Source;
                        line = information.LineNumber;
                    }
                }
                throw new ConfigurationErrorsException(exception.Message, exception, filename, line);
            }
            ConfigUtil.CheckAssignableType(assignableType, type, element, elementPropertyName);
            return type;
        }

        internal static ProcessHost DefaultHost
        {
            get
            {
                return _theProcessHost;
            }
        }

        private ProtocolsSection ProtocolsConfig
        {
            get
            {
                if (this._protocolsConfig == null)
                {
                    lock (this)
                    {
                        if (this._protocolsConfig == null)
                        {
                            if (HttpConfigurationSystem.IsSet)
                            {
                                this._protocolsConfig = RuntimeConfig.GetRootWebConfig().Protocols;
                            }
                            else
                            {
                                System.Configuration.Configuration configuration = WebConfigurationManager.OpenWebConfiguration(null);
                                this._protocolsConfig = (ProtocolsSection) configuration.GetSection("system.web/protocols");
                            }
                        }
                    }
                }
                return this._protocolsConfig;
            }
        }

        internal IProcessHostSupportFunctions SupportFunctions
        {
            get
            {
                return this._functions;
            }
        }
    }
}


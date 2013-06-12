namespace System.Web
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web.Caching;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;

    public sealed class HttpRuntime
    {
        private int _activeRequestCount;
        private bool _apartmentThreading;
        private string _appDomainAppId;
        private string _appDomainAppPath;
        private VirtualPath _appDomainAppVPath;
        private string _appDomainId;
        private System.Threading.Timer _appDomainShutdownTimer;
        private WaitCallback _appDomainUnloadallback;
        private byte[] _appOfflineMessage;
        private HttpWorkerRequest.EndOfSendNotification _asyncEndOfSendCallback;
        private bool _beforeFirstRequest = true;
        private System.Web.Caching.CacheInternal _cacheInternal;
        private System.Web.Caching.Cache _cachePublic;
        private string _clientScriptPhysicalPath;
        private string _clientScriptVirtualPath;
        private string _codegenDir;
        private bool _configInited;
        private bool _debuggingEnabled;
        private static string _DefaultPhysicalPathOnMapPathFailure;
        private bool _disableProcessRequestInApplicationTrust;
        private bool _enableHeaderChecking;
        private System.Web.FileChangesMonitor _fcm;
        private bool _firstRequestCompleted;
        private DateTime _firstRequestStartTime;
        private bool _fusionInited;
        private AsyncCallback _handlerCompletionCallback;
        private bool _hostingInitFailed;
        private string _hostSecurityPolicyResolverType;
        private Exception _initializationError;
        private bool _isLegacyCas;
        private bool _isOnUNCShare;
        private DateTime _lastShutdownAttemptTime;
        private System.Security.NamedPermissionSet _namedPermissionSet;
        private System.Security.Policy.PolicyLevel _policyLevel;
        private bool _processRequestInApplicationTrust;
        private Profiler _profiler;
        private AsyncCallback _requestNotificationCompletionCallback;
        private RequestQueue _requestQueue;
        private bool _shutdownInProgress;
        private string _shutDownMessage;
        private ApplicationShutdownReason _shutdownReason;
        private string _shutDownStack;
        private bool _shutdownWebEventRaised;
        private string _tempDir;
        private static HttpRuntime _theRuntime;
        private System.Web.RequestTimeoutManager _timeoutManager;
        private string _trustLevel;
        private static bool _useIntegratedPipeline;
        private bool _userForcedShutdown;
        private string _wpUserId;
        private const string AppOfflineFileName = "App_Offline.htm";
        private const string AspNetClientFilesParentVirtualPath = "/aspnet_client/system_web/";
        private const string AspNetClientFilesSubDirectory = "asp.netclientfiles";
        internal const string BinDirectoryName = "bin";
        internal const string BrowsersDirectoryName = "App_Browsers";
        internal const string CodeDirectoryName = "App_Code";
        internal const string codegenDirName = "Temporary ASP.NET Files";
        internal const string DataDirectoryName = "App_Data";
        private static string DirectorySeparatorString = new string(Path.DirectorySeparatorChar, 1);
        private static string DoubleDirectorySeparatorString = new string(Path.DirectorySeparatorChar, 2);
        internal const string GlobalThemesDirectoryName = "Themes";
        internal const string LocalResourcesDirectoryName = "App_LocalResources";
        private const long MaxAppOfflineFileLength = 0x100000L;
        internal const string ResourcesDirectoryName = "App_GlobalResources";
        internal static byte[] s_autogenKeys = new byte[0x400];
        private static Hashtable s_factoryCache;
        private static FactoryGenerator s_factoryGenerator;
        private static object s_factoryLock = new object();
        private static bool s_initialized = false;
        private static bool s_initializedFactory;
        private static string s_installDirectory;
        private static char[] s_InvalidPhysicalPathChars = new char[] { '/', '?', '*', '<', '>', '|', '"' };
        private static bool s_isEngineLoaded = false;
        internal const string ThemesDirectoryName = "App_Themes";
        internal const string WebRefDirectoryName = "App_WebReferences";

        internal static  event BuildManagerHostUnloadEventHandler AppDomainShutdown;

        static HttpRuntime()
        {
            AddAppDomainTraceMessage("*HttpRuntime::cctor");
            StaticInit();
            _theRuntime = new HttpRuntime();
            _theRuntime.Init();
            AddAppDomainTraceMessage("HttpRuntime::cctor*");
        }

        internal static void AddAppDomainTraceMessage(string message)
        {
            AppDomain domain = Thread.GetDomain();
            string data = domain.GetData("ASP.NET Domain Trace") as string;
            domain.SetData("ASP.NET Domain Trace", (data != null) ? (data + " ... " + message) : message);
        }

        private void AppDomainShutdownTimerCallback(object state)
        {
            try
            {
                this.DisposeAppDomainShutdownTimer();
                ShutdownAppDomain(ApplicationShutdownReason.InitializationError, "Initialization Error");
            }
            catch
            {
            }
        }

        private static void CalculateWaitTimeAndUpdatePerfCounter(HttpWorkerRequest wr)
        {
            DateTime startTime = wr.GetStartTime();
            long num = DateTime.UtcNow.Subtract(startTime).Ticks / 0x2710L;
            if (num > 0x7fffffffL)
            {
                num = 0x7fffffffL;
            }
            PerfCounters.SetGlobalCounter(GlobalPerfCounter.REQUEST_WAIT_TIME, (int) num);
            PerfCounters.SetCounter(AppPerfCounter.APP_REQUEST_WAIT_TIME, (int) num);
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
        private void CheckAccessToTempDirectory()
        {
            if (HostingEnvironment.HasHostingIdentity)
            {
                using (new ApplicationImpersonationContext())
                {
                    if (!Util.HasWriteAccessToDirectory(this._tempDir))
                    {
                        throw new HttpException(System.Web.SR.GetString("No_codegen_access", new object[] { Util.GetCurrentAccountName(), this._tempDir }));
                    }
                }
            }
        }

        internal static void CheckApplicationEnabled()
        {
            string alias = Path.Combine(_theRuntime._appDomainAppPath, "App_Offline.htm");
            bool flag = false;
            _theRuntime._fcm.StartMonitoringFile(alias, new FileChangeEventHandler(_theRuntime.OnAppOfflineFileChange));
            try
            {
                if (System.IO.File.Exists(alias))
                {
                    using (FileStream stream = new FileStream(alias, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        if (stream.Length <= 0x100000L)
                        {
                            int length = (int) stream.Length;
                            if (length > 0)
                            {
                                byte[] buffer = new byte[length];
                                if (stream.Read(buffer, 0, length) == length)
                                {
                                    _theRuntime._appOfflineMessage = buffer;
                                    flag = true;
                                }
                            }
                            else
                            {
                                flag = true;
                                _theRuntime._appOfflineMessage = new byte[0];
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            if (flag)
            {
                throw new HttpException(0x1f7, string.Empty);
            }
            if (!RuntimeConfig.GetAppConfig().HttpRuntime.Enable)
            {
                throw new HttpException(0x194, string.Empty);
            }
        }

        internal static void CheckAspNetHostingPermission(AspNetHostingPermissionLevel level, string errorMessageId)
        {
            if (!HasAspNetHostingPermission(level))
            {
                throw new HttpException(System.Web.SR.GetString(errorMessageId));
            }
        }

        internal static void CheckFilePermission(string path)
        {
            CheckFilePermission(path, false);
        }

        internal static void CheckFilePermission(string path, bool writePermissions)
        {
            if (!HasFilePermission(path, writePermissions))
            {
                throw new HttpException(System.Web.SR.GetString("Access_denied_to_path", new object[] { GetSafePath(path) }));
            }
        }

        internal static void CheckVirtualFilePermission(string virtualPath)
        {
            CheckFilePermission(HostingEnvironment.MapPath(virtualPath));
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public static void Close()
        {
            if (_theRuntime.InitiateShutdownOnce())
            {
                SetShutdownReason(ApplicationShutdownReason.HttpRuntimeClose, "HttpRuntime.Close is called");
                if (HostingEnvironment.IsHosted)
                {
                    HostingEnvironment.InitiateShutdownWithoutDemand();
                }
                else
                {
                    _theRuntime.Dispose();
                }
            }
        }

        internal static void CoalesceNotifications()
        {
            int waitChangeNotification = 0;
            int maxWaitChangeNotification = 0;
            try
            {
                HttpRuntimeSection httpRuntime = RuntimeConfig.GetAppLKGConfig().HttpRuntime;
                if (httpRuntime != null)
                {
                    waitChangeNotification = httpRuntime.WaitChangeNotification;
                    maxWaitChangeNotification = httpRuntime.MaxWaitChangeNotification;
                }
            }
            catch
            {
            }
            if ((waitChangeNotification != 0) && (maxWaitChangeNotification != 0))
            {
                DateTime time = DateTime.UtcNow.AddSeconds((double) maxWaitChangeNotification);
                try
                {
                    while (DateTime.UtcNow < time)
                    {
                        if (DateTime.UtcNow > _theRuntime.LastShutdownAttemptTime.AddSeconds((double) waitChangeNotification))
                        {
                            return;
                        }
                        Thread.Sleep(250);
                    }
                }
                catch
                {
                }
            }
        }

        private void CreateCache()
        {
            lock (this)
            {
                if (this._cacheInternal == null)
                {
                    this._cacheInternal = System.Web.Caching.CacheInternal.Create();
                }
            }
        }

        internal static object CreateNonPublicInstance(Type type)
        {
            return CreateNonPublicInstance(type, null);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal static object CreateNonPublicInstance(Type type, object[] args)
        {
            return Activator.CreateInstance(type, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, args, null);
        }

        private static System.Security.Policy.PolicyLevel CreatePolicyLevel(string configFile, string appDir, string binDir, string strOriginUrl, out bool foundGacToken)
        {
            FileStream stream = new FileStream(configFile, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string str = reader.ReadToEnd();
            reader.Close();
            appDir = System.Web.Util.FileUtil.RemoveTrailingDirectoryBackSlash(appDir);
            binDir = System.Web.Util.FileUtil.RemoveTrailingDirectoryBackSlash(binDir);
            str = str.Replace("$AppDir$", appDir).Replace("$AppDirUrl$", MakeFileUrl(appDir)).Replace("$CodeGen$", MakeFileUrl(binDir));
            if (strOriginUrl == null)
            {
                strOriginUrl = string.Empty;
            }
            str = str.Replace("$OriginHost$", strOriginUrl);
            if (str.IndexOf("$Gac$", StringComparison.Ordinal) != -1)
            {
                string gacLocation = GetGacLocation();
                if (gacLocation != null)
                {
                    gacLocation = MakeFileUrl(gacLocation);
                }
                if (gacLocation == null)
                {
                    gacLocation = string.Empty;
                }
                str = str.Replace("$Gac$", gacLocation);
                foundGacToken = true;
            }
            else
            {
                foundGacToken = false;
            }
            return SecurityManager.LoadPolicyLevelFromString(str, PolicyLevelType.AppDomain);
        }

        internal static object CreatePublicInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }

        internal static object CreatePublicInstance(Type type, object[] args)
        {
            if (args == null)
            {
                return Activator.CreateInstance(type);
            }
            return Activator.CreateInstance(type, args);
        }

        internal static void DecrementActivePipelineCount()
        {
            HostingEnvironment.DecrementBusyCount();
            Interlocked.Decrement(ref _theRuntime._activeRequestCount);
        }

        private void Dispose()
        {
            int totalSeconds = 90;
            HttpRuntimeSection httpRuntime = RuntimeConfig.GetAppLKGConfig().HttpRuntime;
            if (httpRuntime != null)
            {
                totalSeconds = (int) httpRuntime.ShutdownTimeout.TotalSeconds;
            }
            this.WaitForRequestsToFinish(totalSeconds * 0x3e8);
            if (this._requestQueue != null)
            {
                this._requestQueue.Drain();
            }
            this.WaitForRequestsToFinish((totalSeconds * 0x3e8) / 6);
            ISAPIWorkerRequestInProcForIIS6.WaitForPendingAsyncIo();
            if (!UseIntegratedPipeline)
            {
                while (this._activeRequestCount != 0)
                {
                    Thread.Sleep(250);
                }
            }
            else
            {
                PipelineRuntime.WaitForRequestsToDrain();
            }
            this.DisposeAppDomainShutdownTimer();
            this._timeoutManager.Stop();
            AppDomainResourcePerfCounters.Stop();
            ISAPIWorkerRequestInProcForIIS6.WaitForPendingAsyncIo();
            SqlCacheDependencyManager.Dispose((totalSeconds * 0x3e8) / 2);
            if (this._cacheInternal != null)
            {
                this._cacheInternal.Dispose();
            }
            HttpApplicationFactory.EndApplication();
            this._fcm.Stop();
            HealthMonitoringManager.Shutdown();
        }

        private void DisposeAppDomainShutdownTimer()
        {
            System.Threading.Timer comparand = this._appDomainShutdownTimer;
            if ((comparand != null) && (Interlocked.CompareExchange<System.Threading.Timer>(ref this._appDomainShutdownTimer, null, comparand) == comparand))
            {
                comparand.Dispose();
            }
        }

        private void EndOfSendCallback(HttpWorkerRequest wr, object arg)
        {
            HttpContext context = (HttpContext) arg;
            context.Request.Dispose();
            context.Response.Dispose();
        }

        private void EnsureAccessToApplicationDirectory()
        {
            if (!System.Web.Util.FileUtil.DirectoryAccessible(this._appDomainAppPath))
            {
                if (this._appDomainAppPath.IndexOf('?') >= 0)
                {
                    throw new HttpException(System.Web.SR.GetString("Access_denied_to_unicode_app_dir", new object[] { this._appDomainAppPath }));
                }
                throw new HttpException(System.Web.SR.GetString("Access_denied_to_app_dir", new object[] { this._appDomainAppPath }));
            }
        }

        private void EnsureFirstRequestInit(HttpContext context)
        {
            if (this._beforeFirstRequest)
            {
                lock (this)
                {
                    if (this._beforeFirstRequest)
                    {
                        this._firstRequestStartTime = DateTime.UtcNow;
                        this.FirstRequestInit(context);
                        this._beforeFirstRequest = false;
                    }
                }
            }
        }

        internal static void FailIfNoAPTCABit(Type t, XmlNode node)
        {
            if (!IsTypeAllowedInConfig(t))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Type_from_untrusted_assembly", new object[] { t.FullName }), node);
            }
        }

        internal static void FailIfNoAPTCABit(Type t, ElementInformation elemInfo, string propertyName)
        {
            if (!IsTypeAllowedInConfig(t))
            {
                if (elemInfo != null)
                {
                    PropertyInformation information = elemInfo.Properties[propertyName];
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Type_from_untrusted_assembly", new object[] { t.FullName }), information.Source, information.LineNumber);
                }
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Type_from_untrusted_assembly", new object[] { t.FullName }));
            }
        }

        internal static object FastCreatePublicInstance(Type type)
        {
            if (!type.Assembly.GlobalAssemblyCache)
            {
                return CreatePublicInstance(type);
            }
            if (!s_initializedFactory)
            {
                lock (s_factoryLock)
                {
                    if (!s_initializedFactory)
                    {
                        s_factoryGenerator = new FactoryGenerator();
                        s_factoryCache = Hashtable.Synchronized(new Hashtable());
                        s_initializedFactory = true;
                    }
                }
            }
            IWebObjectFactory factory = (IWebObjectFactory) s_factoryCache[type];
            if (factory == null)
            {
                factory = s_factoryGenerator.CreateFactory(type);
                s_factoryCache[type] = factory;
            }
            return factory.CreateInstance();
        }

        internal static void FinishPipelineRequest(HttpContext context)
        {
            _theRuntime._firstRequestCompleted = true;
            context.Request.Dispose();
            context.Response.Dispose();
            HttpApplication applicationInstance = context.ApplicationInstance;
            if (applicationInstance != null)
            {
                HttpApplication.ThreadContext indicateCompletionContext = context.IndicateCompletionContext;
                if ((indicateCompletionContext != null) && !indicateCompletionContext.HasLeaveBeenCalled)
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
                applicationInstance.ReleaseAppInstance();
            }
            SetExecutionTimePerformanceCounter(context);
            UpdatePerfCounters(context.Response.StatusCode);
            if (EtwTrace.IsTraceEnabled(5, 1))
            {
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_END_HANDLER, context.WorkerRequest);
            }
            if (HostingInitFailed)
            {
                ShutdownAppDomain(ApplicationShutdownReason.HostingEnvironment, "HostingInit error");
            }
        }

        private void FinishRequest(HttpWorkerRequest wr, HttpContext context, Exception e)
        {
            HttpResponse response = context.Response;
            if (EtwTrace.IsTraceEnabled(5, 1))
            {
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_END_HANDLER, context.WorkerRequest);
            }
            SetExecutionTimePerformanceCounter(context);
            if (e == null)
            {
                ClientImpersonationContext context2 = new ClientImpersonationContext(context, false);
                try
                {
                    response.FinalFlushAtTheEndOfRequestProcessing();
                }
                catch (Exception exception)
                {
                    e = exception;
                }
                finally
                {
                    if (context2 != null)
                    {
                        ((IDisposable) context2).Dispose();
                    }
                }
            }
            if (e != null)
            {
                using (new DisposableHttpContextWrapper(context))
                {
                    context.DisableCustomHttpEncoder = true;
                    if (this._appOfflineMessage != null)
                    {
                        try
                        {
                            ReportAppOfflineErrorMessage(response, this._appOfflineMessage);
                            response.FinalFlushAtTheEndOfRequestProcessing();
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        using (new ApplicationImpersonationContext())
                        {
                            try
                            {
                                try
                                {
                                    response.ReportRuntimeError(e, true, false);
                                }
                                catch (Exception exception2)
                                {
                                    response.ReportRuntimeError(exception2, false, false);
                                }
                                response.FinalFlushAtTheEndOfRequestProcessing();
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            this._firstRequestCompleted = true;
            if (this._hostingInitFailed)
            {
                ShutdownAppDomain(ApplicationShutdownReason.HostingEnvironment, "HostingInit error");
            }
            int statusCode = response.StatusCode;
            UpdatePerfCounters(statusCode);
            context.FinishRequestForCachedPathData(statusCode);
            try
            {
                wr.EndOfRequest();
            }
            catch (Exception exception3)
            {
                WebBaseEvent.RaiseRuntimeError(exception3, this);
            }
            HostingEnvironment.DecrementBusyCount();
            Interlocked.Decrement(ref this._activeRequestCount);
            if (this._requestQueue != null)
            {
                this._requestQueue.ScheduleMoreWorkIfNeeded();
            }
        }

        private void FinishRequestNotification(IIS7WorkerRequest wr, HttpContext context, ref RequestNotificationStatus status)
        {
            HttpApplication applicationInstance = context.ApplicationInstance;
            if (context.NotificationContext.RequestCompleted)
            {
                status = RequestNotificationStatus.FinishRequest;
            }
            context.ReportRuntimeErrorIfExists(ref status);
            if ((status == RequestNotificationStatus.FinishRequest) && ((context.CurrentNotification == RequestNotification.LogRequest) || (context.CurrentNotification == RequestNotification.EndRequest)))
            {
                status = RequestNotificationStatus.Continue;
            }
            IntPtr requestContext = wr.RequestContext;
            bool sendHeaders = UnsafeIISMethods.MgdIsLastNotification(requestContext, status);
            try
            {
                context.Response.UpdateNativeResponse(sendHeaders);
            }
            catch (Exception exception)
            {
                wr.UnlockCachedResponseBytes();
                context.AddError(exception);
                context.ReportRuntimeErrorIfExists(ref status);
                context.Response.UpdateNativeResponse(sendHeaders);
            }
            if (sendHeaders)
            {
                context.FinishPipelineRequest();
            }
            if (status != RequestNotificationStatus.Pending)
            {
                PipelineRuntime.DisposeHandler(context, requestContext, status);
            }
        }

        private void FirstRequestInit(HttpContext context)
        {
            Exception exception = null;
            if ((InitializationException == null) && (this._appDomainId != null))
            {
                try
                {
                    using (new ApplicationImpersonationContext())
                    {
                        CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                        CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
                        try
                        {
                            InitHttpConfiguration();
                            CheckApplicationEnabled();
                            this.CheckAccessToTempDirectory();
                            this.InitializeHealthMonitoring();
                            this.InitRequestQueue();
                            this.InitTrace(context);
                            HealthMonitoringManager.StartHealthMonitoringHeartbeat();
                            RestrictIISFolders(context);
                            this.PreloadAssembliesFromBin();
                            this.InitHeaderEncoding();
                            HttpEncoder.InitializeOnFirstRequest();
                            RequestValidator.InitializeOnFirstRequest();
                            if (context.WorkerRequest is ISAPIWorkerRequestOutOfProc)
                            {
                                ProcessModelSection processModel = RuntimeConfig.GetMachineConfig().ProcessModel;
                            }
                        }
                        finally
                        {
                            Thread.CurrentThread.CurrentUICulture = currentUICulture;
                            SetCurrentThreadCultureWithAssert(currentCulture);
                        }
                    }
                }
                catch (ConfigurationException exception2)
                {
                    exception = exception2;
                }
                catch (Exception exception3)
                {
                    exception = new HttpException(System.Web.SR.GetString("XSP_init_error", new object[] { exception3.Message }), exception3);
                }
            }
            if (InitializationException != null)
            {
                throw new HttpException(InitializationException.Message, InitializationException);
            }
            if (exception != null)
            {
                InitializationException = exception;
                throw exception;
            }
            AddAppDomainTraceMessage("FirstRequestInit");
        }

        internal static void ForceStaticInit()
        {
        }

        private static string GetAppDomainString(string key)
        {
            return (Thread.GetDomain().GetData(key) as string);
        }

        private static string GetCurrentUserName()
        {
            try
            {
                return WindowsIdentity.GetCurrent().Name;
            }
            catch
            {
                return null;
            }
        }

        internal static string GetGacLocation()
        {
            StringBuilder pwzCachePath = new StringBuilder(0x106);
            int pcchPath = 260;
            if (System.Web.UnsafeNativeMethods.GetCachePath(2, pwzCachePath, ref pcchPath) < 0)
            {
                throw new HttpException(System.Web.SR.GetString("GetGacLocaltion_failed"));
            }
            return pwzCachePath.ToString();
        }

        private void GetInitConfigSections(out CacheSection cacheSection, out TrustSection trustSection, out SecurityPolicySection securityPolicySection, out CompilationSection compilationSection, out HostingEnvironmentSection hostingEnvironmentSection, out Exception initException)
        {
            cacheSection = null;
            trustSection = null;
            securityPolicySection = null;
            compilationSection = null;
            hostingEnvironmentSection = null;
            initException = null;
            RuntimeConfig appLKGConfig = RuntimeConfig.GetAppLKGConfig();
            RuntimeConfig appConfig = null;
            try
            {
                appConfig = RuntimeConfig.GetAppConfig();
            }
            catch (Exception exception)
            {
                initException = exception;
            }
            if (appConfig != null)
            {
                try
                {
                    cacheSection = appConfig.Cache;
                }
                catch (Exception exception2)
                {
                    if (initException == null)
                    {
                        initException = exception2;
                    }
                }
            }
            if (cacheSection == null)
            {
                cacheSection = appLKGConfig.Cache;
            }
            if (appConfig != null)
            {
                try
                {
                    trustSection = appConfig.Trust;
                }
                catch (Exception exception3)
                {
                    if (initException == null)
                    {
                        initException = exception3;
                    }
                }
            }
            if (trustSection == null)
            {
                trustSection = appLKGConfig.Trust;
            }
            if (appConfig != null)
            {
                try
                {
                    securityPolicySection = appConfig.SecurityPolicy;
                }
                catch (Exception exception4)
                {
                    if (initException == null)
                    {
                        initException = exception4;
                    }
                }
            }
            if (securityPolicySection == null)
            {
                securityPolicySection = appLKGConfig.SecurityPolicy;
            }
            if (appConfig != null)
            {
                try
                {
                    compilationSection = appConfig.Compilation;
                }
                catch (Exception exception5)
                {
                    if (initException == null)
                    {
                        initException = exception5;
                    }
                }
            }
            if (compilationSection == null)
            {
                compilationSection = appLKGConfig.Compilation;
            }
            if (appConfig != null)
            {
                try
                {
                    hostingEnvironmentSection = appConfig.HostingEnvironment;
                }
                catch (Exception exception6)
                {
                    if (initException == null)
                    {
                        initException = exception6;
                    }
                }
            }
            if (hostingEnvironmentSection == null)
            {
                hostingEnvironmentSection = appLKGConfig.HostingEnvironment;
            }
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Unrestricted)]
        public static System.Security.NamedPermissionSet GetNamedPermissionSet()
        {
            System.Security.NamedPermissionSet permSet = _theRuntime._namedPermissionSet;
            if (permSet == null)
            {
                return null;
            }
            return new System.Security.NamedPermissionSet(permSet);
        }

        internal static string GetRelaxedMapPathResult(string originalResult)
        {
            if (!IsMapPathRelaxed)
            {
                return originalResult;
            }
            if (originalResult == null)
            {
                return _DefaultPhysicalPathOnMapPathFailure;
            }
            if (originalResult.IndexOfAny(s_InvalidPhysicalPathChars) >= 0)
            {
                return _DefaultPhysicalPathOnMapPathFailure;
            }
            if (((originalResult.Length > 0) && (originalResult[0] == ':')) || ((originalResult.Length > 2) && (originalResult.IndexOf(':', 2) > 0)))
            {
                return _DefaultPhysicalPathOnMapPathFailure;
            }
            try
            {
                bool flag;
                if (!System.Web.Util.FileUtil.IsSuspiciousPhysicalPath(originalResult, out flag) && !flag)
                {
                    return originalResult;
                }
                return _DefaultPhysicalPathOnMapPathFailure;
            }
            catch
            {
                return _DefaultPhysicalPathOnMapPathFailure;
            }
        }

        internal static string GetSafePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            try
            {
                if (HasPathDiscoveryPermission(path))
                {
                    return path;
                }
            }
            catch
            {
            }
            return Path.GetFileName(path);
        }

        internal static bool HasAppPathDiscoveryPermission()
        {
            return HasPathDiscoveryPermission(AppDomainAppPathInternal);
        }

        private static bool HasAPTCABit(Assembly assembly)
        {
            object[] customAttributes = assembly.GetCustomAttributes(typeof(AllowPartiallyTrustedCallersAttribute), false);
            return ((customAttributes != null) && (customAttributes.Length > 0));
        }

        internal static bool HasAspNetHostingPermission(AspNetHostingPermissionLevel level)
        {
            if (NamedPermissionSet == null)
            {
                return true;
            }
            AspNetHostingPermission permission = (AspNetHostingPermission) NamedPermissionSet.GetPermission(typeof(AspNetHostingPermission));
            if (permission == null)
            {
                return false;
            }
            return (permission.Level >= level);
        }

        internal static bool HasDbPermission(DbProviderFactory factory)
        {
            if (NamedPermissionSet == null)
            {
                return true;
            }
            bool flag = false;
            CodeAccessPermission permission = factory.CreatePermission(PermissionState.Unrestricted);
            if (permission != null)
            {
                IPermission target = NamedPermissionSet.GetPermission(permission.GetType());
                if (target != null)
                {
                    flag = permission.IsSubsetOf(target);
                }
            }
            return flag;
        }

        internal static bool HasFilePermission(string path)
        {
            return HasFilePermission(path, false);
        }

        internal static bool HasFilePermission(string path, bool writePermissions)
        {
            if ((TrustLevel == null) && (InitializationException != null))
            {
                return true;
            }
            if (NamedPermissionSet == null)
            {
                return true;
            }
            bool flag = false;
            IPermission target = NamedPermissionSet.GetPermission(typeof(FileIOPermission));
            if (target == null)
            {
                return flag;
            }
            IPermission permission2 = null;
            try
            {
                if (!writePermissions)
                {
                    permission2 = new FileIOPermission(FileIOPermissionAccess.Read, path);
                }
                else
                {
                    permission2 = new FileIOPermission(FileIOPermissionAccess.AllAccess, path);
                }
            }
            catch
            {
                return false;
            }
            return permission2.IsSubsetOf(target);
        }

        internal static bool HasPathDiscoveryPermission(string path)
        {
            if ((TrustLevel == null) && (InitializationException != null))
            {
                return true;
            }
            if (NamedPermissionSet == null)
            {
                return true;
            }
            bool flag = false;
            IPermission target = NamedPermissionSet.GetPermission(typeof(FileIOPermission));
            if (target != null)
            {
                IPermission permission2 = new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path);
                flag = permission2.IsSubsetOf(target);
            }
            return flag;
        }

        internal static bool HasUnmanagedPermission()
        {
            if (NamedPermissionSet == null)
            {
                return true;
            }
            SecurityPermission permission = (SecurityPermission) NamedPermissionSet.GetPermission(typeof(SecurityPermission));
            if (permission == null)
            {
                return false;
            }
            return ((permission.Flags & SecurityPermissionFlag.UnmanagedCode) != SecurityPermissionFlag.NoFlags);
        }

        internal static bool HasWebPermission(Uri uri)
        {
            if (NamedPermissionSet == null)
            {
                return true;
            }
            bool flag = false;
            IPermission target = NamedPermissionSet.GetPermission(typeof(WebPermission));
            if (target == null)
            {
                return flag;
            }
            IPermission permission2 = null;
            try
            {
                permission2 = new WebPermission(NetworkAccess.Connect, uri.ToString());
            }
            catch
            {
                return false;
            }
            return permission2.IsSubsetOf(target);
        }

        private void HostingInit(HostingEnvironmentFlags hostingFlags, System.Security.Policy.PolicyLevel policyLevel, Exception appDomainCreationException)
        {
            ApplicationImpersonationContext context = new ApplicationImpersonationContext();
            try
            {
                CacheSection section;
                TrustSection section2;
                SecurityPolicySection section3;
                CompilationSection section4;
                HostingEnvironmentSection section5;
                Exception exception;
                this._firstRequestStartTime = DateTime.UtcNow;
                this.SetUpDataDirectory();
                this.EnsureAccessToApplicationDirectory();
                this.StartMonitoringDirectoryRenamesAndBinDirectory();
                if (InitializationException == null)
                {
                    HostingEnvironment.InitializeObjectCacheHost();
                }
                this.GetInitConfigSections(out section, out section2, out section3, out section4, out section5, out exception);
                CacheInternal.ReadCacheInternalConfig(section);
                this.SetUpCodegenDirectory(section4);
                if (appDomainCreationException != null)
                {
                    throw appDomainCreationException;
                }
                if ((section2 == null) || string.IsNullOrEmpty(section2.Level))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_section_not_present", new object[] { "trust" }));
                }
                if (section2.LegacyCasModel)
                {
                    try
                    {
                        this._disableProcessRequestInApplicationTrust = false;
                        this._isLegacyCas = true;
                        this.SetTrustLevel(section2, section3);
                        goto Label_00D8;
                    }
                    catch
                    {
                        if (exception != null)
                        {
                            throw exception;
                        }
                        throw;
                    }
                }
                if ((hostingFlags & HostingEnvironmentFlags.ClientBuildManager) != HostingEnvironmentFlags.Default)
                {
                    this._trustLevel = "Full";
                }
                else
                {
                    this._disableProcessRequestInApplicationTrust = true;
                    this.SetTrustParameters(section2, section3, policyLevel);
                }
            Label_00D8:
                this.InitFusion(section5);
                CachedPathData.InitializeUrlMetadataSlidingExpiration(section5);
                HttpConfigurationSystem.CompleteInit();
                if (exception != null)
                {
                    throw exception;
                }
                this.SetThreadPoolLimits();
                SetAutogenKeys();
                BuildManager.InitializeBuildManager();
                this.InitApartmentThreading();
                this.InitDebuggingSupport();
                this._processRequestInApplicationTrust = section2.ProcessRequestInApplicationTrust;
                AppDomainResourcePerfCounters.Init();
                this.RelaxMapPathIfRequired();
            }
            catch (Exception exception2)
            {
                this._hostingInitFailed = true;
                InitializationException = exception2;
                if ((hostingFlags & HostingEnvironmentFlags.ThrowHostingInitErrors) != HostingEnvironmentFlags.Default)
                {
                    throw;
                }
            }
            finally
            {
                if (context != null)
                {
                    ((IDisposable) context).Dispose();
                }
            }
        }

        internal static void IncrementActivePipelineCount()
        {
            Interlocked.Increment(ref _theRuntime._activeRequestCount);
            HostingEnvironment.IncrementBusyCount();
        }

        private void Init()
        {
            try
            {
                if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                {
                    throw new PlatformNotSupportedException(System.Web.SR.GetString("RequiresNT"));
                }
                this._profiler = new Profiler();
                this._timeoutManager = new System.Web.RequestTimeoutManager();
                this._wpUserId = GetCurrentUserName();
                this._requestNotificationCompletionCallback = new AsyncCallback(this.OnRequestNotificationCompletion);
                this._handlerCompletionCallback = new AsyncCallback(this.OnHandlerCompletion);
                this._asyncEndOfSendCallback = new HttpWorkerRequest.EndOfSendNotification(this.EndOfSendCallback);
                this._appDomainUnloadallback = new WaitCallback(this.ReleaseResourcesAndUnloadAppDomain);
                if (GetAppDomainString(".appDomain") != null)
                {
                    this._appDomainAppId = GetAppDomainString(".appId");
                    this._appDomainAppPath = GetAppDomainString(".appPath");
                    this._appDomainAppVPath = VirtualPath.CreateNonRelativeTrailingSlash(GetAppDomainString(".appVPath"));
                    this._appDomainId = GetAppDomainString(".domainId");
                    this._isOnUNCShare = System.Web.Util.StringUtil.StringStartsWith(this._appDomainAppPath, @"\\");
                    PerfCounters.Open(this._appDomainAppId);
                }
                this._fcm = new System.Web.FileChangesMonitor();
            }
            catch (Exception exception)
            {
                InitializationException = exception;
            }
        }

        private void InitApartmentThreading()
        {
            HttpRuntimeSection httpRuntime = RuntimeConfig.GetAppConfig().HttpRuntime;
            if (httpRuntime != null)
            {
                this._apartmentThreading = httpRuntime.ApartmentThreading;
            }
            else
            {
                this._apartmentThreading = false;
            }
        }

        private void InitDebuggingSupport()
        {
            CompilationSection compilation = RuntimeConfig.GetAppConfig().Compilation;
            this._debuggingEnabled = compilation.Debug;
        }

        private void InitFusion(HostingEnvironmentSection hostingEnvironmentSection)
        {
            AppDomain domain = Thread.GetDomain();
            string str = this._appDomainAppPath;
            if (str.IndexOf(DoubleDirectorySeparatorString, 1, StringComparison.Ordinal) >= 1)
            {
                str = str[0] + str.Substring(1).Replace(DoubleDirectorySeparatorString, DirectorySeparatorString);
            }
            domain.AppendPrivatePath(str + "bin");
            if ((hostingEnvironmentSection != null) && !hostingEnvironmentSection.ShadowCopyBinAssemblies)
            {
                domain.ClearShadowCopyPath();
            }
            else
            {
                domain.SetShadowCopyPath(str + "bin");
            }
            string fullName = Directory.GetParent(this._codegenDir).FullName;
            domain.SetCachePath(fullName);
            this._fusionInited = true;
        }

        private void InitHeaderEncoding()
        {
            HttpRuntimeSection httpRuntime = RuntimeConfig.GetAppConfig().HttpRuntime;
            this._enableHeaderChecking = httpRuntime.EnableHeaderChecking;
        }

        private static void InitHttpConfiguration()
        {
            if (!_theRuntime._configInited)
            {
                _theRuntime._configInited = true;
                HttpConfigurationSystem.EnsureInit(null, true, true);
                GlobalizationSection globalization = RuntimeConfig.GetAppLKGConfig().Globalization;
                if (globalization != null)
                {
                    if (!string.IsNullOrEmpty(globalization.Culture) && !System.Web.Util.StringUtil.StringStartsWithIgnoreCase(globalization.Culture, "auto"))
                    {
                        SetCurrentThreadCultureWithAssert(HttpServerUtility.CreateReadOnlyCultureInfo(globalization.Culture));
                    }
                    if (!string.IsNullOrEmpty(globalization.UICulture) && !System.Web.Util.StringUtil.StringStartsWithIgnoreCase(globalization.UICulture, "auto"))
                    {
                        Thread.CurrentThread.CurrentUICulture = HttpServerUtility.CreateReadOnlyCultureInfo(globalization.UICulture);
                    }
                }
                RuntimeConfig appConfig = RuntimeConfig.GetAppConfig();
                ProcessModelSection processModel = appConfig.ProcessModel;
                HostingEnvironmentSection hostingEnvironment = appConfig.HostingEnvironment;
            }
        }

        private void InitializeHealthMonitoring()
        {
            ProcessModelSection processModel = RuntimeConfig.GetMachineConfig().ProcessModel;
            int totalSeconds = (int) processModel.ResponseDeadlockInterval.TotalSeconds;
            int requestQueueLimit = processModel.RequestQueueLimit;
            System.Web.UnsafeNativeMethods.InitializeHealthMonitor(totalSeconds, requestQueueLimit);
        }

        internal static void InitializeHostingFeatures(HostingEnvironmentFlags hostingFlags, System.Security.Policy.PolicyLevel policyLevel, Exception appDomainCreationException)
        {
            _theRuntime.HostingInit(hostingFlags, policyLevel, appDomainCreationException);
        }

        private bool InitiateShutdownOnce()
        {
            if (this._shutdownInProgress)
            {
                return false;
            }
            lock (this)
            {
                if (this._shutdownInProgress)
                {
                    return false;
                }
                this._shutdownInProgress = true;
            }
            return true;
        }

        private void InitRequestQueue()
        {
            RuntimeConfig appConfig = RuntimeConfig.GetAppConfig();
            HttpRuntimeSection httpRuntime = appConfig.HttpRuntime;
            ProcessModelSection processModel = appConfig.ProcessModel;
            if (processModel.AutoConfig)
            {
                this._requestQueue = new RequestQueue(0x58 * processModel.CpuCount, 0x4c * processModel.CpuCount, httpRuntime.AppRequestQueueLimit, processModel.ClientConnectedCheck);
            }
            else
            {
                int num = (processModel.MaxWorkerThreadsTimesCpuCount < processModel.MaxIoThreadsTimesCpuCount) ? processModel.MaxWorkerThreadsTimesCpuCount : processModel.MaxIoThreadsTimesCpuCount;
                if (httpRuntime.MinFreeThreads >= num)
                {
                    if (httpRuntime.ElementInformation.Properties["minFreeThreads"].LineNumber == 0)
                    {
                        if (processModel.ElementInformation.Properties["maxWorkerThreads"].LineNumber != 0)
                        {
                            throw new ConfigurationErrorsException(System.Web.SR.GetString("Thread_pool_limit_must_be_greater_than_minFreeThreads", new object[] { httpRuntime.MinFreeThreads.ToString(CultureInfo.InvariantCulture) }), processModel.ElementInformation.Properties["maxWorkerThreads"].Source, processModel.ElementInformation.Properties["maxWorkerThreads"].LineNumber);
                        }
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Thread_pool_limit_must_be_greater_than_minFreeThreads", new object[] { httpRuntime.MinFreeThreads.ToString(CultureInfo.InvariantCulture) }), processModel.ElementInformation.Properties["maxIoThreads"].Source, processModel.ElementInformation.Properties["maxIoThreads"].LineNumber);
                    }
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Min_free_threads_must_be_under_thread_pool_limits", new object[] { num.ToString(CultureInfo.InvariantCulture) }), httpRuntime.ElementInformation.Properties["minFreeThreads"].Source, httpRuntime.ElementInformation.Properties["minFreeThreads"].LineNumber);
                }
                if (httpRuntime.MinLocalRequestFreeThreads > httpRuntime.MinFreeThreads)
                {
                    if (httpRuntime.ElementInformation.Properties["minLocalRequestFreeThreads"].LineNumber == 0)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Local_free_threads_cannot_exceed_free_threads"), processModel.ElementInformation.Properties["minFreeThreads"].Source, processModel.ElementInformation.Properties["minFreeThreads"].LineNumber);
                    }
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Local_free_threads_cannot_exceed_free_threads"), httpRuntime.ElementInformation.Properties["minLocalRequestFreeThreads"].Source, httpRuntime.ElementInformation.Properties["minLocalRequestFreeThreads"].LineNumber);
                }
                this._requestQueue = new RequestQueue(httpRuntime.MinFreeThreads, httpRuntime.MinLocalRequestFreeThreads, httpRuntime.AppRequestQueueLimit, processModel.ClientConnectedCheck);
            }
        }

        private void InitTrace(HttpContext context)
        {
            System.Web.Configuration.TraceSection trace = RuntimeConfig.GetAppConfig().Trace;
            Profile.RequestsToProfile = trace.RequestLimit;
            Profile.PageOutput = trace.PageOutput;
            Profile.OutputMode = TraceMode.SortByTime;
            if (trace.TraceMode == TraceDisplayMode.SortByCategory)
            {
                Profile.OutputMode = TraceMode.SortByCategory;
            }
            Profile.LocalOnly = trace.LocalOnly;
            Profile.IsEnabled = trace.Enabled;
            Profile.MostRecent = trace.MostRecent;
            Profile.Reset();
            context.TraceIsEnabled = trace.Enabled;
            TraceContext.SetWriteToDiagnosticsTrace(trace.WriteToDiagnosticsTrace);
        }

        internal static bool IsPathWithinAppRoot(string path)
        {
            return ((AppDomainIdInternal == null) || System.Web.Util.UrlPath.IsEqualOrSubpath(AppDomainAppVirtualPathString, path));
        }

        internal static bool IsTypeAllowedInConfig(Type t)
        {
            if (HasAspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted))
            {
                return true;
            }
            Assembly assembly = t.Assembly;
            return (!assembly.GlobalAssemblyCache || HasAPTCABit(assembly));
        }

        internal static string MakeFileUrl(string path)
        {
            Uri uri = new Uri(path);
            return uri.ToString();
        }

        internal static void OnAppDomainShutdown(BuildManagerHostUnloadEventArgs e)
        {
            if (AppDomainShutdown != null)
            {
                AppDomainShutdown(_theRuntime, e);
            }
        }

        private void OnAppOfflineFileChange(object sender, FileChangeEvent e)
        {
            SetUserForcedShutdown();
            ShutdownAppDomain(ApplicationShutdownReason.ConfigurationChange, "Change in App_Offline.htm");
        }

        internal static void OnConfigChange()
        {
            ShutdownAppDomain(ApplicationShutdownReason.ConfigurationChange, "CONFIG change");
        }

        private void OnCriticalDirectoryChange(object sender, FileChangeEvent e)
        {
            ApplicationShutdownReason none = ApplicationShutdownReason.None;
            string name = new DirectoryInfo(e.FileName).Name;
            string message = name + " dir change or directory rename";
            if (System.Web.Util.StringUtil.EqualsIgnoreCase(name, "App_Code"))
            {
                none = ApplicationShutdownReason.CodeDirChangeOrDirectoryRename;
            }
            else if (System.Web.Util.StringUtil.EqualsIgnoreCase(name, "App_GlobalResources"))
            {
                none = ApplicationShutdownReason.ResourcesDirChangeOrDirectoryRename;
            }
            else if (System.Web.Util.StringUtil.EqualsIgnoreCase(name, "App_Browsers"))
            {
                none = ApplicationShutdownReason.BrowsersDirChangeOrDirectoryRename;
            }
            else if (System.Web.Util.StringUtil.EqualsIgnoreCase(name, "bin"))
            {
                none = ApplicationShutdownReason.BinDirChangeOrDirectoryRename;
            }
            if (e.Action == FileAction.Added)
            {
                SetUserForcedShutdown();
            }
            ShutdownAppDomain(none, message);
        }

        private void OnHandlerCompletion(IAsyncResult ar)
        {
            HttpContext asyncState = (HttpContext) ar.AsyncState;
            try
            {
                asyncState.AsyncAppHandler.EndProcessRequest(ar);
            }
            catch (Exception exception)
            {
                asyncState.AddError(exception);
            }
            finally
            {
                asyncState.AsyncAppHandler = null;
            }
            this.FinishRequest(asyncState.WorkerRequest, asyncState, asyncState.Error);
        }

        private void OnRequestNotificationCompletion(IAsyncResult ar)
        {
            try
            {
                this.OnRequestNotificationCompletionHelper(ar);
            }
            catch (Exception exception)
            {
                ApplicationManager.RecordFatalException(exception);
                throw;
            }
        }

        private void OnRequestNotificationCompletionHelper(IAsyncResult ar)
        {
            if (!ar.CompletedSynchronously)
            {
                RequestNotificationStatus finishRequest = RequestNotificationStatus.Continue;
                HttpContext asyncState = (HttpContext) ar.AsyncState;
                IIS7WorkerRequest workerRequest = asyncState.WorkerRequest as IIS7WorkerRequest;
                try
                {
                    asyncState.ApplicationInstance.EndProcessRequestNotification(ar);
                }
                catch (Exception exception)
                {
                    finishRequest = RequestNotificationStatus.FinishRequest;
                    asyncState.AddError(exception);
                }
                IntPtr requestContext = workerRequest.RequestContext;
                this.FinishRequestNotification(workerRequest, asyncState, ref finishRequest);
                asyncState.NotificationContext = null;
                Misc.ThrowIfFailedHr(UnsafeIISMethods.MgdPostCompletion(requestContext, finishRequest));
            }
        }

        private void OnSecurityPolicyFileChange(object sender, FileChangeEvent e)
        {
            ShutdownAppDomain(ApplicationShutdownReason.ChangeInSecurityPolicyFile, "Change in code-access security policy file");
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private void PreloadAssembliesFromBin()
        {
            bool flag = false;
            if (!this._isOnUNCShare)
            {
                IdentitySection identity = RuntimeConfig.GetAppConfig().Identity;
                if (identity.Impersonate && (identity.ImpersonateToken == IntPtr.Zero))
                {
                    flag = true;
                }
            }
            if (flag)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(BinDirectoryInternal);
                if (dirInfo.Exists)
                {
                    this.PreloadAssembliesFromBinRecursive(dirInfo);
                }
            }
        }

        private void PreloadAssembliesFromBinRecursive(DirectoryInfo dirInfo)
        {
            foreach (FileInfo info in dirInfo.GetFiles("*.dll"))
            {
                try
                {
                    Assembly.Load(Util.GetAssemblyNameFromFileName(info.Name));
                }
                catch (FileNotFoundException)
                {
                    try
                    {
                        Assembly.LoadFrom(info.FullName);
                    }
                    catch
                    {
                    }
                }
                catch
                {
                }
            }
            foreach (DirectoryInfo info2 in dirInfo.GetDirectories())
            {
                this.PreloadAssembliesFromBinRecursive(info2);
            }
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
        public static void ProcessRequest(HttpWorkerRequest wr)
        {
            if (wr == null)
            {
                throw new ArgumentNullException("wr");
            }
            if (UseIntegratedPipeline)
            {
                throw new PlatformNotSupportedException(System.Web.SR.GetString("Method_Not_Supported_By_Iis_Integrated_Mode", new object[] { "HttpRuntime.ProcessRequest" }));
            }
            ProcessRequestNoDemand(wr);
        }

        private void ProcessRequestInternal(HttpWorkerRequest wr)
        {
            HttpContext context;
            try
            {
                context = new HttpContext(wr, false);
            }
            catch
            {
                wr.SendStatus(400, "Bad Request");
                wr.SendKnownResponseHeader(12, "text/html; charset=utf-8");
                byte[] bytes = Encoding.ASCII.GetBytes("<html><body>Bad Request</body></html>");
                wr.SendResponseFromMemory(bytes, bytes.Length);
                wr.FlushResponse(true);
                wr.EndOfRequest();
                return;
            }
            wr.SetEndOfSendNotification(this._asyncEndOfSendCallback, context);
            Interlocked.Increment(ref this._activeRequestCount);
            HostingEnvironment.IncrementBusyCount();
            try
            {
                try
                {
                    this.EnsureFirstRequestInit(context);
                }
                catch
                {
                    if (!context.Request.IsDebuggingRequest)
                    {
                        throw;
                    }
                }
                context.Response.InitResponseWriter();
                IHttpHandler applicationInstance = HttpApplicationFactory.GetApplicationInstance(context);
                if (applicationInstance == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Unable_create_app_object"));
                }
                if (EtwTrace.IsTraceEnabled(5, 1))
                {
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_START_HANDLER, context.WorkerRequest, applicationInstance.GetType().FullName, "Start");
                }
                if (applicationInstance is IHttpAsyncHandler)
                {
                    IHttpAsyncHandler handler2 = (IHttpAsyncHandler) applicationInstance;
                    context.AsyncAppHandler = handler2;
                    handler2.BeginProcessRequest(context, this._handlerCompletionCallback, context);
                }
                else
                {
                    applicationInstance.ProcessRequest(context);
                    this.FinishRequest(context.WorkerRequest, context, null);
                }
            }
            catch (Exception exception)
            {
                context.Response.InitResponseWriter();
                this.FinishRequest(wr, context, exception);
            }
        }

        internal static void ProcessRequestNoDemand(HttpWorkerRequest wr)
        {
            RequestQueue queue = _theRuntime._requestQueue;
            wr.UpdateInitialCounters();
            if (queue != null)
            {
                wr = queue.GetRequestToExecute(wr);
            }
            if (wr != null)
            {
                CalculateWaitTimeAndUpdatePerfCounter(wr);
                wr.ResetStartTime();
                ProcessRequestNow(wr);
            }
        }

        internal static RequestNotificationStatus ProcessRequestNotification(IIS7WorkerRequest wr, HttpContext context)
        {
            return _theRuntime.ProcessRequestNotificationPrivate(wr, context);
        }

        private RequestNotificationStatus ProcessRequestNotificationPrivate(IIS7WorkerRequest wr, HttpContext context)
        {
            RequestNotificationStatus pending = RequestNotificationStatus.Pending;
            try
            {
                int num;
                bool flag;
                int num2;
                UnsafeIISMethods.MgdGetCurrentNotificationInfo(wr.RequestContext, out num, out flag, out num2);
                context.CurrentModuleIndex = num;
                context.IsPostNotification = flag;
                context.CurrentNotification = (RequestNotification) num2;
                IHttpHandler applicationInstance = null;
                if (context.NeedToInitializeApp())
                {
                    try
                    {
                        this.EnsureFirstRequestInit(context);
                    }
                    catch
                    {
                        if (!context.Request.IsDebuggingRequest)
                        {
                            throw;
                        }
                    }
                    context.Response.InitResponseWriter();
                    applicationInstance = HttpApplicationFactory.GetApplicationInstance(context);
                    if (applicationInstance == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("Unable_create_app_object"));
                    }
                    if (EtwTrace.IsTraceEnabled(5, 1))
                    {
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_START_HANDLER, context.WorkerRequest, applicationInstance.GetType().FullName, "Start");
                    }
                    HttpApplication application = applicationInstance as HttpApplication;
                    if (application != null)
                    {
                        application.AssignContext(context);
                    }
                }
                wr.SynchronizeVariables(context);
                if (context.ApplicationInstance != null)
                {
                    if (context.ApplicationInstance.BeginProcessRequestNotification(context, this._requestNotificationCompletionCallback).CompletedSynchronously)
                    {
                        pending = RequestNotificationStatus.Continue;
                    }
                }
                else if (applicationInstance != null)
                {
                    applicationInstance.ProcessRequest(context);
                    pending = RequestNotificationStatus.FinishRequest;
                }
                else
                {
                    pending = RequestNotificationStatus.Continue;
                }
            }
            catch (Exception exception)
            {
                pending = RequestNotificationStatus.FinishRequest;
                context.Response.InitResponseWriter();
                context.AddError(exception);
            }
            if (pending != RequestNotificationStatus.Pending)
            {
                this.FinishRequestNotification(wr, context, ref pending);
            }
            return pending;
        }

        internal static void ProcessRequestNow(HttpWorkerRequest wr)
        {
            _theRuntime.ProcessRequestInternal(wr);
        }

        private void RaiseShutdownWebEventOnce()
        {
            if (!this._shutdownWebEventRaised)
            {
                lock (this)
                {
                    if (!this._shutdownWebEventRaised)
                    {
                        WebBaseEvent.RaiseSystemEvent(this, 0x3ea, WebApplicationLifetimeEvent.DetailCodeFromShutdownReason(ShutdownReason));
                        this._shutdownWebEventRaised = true;
                    }
                }
            }
        }

        internal static void RecoverFromUnexceptedAppDomainUnload()
        {
            if (!_theRuntime._shutdownInProgress)
            {
                _theRuntime._shutdownInProgress = true;
                try
                {
                    ISAPIRuntime.RemoveThisAppDomainFromUnmanagedTable();
                    PipelineRuntime.RemoveThisAppDomainFromUnmanagedTable();
                    AddAppDomainTraceMessage("AppDomainRestart");
                }
                finally
                {
                    _theRuntime.Dispose();
                }
            }
        }

        private void RejectRequestInternal(HttpWorkerRequest wr, bool silent)
        {
            HttpContext extraData = new HttpContext(wr, false);
            wr.SetEndOfSendNotification(this._asyncEndOfSendCallback, extraData);
            Interlocked.Increment(ref this._activeRequestCount);
            HostingEnvironment.IncrementBusyCount();
            if (silent)
            {
                extraData.Response.InitResponseWriter();
                this.FinishRequest(wr, extraData, null);
            }
            else
            {
                PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.REQUESTS_REJECTED);
                PerfCounters.IncrementCounter(AppPerfCounter.APP_REQUESTS_REJECTED);
                try
                {
                    throw new HttpException(0x1f7, System.Web.SR.GetString("Server_too_busy"));
                }
                catch (Exception exception)
                {
                    extraData.Response.InitResponseWriter();
                    this.FinishRequest(wr, extraData, exception);
                }
            }
        }

        internal static void RejectRequestNow(HttpWorkerRequest wr, bool silent)
        {
            _theRuntime.RejectRequestInternal(wr, silent);
        }

        private void RelaxMapPathIfRequired()
        {
            try
            {
                RuntimeConfig appConfig = RuntimeConfig.GetAppConfig();
                if (((appConfig != null) && (appConfig.HttpRuntime != null)) && appConfig.HttpRuntime.RelaxedUrlToFileSystemMapping)
                {
                    _DefaultPhysicalPathOnMapPathFailure = Path.Combine(this._appDomainAppPath, "NOT_A_VALID_FILESYSTEM_PATH");
                }
            }
            catch
            {
            }
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private void ReleaseResourcesAndUnloadAppDomain(object state)
        {
            try
            {
                PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.APPLICATION_RESTARTS);
            }
            catch
            {
            }
            try
            {
                this.Dispose();
            }
            catch
            {
            }
            Thread.Sleep(250);
            AddAppDomainTraceMessage("before Unload");
        Label_002A:
            try
            {
                AppDomain.Unload(Thread.GetDomain());
                goto Label_002A;
            }
            catch (CannotUnloadAppDomainException)
            {
                goto Label_002A;
            }
            catch (Exception exception)
            {
                AddAppDomainTraceMessage("Unload Exception: " + exception);
                throw;
            }
        }

        internal static void ReportAppOfflineErrorMessage(HttpResponse response, byte[] appOfflineMessage)
        {
            response.StatusCode = 0x1f7;
            response.ContentType = "text/html";
            response.AddHeader("Retry-After", "3600");
            response.OutputStream.Write(appOfflineMessage, 0, appOfflineMessage.Length);
        }

        internal static void RestrictIISFolders(HttpContext context)
        {
            HttpWorkerRequest workerRequest = context.WorkerRequest;
            if (((workerRequest != null) && (workerRequest is ISAPIWorkerRequest)) && !(workerRequest is ISAPIWorkerRequestInProcForIIS6))
            {
                byte[] bufOut = new byte[1];
                byte[] bytes = BitConverter.GetBytes(1);
                context.CallISAPI(System.Web.UnsafeNativeMethods.CallISAPIFunc.RestrictIISFolders, bytes, bufOut);
            }
        }

        private void SetAutoConfigLimits(ProcessModelSection pmConfig)
        {
            int num;
            int num2;
            ThreadPool.GetMaxThreads(out num, out num2);
            if ((pmConfig.DefaultMaxWorkerThreadsForAutoConfig != num) || (pmConfig.DefaultMaxIoThreadsForAutoConfig != num2))
            {
                System.Web.UnsafeNativeMethods.SetClrThreadPoolLimits(pmConfig.DefaultMaxWorkerThreadsForAutoConfig, pmConfig.DefaultMaxIoThreadsForAutoConfig);
            }
            ServicePointManager.DefaultConnectionLimit = 12 * pmConfig.CpuCount;
        }

        private static void SetAutogenKeys()
        {
            byte[] data = new byte[s_autogenKeys.Length];
            byte[] bufferOut = new byte[s_autogenKeys.Length];
            bool flag = false;
            new RNGCryptoServiceProvider().GetBytes(data);
            if (!flag)
            {
                flag = System.Web.UnsafeNativeMethods.EcbCallISAPI(IntPtr.Zero, System.Web.UnsafeNativeMethods.CallISAPIFunc.GetAutogenKeys, data, data.Length, bufferOut, bufferOut.Length) == 1;
            }
            if (flag)
            {
                Buffer.BlockCopy(bufferOut, 0, s_autogenKeys, 0, s_autogenKeys.Length);
            }
            else
            {
                Buffer.BlockCopy(data, 0, s_autogenKeys, 0, s_autogenKeys.Length);
            }
        }

        [SecurityPermission(SecurityAction.Assert, ControlThread=true)]
        internal static void SetCurrentThreadCultureWithAssert(CultureInfo cultureInfo)
        {
            Thread.CurrentThread.CurrentCulture = cultureInfo;
        }

        private static void SetExecutionTimePerformanceCounter(HttpContext context)
        {
            long num = DateTime.UtcNow.Subtract(context.WorkerRequest.GetStartTime()).Ticks / 0x2710L;
            if (num > 0x7fffffffL)
            {
                num = 0x7fffffffL;
            }
            PerfCounters.SetGlobalCounter(GlobalPerfCounter.REQUEST_EXECUTION_TIME, (int) num);
            PerfCounters.SetCounter(AppPerfCounter.APP_REQUEST_EXEC_TIME, (int) num);
        }

        internal static void SetShutdownMessage(string message)
        {
            if (message != null)
            {
                if (_theRuntime._shutDownMessage == null)
                {
                    _theRuntime._shutDownMessage = message;
                }
                else
                {
                    _theRuntime._shutDownMessage = _theRuntime._shutDownMessage + "\r\n" + message;
                }
            }
        }

        internal static void SetShutdownReason(ApplicationShutdownReason reason, string message)
        {
            if (_theRuntime._shutdownReason == ApplicationShutdownReason.None)
            {
                _theRuntime._shutdownReason = reason;
            }
            SetShutdownMessage(message);
        }

        private void SetThreadPoolLimits()
        {
            try
            {
                ProcessModelSection processModel = RuntimeConfig.GetMachineConfig().ProcessModel;
                if (processModel.AutoConfig)
                {
                    this.SetAutoConfigLimits(processModel);
                }
                else if ((processModel.MaxWorkerThreadsTimesCpuCount > 0) && (processModel.MaxIoThreadsTimesCpuCount > 0))
                {
                    int num;
                    int num2;
                    ThreadPool.GetMaxThreads(out num, out num2);
                    if ((processModel.MaxWorkerThreadsTimesCpuCount != num) || (processModel.MaxIoThreadsTimesCpuCount != num2))
                    {
                        System.Web.UnsafeNativeMethods.SetClrThreadPoolLimits(processModel.MaxWorkerThreadsTimesCpuCount, processModel.MaxIoThreadsTimesCpuCount);
                    }
                }
                if ((processModel.MinWorkerThreadsTimesCpuCount > 0) || (processModel.MinIoThreadsTimesCpuCount > 0))
                {
                    int num3;
                    int num4;
                    ThreadPool.GetMinThreads(out num3, out num4);
                    int workerThreads = (processModel.MinWorkerThreadsTimesCpuCount > 0) ? processModel.MinWorkerThreadsTimesCpuCount : num3;
                    int completionPortThreads = (processModel.MinIoThreadsTimesCpuCount > 0) ? processModel.MinIoThreadsTimesCpuCount : num4;
                    if (((workerThreads > 0) && (completionPortThreads > 0)) && ((workerThreads != num3) || (completionPortThreads != num4)))
                    {
                        ThreadPool.SetMinThreads(workerThreads, completionPortThreads);
                    }
                }
            }
            catch
            {
            }
        }

        private void SetTrustLevel(TrustSection trustSection, SecurityPolicySection securityPolicySection)
        {
            string str = trustSection.Level;
            if (trustSection.Level == "Full")
            {
                this._trustLevel = str;
            }
            else
            {
                if ((securityPolicySection == null) || (securityPolicySection.TrustLevels[trustSection.Level] == null))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Unable_to_get_policy_file", new object[] { trustSection.Level }), string.Empty, 0);
                }
                string filename = null;
                if (((trustSection.Level == "Minimal") || (trustSection.Level == "Low")) || ((trustSection.Level == "Medium") || (trustSection.Level == "High")))
                {
                    filename = securityPolicySection.TrustLevels[trustSection.Level].LegacyPolicyFileExpanded;
                }
                else
                {
                    filename = securityPolicySection.TrustLevels[trustSection.Level].PolicyFileExpanded;
                }
                if ((filename == null) || !System.Web.Util.FileUtil.FileExists(filename))
                {
                    throw new HttpException(System.Web.SR.GetString("Unable_to_get_policy_file", new object[] { trustSection.Level }));
                }
                bool foundGacToken = false;
                System.Security.Policy.PolicyLevel domainPolicy = CreatePolicyLevel(filename, AppDomainAppPathInternal, CodegenDirInternal, trustSection.OriginUrl, out foundGacToken);
                if (foundGacToken)
                {
                    CodeGroup rootCodeGroup = domainPolicy.RootCodeGroup;
                    bool flag2 = false;
                    foreach (CodeGroup group2 in rootCodeGroup.Children)
                    {
                        if (group2.MembershipCondition is GacMembershipCondition)
                        {
                            flag2 = true;
                            break;
                        }
                    }
                    if (!flag2 && (rootCodeGroup is FirstMatchCodeGroup))
                    {
                        FirstMatchCodeGroup group3 = (FirstMatchCodeGroup) rootCodeGroup;
                        if ((group3.MembershipCondition is AllMembershipCondition) && (group3.PermissionSetName == "Nothing"))
                        {
                            PermissionSet permSet = new PermissionSet(PermissionState.Unrestricted);
                            CodeGroup group = new UnionCodeGroup(new GacMembershipCondition(), new PolicyStatement(permSet));
                            CodeGroup group5 = new FirstMatchCodeGroup(rootCodeGroup.MembershipCondition, rootCodeGroup.PolicyStatement);
                            foreach (CodeGroup group6 in rootCodeGroup.Children)
                            {
                                if (((group6 is UnionCodeGroup) && (group6.MembershipCondition is UrlMembershipCondition)) && (group6.PolicyStatement.PermissionSet.IsUnrestricted() && (group != null)))
                                {
                                    group5.AddChild(group);
                                    group = null;
                                }
                                group5.AddChild(group6);
                            }
                            domainPolicy.RootCodeGroup = group5;
                        }
                    }
                }
                AppDomain.CurrentDomain.SetAppDomainPolicy(domainPolicy);
                this._namedPermissionSet = domainPolicy.GetNamedPermissionSet(trustSection.PermissionSetName);
                this._trustLevel = str;
                this._fcm.StartMonitoringFile(filename, new FileChangeEventHandler(this.OnSecurityPolicyFileChange));
            }
        }

        private void SetTrustParameters(TrustSection trustSection, SecurityPolicySection securityPolicySection, System.Security.Policy.PolicyLevel policyLevel)
        {
            this._trustLevel = trustSection.Level;
            if (this._trustLevel != "Full")
            {
                this._namedPermissionSet = policyLevel.GetNamedPermissionSet(trustSection.PermissionSetName);
                this._policyLevel = policyLevel;
                this._hostSecurityPolicyResolverType = trustSection.HostSecurityPolicyResolverType;
                string policyFileExpanded = securityPolicySection.TrustLevels[trustSection.Level].PolicyFileExpanded;
                this._fcm.StartMonitoringFile(policyFileExpanded, new FileChangeEventHandler(this.OnSecurityPolicyFileChange));
            }
        }

        private void SetUpCodegenDirectory(CompilationSection compilationSection)
        {
            AppDomain domain = Thread.GetDomain();
            string str2 = AppManagerAppDomainFactory.ConstructSimpleAppName(AppDomainAppVirtualPath);
            string path = null;
            string tempDirAttribName = null;
            string configFileName = null;
            int configLineNumber = 0;
            if ((compilationSection != null) && !string.IsNullOrEmpty(compilationSection.TempDirectory))
            {
                path = compilationSection.TempDirectory;
                compilationSection.GetTempDirectoryErrorInfo(out tempDirAttribName, out configFileName, out configLineNumber);
            }
            if (path != null)
            {
                path = path.Trim();
                if (!Path.IsPathRooted(path))
                {
                    path = null;
                }
                else
                {
                    try
                    {
                        path = new DirectoryInfo(path).FullName;
                    }
                    catch
                    {
                        path = null;
                    }
                }
                if (path == null)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_temp_directory", new object[] { tempDirAttribName }), configFileName, configLineNumber);
                }
                try
                {
                    Directory.CreateDirectory(path);
                    goto Label_00D0;
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_temp_directory", new object[] { tempDirAttribName }), exception, configFileName, configLineNumber);
                }
            }
            path = Path.Combine(s_installDirectory, "Temporary ASP.NET Files");
        Label_00D0:
            if (!Util.HasWriteAccessToDirectory(path))
            {
                if (!Environment.UserInteractive)
                {
                    throw new HttpException(System.Web.SR.GetString("No_codegen_access", new object[] { Util.GetCurrentAccountName(), path }));
                }
                path = Path.Combine(Path.GetTempPath(), "Temporary ASP.NET Files");
            }
            this._tempDir = path;
            string str = Path.Combine(path, str2);
            domain.SetDynamicBase(str);
            this._codegenDir = Thread.GetDomain().DynamicDirectory;
            Directory.CreateDirectory(this._codegenDir);
        }

        private void SetUpDataDirectory()
        {
            string data = Path.Combine(this._appDomainAppPath, "App_Data");
            AppDomain.CurrentDomain.SetData("DataDirectory", data, new FileIOPermission(FileIOPermissionAccess.PathDiscovery, data));
        }

        internal static void SetUserForcedShutdown()
        {
            _theRuntime._userForcedShutdown = true;
        }

        private static bool ShutdownAppDomain(string stackTrace)
        {
            if (((_theRuntime.LastShutdownAttemptTime == DateTime.MinValue) && !_theRuntime._firstRequestCompleted) && !_theRuntime._userForcedShutdown)
            {
                int totalSeconds = 0;
                try
                {
                    RuntimeConfig appLKGConfig = RuntimeConfig.GetAppLKGConfig();
                    if (appLKGConfig != null)
                    {
                        HttpRuntimeSection httpRuntime = appLKGConfig.HttpRuntime;
                        if (httpRuntime != null)
                        {
                            totalSeconds = (int) httpRuntime.DelayNotificationTimeout.TotalSeconds;
                            if (DateTime.UtcNow < _theRuntime._firstRequestStartTime.AddSeconds((double) totalSeconds))
                            {
                                return false;
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            try
            {
                _theRuntime.RaiseShutdownWebEventOnce();
            }
            catch
            {
            }
            _theRuntime.LastShutdownAttemptTime = DateTime.UtcNow;
            if (!HostingEnvironment.ShutdownInitiated)
            {
                HostingEnvironment.InitiateShutdownWithoutDemand();
                return true;
            }
            if (HostingEnvironment.ShutdownInProgress)
            {
                return false;
            }
            if (!_theRuntime.InitiateShutdownOnce())
            {
                return false;
            }
            if (string.IsNullOrEmpty(stackTrace))
            {
                new EnvironmentPermission(PermissionState.Unrestricted).Assert();
                try
                {
                    _theRuntime._shutDownStack = Environment.StackTrace;
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            else
            {
                _theRuntime._shutDownStack = stackTrace;
            }
            OnAppDomainShutdown(new BuildManagerHostUnloadEventArgs(_theRuntime._shutdownReason));
            ThreadPool.QueueUserWorkItem(_theRuntime._appDomainUnloadallback);
            return true;
        }

        internal static bool ShutdownAppDomain(ApplicationShutdownReason reason, string message)
        {
            return ShutdownAppDomainWithStackTrace(reason, message, null);
        }

        internal static bool ShutdownAppDomainWithStackTrace(ApplicationShutdownReason reason, string message, string stackTrace)
        {
            SetShutdownReason(reason, message);
            return ShutdownAppDomain(stackTrace);
        }

        private void StartAppDomainShutdownTimer()
        {
            if ((this._appDomainShutdownTimer == null) && !this._shutdownInProgress)
            {
                lock (this)
                {
                    if ((this._appDomainShutdownTimer == null) && !this._shutdownInProgress)
                    {
                        this._appDomainShutdownTimer = new System.Threading.Timer(new TimerCallback(this.AppDomainShutdownTimerCallback), null, 0x2710, 0);
                    }
                }
            }
        }

        internal static void StartListeningToLocalResourcesDirectory(VirtualPath virtualDir)
        {
            _theRuntime._fcm.StartListeningToLocalResourcesDirectory(virtualDir);
        }

        private void StartMonitoringDirectoryRenamesAndBinDirectory()
        {
            this._fcm.StartMonitoringDirectoryRenamesAndBinDirectory(AppDomainAppPathInternal, new FileChangeEventHandler(this.OnCriticalDirectoryChange));
        }

        private static void StaticInit()
        {
            if (!s_initialized)
            {
                bool flag = false;
                bool flag2 = false;
                string runtimeDirectory = null;
                runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
                if (System.Web.UnsafeNativeMethods.GetModuleHandle("webengine4.dll") != IntPtr.Zero)
                {
                    flag = true;
                }
                if (!flag && (System.Web.UnsafeNativeMethods.LoadLibrary(runtimeDirectory + Path.DirectorySeparatorChar + "webengine4.dll") != IntPtr.Zero))
                {
                    flag = true;
                    flag2 = true;
                }
                if (flag)
                {
                    System.Web.UnsafeNativeMethods.InitializeLibrary(false);
                    if (flag2)
                    {
                        System.Web.UnsafeNativeMethods.PerfCounterInitialize();
                    }
                }
                s_installDirectory = runtimeDirectory;
                s_isEngineLoaded = flag;
                s_initialized = true;
                AddAppDomainTraceMessage("Initialize");
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public static void UnloadAppDomain()
        {
            _theRuntime._userForcedShutdown = true;
            ShutdownAppDomain(ApplicationShutdownReason.UnloadAppDomainCalled, "User code called UnloadAppDomain");
        }

        private static void UpdatePerfCounters(int statusCode)
        {
            if (400 <= statusCode)
            {
                PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_FAILED);
                switch (statusCode)
                {
                    case 0x191:
                        PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_NOT_AUTHORIZED);
                        return;

                    case 0x194:
                    case 0x19e:
                        PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_NOT_FOUND);
                        break;
                }
            }
            else
            {
                PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_SUCCEDED);
            }
        }

        private void WaitForRequestsToFinish(int waitTimeoutMs)
        {
            DateTime time = DateTime.UtcNow.AddMilliseconds((double) waitTimeoutMs);
            do
            {
                if (this._activeRequestCount == 0)
                {
                    if (this._requestQueue == null)
                    {
                        break;
                    }
                    if (this._requestQueue.IsEmpty)
                    {
                        return;
                    }
                }
                Thread.Sleep(250);
            }
            while (Debugger.IsAttached || (DateTime.UtcNow <= time));
        }

        internal static bool ApartmentThreading
        {
            get
            {
                return _theRuntime._apartmentThreading;
            }
        }

        public static string AppDomainAppId
        {
            [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
            get
            {
                return AppDomainAppIdInternal;
            }
        }

        internal static string AppDomainAppIdInternal
        {
            get
            {
                return _theRuntime._appDomainAppId;
            }
        }

        public static string AppDomainAppPath
        {
            get
            {
                InternalSecurityPermissions.AppPathDiscovery.Demand();
                return AppDomainAppPathInternal;
            }
        }

        internal static string AppDomainAppPathInternal
        {
            get
            {
                return _theRuntime._appDomainAppPath;
            }
        }

        public static string AppDomainAppVirtualPath
        {
            get
            {
                return VirtualPath.GetVirtualPathStringNoTrailingSlash(_theRuntime._appDomainAppVPath);
            }
        }

        internal static VirtualPath AppDomainAppVirtualPathObject
        {
            get
            {
                return _theRuntime._appDomainAppVPath;
            }
        }

        internal static string AppDomainAppVirtualPathString
        {
            get
            {
                return VirtualPath.GetVirtualPathString(_theRuntime._appDomainAppVPath);
            }
        }

        public static string AppDomainId
        {
            [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
            get
            {
                return AppDomainIdInternal;
            }
        }

        internal static string AppDomainIdInternal
        {
            get
            {
                return _theRuntime._appDomainId;
            }
        }

        internal static byte[] AppOfflineMessage
        {
            get
            {
                return _theRuntime._appOfflineMessage;
            }
        }

        public static string AspClientScriptPhysicalPath
        {
            get
            {
                string aspClientScriptPhysicalPathInternal = AspClientScriptPhysicalPathInternal;
                if (aspClientScriptPhysicalPathInternal == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Aspnet_not_installed", new object[] { VersionInfo.SystemWebVersion }));
                }
                return aspClientScriptPhysicalPathInternal;
            }
        }

        internal static string AspClientScriptPhysicalPathInternal
        {
            get
            {
                if (_theRuntime._clientScriptPhysicalPath == null)
                {
                    string str = Path.Combine(AspInstallDirectoryInternal, "asp.netclientfiles");
                    _theRuntime._clientScriptPhysicalPath = str;
                }
                return _theRuntime._clientScriptPhysicalPath;
            }
        }

        public static string AspClientScriptVirtualPath
        {
            get
            {
                if (_theRuntime._clientScriptVirtualPath == null)
                {
                    string systemWebVersion = VersionInfo.SystemWebVersion;
                    string str2 = "/aspnet_client/system_web/" + systemWebVersion.Substring(0, systemWebVersion.LastIndexOf('.')).Replace('.', '_');
                    _theRuntime._clientScriptVirtualPath = str2;
                }
                return _theRuntime._clientScriptVirtualPath;
            }
        }

        public static string AspInstallDirectory
        {
            get
            {
                string aspInstallDirectoryInternal = AspInstallDirectoryInternal;
                if (aspInstallDirectoryInternal == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Aspnet_not_installed", new object[] { VersionInfo.SystemWebVersion }));
                }
                InternalSecurityPermissions.PathDiscovery(aspInstallDirectoryInternal).Demand();
                return aspInstallDirectoryInternal;
            }
        }

        internal static string AspInstallDirectoryInternal
        {
            get
            {
                return s_installDirectory;
            }
        }

        public static string BinDirectory
        {
            get
            {
                string binDirectoryInternal = BinDirectoryInternal;
                InternalSecurityPermissions.PathDiscovery(binDirectoryInternal).Demand();
                return binDirectoryInternal;
            }
        }

        internal static string BinDirectoryInternal
        {
            get
            {
                return (Path.Combine(_theRuntime._appDomainAppPath, "bin") + Path.DirectorySeparatorChar);
            }
        }

        public static System.Web.Caching.Cache Cache
        {
            get
            {
                if (AspInstallDirectoryInternal == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Aspnet_not_installed", new object[] { VersionInfo.SystemWebVersion }));
                }
                System.Web.Caching.Cache cache = _theRuntime._cachePublic;
                if (cache == null)
                {
                    System.Web.Caching.CacheInternal cacheInternal = CacheInternal;
                    CacheSection cacheSection = RuntimeConfig.GetAppConfig().Cache;
                    cacheInternal.ReadCacheInternalConfig(cacheSection);
                    _theRuntime._cachePublic = cacheInternal.CachePublic;
                    cache = _theRuntime._cachePublic;
                }
                return cache;
            }
        }

        internal static System.Web.Caching.CacheInternal CacheInternal
        {
            get
            {
                System.Web.Caching.CacheInternal internal2 = _theRuntime._cacheInternal;
                if (internal2 == null)
                {
                    _theRuntime.CreateCache();
                    internal2 = _theRuntime._cacheInternal;
                }
                return internal2;
            }
        }

        public static string ClrInstallDirectory
        {
            get
            {
                string clrInstallDirectoryInternal = ClrInstallDirectoryInternal;
                InternalSecurityPermissions.PathDiscovery(clrInstallDirectoryInternal).Demand();
                return clrInstallDirectoryInternal;
            }
        }

        internal static string ClrInstallDirectoryInternal
        {
            get
            {
                return HttpConfigurationSystem.MsCorLibDirectory;
            }
        }

        internal static VirtualPath CodeDirectoryVirtualPath
        {
            get
            {
                return _theRuntime._appDomainAppVPath.SimpleCombineWithDir("App_Code");
            }
        }

        public static string CodegenDir
        {
            get
            {
                string codegenDirInternal = CodegenDirInternal;
                InternalSecurityPermissions.PathDiscovery(codegenDirInternal).Demand();
                return codegenDirInternal;
            }
        }

        internal static string CodegenDirInternal
        {
            get
            {
                return _theRuntime._codegenDir;
            }
        }

        internal static bool ConfigInited
        {
            get
            {
                return _theRuntime._configInited;
            }
        }

        internal static bool DebuggingEnabled
        {
            get
            {
                return _theRuntime._debuggingEnabled;
            }
        }

        internal static bool DisableProcessRequestInApplicationTrust
        {
            get
            {
                return _theRuntime._disableProcessRequestInApplicationTrust;
            }
        }

        internal static bool EnableHeaderChecking
        {
            get
            {
                return _theRuntime._enableHeaderChecking;
            }
        }

        internal static System.Web.FileChangesMonitor FileChangesMonitor
        {
            get
            {
                return _theRuntime._fcm;
            }
        }

        internal static bool FusionInited
        {
            get
            {
                return _theRuntime._fusionInited;
            }
        }

        internal static bool HostingInitFailed
        {
            get
            {
                return _theRuntime._hostingInitFailed;
            }
        }

        internal static string HostSecurityPolicyResolverType
        {
            get
            {
                return _theRuntime._hostSecurityPolicyResolverType;
            }
        }

        internal static Exception InitializationException
        {
            get
            {
                return _theRuntime._initializationError;
            }
            set
            {
                _theRuntime._initializationError = value;
                if (!HostingInitFailed)
                {
                    _theRuntime.StartAppDomainShutdownTimer();
                }
            }
        }

        internal static bool IsAspNetAppDomain
        {
            get
            {
                return (AppDomainAppIdInternal != null);
            }
        }

        internal static bool IsEngineLoaded
        {
            get
            {
                return s_isEngineLoaded;
            }
        }

        internal static bool IsFullTrust
        {
            get
            {
                return (_theRuntime._namedPermissionSet == null);
            }
        }

        internal static bool IsLegacyCas
        {
            get
            {
                return _theRuntime._isLegacyCas;
            }
        }

        internal static bool IsMapPathRelaxed
        {
            get
            {
                return (_DefaultPhysicalPathOnMapPathFailure != null);
            }
        }

        public static bool IsOnUNCShare
        {
            [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Low)]
            get
            {
                return IsOnUNCShareInternal;
            }
        }

        internal static bool IsOnUNCShareInternal
        {
            get
            {
                return _theRuntime._isOnUNCShare;
            }
        }

        internal static bool IsTrustLevelInitialized
        {
            get
            {
                if (HostingEnvironment.IsHosted)
                {
                    return (TrustLevel != null);
                }
                return true;
            }
        }

        private DateTime LastShutdownAttemptTime
        {
            get
            {
                lock (this)
                {
                    return this._lastShutdownAttemptTime;
                }
            }
            set
            {
                lock (this)
                {
                    this._lastShutdownAttemptTime = value;
                }
            }
        }

        public static string MachineConfigurationDirectory
        {
            get
            {
                string machineConfigurationDirectoryInternal = MachineConfigurationDirectoryInternal;
                InternalSecurityPermissions.PathDiscovery(machineConfigurationDirectoryInternal).Demand();
                return machineConfigurationDirectoryInternal;
            }
        }

        internal static string MachineConfigurationDirectoryInternal
        {
            get
            {
                return HttpConfigurationSystem.MachineConfigurationDirectory;
            }
        }

        internal static System.Security.NamedPermissionSet NamedPermissionSet
        {
            get
            {
                return _theRuntime._namedPermissionSet;
            }
        }

        internal static System.Security.Policy.PolicyLevel PolicyLevel
        {
            get
            {
                return _theRuntime._policyLevel;
            }
        }

        internal static bool ProcessRequestInApplicationTrust
        {
            get
            {
                return _theRuntime._processRequestInApplicationTrust;
            }
        }

        internal static Profiler Profile
        {
            get
            {
                return _theRuntime._profiler;
            }
        }

        internal static System.Web.RequestTimeoutManager RequestTimeoutManager
        {
            get
            {
                return _theRuntime._timeoutManager;
            }
        }

        internal static VirtualPath ResourcesDirectoryVirtualPath
        {
            get
            {
                return _theRuntime._appDomainAppVPath.SimpleCombineWithDir("App_GlobalResources");
            }
        }

        internal static bool ShutdownInProgress
        {
            get
            {
                return _theRuntime._shutdownInProgress;
            }
        }

        internal static ApplicationShutdownReason ShutdownReason
        {
            get
            {
                return _theRuntime._shutdownReason;
            }
        }

        internal static string TempDirInternal
        {
            get
            {
                return _theRuntime._tempDir;
            }
        }

        internal static string TrustLevel
        {
            get
            {
                return _theRuntime._trustLevel;
            }
        }

        internal static bool UseIntegratedPipeline
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return _useIntegratedPipeline;
            }
            set
            {
                _useIntegratedPipeline = value;
            }
        }

        public static bool UsingIntegratedPipeline
        {
            get
            {
                return UseIntegratedPipeline;
            }
        }

        internal static VirtualPath WebRefDirectoryVirtualPath
        {
            get
            {
                return _theRuntime._appDomainAppVPath.SimpleCombineWithDir("App_WebReferences");
            }
        }

        internal static string WpUserId
        {
            get
            {
                return _theRuntime._wpUserId;
            }
        }
    }
}


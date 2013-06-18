namespace System.ServiceModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Activation.Diagnostics;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Threading;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Routing;
    using System.Xaml.Hosting;
    using System.Xaml.Hosting.Configuration;

    [TypeForwardedFrom("System.ServiceModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public static class ServiceHostingEnvironment
    {
        private static string applicationVirtualPath;
        private static bool canGetHtmlErrorMessage = true;
        private static bool didAssemblyCheck;
        private const char FileExtensionSeparator = '.';
        private static HostingManager hostingManager;
        internal const string ISAPIApplicationIdPrefix = "/LM/W3SVC/";
        private static bool isApplicationDomainHosted;
        private static bool isHosted;
        private static bool isSimpleApplicationHost;
        private const char PathSeparator = '/';
        internal const string PathSeparatorString = "/";
        internal const string RelativeVirtualPathPrefix = "~";
        private static long requestCount;
        internal const string RootVirtualPath = "~/";
        private static string serviceActivationElementPath;
        internal const string ServiceParserDelimiter = "|";
        private static string siteName;
        private static object syncRoot = new object();
        private const string SystemWebComma = "System.Web,";
        private const char UriSchemeSeparator = ':';
        internal const string VerbPost = "POST";

        internal static void DecrementRequestCount()
        {
            Interlocked.Decrement(ref requestCount);
            if ((requestCount == 0L) && (hostingManager != null))
            {
                hostingManager.NotifyAllRequestDone();
            }
            if (System.ServiceModel.Activation.TD.WebHostRequestStopIsEnabled())
            {
                System.ServiceModel.Activation.TD.WebHostRequestStop();
            }
        }

        internal static void EnsureAllReferencedAssemblyLoaded()
        {
            BuildManager.GetReferencedAssemblies();
        }

        internal static void EnsureInitialized()
        {
            System.ServiceModel.Diagnostics.TraceUtility.SetEtwProviderId();
            if (hostingManager == null)
            {
                System.ServiceModel.Activation.FxTrace.Trace.SetAnnotation(() => System.ServiceModel.Diagnostics.TraceUtility.GetAnnotation(OperationContext.Current));
                lock (ThisLock)
                {
                    if (hostingManager == null)
                    {
                        if (!System.ServiceModel.HostingEnvironmentWrapper.IsHosted)
                        {
                            throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.SR.GetString("Hosting_ProcessNotExecutingUnderHostedContext", new object[] { "ServiceHostingEnvironment.EnsureServiceAvailable" })));
                        }
                        HostingManager manager = new HostingManager();
                        HookADUnhandledExceptionEvent();
                        Thread.MemoryBarrier();
                        isSimpleApplicationHost = GetIsSimpleApplicationHost();
                        HostedAspNetEnvironment.Enable();
                        hostingManager = manager;
                        isHosted = true;
                    }
                }
            }
        }

        public static void EnsureServiceAvailable(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                throw System.ServiceModel.Activation.FxTrace.Exception.ArgumentNull("virtualPath");
            }
            if (virtualPath.IndexOf(':') > 0)
            {
                throw System.ServiceModel.Activation.FxTrace.Exception.Argument("virtualPath", System.ServiceModel.Activation.SR.Hosting_AddressIsAbsoluteUri(virtualPath));
            }
            EnsureInitialized();
            virtualPath = NormalizeVirtualPath(virtualPath);
            EnsureServiceAvailableFast(virtualPath);
        }

        internal static void EnsureServiceAvailableFast(string relativeVirtualPath)
        {
            try
            {
                hostingManager.EnsureServiceAvailable(relativeVirtualPath);
            }
            catch (ServiceActivationException exception)
            {
                LogServiceActivationException(exception);
                throw;
            }
        }

        internal static bool EnsureWorkflowService(string path)
        {
            return PathCache.EnsurePathInfo(path).IsWorkflowService();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Uri[] GetBaseAddressPrefixFilters()
        {
            return hostingManager.BaseAddressPrefixFilters;
        }

        [SecuritySafeCritical]
        private static bool GetIsSimpleApplicationHost()
        {
            return (string.Compare("/LM/W3SVC/", 0, System.ServiceModel.HostingEnvironmentWrapper.UnsafeApplicationID, 0, "/LM/W3SVC/".Length, StringComparison.OrdinalIgnoreCase) != 0);
        }

        internal static ServiceType GetServiceType(string extension)
        {
            return hostingManager.GetServiceType(extension);
        }

        [SecuritySafeCritical]
        private static void HookADUnhandledExceptionEvent()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ServiceHostingEnvironment.OnUnhandledException);
        }

        internal static void IncrementRequestCount()
        {
            Interlocked.Increment(ref requestCount);
            if (System.ServiceModel.Activation.TD.WebHostRequestStartIsEnabled())
            {
                System.ServiceModel.Activation.TD.WebHostRequestStart();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, AspNetHostingPermission(SecurityAction.Assert, Level=AspNetHostingPermissionLevel.Minimal)]
        private static bool IsApplicationDomainHosted()
        {
            return HostingEnvironment.IsHosted;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsAspNetCompatibilityEnabled()
        {
            return hostingManager.AspNetCompatibilityEnabled;
        }

        internal static bool IsConfigurationBasedService(string virtualPath)
        {
            return hostingManager.IsConfigurationBasedServiceVirtualPath(virtualPath);
        }

        internal static bool IsConfigurationBasedService(HttpApplication application)
        {
            string str;
            return IsConfigurationBasedService(application, out str);
        }

        internal static bool IsConfigurationBasedService(HttpApplication application, out string matchedVirtualPath)
        {
            bool flag = false;
            matchedVirtualPath = null;
            string appRelativeCurrentExecutionFilePath = application.Request.AppRelativeCurrentExecutionFilePath;
            if (!string.IsNullOrEmpty(appRelativeCurrentExecutionFilePath) && hostingManager.IsConfigurationBasedServiceVirtualPath(appRelativeCurrentExecutionFilePath))
            {
                matchedVirtualPath = appRelativeCurrentExecutionFilePath;
                flag = true;
            }
            return flag;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsMultipleSiteBindingsEnabledEnabled()
        {
            return hostingManager.MultipleSiteBindingsEnabled;
        }

        [SecuritySafeCritical]
        private static void LogServiceActivationException(ServiceActivationException exception)
        {
            if (exception.InnerException is HttpException)
            {
                string message = SafeTryGetHtmlErrorMessage((HttpException) exception.InnerException);
                if (string.IsNullOrEmpty(message))
                {
                    message = exception.Message;
                }
                DiagnosticUtility.UnsafeEventLog.UnsafeLogEvent(TraceEventType.Error, EventLogCategory.WebHost, (System.ServiceModel.Diagnostics.EventLogEventId) (-1073610750), true, new string[] { System.ServiceModel.Activation.Diagnostics.TraceUtility.CreateSourceString(hostingManager), message, exception.ToString() });
            }
            else
            {
                DiagnosticUtility.UnsafeEventLog.UnsafeLogEvent(TraceEventType.Error, EventLogCategory.WebHost, (System.ServiceModel.Diagnostics.EventLogEventId) (-1073610749), true, new string[] { System.ServiceModel.Activation.Diagnostics.TraceUtility.CreateSourceString(hostingManager), exception.ToString() });
            }
            if (System.ServiceModel.Diagnostics.Application.TD.ServiceExceptionIsEnabled())
            {
                System.ServiceModel.Diagnostics.Application.TD.ServiceException(exception.ToString(), typeof(ServiceActivationException).FullName);
            }
        }

        internal static string NormalizeVirtualPath(string virtualPath)
        {
            string str = null;
            try
            {
                str = VirtualPathUtility.ToAppRelative(virtualPath, System.ServiceModel.HostingEnvironmentWrapper.ApplicationVirtualPath);
            }
            catch (HttpException exception)
            {
                throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new ArgumentException(exception.Message, "virtualPath", exception));
            }
            if (string.IsNullOrEmpty(str) || !str.StartsWith("~", StringComparison.Ordinal))
            {
                throw System.ServiceModel.Activation.FxTrace.Exception.Argument("virtualPath", System.ServiceModel.Activation.SR.Hosting_AddressPointsOutsideTheVirtualDirectory(virtualPath, System.ServiceModel.HostingEnvironmentWrapper.ApplicationVirtualPath));
            }
            int index = str.IndexOf('.');
            while (index > 0)
            {
                index = str.IndexOf('/', index + 1);
                string str2 = (index == -1) ? str : str.Substring(0, index);
                string extension = VirtualPathUtility.GetExtension(str2);
                if (!string.IsNullOrEmpty(extension) && (GetServiceType(extension) != ServiceType.Unknown))
                {
                    return str2;
                }
            }
            throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new EndpointNotFoundException(System.ServiceModel.Activation.SR.Hosting_ServiceNotExist(virtualPath)));
        }

        private static void OnEnsureInitialized(object state)
        {
            EnsureInitialized();
        }

        [SecuritySafeCritical]
        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (DiagnosticUtility.ShouldTraceError)
            {
                Exception exceptionObject = e.ExceptionObject as Exception;
                DiagnosticUtility.UnsafeEventLog.UnsafeLogEvent(TraceEventType.Error, EventLogCategory.WebHost, (System.ServiceModel.Diagnostics.EventLogEventId) (-1073610751), true, new string[] { System.ServiceModel.Activation.Diagnostics.TraceUtility.CreateSourceString(sender), (exceptionObject == null) ? string.Empty : exceptionObject.ToString() });
            }
        }

        internal static void SafeEnsureInitialized()
        {
            if (hostingManager == null)
            {
                AspNetPartialTrustHelpers.PartialTrustInvoke(new ContextCallback(ServiceHostingEnvironment.OnEnsureInitialized), null);
            }
        }

        private static string SafeTryGetHtmlErrorMessage(HttpException exception)
        {
            if ((exception != null) && canGetHtmlErrorMessage)
            {
                try
                {
                    return exception.GetHtmlErrorMessage();
                }
                catch (SecurityException exception2)
                {
                    canGetHtmlErrorMessage = false;
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                    }
                }
            }
            return null;
        }

        internal static bool ApplicationDomainHosted
        {
            get
            {
                if (!didAssemblyCheck)
                {
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    for (int i = 0; i < assemblies.Length; i++)
                    {
                        if (string.Compare(assemblies[i].FullName, 0, "System.Web,", 0, "System.Web,".Length, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            isApplicationDomainHosted = IsApplicationDomainHosted();
                            break;
                        }
                    }
                    didAssemblyCheck = true;
                }
                return isApplicationDomainHosted;
            }
        }

        internal static string ApplicationVirtualPath
        {
            get
            {
                if (applicationVirtualPath == null)
                {
                    applicationVirtualPath = HostingEnvironment.ApplicationVirtualPath;
                }
                return applicationVirtualPath;
            }
        }

        public static bool AspNetCompatibilityEnabled
        {
            get
            {
                if (!IsHosted)
                {
                    return false;
                }
                return IsAspNetCompatibilityEnabled();
            }
        }

        internal static string CurrentVirtualPath
        {
            get
            {
                return HostingManager.CurrentVirtualPath;
            }
        }

        internal static string FullVirtualPath
        {
            get
            {
                return HostingManager.FullVirtualPath;
            }
        }

        internal static bool IsConfigurationBased
        {
            get
            {
                return HostingManager.IsConfigurationBased;
            }
        }

        internal static bool IsHosted
        {
            get
            {
                return isHosted;
            }
        }

        internal static bool IsRecycling
        {
            get
            {
                return hostingManager.IsRecycling;
            }
        }

        internal static bool IsSimpleApplicationHost
        {
            get
            {
                return isSimpleApplicationHost;
            }
        }

        public static bool MultipleSiteBindingsEnabled
        {
            get
            {
                if (!IsHosted)
                {
                    return false;
                }
                return IsMultipleSiteBindingsEnabledEnabled();
            }
        }

        internal static Uri[] PrefixFilters
        {
            get
            {
                if (!IsHosted)
                {
                    return null;
                }
                return GetBaseAddressPrefixFilters();
            }
        }

        internal static string ServiceActivationElementPath
        {
            get
            {
                if (serviceActivationElementPath == null)
                {
                    serviceActivationElementPath = string.Format(CultureInfo.CurrentCulture, "{0}/{1}", new object[] { ConfigurationStrings.ServiceHostingEnvironmentSectionPath, "serviceActivations" });
                }
                return serviceActivationElementPath;
            }
        }

        internal static string SiteName
        {
            get
            {
                if (siteName == null)
                {
                    siteName = HostingEnvironment.SiteName;
                }
                return siteName;
            }
        }

        private static object ThisLock
        {
            get
            {
                return syncRoot;
            }
        }

        internal static string XamlFileBaseLocation
        {
            get
            {
                return HostingManager.XamlFileBaseLocation;
            }
        }

        private class BuildProviderInfo
        {
            [SecurityCritical]
            private System.Web.Configuration.BuildProvider buildProvider;
            private bool initialized;
            private bool isSupported;
            private bool isXamlBuildProvider;
            private object thisLock = new object();

            [SecuritySafeCritical]
            public BuildProviderInfo(System.Web.Configuration.BuildProvider buildProvider)
            {
                this.buildProvider = buildProvider;
            }

            [SecuritySafeCritical]
            private void ClearBuildProvider()
            {
                this.buildProvider = null;
            }

            private void EnsureInitialized()
            {
                if (!this.initialized)
                {
                    lock (this.thisLock)
                    {
                        if (!this.initialized)
                        {
                            Type attrProvider = Type.GetType(this.BuildProviderType, false);
                            if (attrProvider == null)
                            {
                                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                                for (int i = 0; i < assemblies.Length; i++)
                                {
                                    attrProvider = assemblies[i].GetType(this.BuildProviderType, false);
                                    if (attrProvider != null)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (attrProvider != null)
                            {
                                if (ServiceReflector.GetCustomAttributes(attrProvider, typeof(ServiceActivationBuildProviderAttribute), true).Length > 0)
                                {
                                    this.isSupported = true;
                                }
                                else if (typeof(XamlBuildProvider).IsAssignableFrom(attrProvider))
                                {
                                    this.isXamlBuildProvider = true;
                                }
                            }
                            this.ClearBuildProvider();
                            this.initialized = true;
                        }
                    }
                }
            }

            private string BuildProviderType
            {
                [SecuritySafeCritical]
                get
                {
                    return this.buildProvider.Type;
                }
            }

            public bool IsSupported
            {
                get
                {
                    this.EnsureInitialized();
                    return this.isSupported;
                }
            }

            public bool IsXamlBuildProvider
            {
                get
                {
                    this.EnsureInitialized();
                    return this.isXamlBuildProvider;
                }
            }
        }

        private class HostingManager : IRegisteredObject
        {
            private ManualResetEvent allRequestDoneInStop = new ManualResetEvent(false);
            private bool aspNetCompatibilityEnabled;
            private Uri[] baseAddressPrefixFilters;
            private static bool canDebugPrint = true;
            [ThreadStatic]
            private static string currentVirtualPath;
            private readonly Hashtable directory = new Hashtable(0x10, StringComparer.OrdinalIgnoreCase);
            private readonly ExtensionHelper extensions = new ExtensionHelper();
            [ThreadStatic]
            private static string fullVirtualPath;
            [ThreadStatic]
            private static bool isAspNetRoutedRequest;
            [ThreadStatic]
            private static bool isConfigurationBased;
            private bool isRecycling;
            private bool isRegistered;
            private bool isStopStarted;
            private bool isUnregistered;
            [SecurityCritical]
            private int minFreeMemoryPercentageToActivateService;
            private bool multipleSiteBindingsEnabled;
            private Hashtable serviceActivations;
            private static object syncRoot = new object();
            [ThreadStatic]
            private static string xamlFileBaseLocation;

            internal HostingManager()
            {
                this.LoadConfigParameters();
            }

            private void Abort()
            {
                DictionaryEntry entry;
                this.allRequestDoneInStop.Set();
                Stack stack = null;
                lock (ThisLock)
                {
                    this.isRecycling = true;
                    if (this.UnregisterObject())
                    {
                        return;
                    }
                    stack = new Stack(this.directory);
                    goto Label_0087;
                }
            Label_0049:
                entry = (DictionaryEntry) stack.Pop();
                ServiceHostingEnvironment.ServiceActivationInfo info = (ServiceHostingEnvironment.ServiceActivationInfo) entry.Value;
                if (info.Service != null)
                {
                    info.Service.Abort();
                }
                this.RemoveCachedService((string) entry.Key);
            Label_0087:
                if (stack.Count > 0)
                {
                    goto Label_0049;
                }
            }

            private ServiceHostBase ActivateService(string normalizedVirtualPath)
            {
                ServiceHostBase base2 = this.CreateService(normalizedVirtualPath);
                base2.Closed += new EventHandler(this.OnServiceClosed);
                base2.Faulted += new EventHandler(this.OnServiceFaulted);
                this.FailActivationIfRecyling(normalizedVirtualPath);
                try
                {
                    if (System.ServiceModel.Activation.TD.ServiceHostOpenStartIsEnabled())
                    {
                        System.ServiceModel.Activation.TD.ServiceHostOpenStart();
                    }
                    base2.Open();
                    if (System.ServiceModel.Activation.TD.ServiceHostOpenStopIsEnabled())
                    {
                        System.ServiceModel.Activation.TD.ServiceHostOpenStop();
                    }
                }
                finally
                {
                    if (base2.State != CommunicationState.Opened)
                    {
                        base2.Abort();
                    }
                }
                if (System.ServiceModel.Activation.TD.AspNetRoutingServiceIsEnabled() && isAspNetRoutedRequest)
                {
                    System.ServiceModel.Activation.TD.AspNetRoutingService(normalizedVirtualPath);
                }
                return base2;
            }

            [SecuritySafeCritical]
            private void CheckMemoryGates()
            {
                ServiceMemoryGates.Check(this.minFreeMemoryPercentageToActivateService);
            }

            private ServiceHostBase CreateService(string normalizedVirtualPath)
            {
                string virtualPath;
                string serviceType;
                string str2 = "";
                ServiceHostBase base2 = null;
                ServiceHostFactoryBase serviceHostFactory = null;
                string[] strArray = null;
                string compiledCustomString = "";
                if (System.ServiceModel.Activation.TD.CompilationStartIsEnabled())
                {
                    System.ServiceModel.Activation.TD.CompilationStart();
                }
                if (isAspNetRoutedRequest && isConfigurationBased)
                {
                    if (!RouteTable.Routes.RouteExistingFiles)
                    {
                        ServiceRouteHandler.MarkARouteAsInactive(normalizedVirtualPath);
                        isAspNetRoutedRequest = false;
                    }
                    else
                    {
                        isConfigurationBased = false;
                    }
                }
                if (!isAspNetRoutedRequest)
                {
                    compiledCustomString = this.GetCompiledCustomString(normalizedVirtualPath);
                    if (string.IsNullOrEmpty(compiledCustomString))
                    {
                        string name = System.ServiceModel.HostingEnvironmentWrapper.GetServiceFile(normalizedVirtualPath).Name;
                        string str6 = normalizedVirtualPath.Substring(0, normalizedVirtualPath.LastIndexOf('/') + 1);
                        normalizedVirtualPath = string.Format(CultureInfo.CurrentCulture, "{0}{1}", new object[] { str6, name });
                        serviceType = virtualPath = normalizedVirtualPath;
                        serviceHostFactory = this.CreateWorkflowServiceHostFactory(normalizedVirtualPath);
                    }
                    else
                    {
                        strArray = compiledCustomString.Split("|".ToCharArray());
                        if (strArray.Length < 3)
                        {
                            throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_CompilationResultInvalid(normalizedVirtualPath)));
                        }
                        virtualPath = strArray[0];
                        str2 = strArray[1];
                        serviceType = strArray[2];
                    }
                }
                else
                {
                    ServiceDeploymentInfo serviceInfo = ServiceRouteHandler.GetServiceInfo(normalizedVirtualPath);
                    virtualPath = serviceInfo.VirtualPath;
                    serviceType = serviceInfo.ServiceType;
                    serviceHostFactory = serviceInfo.ServiceHostFactory;
                }
                normalizedVirtualPath = virtualPath;
                virtualPath = VirtualPathUtility.ToAbsolute(virtualPath);
                Uri[] baseAddresses = HostedTransportConfigurationManager.GetBaseAddresses(virtualPath);
                Uri[] prefixFilters = ServiceHostingEnvironment.PrefixFilters;
                if ((!this.multipleSiteBindingsEnabled && (prefixFilters != null)) && (prefixFilters.Length > 0))
                {
                    baseAddresses = FilterBaseAddressList(baseAddresses, prefixFilters);
                }
                fullVirtualPath = virtualPath;
                if (fullVirtualPath.Length == 0)
                {
                    fullVirtualPath = "/";
                }
                currentVirtualPath = virtualPath.Substring(0, virtualPath.LastIndexOf('/'));
                if (currentVirtualPath.Length == 0)
                {
                    currentVirtualPath = "/";
                    xamlFileBaseLocation = "~/";
                }
                else
                {
                    xamlFileBaseLocation = VirtualPathUtility.AppendTrailingSlash(currentVirtualPath);
                }
                if (isConfigurationBased)
                {
                    xamlFileBaseLocation = "~/";
                    if (System.ServiceModel.Activation.TD.CBAMatchFoundIsEnabled())
                    {
                        System.ServiceModel.Activation.TD.CBAMatchFound(normalizedVirtualPath);
                    }
                }
                if (System.ServiceModel.Activation.TD.ServiceHostFactoryCreationStartIsEnabled())
                {
                    System.ServiceModel.Activation.TD.ServiceHostFactoryCreationStart();
                }
                if (serviceHostFactory == null)
                {
                    if (string.IsNullOrEmpty(str2))
                    {
                        serviceHostFactory = new ServiceHostFactory();
                    }
                    else
                    {
                        Type c = Type.GetType(str2);
                        if ((c == null) && isConfigurationBased)
                        {
                            ServiceHostingEnvironment.EnsureAllReferencedAssemblyLoaded();
                            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                            for (int i = 0; i < assemblies.Length; i++)
                            {
                                c = assemblies[i].GetType(str2, false);
                                if (c != null)
                                {
                                    break;
                                }
                            }
                        }
                        if (c == null)
                        {
                            throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_FactoryTypeNotResolved(str2)));
                        }
                        if (!typeof(ServiceHostFactoryBase).IsAssignableFrom(c))
                        {
                            throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_IServiceHostNotImplemented(str2)));
                        }
                        ConstructorInfo constructor = c.GetConstructor(new Type[0]);
                        if (constructor == null)
                        {
                            throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_NoDefaultCtor(str2)));
                        }
                        serviceHostFactory = (ServiceHostFactoryBase) constructor.Invoke(new object[0]);
                    }
                }
                if (System.ServiceModel.Activation.TD.ServiceHostFactoryCreationStopIsEnabled())
                {
                    System.ServiceModel.Activation.TD.ServiceHostFactoryCreationStop();
                }
                if (((serviceHostFactory is ServiceHostFactory) && !isConfigurationBased) && !isAspNetRoutedRequest)
                {
                    for (int j = 3; j < strArray.Length; j++)
                    {
                        ((ServiceHostFactory) serviceHostFactory).AddAssemblyReference(strArray[j]);
                    }
                }
                if (System.ServiceModel.Activation.TD.CreateServiceHostStartIsEnabled())
                {
                    System.ServiceModel.Activation.TD.CreateServiceHostStart();
                }
                base2 = serviceHostFactory.CreateServiceHost(serviceType, baseAddresses);
                if (System.ServiceModel.Activation.TD.CreateServiceHostStopIsEnabled())
                {
                    System.ServiceModel.Activation.TD.CreateServiceHostStop();
                }
                if (base2 == null)
                {
                    throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_ServiceHostBaseIsNull(serviceType)));
                }
                base2.Extensions.Add(new VirtualPathExtension(normalizedVirtualPath, ServiceHostingEnvironment.ApplicationVirtualPath, ServiceHostingEnvironment.SiteName));
                if (base2.Description != null)
                {
                    base2.Description.Behaviors.Add(new ApplyHostConfigurationBehavior());
                    if (this.multipleSiteBindingsEnabled && (base2.Description.Behaviors.Find<UseRequestHeadersForMetadataAddressBehavior>() == null))
                    {
                        base2.Description.Behaviors.Add(new UseRequestHeadersForMetadataAddressBehavior());
                    }
                }
                if (System.ServiceModel.Activation.TD.CompilationStopIsEnabled())
                {
                    System.ServiceModel.Activation.TD.CompilationStop();
                }
                return base2;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private ServiceHostFactoryBase CreateWorkflowServiceHostFactory(string path)
            {
                return ServiceHostingEnvironment.PathCache.EnsurePathInfo(path).ServiceModelActivationHandler.GetFactory();
            }

            private void EndCloseService(IAsyncResult result)
            {
                DictionaryEntry asyncState = (DictionaryEntry) result.AsyncState;
                try
                {
                    ((ServiceHostingEnvironment.ServiceActivationInfo) asyncState.Value).Service.EndClose(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.LogServiceCloseError((string) asyncState.Key, exception);
                }
                this.RemoveCachedService((string) asyncState.Key);
            }

            internal void EnsureServiceAvailable(string normalizedVirtualPath)
            {
                ServiceHostingEnvironment.ServiceActivationInfo info = null;
                info = (ServiceHostingEnvironment.ServiceActivationInfo) this.directory[normalizedVirtualPath];
                if ((info == null) || (info.Service == null))
                {
                    isAspNetRoutedRequest = ServiceRouteHandler.IsActiveAspNetRoute(normalizedVirtualPath);
                    isConfigurationBased = this.IsConfigurationBasedServiceVirtualPath(normalizedVirtualPath);
                    lock (ThisLock)
                    {
                        if (!this.isRegistered)
                        {
                            this.RegisterObject();
                            this.isRegistered = true;
                        }
                        info = (ServiceHostingEnvironment.ServiceActivationInfo) this.directory[normalizedVirtualPath];
                        if ((info != null) && (info.Service != null))
                        {
                            return;
                        }
                        this.FailActivationIfRecyling(normalizedVirtualPath);
                        if (info == null)
                        {
                            if ((!isAspNetRoutedRequest && !isConfigurationBased) && !System.ServiceModel.HostingEnvironmentWrapper.ServiceFileExists(normalizedVirtualPath))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.Activation.SR.Hosting_ServiceNotExist(VirtualPathUtility.ToAbsolute(normalizedVirtualPath, System.ServiceModel.HostingEnvironmentWrapper.ApplicationVirtualPath))));
                            }
                            info = new ServiceHostingEnvironment.ServiceActivationInfo(normalizedVirtualPath);
                            this.directory.Add(normalizedVirtualPath, info);
                        }
                    }
                    ServiceHostBase base2 = null;
                    lock (info)
                    {
                        if (info.Service != null)
                        {
                            return;
                        }
                        this.FailActivationIfRecyling(normalizedVirtualPath);
                        try
                        {
                            this.CheckMemoryGates();
                            base2 = this.ActivateService(normalizedVirtualPath);
                            lock (ThisLock)
                            {
                                if (!this.IsRecycling)
                                {
                                    info.Service = base2;
                                }
                            }
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                System.ServiceModel.Activation.Diagnostics.TraceUtility.TraceEvent(TraceEventType.Information, 0x90002, System.ServiceModel.Activation.SR.TraceCodeWebHostServiceActivated, new StringTraceRecord("VirtualPath", VirtualPathUtility.ToAbsolute(normalizedVirtualPath, System.ServiceModel.HostingEnvironmentWrapper.ApplicationVirtualPath)), this, null);
                            }
                            if (System.ServiceModel.Activation.TD.ServiceHostStartedIsEnabled())
                            {
                                string fullName = string.Empty;
                                ServiceHostBase base3 = base2;
                                if (base3 != null)
                                {
                                    if (null != base3.Description.ServiceType)
                                    {
                                        fullName = base3.Description.ServiceType.FullName;
                                    }
                                    else
                                    {
                                        fullName = base3.Description.Namespace + base3.Description.Name;
                                    }
                                }
                                if (string.IsNullOrEmpty(fullName))
                                {
                                    fullName = System.ServiceModel.Activation.SR.ServiceTypeUnknown;
                                }
                                string str2 = normalizedVirtualPath.Replace("~", ServiceHostingEnvironment.ApplicationVirtualPath + "|");
                                string reference = string.Format(CultureInfo.InvariantCulture, "{0}{1}|{2}", new object[] { ServiceHostingEnvironment.SiteName, str2, base3.Description.Name });
                                System.ServiceModel.Activation.TD.ServiceHostStarted(fullName, reference);
                            }
                        }
                        catch (HttpCompileException exception)
                        {
                            throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new ServiceActivationException(System.ServiceModel.Activation.SR.Hosting_ServiceCannotBeActivated(VirtualPathUtility.ToAbsolute(normalizedVirtualPath, System.ServiceModel.HostingEnvironmentWrapper.ApplicationVirtualPath), exception.Message), exception));
                        }
                        catch (ServiceActivationException)
                        {
                            throw;
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new ServiceActivationException(System.ServiceModel.Activation.SR.Hosting_ServiceCannotBeActivated(VirtualPathUtility.ToAbsolute(normalizedVirtualPath, System.ServiceModel.HostingEnvironmentWrapper.ApplicationVirtualPath), exception2.Message), exception2));
                        }
                        finally
                        {
                            currentVirtualPath = null;
                            fullVirtualPath = null;
                            xamlFileBaseLocation = null;
                        }
                    }
                    if (info.Service == null)
                    {
                        base2.Abort();
                    }
                    this.FailActivationIfRecyling(normalizedVirtualPath);
                }
            }

            private void FailActivationIfRecyling(string normalizedVirtualPath)
            {
                if (this.IsRecycling)
                {
                    InvalidOperationException innerException = new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_EnvironmentShuttingDown(normalizedVirtualPath, System.ServiceModel.HostingEnvironmentWrapper.ApplicationVirtualPath));
                    throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new ServiceActivationException(innerException.Message, innerException));
                }
            }

            private static Uri[] FilterBaseAddressList(Uri[] baseAddresses, Uri[] prefixFilters)
            {
                List<Uri> list = new List<Uri>();
                Dictionary<string, Uri> dictionary = new Dictionary<string, Uri>();
                foreach (Uri uri in prefixFilters)
                {
                    if (dictionary.ContainsKey(uri.Scheme))
                    {
                        throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.SR.GetString("BaseAddressDuplicateScheme", new object[] { uri.Scheme })));
                    }
                    dictionary.Add(uri.Scheme, uri);
                }
                foreach (Uri uri2 in baseAddresses)
                {
                    string scheme = uri2.Scheme;
                    if (dictionary.ContainsKey(scheme))
                    {
                        Uri uri3 = dictionary[scheme];
                        if ((uri2.Port == uri3.Port) && (string.Compare(uri2.Host, uri3.Host, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            list.Add(uri2);
                        }
                    }
                    else
                    {
                        list.Add(uri2);
                    }
                }
                return list.ToArray();
            }

            [SecuritySafeCritical]
            private string GetCompiledCustomString(string normalizedVirtualPath)
            {
                string str2;
                try
                {
                    using (IDisposable disposable = null)
                    {
                        string compiledCustomString = null;
                        if (!this.TryGetCompiledCustomStringFromCBA(normalizedVirtualPath, out compiledCustomString))
                        {
                            try
                            {
                            }
                            finally
                            {
                                disposable = System.ServiceModel.HostingEnvironmentWrapper.UnsafeImpersonate();
                            }
                            compiledCustomString = BuildManager.GetCompiledCustomString(normalizedVirtualPath);
                        }
                        str2 = compiledCustomString;
                    }
                }
                catch
                {
                    throw;
                }
                return str2;
            }

            [SecuritySafeCritical]
            internal Type GetCompiledType(string normalizedVirtualPath)
            {
                Type compiledType;
                try
                {
                    using (IDisposable disposable = null)
                    {
                        try
                        {
                        }
                        finally
                        {
                            disposable = System.ServiceModel.HostingEnvironmentWrapper.UnsafeImpersonate();
                        }
                        compiledType = BuildManager.GetCompiledType(normalizedVirtualPath);
                    }
                }
                catch
                {
                    throw;
                }
                return compiledType;
            }

            internal ServiceHostingEnvironment.ServiceType GetServiceType(string extension)
            {
                return this.extensions.GetServiceType(extension);
            }

            internal bool IsConfigurationBasedServiceVirtualPath(string normalizedVirtualPath)
            {
                return this.serviceActivations.ContainsKey(normalizedVirtualPath);
            }

            [SecuritySafeCritical]
            private void LoadConfigParameters()
            {
                ServiceHostingEnvironmentSection section = ServiceHostingEnvironmentSection.UnsafeGetSection();
                this.aspNetCompatibilityEnabled = section.AspNetCompatibilityEnabled;
                this.multipleSiteBindingsEnabled = section.MultipleSiteBindingsEnabled;
                this.minFreeMemoryPercentageToActivateService = section.MinFreeMemoryPercentageToActivateService;
                List<Uri> list = new List<Uri>();
                foreach (BaseAddressPrefixFilterElement element in section.BaseAddressPrefixFilters)
                {
                    list.Add(element.Prefix);
                }
                this.baseAddressPrefixFilters = list.ToArray();
                this.serviceActivations = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                foreach (ServiceActivationElement element2 in section.ServiceActivations)
                {
                    if (string.IsNullOrEmpty(element2.Factory) && string.IsNullOrEmpty(element2.Service))
                    {
                        throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new ConfigurationErrorsException(System.ServiceModel.Activation.SR.Hosting_NoServiceAndFactorySpecifiedForFilelessService("factory", "service", element2.RelativeAddress, ServiceHostingEnvironment.ServiceActivationElementPath)));
                    }
                    string key = this.NormalizedRelativeAddress(element2.RelativeAddress);
                    string str2 = string.Format(CultureInfo.CurrentCulture, "{0}|{1}|{2}", new object[] { key, element2.Factory, element2.Service });
                    try
                    {
                        this.serviceActivations.Add(key, str2);
                        if (System.ServiceModel.Activation.TD.CBAEntryReadIsEnabled())
                        {
                            System.ServiceModel.Activation.TD.CBAEntryRead(element2.RelativeAddress, key);
                        }
                    }
                    catch (ArgumentException)
                    {
                        throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new ConfigurationErrorsException(System.ServiceModel.Activation.SR.Hosting_RelativeAddressHasBeenAdded(element2.RelativeAddress, ServiceHostingEnvironment.ServiceActivationElementPath)));
                    }
                }
            }

            private void LogServiceCloseError(string virtualPath, Exception exception)
            {
                if (DiagnosticUtility.ShouldTraceError)
                {
                    System.ServiceModel.Activation.Diagnostics.TraceUtility.TraceEvent(TraceEventType.Error, 0x90007, System.ServiceModel.Activation.SR.TraceCodeWebHostServiceCloseFailed, new StringTraceRecord("VirtualPath", VirtualPathUtility.ToAbsolute(virtualPath, System.ServiceModel.HostingEnvironmentWrapper.ApplicationVirtualPath)), this, exception);
                }
            }

            internal string NormalizedRelativeAddress(string relativeAddress)
            {
                string str = relativeAddress;
                try
                {
                    if (VirtualPathUtility.IsAbsolute(relativeAddress))
                    {
                        throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new ConfigurationErrorsException(System.ServiceModel.Activation.SR.Hosting_RelativeAddressFormatError(relativeAddress)));
                    }
                    relativeAddress = VirtualPathUtility.Combine("~/", relativeAddress);
                    string extension = VirtualPathUtility.GetExtension(relativeAddress);
                    if (string.IsNullOrEmpty(extension))
                    {
                        throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new ConfigurationErrorsException(System.ServiceModel.Activation.SR.Hosting_NoValidExtensionFoundForRegistedFilelessService(str, ServiceHostingEnvironment.ServiceActivationElementPath)));
                    }
                    if (this.GetServiceType(extension) == ServiceHostingEnvironment.ServiceType.Unknown)
                    {
                        throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new ConfigurationErrorsException(System.ServiceModel.Activation.SR.Hosting_RelativeAddressExtensionNotSupportError(extension, str, ServiceHostingEnvironment.ServiceActivationElementPath)));
                    }
                }
                catch (HttpException exception)
                {
                    throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new ConfigurationErrorsException(System.ServiceModel.Activation.SR.Hosting_RelativeAddressFormatError(str), exception));
                }
                return relativeAddress;
            }

            internal void NotifyAllRequestDone()
            {
                if (this.isStopStarted)
                {
                    this.allRequestDoneInStop.Set();
                }
            }

            private void OnCloseService(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    this.EndCloseService(result);
                }
            }

            private void OnServiceClosed(object sender, EventArgs e)
            {
                lock (ThisLock)
                {
                    if (!this.isRecycling)
                    {
                        ServiceHostBase base2 = (ServiceHostBase) sender;
                        string key = null;
                        foreach (string str2 in this.directory.Keys)
                        {
                            if (((ServiceHostingEnvironment.ServiceActivationInfo) this.directory[str2]).Service == base2)
                            {
                                key = str2;
                                break;
                            }
                        }
                        if (key != null)
                        {
                            this.directory.Remove(key);
                        }
                    }
                }
            }

            private void OnServiceFaulted(object sender, EventArgs e)
            {
                ((ServiceHostBase) sender).Abort();
            }

            [SecuritySafeCritical]
            private void RegisterObject()
            {
                System.ServiceModel.HostingEnvironmentWrapper.UnsafeRegisterObject(this);
            }

            private void RemoveCachedService(string path)
            {
                lock (ThisLock)
                {
                    this.directory.Remove(path);
                    this.UnregisterObject();
                }
            }

            public void Stop(bool immediate)
            {
                if (!immediate)
                {
                    ActionItem.Schedule(new Action<object>(this.WaitAndCloseCallback), this);
                }
                else
                {
                    this.Abort();
                }
            }

            [Conditional("DEBUG")]
            private static void TryDebugPrint(string message)
            {
                if (canDebugPrint)
                {
                }
            }

            internal bool TryGetCompiledCustomStringFromCBA(string normalizedVirtualPath, out string compiledCustomString)
            {
                compiledCustomString = null;
                bool flag = false;
                if (isConfigurationBased)
                {
                    compiledCustomString = (string) this.serviceActivations[normalizedVirtualPath];
                    flag = true;
                }
                return flag;
            }

            [SecuritySafeCritical]
            private bool UnregisterObject()
            {
                if (this.directory.Count != 0)
                {
                    return false;
                }
                if (!this.isUnregistered)
                {
                    this.isUnregistered = true;
                    System.ServiceModel.HostingEnvironmentWrapper.UnsafeUnregisterObject(this);
                }
                return true;
            }

            private void WaitAndCloseCallback(object obj)
            {
                this.isStopStarted = true;
                if (ServiceHostingEnvironment.requestCount != 0L)
                {
                    this.allRequestDoneInStop.WaitOne();
                }
                Stack stack = null;
                lock (ThisLock)
                {
                    if (this.UnregisterObject())
                    {
                        return;
                    }
                    stack = new Stack(this.directory);
                }
                AsyncCallback callback = null;
                while (stack.Count > 0)
                {
                    DictionaryEntry state = (DictionaryEntry) stack.Pop();
                    ServiceHostingEnvironment.ServiceActivationInfo info = (ServiceHostingEnvironment.ServiceActivationInfo) state.Value;
                    if (info.Service != null)
                    {
                        if (callback == null)
                        {
                            callback = Fx.ThunkCallback(new AsyncCallback(this.OnCloseService));
                        }
                        IAsyncResult result = null;
                        try
                        {
                            result = info.Service.BeginClose(TimeSpan.MaxValue, callback, state);
                        }
                        catch (Exception exception)
                        {
                            if (!Fx.IsFatal(exception))
                            {
                                this.LogServiceCloseError((string) state.Key, exception);
                            }
                            if (!(exception is CommunicationException))
                            {
                                throw;
                            }
                            this.RemoveCachedService((string) state.Key);
                        }
                        if ((result != null) && result.CompletedSynchronously)
                        {
                            this.EndCloseService(result);
                        }
                    }
                    else
                    {
                        this.RemoveCachedService((string) state.Key);
                    }
                }
            }

            internal bool AspNetCompatibilityEnabled
            {
                get
                {
                    return this.aspNetCompatibilityEnabled;
                }
            }

            internal Uri[] BaseAddressPrefixFilters
            {
                get
                {
                    return this.baseAddressPrefixFilters;
                }
            }

            internal static string CurrentVirtualPath
            {
                get
                {
                    return currentVirtualPath;
                }
            }

            internal static string FullVirtualPath
            {
                get
                {
                    return fullVirtualPath;
                }
            }

            internal static bool IsConfigurationBased
            {
                get
                {
                    return isConfigurationBased;
                }
            }

            internal bool IsRecycling
            {
                get
                {
                    return this.isRecycling;
                }
            }

            internal bool MultipleSiteBindingsEnabled
            {
                get
                {
                    return this.multipleSiteBindingsEnabled;
                }
            }

            internal static object ThisLock
            {
                get
                {
                    return syncRoot;
                }
            }

            internal static string XamlFileBaseLocation
            {
                get
                {
                    return xamlFileBaseLocation;
                }
            }

            private class ExtensionHelper
            {
                private readonly IDictionary<string, ServiceHostingEnvironment.BuildProviderInfo> buildProviders = new Dictionary<string, ServiceHostingEnvironment.BuildProviderInfo>(8, StringComparer.OrdinalIgnoreCase);

                [SecuritySafeCritical]
                public ExtensionHelper()
                {
                    CompilationSection section = (CompilationSection) HostedAspNetEnvironment.UnsafeGetSectionFromWebConfigurationManager("system.web/compilation", null);
                    foreach (System.Web.Configuration.BuildProvider provider in section.BuildProviders)
                    {
                        this.buildProviders.Add(provider.Extension, new ServiceHostingEnvironment.BuildProviderInfo(provider));
                    }
                }

                public ServiceHostingEnvironment.ServiceType GetServiceType(string extension)
                {
                    ServiceHostingEnvironment.BuildProviderInfo info;
                    ServiceHostingEnvironment.ServiceType unknown = ServiceHostingEnvironment.ServiceType.Unknown;
                    if (this.buildProviders.TryGetValue(extension, out info))
                    {
                        if (info.IsSupported)
                        {
                            return ServiceHostingEnvironment.ServiceType.WCF;
                        }
                        if (info.IsXamlBuildProvider)
                        {
                            unknown = ServiceHostingEnvironment.ServiceType.Workflow;
                        }
                    }
                    return unknown;
                }
            }
        }

        private static class PathCache
        {
            private static Hashtable pathCache = new Hashtable(StringComparer.OrdinalIgnoreCase);
            private static object writeLock = new object();

            public static ServiceHostingEnvironment.PathInfo EnsurePathInfo(string path)
            {
                ServiceHostingEnvironment.PathInfo info = (ServiceHostingEnvironment.PathInfo) pathCache[path];
                if (info != null)
                {
                    return info;
                }
                lock (writeLock)
                {
                    info = (ServiceHostingEnvironment.PathInfo) pathCache[path];
                    if (info == null)
                    {
                        if (!System.ServiceModel.HostingEnvironmentWrapper.ServiceFileExists(path))
                        {
                            throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new EndpointNotFoundException(System.ServiceModel.Activation.SR.Hosting_ServiceNotExist(path)));
                        }
                        info = new ServiceHostingEnvironment.PathInfo(path);
                        pathCache.Add(path, info);
                    }
                    return info;
                }
            }
        }

        private class PathInfo
        {
            private Type hostedXamlType;
            private string path;
            private IServiceModelActivationHandler serviceModelActivationHandler;
            private Type serviceModelActivationHandlerType;
            private PathType type = PathType.Unknown;
            private object writeLock;

            public PathInfo(string path)
            {
                this.path = path;
                this.writeLock = new object();
            }

            private static object CreateServiceModelActivationHandler(Type type)
            {
                return Activator.CreateInstance(type, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, null);
            }

            private bool IsConfiguredWithSMActivationHandler()
            {
                return (XamlHostingConfiguration.TryGetHttpHandlerType(this.path, this.hostedXamlType, out this.serviceModelActivationHandlerType) && typeof(IServiceModelActivationHandler).IsAssignableFrom(this.serviceModelActivationHandlerType));
            }

            public bool IsWorkflowService()
            {
                if (this.type == PathType.Unknown)
                {
                    lock (this.writeLock)
                    {
                        if (this.type == PathType.Unknown)
                        {
                            this.hostedXamlType = ServiceHostingEnvironment.hostingManager.GetCompiledType(this.path);
                            if (this.IsConfiguredWithSMActivationHandler())
                            {
                                this.type = PathType.WorkflowService;
                            }
                            else
                            {
                                this.type = PathType.NotWorkflowService;
                            }
                        }
                    }
                }
                return (this.type == PathType.WorkflowService);
            }

            public IServiceModelActivationHandler ServiceModelActivationHandler
            {
                get
                {
                    if (this.serviceModelActivationHandler == null)
                    {
                        if (!this.IsWorkflowService())
                        {
                            throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new EndpointNotFoundException(System.ServiceModel.Activation.SR.Hosting_InvalidHandlerForWorkflowService(this.serviceModelActivationHandlerType.FullName, this.hostedXamlType.FullName, this.path)));
                        }
                        this.serviceModelActivationHandler = CreateServiceModelActivationHandler(this.serviceModelActivationHandlerType) as IServiceModelActivationHandler;
                    }
                    return this.serviceModelActivationHandler;
                }
            }

            private enum PathType
            {
                Unknown,
                WorkflowService,
                NotWorkflowService
            }
        }

        private class ServiceActivationInfo
        {
            private ServiceHostBase service;
            private string virtualPath;

            public ServiceActivationInfo(string virtualPath)
            {
                this.virtualPath = virtualPath;
            }

            public ServiceHostBase Service
            {
                get
                {
                    return this.service;
                }
                set
                {
                    this.service = value;
                }
            }
        }

        internal enum ServiceType
        {
            Unknown,
            WCF,
            Workflow
        }
    }
}


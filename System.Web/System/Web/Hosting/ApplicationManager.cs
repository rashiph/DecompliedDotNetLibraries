namespace System.Web.Hosting
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.Util;

    public sealed class ApplicationManager : MarshalByRefObject
    {
        private int _accessibleHostingEnvCount;
        private int _activeHostingEnvCount;
        private Dictionary<string, LockableAppDomainContext> _appDomains = new Dictionary<string, LockableAppDomainContext>(StringComparer.OrdinalIgnoreCase);
        private static object _applicationManagerStaticLock = new object();
        private static CacheManager _cm;
        private static Exception _fatalException = null;
        private static readonly StrongName _mwiV1StrongName = GetMicrosoftWebInfrastructureV1StrongName();
        private WaitCallback _onRespondToPingWaitCallback;
        private int _openCount;
        private object _pendingPingCallback;
        private bool _shutdownInProgress;
        private static ApplicationManager _theAppManager;
        private static int s_domainCount = 0;
        private static object s_domainCountLock = new object();

        internal ApplicationManager()
        {
            this._onRespondToPingWaitCallback = new WaitCallback(this.OnRespondToPingWaitCallback);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ApplicationManager.OnUnhandledException);
        }

        private Dictionary<string, LockableAppDomainContext> CloneAppDomainsCollection()
        {
            lock (this)
            {
                return new Dictionary<string, LockableAppDomainContext>(this._appDomains, StringComparer.OrdinalIgnoreCase);
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public void Close()
        {
            if (Interlocked.Decrement(ref this._openCount) <= 0)
            {
                this.ShutdownAll();
            }
        }

        private static string ConstructAppDomainId(string id)
        {
            int num = 0;
            lock (s_domainCountLock)
            {
                num = ++s_domainCount;
            }
            return (id + "-" + num.ToString(NumberFormatInfo.InvariantInfo) + "-" + DateTime.UtcNow.ToFileTime().ToString());
        }

        private HostingEnvironment CreateAppDomainWithHostingEnvironment(string appId, IApplicationHost appHost, HostingEnvironmentParameters hostingParameters)
        {
            string physicalPath = appHost.GetPhysicalPath();
            if (!System.Web.Util.StringUtil.StringEndsWith(physicalPath, Path.DirectorySeparatorChar))
            {
                physicalPath = physicalPath + Path.DirectorySeparatorChar;
            }
            string domainId = ConstructAppDomainId(appId);
            string appName = System.Web.Util.StringUtil.GetStringHashCode(appId.ToLower(CultureInfo.InvariantCulture) + physicalPath.ToLower(CultureInfo.InvariantCulture)).ToString("x", CultureInfo.InvariantCulture);
            VirtualPath appVPath = VirtualPath.Create(appHost.GetVirtualPath());
            IDictionary dict = new Hashtable(20);
            AppDomainSetup setup = new AppDomainSetup();
            PopulateDomainBindings(domainId, appId, appName, physicalPath, appVPath, setup, dict);
            AppDomain domain = null;
            Exception innerException = null;
            string siteID = appHost.GetSiteID();
            string virtualPathStringNoTrailingSlash = appVPath.VirtualPathStringNoTrailingSlash;
            bool flag = false;
            bool flag2 = false;
            System.Configuration.Configuration configuration = null;
            PolicyLevel policyLevel = null;
            PermissionSet grantSet = null;
            List<StrongName> list = new List<StrongName>();
            string[] strArray = new string[] { "System.Web, PublicKey=002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293", "System.Web.Extensions, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9", "System.Web.Abstractions, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9", "System.Web.Routing, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9", "System.ComponentModel.DataAnnotations, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9", "System.Web.DynamicData, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9", "System.Web.DataVisualization, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9", "System.Web.ApplicationServices, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9" };
            Exception appDomainCreationException = null;
            ImpersonationContext context = null;
            IntPtr zero = IntPtr.Zero;
            if ((hostingParameters != null) && ((hostingParameters.HostingFlags & HostingEnvironmentFlags.ClientBuildManager) != HostingEnvironmentFlags.Default))
            {
                flag2 = true;
                setup.LoaderOptimization = LoaderOptimization.MultiDomainHost;
            }
            try
            {
                zero = appHost.GetConfigToken();
                if (zero != IntPtr.Zero)
                {
                    context = new ImpersonationContext(zero);
                }
                try
                {
                    if (flag2 && (hostingParameters.IISExpressVersion != null))
                    {
                        grantSet = new PermissionSet(PermissionState.Unrestricted);
                        setup.PartialTrustVisibleAssemblies = strArray;
                    }
                    else
                    {
                        if (appHost is ISAPIApplicationHost)
                        {
                            string key = "f" + siteID + appVPath.VirtualPathString;
                            MapPathCacheInfo info1 = (MapPathCacheInfo) HttpRuntime.CacheInternal.Remove(key);
                            configuration = WebConfigurationManager.OpenWebConfiguration(virtualPathStringNoTrailingSlash, siteID);
                        }
                        else
                        {
                            WebConfigurationFileMap fileMap = new WebConfigurationFileMap();
                            IConfigMapPath path2 = appHost.GetConfigMapPathFactory().Create(appVPath.VirtualPathString, physicalPath);
                            string directory = null;
                            string baseName = null;
                            string path = "/";
                            path2.GetPathConfigFilename(siteID, path, out directory, out baseName);
                            if (directory != null)
                            {
                                fileMap.VirtualDirectories.Add(path, new VirtualDirectoryMapping(Path.GetFullPath(directory), true));
                            }
                            foreach (string str10 in virtualPathStringNoTrailingSlash.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                path = path + str10;
                                path2.GetPathConfigFilename(siteID, path, out directory, out baseName);
                                if (directory != null)
                                {
                                    fileMap.VirtualDirectories.Add(path, new VirtualDirectoryMapping(Path.GetFullPath(directory), true));
                                }
                                path = path + "/";
                            }
                            configuration = WebConfigurationManager.OpenMappedWebConfiguration(fileMap, virtualPathStringNoTrailingSlash, siteID);
                        }
                        TrustSection trustSection = (TrustSection) configuration.GetSection("system.web/trust");
                        if ((trustSection == null) || string.IsNullOrEmpty(trustSection.Level))
                        {
                            throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_section_not_present", new object[] { "trust" }));
                        }
                        bool legacyCasModel = trustSection.LegacyCasModel;
                        if (legacyCasModel)
                        {
                            SetNetFx40LegacySecurityPolicy(setup);
                        }
                        if (flag2)
                        {
                            grantSet = new PermissionSet(PermissionState.Unrestricted);
                            setup.PartialTrustVisibleAssemblies = strArray;
                        }
                        else
                        {
                            flag = legacyCasModel;
                            if (!flag)
                            {
                                if (trustSection.Level == "Full")
                                {
                                    grantSet = new PermissionSet(PermissionState.Unrestricted);
                                    setup.PartialTrustVisibleAssemblies = strArray;
                                }
                                else
                                {
                                    SecurityPolicySection section = (SecurityPolicySection) configuration.GetSection("system.web/securityPolicy");
                                    CompilationSection compilationSection = (CompilationSection) configuration.GetSection("system.web/compilation");
                                    FullTrustAssembliesSection section4 = (FullTrustAssembliesSection) configuration.GetSection("system.web/fullTrustAssemblies");
                                    policyLevel = GetPartialTrustPolicyLevel(trustSection, section, compilationSection, physicalPath, appVPath);
                                    grantSet = policyLevel.GetNamedPermissionSet(trustSection.PermissionSetName);
                                    if (grantSet == null)
                                    {
                                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Permission_set_not_found", new object[] { trustSection.PermissionSetName }));
                                    }
                                    if (section4 != null)
                                    {
                                        FullTrustAssemblyCollection fullTrustAssemblies = section4.FullTrustAssemblies;
                                        if (fullTrustAssemblies != null)
                                        {
                                            list.AddRange(from fta in fullTrustAssemblies.Cast<FullTrustAssembly>() select CreateStrongName(fta.AssemblyName, fta.Version, fta.PublicKey));
                                        }
                                    }
                                    if (list.Contains(_mwiV1StrongName))
                                    {
                                        list.AddRange(CreateFutureMicrosoftWebInfrastructureStrongNames());
                                    }
                                    setup.AppDomainManagerType = typeof(AspNetAppDomainManager).FullName;
                                    setup.AppDomainManagerAssembly = typeof(AspNetAppDomainManager).Assembly.FullName;
                                }
                            }
                            if (trustSection.Level != "Full")
                            {
                                PartialTrustVisibleAssembliesSection section5 = (PartialTrustVisibleAssembliesSection) configuration.GetSection("system.web/partialTrustVisibleAssemblies");
                                string[] array = null;
                                if (section5 != null)
                                {
                                    PartialTrustVisibleAssemblyCollection partialTrustVisibleAssemblies = section5.PartialTrustVisibleAssemblies;
                                    if ((partialTrustVisibleAssemblies != null) && (partialTrustVisibleAssemblies.Count != 0))
                                    {
                                        array = new string[partialTrustVisibleAssemblies.Count + strArray.Length];
                                        for (int i = 0; i < partialTrustVisibleAssemblies.Count; i++)
                                        {
                                            array[i] = partialTrustVisibleAssemblies[i].AssemblyName + ", PublicKey=" + NormalizePublicKeyBlob(partialTrustVisibleAssemblies[i].PublicKey);
                                        }
                                        strArray.CopyTo(array, partialTrustVisibleAssemblies.Count);
                                    }
                                }
                                if (array == null)
                                {
                                    array = strArray;
                                }
                                setup.PartialTrustVisibleAssemblies = array;
                            }
                        }
                    }
                }
                catch (Exception exception3)
                {
                    appDomainCreationException = exception3;
                    grantSet = new PermissionSet(PermissionState.Unrestricted);
                }
                try
                {
                    if (flag)
                    {
                        domain = AppDomain.CreateDomain(domainId, GetDefaultDomainIdentity(), setup);
                    }
                    else
                    {
                        domain = AppDomain.CreateDomain(domainId, GetDefaultDomainIdentity(), setup, grantSet, list.ToArray());
                    }
                    foreach (DictionaryEntry entry in dict)
                    {
                        domain.SetData((string) entry.Key, (string) entry.Value);
                    }
                }
                catch (Exception exception4)
                {
                    innerException = exception4;
                }
            }
            finally
            {
                if (context != null)
                {
                    context.Undo();
                    context = null;
                }
                if (zero != IntPtr.Zero)
                {
                    System.Web.UnsafeNativeMethods.CloseHandle(zero);
                    zero = IntPtr.Zero;
                }
            }
            if (domain == null)
            {
                throw new SystemException(System.Web.SR.GetString("Cannot_create_AppDomain"), innerException);
            }
            Type type = typeof(HostingEnvironment);
            string fullName = type.Module.Assembly.FullName;
            string typeName = type.FullName;
            ObjectHandle handle = null;
            ImpersonationContext context2 = null;
            IntPtr token = IntPtr.Zero;
            int num2 = 10;
            int num3 = 0;
            while (num3 < num2)
            {
                try
                {
                    token = appHost.GetConfigToken();
                    break;
                }
                catch (InvalidOperationException)
                {
                    num3++;
                    Thread.Sleep(250);
                    continue;
                }
            }
            if (token != IntPtr.Zero)
            {
                try
                {
                    context2 = new ImpersonationContext(token);
                }
                catch
                {
                }
                finally
                {
                    System.Web.UnsafeNativeMethods.CloseHandle(token);
                }
            }
            try
            {
                handle = Activator.CreateInstance(domain, fullName, typeName);
            }
            finally
            {
                if (context2 != null)
                {
                    context2.Undo();
                }
                if (handle == null)
                {
                    AppDomain.Unload(domain);
                }
            }
            HostingEnvironment environment = (handle != null) ? (handle.Unwrap() as HostingEnvironment) : null;
            if (environment == null)
            {
                throw new SystemException(System.Web.SR.GetString("Cannot_create_HostEnv"));
            }
            IConfigMapPathFactory configMapPathFactory = appHost.GetConfigMapPathFactory();
            if (appDomainCreationException == null)
            {
                environment.Initialize(this, appHost, configMapPathFactory, hostingParameters, policyLevel);
                return environment;
            }
            environment.Initialize(this, appHost, configMapPathFactory, hostingParameters, policyLevel, appDomainCreationException);
            return environment;
        }

        private HostingEnvironment CreateAppDomainWithHostingEnvironmentAndReportErrors(string appId, IApplicationHost appHost, HostingEnvironmentParameters hostingParameters)
        {
            HostingEnvironment environment;
            try
            {
                environment = this.CreateAppDomainWithHostingEnvironment(appId, appHost, hostingParameters);
            }
            catch (Exception exception)
            {
                Misc.ReportUnhandledException(exception, new string[] { System.Web.SR.GetString("Failed_to_initialize_AppDomain"), appId });
                throw;
            }
            return environment;
        }

        private static IEnumerable<StrongName> CreateFutureMicrosoftWebInfrastructureStrongNames()
        {
            string name = _mwiV1StrongName.Name;
            StrongNamePublicKeyBlob publicKey = _mwiV1StrongName.PublicKey;
            int major = 2;
            while (true)
            {
                if (major > 10)
                {
                    yield break;
                }
                yield return new StrongName(publicKey, name, new Version(major, 0, 0, 0));
                major++;
            }
        }

        internal ObjectHandle CreateInstanceInNewWorkerAppDomain(Type type, string appId, VirtualPath virtualPath, string physicalPath)
        {
            IApplicationHost appHost = new SimpleApplicationHost(virtualPath, physicalPath);
            HostingEnvironmentParameters hostingParameters = new HostingEnvironmentParameters {
                HostingFlags = HostingEnvironmentFlags.HideFromAppManager
            };
            return this.CreateAppDomainWithHostingEnvironmentAndReportErrors(appId, appHost, hostingParameters).CreateInstance(type.AssemblyQualifiedName);
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public IRegisteredObject CreateObject(IApplicationHost appHost, Type type)
        {
            if (appHost == null)
            {
                throw new ArgumentNullException("appHost");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            string appId = this.CreateSimpleAppID(appHost);
            return this.CreateObjectInternal(appId, type, appHost, false);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public IRegisteredObject CreateObject(string appId, Type type, string virtualPath, string physicalPath, bool failIfExists)
        {
            return this.CreateObject(appId, type, virtualPath, physicalPath, failIfExists, false);
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public IRegisteredObject CreateObject(string appId, Type type, string virtualPath, string physicalPath, bool failIfExists, bool throwOnError)
        {
            if (appId == null)
            {
                throw new ArgumentNullException("appId");
            }
            SimpleApplicationHost appHost = new SimpleApplicationHost(VirtualPath.CreateAbsolute(virtualPath), physicalPath);
            HostingEnvironmentParameters hostingParameters = null;
            if (throwOnError)
            {
                hostingParameters = new HostingEnvironmentParameters {
                    HostingFlags = HostingEnvironmentFlags.ThrowHostingInitErrors
                };
            }
            return this.CreateObjectInternal(appId, type, appHost, failIfExists, hostingParameters);
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        internal IRegisteredObject CreateObjectInternal(string appId, Type type, IApplicationHost appHost, bool failIfExists)
        {
            if (appId == null)
            {
                throw new ArgumentNullException("appId");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (appHost == null)
            {
                throw new ArgumentNullException("appHost");
            }
            return this.CreateObjectInternal(appId, type, appHost, failIfExists, null);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal IRegisteredObject CreateObjectInternal(string appId, Type type, IApplicationHost appHost, bool failIfExists, HostingEnvironmentParameters hostingParameters)
        {
            if (!typeof(IRegisteredObject).IsAssignableFrom(type))
            {
                throw new ArgumentException(System.Web.SR.GetString("Not_IRegisteredObject", new object[] { type.FullName }), "type");
            }
            ObjectHandle handle = this.GetAppDomainWithHostingEnvironment(appId, appHost, hostingParameters).CreateWellKnownObjectInstance(type.AssemblyQualifiedName, failIfExists);
            if (handle == null)
            {
                return null;
            }
            return (handle.Unwrap() as IRegisteredObject);
        }

        internal IRegisteredObject CreateObjectWithDefaultAppHostAndAppId(string physicalPath, string virtualPath, Type type, out string appId)
        {
            return this.CreateObjectWithDefaultAppHostAndAppId(physicalPath, VirtualPath.CreateNonRelative(virtualPath), type, out appId);
        }

        internal IRegisteredObject CreateObjectWithDefaultAppHostAndAppId(string physicalPath, VirtualPath virtualPath, Type type, out string appId)
        {
            HostingEnvironmentParameters hostingParameters = new HostingEnvironmentParameters {
                HostingFlags = HostingEnvironmentFlags.DontCallAppInitialize
            };
            return this.CreateObjectWithDefaultAppHostAndAppId(physicalPath, virtualPath, type, false, hostingParameters, out appId);
        }

        internal IRegisteredObject CreateObjectWithDefaultAppHostAndAppId(string physicalPath, VirtualPath virtualPath, Type type, bool failIfExists, HostingEnvironmentParameters hostingParameters, out string appId)
        {
            IApplicationHost host;
            if (physicalPath == null)
            {
                HttpRuntime.ForceStaticInit();
                ISAPIApplicationHost host2 = new ISAPIApplicationHost(virtualPath.VirtualPathString, null, true, null, hostingParameters.IISExpressVersion);
                host = host2;
                appId = host2.AppId;
                virtualPath = VirtualPath.Create(host.GetVirtualPath());
                physicalPath = System.Web.Util.FileUtil.FixUpPhysicalDirectory(host.GetPhysicalPath());
            }
            else
            {
                appId = this.CreateSimpleAppID(virtualPath, physicalPath, null);
                host = new SimpleApplicationHost(virtualPath, physicalPath);
            }
            string precompilationTargetPhysicalDirectory = hostingParameters.PrecompilationTargetPhysicalDirectory;
            if (precompilationTargetPhysicalDirectory != null)
            {
                BuildManager.VerifyUnrelatedSourceAndDest(physicalPath, precompilationTargetPhysicalDirectory);
                if ((hostingParameters.ClientBuildManagerParameter != null) && ((hostingParameters.ClientBuildManagerParameter.PrecompilationFlags & PrecompilationFlags.Updatable) == PrecompilationFlags.Default))
                {
                    appId = appId + "_precompile";
                }
                else
                {
                    appId = appId + "_precompile_u";
                }
            }
            return this.CreateObjectInternal(appId, type, host, failIfExists, hostingParameters);
        }

        private string CreateSimpleAppID(IApplicationHost appHost)
        {
            if (appHost == null)
            {
                throw new ArgumentNullException("appHost");
            }
            return this.CreateSimpleAppID(VirtualPath.Create(appHost.GetVirtualPath()), appHost.GetPhysicalPath(), appHost.GetSiteName());
        }

        private string CreateSimpleAppID(VirtualPath virtualPath, string physicalPath, string siteName)
        {
            string str = virtualPath.VirtualPathString + physicalPath;
            if (!string.IsNullOrEmpty(siteName))
            {
                str = str + siteName;
            }
            return str.GetHashCode().ToString("x", CultureInfo.InvariantCulture);
        }

        private static StrongName CreateStrongName(string assemblyName, string version, string publicKeyString)
        {
            byte[] publicKey = null;
            publicKeyString = NormalizePublicKeyBlob(publicKeyString);
            int num = publicKeyString.Length / 2;
            publicKey = new byte[num];
            for (int i = 0; i < num; i++)
            {
                publicKey[i] = byte.Parse(publicKeyString.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return new StrongName(new StrongNamePublicKeyBlob(publicKey), assemblyName, new Version(version));
        }

        private void DisposeCacheManager()
        {
            if (_cm != null)
            {
                lock (_applicationManagerStaticLock)
                {
                    if (_cm != null)
                    {
                        _cm.Dispose();
                        _cm = null;
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public AppDomain GetAppDomain(string appId)
        {
            if (appId == null)
            {
                throw new ArgumentNullException("appId");
            }
            LockableAppDomainContext lockableAppDomainContext = this.GetLockableAppDomainContext(appId);
            lock (lockableAppDomainContext)
            {
                HostingEnvironment hostEnv = lockableAppDomainContext.HostEnv;
                if (hostEnv == null)
                {
                    return null;
                }
                return hostEnv.HostedAppDomain;
            }
        }

        public AppDomain GetAppDomain(IApplicationHost appHost)
        {
            if (appHost == null)
            {
                throw new ArgumentNullException("appHost");
            }
            string appId = this.CreateSimpleAppID(appHost);
            return this.GetAppDomain(appId);
        }

        internal AppDomainInfo[] GetAppDomainInfos()
        {
            ArrayList list = new ArrayList();
            foreach (LockableAppDomainContext context in this.CloneAppDomainsCollection().Values)
            {
                lock (context)
                {
                    HostingEnvironment hostEnv = context.HostEnv;
                    if (hostEnv != null)
                    {
                        IApplicationHost internalApplicationHost = hostEnv.InternalApplicationHost;
                        ApplicationInfo applicationInfo = hostEnv.GetApplicationInfo();
                        int siteId = 0;
                        if (internalApplicationHost != null)
                        {
                            try
                            {
                                siteId = int.Parse(internalApplicationHost.GetSiteID(), CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                            }
                        }
                        AppDomainInfo info2 = new AppDomainInfo(applicationInfo.ID, applicationInfo.VirtualPath, applicationInfo.PhysicalPath, siteId, hostEnv.GetIdleValue());
                        list.Add(info2);
                    }
                }
            }
            return (AppDomainInfo[]) list.ToArray(typeof(AppDomainInfo));
        }

        private HostingEnvironment GetAppDomainWithHostingEnvironment(string appId, IApplicationHost appHost, HostingEnvironmentParameters hostingParameters)
        {
            LockableAppDomainContext lockableAppDomainContext = this.GetLockableAppDomainContext(appId);
            lock (lockableAppDomainContext)
            {
                HostingEnvironment hostEnv = lockableAppDomainContext.HostEnv;
                if (hostEnv != null)
                {
                    try
                    {
                        hostEnv.IsUnloaded();
                    }
                    catch (AppDomainUnloadedException)
                    {
                        hostEnv = null;
                    }
                }
                if (hostEnv == null)
                {
                    hostEnv = this.CreateAppDomainWithHostingEnvironmentAndReportErrors(appId, appHost, hostingParameters);
                    lockableAppDomainContext.HostEnv = hostEnv;
                    Interlocked.Increment(ref this._accessibleHostingEnvCount);
                }
                return hostEnv;
            }
        }

        public static ApplicationManager GetApplicationManager()
        {
            if (_theAppManager == null)
            {
                lock (_applicationManagerStaticLock)
                {
                    if (_theAppManager == null)
                    {
                        if (HostingEnvironment.IsHosted)
                        {
                            _theAppManager = HostingEnvironment.GetApplicationManager();
                        }
                        if (_theAppManager == null)
                        {
                            _theAppManager = new ApplicationManager();
                        }
                    }
                }
            }
            return _theAppManager;
        }

        private static Evidence GetDefaultDomainIdentity()
        {
            Evidence evidence = AppDomain.CurrentDomain.Evidence;
            bool flag = evidence.GetHostEvidence<Zone>() != null;
            bool flag2 = evidence.GetHostEvidence<Url>() != null;
            if (!flag)
            {
                evidence.AddHostEvidence<Zone>(new Zone(SecurityZone.MyComputer));
            }
            if (!flag2)
            {
                evidence.AddHostEvidence<Url>(new Url("ms-internal-microsoft-asp-net-webhost-20"));
            }
            return evidence;
        }

        internal LockableAppDomainContext GetLockableAppDomainContext(string appId)
        {
            lock (this)
            {
                LockableAppDomainContext context;
                if (!this._appDomains.TryGetValue(appId, out context))
                {
                    context = new LockableAppDomainContext();
                    this._appDomains.Add(appId, context);
                }
                return context;
            }
        }

        private static StrongName GetMicrosoftWebInfrastructureV1StrongName()
        {
            string assemblyName = "Microsoft.Web.Infrastructure";
            string version = "1.0.0.0";
            string publicKeyString = "0024000004800000940000000602000000240000525341310004000001000100B5FC90E7027F67871E773A8FDE8938C81DD402BA65B9201D60593E96C492651E889CC13F1415EBB53FAC1131AE0BD333C5EE6021672D9718EA31A8AEBD0DA0072F25D87DBA6FC90FFD598ED4DA35E44C398C454307E8E33B8426143DAEC9F596836F97C8F74750E5975C64E2189F45DEF46B2A2B1247ADC3652BF5C308055DA9";
            return CreateStrongName(assemblyName, version, publicKeyString);
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public IRegisteredObject GetObject(string appId, Type type)
        {
            if (appId == null)
            {
                throw new ArgumentNullException("appId");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            LockableAppDomainContext lockableAppDomainContext = this.GetLockableAppDomainContext(appId);
            lock (lockableAppDomainContext)
            {
                HostingEnvironment hostEnv = lockableAppDomainContext.HostEnv;
                if (hostEnv == null)
                {
                    return null;
                }
                ObjectHandle handle = hostEnv.FindWellKnownObject(type.AssemblyQualifiedName);
                return ((handle != null) ? (handle.Unwrap() as IRegisteredObject) : null);
            }
        }

        private static PolicyLevel GetPartialTrustPolicyLevel(TrustSection trustSection, SecurityPolicySection securityPolicySection, CompilationSection compilationSection, string physicalPath, VirtualPath virtualPath)
        {
            if ((securityPolicySection == null) || (securityPolicySection.TrustLevels[trustSection.Level] == null))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Unable_to_get_policy_file", new object[] { trustSection.Level }), string.Empty, 0);
            }
            string policyFileExpanded = securityPolicySection.TrustLevels[trustSection.Level].PolicyFileExpanded;
            if ((policyFileExpanded == null) || !System.Web.Util.FileUtil.FileExists(policyFileExpanded))
            {
                throw new HttpException(System.Web.SR.GetString("Unable_to_get_policy_file", new object[] { trustSection.Level }));
            }
            PolicyLevel level = null;
            string path = System.Web.Util.FileUtil.RemoveTrailingDirectoryBackSlash(physicalPath);
            string newValue = HttpRuntime.MakeFileUrl(path);
            string tempDirectory = null;
            string tempDirAttribName = null;
            string configFileName = null;
            int configLineNumber = 0;
            if ((compilationSection != null) && !string.IsNullOrEmpty(compilationSection.TempDirectory))
            {
                tempDirectory = compilationSection.TempDirectory;
                compilationSection.GetTempDirectoryErrorInfo(out tempDirAttribName, out configFileName, out configLineNumber);
            }
            if (tempDirectory != null)
            {
                tempDirectory = tempDirectory.Trim();
                if (!Path.IsPathRooted(tempDirectory))
                {
                    tempDirectory = null;
                }
                else
                {
                    try
                    {
                        tempDirectory = new DirectoryInfo(tempDirectory).FullName;
                    }
                    catch
                    {
                        tempDirectory = null;
                    }
                }
                if (tempDirectory == null)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_temp_directory", new object[] { tempDirAttribName }), configFileName, configLineNumber);
                }
                try
                {
                    Directory.CreateDirectory(tempDirectory);
                    goto Label_0165;
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_temp_directory", new object[] { tempDirAttribName }), exception, configFileName, configLineNumber);
                }
            }
            tempDirectory = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "Temporary ASP.NET Files");
        Label_0165:
            if (!Util.HasWriteAccessToDirectory(tempDirectory))
            {
                if (!Environment.UserInteractive)
                {
                    throw new HttpException(System.Web.SR.GetString("No_codegen_access", new object[] { Util.GetCurrentAccountName(), tempDirectory }));
                }
                tempDirectory = Path.Combine(Path.GetTempPath(), "Temporary ASP.NET Files");
            }
            string str7 = AppManagerAppDomainFactory.ConstructSimpleAppName(VirtualPath.GetVirtualPathStringNoTrailingSlash(virtualPath));
            string str9 = HttpRuntime.MakeFileUrl(System.Web.Util.FileUtil.RemoveTrailingDirectoryBackSlash(Path.Combine(tempDirectory, str7)));
            string originUrl = trustSection.OriginUrl;
            FileStream stream = new FileStream(policyFileExpanded, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string str = reader.ReadToEnd();
            reader.Close();
            str = str.Replace("$AppDir$", path).Replace("$AppDirUrl$", newValue).Replace("$CodeGen$", str9);
            if (originUrl == null)
            {
                originUrl = string.Empty;
            }
            str = str.Replace("$OriginHost$", originUrl);
            string gacLocation = null;
            if (str.IndexOf("$Gac$", StringComparison.Ordinal) != -1)
            {
                gacLocation = HttpRuntime.GetGacLocation();
                if (gacLocation != null)
                {
                    gacLocation = HttpRuntime.MakeFileUrl(gacLocation);
                }
                if (gacLocation == null)
                {
                    gacLocation = string.Empty;
                }
                str = str.Replace("$Gac$", gacLocation);
            }
            level = SecurityManager.LoadPolicyLevelFromString(str, PolicyLevelType.AppDomain);
            if (level == null)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Unable_to_get_policy_file", new object[] { trustSection.Level }));
            }
            if (gacLocation != null)
            {
                CodeGroup rootCodeGroup = level.RootCodeGroup;
                bool flag = false;
                foreach (CodeGroup group2 in rootCodeGroup.Children)
                {
                    if (group2.MembershipCondition is GacMembershipCondition)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag && (rootCodeGroup is FirstMatchCodeGroup))
                {
                    FirstMatchCodeGroup group3 = (FirstMatchCodeGroup) rootCodeGroup;
                    if (!(group3.MembershipCondition is AllMembershipCondition) || !(group3.PermissionSetName == "Nothing"))
                    {
                        return level;
                    }
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
                    level.RootCodeGroup = group5;
                }
            }
            return level;
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public ApplicationInfo[] GetRunningApplications()
        {
            ArrayList list = new ArrayList();
            foreach (LockableAppDomainContext context in this.CloneAppDomainsCollection().Values)
            {
                lock (context)
                {
                    HostingEnvironment hostEnv = context.HostEnv;
                    if (hostEnv != null)
                    {
                        list.Add(hostEnv.GetApplicationInfo());
                    }
                }
            }
            int count = list.Count;
            ApplicationInfo[] array = new ApplicationInfo[count];
            if (count > 0)
            {
                list.CopyTo(array);
            }
            return array;
        }

        internal long GetUpdatedTotalCacheSize(long sizeUpdate)
        {
            CacheManager manager = _cm;
            if (manager == null)
            {
                return 0L;
            }
            return manager.GetUpdatedTotalCacheSize(sizeUpdate);
        }

        internal void HostingEnvironmentActivated(long privateBytesLimit)
        {
            if (Interlocked.Increment(ref this._activeHostingEnvCount) == 1)
            {
                this.InitCacheManager(privateBytesLimit);
            }
        }

        internal void HostingEnvironmentShutdownComplete(string appId, IApplicationHost appHost)
        {
            try
            {
                if (appHost != null)
                {
                    MarshalByRefObject obj2 = appHost as MarshalByRefObject;
                    if (obj2 != null)
                    {
                        RemotingServices.Disconnect(obj2);
                    }
                }
            }
            finally
            {
                Interlocked.Decrement(ref this._activeHostingEnvCount);
            }
        }

        internal void HostingEnvironmentShutdownInitiated(string appId, HostingEnvironment env)
        {
            if (!this._shutdownInProgress)
            {
                LockableAppDomainContext lockableAppDomainContext = this.GetLockableAppDomainContext(appId);
                lock (lockableAppDomainContext)
                {
                    if (!env.HasBeenRemovedFromAppManagerTable)
                    {
                        env.HasBeenRemovedFromAppManagerTable = true;
                        lockableAppDomainContext.HostEnv = null;
                        Interlocked.Decrement(ref this._accessibleHostingEnvCount);
                        if ((lockableAppDomainContext.PreloadContext != null) && !lockableAppDomainContext.RetryingPreload)
                        {
                            ProcessHost.PreloadApplicationIfNotShuttingdown(appId, lockableAppDomainContext);
                        }
                    }
                }
            }
        }

        private void InitCacheManager(long privateBytesLimit)
        {
            if (_cm == null)
            {
                lock (_applicationManagerStaticLock)
                {
                    if ((_cm == null) && !this._shutdownInProgress)
                    {
                        _cm = new CacheManager(this, privateBytesLimit);
                    }
                }
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public bool IsIdle()
        {
            foreach (LockableAppDomainContext context in this.CloneAppDomainsCollection().Values)
            {
                lock (context)
                {
                    HostingEnvironment hostEnv = context.HostEnv;
                    if (!((hostEnv == null) || hostEnv.IsIdle()))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static string NormalizePublicKeyBlob(string publicKey)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < publicKey.Length; i++)
            {
                if (!char.IsWhiteSpace(publicKey[i]))
                {
                    builder.Append(publicKey[i]);
                }
            }
            publicKey = builder.ToString();
            return publicKey;
        }

        internal void OnRespondToPingWaitCallback(object state)
        {
            this.RespondToPingIfNeeded();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs eventArgs)
        {
            if (eventArgs.IsTerminating)
            {
                Exception exceptionObject = eventArgs.ExceptionObject as Exception;
                if (exceptionObject != null)
                {
                    AppDomain appDomain = sender as AppDomain;
                    if (appDomain != null)
                    {
                        RecordFatalException(appDomain, exceptionObject);
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public void Open()
        {
            Interlocked.Increment(ref this._openCount);
        }

        internal void Ping(IProcessPingCallback callback)
        {
            if (((callback != null) && (this._pendingPingCallback == null)) && (Interlocked.CompareExchange(ref this._pendingPingCallback, callback, null) == null))
            {
                ThreadPool.QueueUserWorkItem(this._onRespondToPingWaitCallback);
            }
        }

        private static void PopulateDomainBindings(string domainId, string appId, string appName, string appPath, VirtualPath appVPath, AppDomainSetup setup, IDictionary dict)
        {
            setup.PrivateBinPathProbe = "*";
            setup.ShadowCopyFiles = "true";
            setup.ApplicationBase = appPath;
            setup.ApplicationName = appName;
            setup.ConfigurationFile = "web.config";
            setup.DisallowCodeDownload = true;
            dict.Add(".appDomain", "*");
            dict.Add(".appId", appId);
            dict.Add(".appPath", appPath);
            dict.Add(".appVPath", appVPath.VirtualPathString);
            dict.Add(".domainId", domainId);
        }

        internal static void RecordFatalException(Exception e)
        {
            RecordFatalException(AppDomain.CurrentDomain, e);
        }

        internal static void RecordFatalException(AppDomain appDomain, Exception e)
        {
            if (Interlocked.CompareExchange<Exception>(ref _fatalException, e, null) == null)
            {
                Misc.WriteUnhandledExceptionToEventLog(appDomain, e);
            }
        }

        internal void ReduceAppDomainsCount(int limit)
        {
            Dictionary<string, LockableAppDomainContext> dictionary = this.CloneAppDomainsCollection();
            while ((this._accessibleHostingEnvCount >= limit) && !this._shutdownInProgress)
            {
                LockableAppDomainContext context = null;
                int num = 0;
                foreach (LockableAppDomainContext context2 in dictionary.Values)
                {
                    if (context2.HostEnv != null)
                    {
                        lock (context2)
                        {
                            HostingEnvironment hostEnv = context2.HostEnv;
                            if (hostEnv != null)
                            {
                                int lruScore = hostEnv.LruScore;
                                if (((context == null) || (context.HostEnv == null)) || (lruScore < num))
                                {
                                    num = lruScore;
                                    context = context2;
                                }
                            }
                        }
                    }
                }
                if (context == null)
                {
                    return;
                }
                lock (context)
                {
                    if (context.HostEnv != null)
                    {
                        context.HostEnv.InitiateShutdownInternal();
                    }
                    continue;
                }
            }
        }

        internal void RemoveFromTableIfRuntimeExists(string appId, Type runtimeType)
        {
            if (appId == null)
            {
                throw new ArgumentNullException("appId");
            }
            if (runtimeType == null)
            {
                throw new ArgumentNullException("runtimeType");
            }
            LockableAppDomainContext lockableAppDomainContext = this.GetLockableAppDomainContext(appId);
            lock (lockableAppDomainContext)
            {
                HostingEnvironment hostEnv = lockableAppDomainContext.HostEnv;
                if ((hostEnv != null) && (hostEnv.FindWellKnownObject(runtimeType.AssemblyQualifiedName) != null))
                {
                    this.HostingEnvironmentShutdownInitiated(appId, hostEnv);
                }
            }
        }

        internal void RespondToPingIfNeeded()
        {
            IProcessPingCallback comparand = this._pendingPingCallback as IProcessPingCallback;
            if ((comparand != null) && (Interlocked.CompareExchange(ref this._pendingPingCallback, null, comparand) == comparand))
            {
                comparand.Respond();
            }
        }

        private static void SetNetFx40LegacySecurityPolicy(AppDomainSetup setup)
        {
            List<string> switches = new List<string> { "NetFx40_LegacySecurityPolicy" };
            setup.SetCompatibilitySwitches(switches);
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public void ShutdownAll()
        {
            this._shutdownInProgress = true;
            Dictionary<string, LockableAppDomainContext> dictionary = null;
            this.DisposeCacheManager();
            lock (this)
            {
                dictionary = this._appDomains;
                this._appDomains = new Dictionary<string, LockableAppDomainContext>(StringComparer.OrdinalIgnoreCase);
            }
            foreach (KeyValuePair<string, LockableAppDomainContext> pair in dictionary)
            {
                LockableAppDomainContext context = pair.Value;
                lock (context)
                {
                    HostingEnvironment hostEnv = context.HostEnv;
                    if (hostEnv != null)
                    {
                        hostEnv.InitiateShutdownInternal();
                    }
                }
            }
            for (int i = 0; (this._activeHostingEnvCount > 0) && (i < 0xbb8); i++)
            {
                Thread.Sleep(100);
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public void ShutdownApplication(string appId)
        {
            if (appId == null)
            {
                throw new ArgumentNullException("appId");
            }
            LockableAppDomainContext lockableAppDomainContext = this.GetLockableAppDomainContext(appId);
            lock (lockableAppDomainContext)
            {
                if (lockableAppDomainContext.HostEnv != null)
                {
                    lockableAppDomainContext.HostEnv.InitiateShutdownInternal();
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public void StopObject(string appId, Type type)
        {
            if (appId == null)
            {
                throw new ArgumentNullException("appId");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            LockableAppDomainContext lockableAppDomainContext = this.GetLockableAppDomainContext(appId);
            lock (lockableAppDomainContext)
            {
                HostingEnvironment hostEnv = lockableAppDomainContext.HostEnv;
                if (hostEnv != null)
                {
                    hostEnv.StopWellKnownObject(type.AssemblyQualifiedName);
                }
            }
        }

        internal long TrimCaches(int percent)
        {
            long num = 0L;
            foreach (LockableAppDomainContext context in this.CloneAppDomainsCollection().Values)
            {
                lock (context)
                {
                    HostingEnvironment hostEnv = context.HostEnv;
                    if (this._shutdownInProgress)
                    {
                        return num;
                    }
                    if (hostEnv != null)
                    {
                        num += hostEnv.TrimCache(percent);
                    }
                }
            }
            return num;
        }

        internal int AppDomainsCount
        {
            get
            {
                return this._accessibleHostingEnvCount;
            }
        }

        internal bool ShutdownInProgress
        {
            get
            {
                return this._shutdownInProgress;
            }
        }


        internal class AspNetAppDomainManager : AppDomainManager
        {
            private System.Security.HostSecurityManager aspNetHostSecurityManager = new ApplicationManager.AspNetHostSecurityManager();

            public override System.Security.HostSecurityManager HostSecurityManager
            {
                get
                {
                    return this.aspNetHostSecurityManager;
                }
            }
        }

        internal class AspNetHostSecurityManager : HostSecurityManager
        {
            private PermissionSet FullTrust = new PermissionSet(PermissionState.Unrestricted);
            private HostSecurityPolicyResolver hostSecurityPolicyResolver;
            private PermissionSet Nothing = new PermissionSet(PermissionState.None);

            [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
            public override PermissionSet ResolvePolicy(Evidence evidence)
            {
                if (base.ResolvePolicy(evidence).IsUnrestricted())
                {
                    return this.FullTrust;
                }
                if (!string.IsNullOrEmpty(HttpRuntime.HostSecurityPolicyResolverType) && (this.hostSecurityPolicyResolver == null))
                {
                    this.hostSecurityPolicyResolver = Activator.CreateInstance(Type.GetType(HttpRuntime.HostSecurityPolicyResolverType)) as HostSecurityPolicyResolver;
                }
                if (this.hostSecurityPolicyResolver != null)
                {
                    switch (this.hostSecurityPolicyResolver.ResolvePolicy(evidence))
                    {
                        case HostSecurityPolicyResults.FullTrust:
                            return this.FullTrust;

                        case HostSecurityPolicyResults.AppDomainTrust:
                            return HttpRuntime.NamedPermissionSet;

                        case HostSecurityPolicyResults.Nothing:
                            return this.Nothing;
                    }
                }
                if ((HttpRuntime.PolicyLevel == null) || HttpRuntime.PolicyLevel.Resolve(evidence).PermissionSet.IsUnrestricted())
                {
                    return this.FullTrust;
                }
                if (HttpRuntime.PolicyLevel.Resolve(evidence).PermissionSet.Equals(this.Nothing))
                {
                    return this.Nothing;
                }
                return HttpRuntime.NamedPermissionSet;
            }

            public override HostSecurityManagerOptions Flags
            {
                get
                {
                    return HostSecurityManagerOptions.HostResolvePolicy;
                }
            }
        }
    }
}


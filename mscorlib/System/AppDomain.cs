namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration.Assemblies;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Hosting;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Contexts;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Security.Principal;
    using System.Security.Util;
    using System.Text;
    using System.Threading;

    [ClassInterface(ClassInterfaceType.None), ComVisible(true), ComDefaultInterface(typeof(_AppDomain))]
    public sealed class AppDomain : MarshalByRefObject, _AppDomain, IEvidenceFactory
    {
        private System.ActivationContext _activationContext;
        private System.ApplicationIdentity _applicationIdentity;
        private System.Security.Policy.ApplicationTrust _applicationTrust;
        private string[] _aptcaVisibleAssemblies;
        private Dictionary<string, object> _compatFlags;
        private Context _DefaultContext;
        private IPrincipal _DefaultPrincipal;
        [SecurityCritical]
        private AppDomainManager _domainManager;
        private EventHandler _domainUnload;
        private EventHandler<FirstChanceExceptionEventArgs> _firstChanceException;
        private AppDomainSetup _FusionStore;
        private bool _HasSetPolicy;
        private Dictionary<string, object[]> _LocalStore;
        private IntPtr _pDomain;
        private object[] _Policies;
        private PrincipalPolicy _PrincipalPolicy;
        private EventHandler _processExit;
        private DomainSpecificRemotingData _RemotingData;
        private System.Security.Policy.Evidence _SecurityIdentity;
        private UnhandledExceptionEventHandler _unhandledException;
        internal const int DefaultADID = 1;

        public event AssemblyLoadEventHandler AssemblyLoad;

        public event ResolveEventHandler AssemblyResolve;

        public event EventHandler DomainUnload
        {
            [SecuritySafeCritical] add
            {
                if (value != null)
                {
                    RuntimeHelpers.PrepareContractedDelegate(value);
                    lock (this)
                    {
                        this._domainUnload = (EventHandler) Delegate.Combine(this._domainUnload, value);
                    }
                }
            }
            [SecuritySafeCritical] remove
            {
                lock (this)
                {
                    this._domainUnload = (EventHandler) Delegate.Remove(this._domainUnload, value);
                }
            }
        }

        public event EventHandler<FirstChanceExceptionEventArgs> FirstChanceException
        {
            [SecurityCritical] add
            {
                if (value != null)
                {
                    RuntimeHelpers.PrepareContractedDelegate(value);
                    lock (this)
                    {
                        this._firstChanceException = (EventHandler<FirstChanceExceptionEventArgs>) Delegate.Combine(this._firstChanceException, value);
                    }
                }
            }
            [SecurityCritical] remove
            {
                lock (this)
                {
                    this._firstChanceException = (EventHandler<FirstChanceExceptionEventArgs>) Delegate.Remove(this._firstChanceException, value);
                }
            }
        }

        public event EventHandler ProcessExit
        {
            [SecuritySafeCritical] add
            {
                if (value != null)
                {
                    RuntimeHelpers.PrepareContractedDelegate(value);
                    lock (this)
                    {
                        this._processExit = (EventHandler) Delegate.Combine(this._processExit, value);
                    }
                }
            }
            [SecuritySafeCritical] remove
            {
                lock (this)
                {
                    this._processExit = (EventHandler) Delegate.Remove(this._processExit, value);
                }
            }
        }

        public event ResolveEventHandler ReflectionOnlyAssemblyResolve;

        public event ResolveEventHandler ResourceResolve;

        public event ResolveEventHandler TypeResolve;

        public event UnhandledExceptionEventHandler UnhandledException
        {
            [SecurityCritical] add
            {
                if (value != null)
                {
                    RuntimeHelpers.PrepareContractedDelegate(value);
                    lock (this)
                    {
                        this._unhandledException = (UnhandledExceptionEventHandler) Delegate.Combine(this._unhandledException, value);
                    }
                }
            }
            [SecurityCritical] remove
            {
                lock (this)
                {
                    this._unhandledException = (UnhandledExceptionEventHandler) Delegate.Remove(this._unhandledException, value);
                }
            }
        }

        private AppDomain()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_Constructor"));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        private extern int _nExecuteAssembly(RuntimeAssembly assembly, string[] args);
        [SecuritySafeCritical]
        private int ActivateApplication()
        {
            return (int) Activator.CreateInstance(CurrentDomain.ActivationContext).Unwrap();
        }

        [Obsolete("AppDomain.AppendPrivatePath has been deprecated. Please investigate the use of AppDomainSetup.PrivateBinPath instead. http://go.microsoft.com/fwlink/?linkid=14202"), SecurityCritical]
        public void AppendPrivatePath(string path)
        {
            if ((path != null) && (path.Length != 0))
            {
                string str = this.FusionStore.Value[5];
                StringBuilder builder = new StringBuilder();
                if ((str != null) && (str.Length > 0))
                {
                    builder.Append(str);
                    if ((str[str.Length - 1] != Path.PathSeparator) && (path[0] != Path.PathSeparator))
                    {
                        builder.Append(Path.PathSeparator);
                    }
                }
                builder.Append(path);
                string str2 = builder.ToString();
                this.InternalSetPrivateBinPath(str2);
            }
        }

        [ComVisible(false)]
        public string ApplyPolicy(string assemblyName)
        {
            AssemblyName an = new AssemblyName(assemblyName);
            byte[] publicKeyToken = an.GetPublicKeyToken();
            if (publicKeyToken == null)
            {
                publicKeyToken = an.GetPublicKey();
            }
            if ((publicKeyToken != null) && (publicKeyToken.Length != 0))
            {
                return this.nApplyPolicy(an);
            }
            return assemblyName;
        }

        [SecuritySafeCritical]
        internal static void CheckDomainCreationEvidence(AppDomainSetup creationDomainSetup, System.Security.Policy.Evidence creationEvidence)
        {
            if (((creationEvidence != null) && !CurrentDomain.IsLegacyCasPolicyEnabled) && ((creationDomainSetup == null) || (creationDomainSetup.ApplicationTrust == null)))
            {
                Zone hostEvidence = CurrentDomain.EvidenceNoDemand.GetHostEvidence<Zone>();
                SecurityZone zone2 = (hostEvidence != null) ? hostEvidence.SecurityZone : SecurityZone.MyComputer;
                Zone zone3 = creationEvidence.GetHostEvidence<Zone>();
                if (((zone3 != null) && (zone3.SecurityZone != zone2)) && (zone3.SecurityZone != SecurityZone.MyComputer))
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
                }
            }
        }

        [SecurityCritical, Obsolete("AppDomain.ClearPrivatePath has been deprecated. Please investigate the use of AppDomainSetup.PrivateBinPath instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public void ClearPrivatePath()
        {
            this.InternalSetPrivateBinPath(string.Empty);
        }

        [SecurityCritical, Obsolete("AppDomain.ClearShadowCopyPath has been deprecated. Please investigate the use of AppDomainSetup.ShadowCopyDirectories instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public void ClearShadowCopyPath()
        {
            this.InternalSetShadowCopyPath(string.Empty);
        }

        [SecuritySafeCritical]
        private void CreateAppDomainManager()
        {
            string str;
            string str2;
            AppDomainSetup fusionStore = this.FusionStore;
            this.GetAppDomainManagerType(out str, out str2);
            if ((str != null) && (str2 != null))
            {
                try
                {
                    new System.Security.PermissionSet(PermissionState.Unrestricted).Assert();
                    this._domainManager = this.CreateInstanceAndUnwrap(str, str2) as AppDomainManager;
                    CodeAccessPermission.RevertAssert();
                }
                catch (FileNotFoundException exception)
                {
                    throw new TypeLoadException(Environment.GetResourceString("Argument_NoDomainManager"), exception);
                }
                catch (SecurityException exception2)
                {
                    throw new TypeLoadException(Environment.GetResourceString("Argument_NoDomainManager"), exception2);
                }
                catch (TypeLoadException exception3)
                {
                    throw new TypeLoadException(Environment.GetResourceString("Argument_NoDomainManager"), exception3);
                }
                if (this._domainManager == null)
                {
                    throw new TypeLoadException(Environment.GetResourceString("Argument_NoDomainManager"));
                }
                this.FusionStore.AppDomainManagerAssembly = str;
                this.FusionStore.AppDomainManagerType = str2;
                bool flag = (this._domainManager.GetType() != typeof(AppDomainManager)) && !this.DisableFusionUpdatesFromADManager();
                AppDomainSetup oldInfo = null;
                if (flag)
                {
                    oldInfo = new AppDomainSetup(this.FusionStore, true);
                }
                this._domainManager.InitializeNewDomain(this.FusionStore);
                if (flag)
                {
                    this.SetupFusionStore(this._FusionStore, oldInfo);
                }
                if ((this._domainManager.InitializationFlags & AppDomainManagerInitializationOptions.RegisterWithHost) == AppDomainManagerInitializationOptions.RegisterWithHost)
                {
                    this._domainManager.RegisterWithHost();
                }
            }
            this.InitializeCompatibilityFlags();
        }

        [SecuritySafeCritical]
        public ObjectHandle CreateComInstanceFrom(string assemblyName, string typeName)
        {
            if (this == null)
            {
                throw new NullReferenceException();
            }
            return Activator.CreateComInstanceFrom(assemblyName, typeName);
        }

        [SecuritySafeCritical]
        public ObjectHandle CreateComInstanceFrom(string assemblyFile, string typeName, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
        {
            if (this == null)
            {
                throw new NullReferenceException();
            }
            return Activator.CreateComInstanceFrom(assemblyFile, typeName, hashValue, hashAlgorithm);
        }

        [SecurityCritical]
        internal void CreateDefaultContext()
        {
            lock (this)
            {
                if (this._DefaultContext == null)
                {
                    this._DefaultContext = Context.CreateDefaultContext();
                }
            }
        }

        [SecuritySafeCritical]
        public static AppDomain CreateDomain(string friendlyName)
        {
            return CreateDomain(friendlyName, null, null);
        }

        [SecuritySafeCritical]
        public static AppDomain CreateDomain(string friendlyName, System.Security.Policy.Evidence securityInfo)
        {
            return CreateDomain(friendlyName, securityInfo, null);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlAppDomain=true)]
        public static AppDomain CreateDomain(string friendlyName, System.Security.Policy.Evidence securityInfo, AppDomainSetup info)
        {
            return InternalCreateDomain(friendlyName, securityInfo, info);
        }

        [SecuritySafeCritical]
        public static AppDomain CreateDomain(string friendlyName, System.Security.Policy.Evidence securityInfo, AppDomainSetup info, System.Security.PermissionSet grantSet, params StrongName[] fullTrustAssemblies)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            if (info.ApplicationBase == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_AppDomainSandboxAPINeedsExplicitAppBase"));
            }
            if (fullTrustAssemblies == null)
            {
                fullTrustAssemblies = new StrongName[0];
            }
            info.ApplicationTrust = new System.Security.Policy.ApplicationTrust(grantSet, fullTrustAssemblies);
            return CreateDomain(friendlyName, securityInfo, info);
        }

        [SecuritySafeCritical]
        public static AppDomain CreateDomain(string friendlyName, System.Security.Policy.Evidence securityInfo, string appBasePath, string appRelativeSearchPath, bool shadowCopyFiles)
        {
            AppDomainSetup info = new AppDomainSetup {
                ApplicationBase = appBasePath,
                PrivateBinPath = appRelativeSearchPath
            };
            if (shadowCopyFiles)
            {
                info.ShadowCopyFiles = "true";
            }
            return CreateDomain(friendlyName, securityInfo, info);
        }

        [SecuritySafeCritical]
        public static AppDomain CreateDomain(string friendlyName, System.Security.Policy.Evidence securityInfo, string appBasePath, string appRelativeSearchPath, bool shadowCopyFiles, AppDomainInitializer adInit, string[] adInitArgs)
        {
            AppDomainSetup info = new AppDomainSetup {
                ApplicationBase = appBasePath,
                PrivateBinPath = appRelativeSearchPath,
                AppDomainInitializer = adInit,
                AppDomainInitializerArguments = adInitArgs
            };
            if (shadowCopyFiles)
            {
                info.ShadowCopyFiles = "true";
            }
            return CreateDomain(friendlyName, securityInfo, info);
        }

        [SecuritySafeCritical]
        public ObjectHandle CreateInstance(string assemblyName, string typeName)
        {
            if (this == null)
            {
                throw new NullReferenceException();
            }
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }
            return Activator.CreateInstance(assemblyName, typeName);
        }

        [SecuritySafeCritical]
        public ObjectHandle CreateInstance(string assemblyName, string typeName, object[] activationAttributes)
        {
            if (this == null)
            {
                throw new NullReferenceException();
            }
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }
            return Activator.CreateInstance(assemblyName, typeName, activationAttributes);
        }

        [SecuritySafeCritical]
        public ObjectHandle CreateInstance(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            if (this == null)
            {
                throw new NullReferenceException();
            }
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }
            return Activator.CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes);
        }

        [Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateInstance which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
        public ObjectHandle CreateInstance(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, System.Security.Policy.Evidence securityAttributes)
        {
            if (this == null)
            {
                throw new NullReferenceException();
            }
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }
            if ((securityAttributes != null) && !this.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
            }
            return Activator.CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
        }

        [SecuritySafeCritical]
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName)
        {
            ObjectHandle handle = this.CreateInstance(assemblyName, typeName);
            if (handle == null)
            {
                return null;
            }
            return handle.Unwrap();
        }

        [SecuritySafeCritical]
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName, object[] activationAttributes)
        {
            ObjectHandle handle = this.CreateInstance(assemblyName, typeName, activationAttributes);
            if (handle == null)
            {
                return null;
            }
            return handle.Unwrap();
        }

        [SecuritySafeCritical]
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            ObjectHandle handle = this.CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes);
            if (handle == null)
            {
                return null;
            }
            return handle.Unwrap();
        }

        [Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateInstanceAndUnwrap which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, System.Security.Policy.Evidence securityAttributes)
        {
            ObjectHandle handle = this.CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
            if (handle == null)
            {
                return null;
            }
            return handle.Unwrap();
        }

        [SecuritySafeCritical]
        public ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName)
        {
            if (this == null)
            {
                throw new NullReferenceException();
            }
            return Activator.CreateInstanceFrom(assemblyFile, typeName);
        }

        [SecuritySafeCritical]
        public ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, object[] activationAttributes)
        {
            if (this == null)
            {
                throw new NullReferenceException();
            }
            return Activator.CreateInstanceFrom(assemblyFile, typeName, activationAttributes);
        }

        [SecuritySafeCritical]
        public ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            if (this == null)
            {
                throw new NullReferenceException();
            }
            return Activator.CreateInstanceFrom(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes);
        }

        [Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateInstanceFrom which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
        public ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, System.Security.Policy.Evidence securityAttributes)
        {
            if (this == null)
            {
                throw new NullReferenceException();
            }
            if ((securityAttributes != null) && !this.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
            }
            return Activator.CreateInstanceFrom(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
        }

        [SecuritySafeCritical]
        public object CreateInstanceFromAndUnwrap(string assemblyName, string typeName)
        {
            ObjectHandle handle = this.CreateInstanceFrom(assemblyName, typeName);
            if (handle == null)
            {
                return null;
            }
            return handle.Unwrap();
        }

        [SecuritySafeCritical]
        public object CreateInstanceFromAndUnwrap(string assemblyName, string typeName, object[] activationAttributes)
        {
            ObjectHandle handle = this.CreateInstanceFrom(assemblyName, typeName, activationAttributes);
            if (handle == null)
            {
                return null;
            }
            return handle.Unwrap();
        }

        [SecuritySafeCritical]
        public object CreateInstanceFromAndUnwrap(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            ObjectHandle handle = this.CreateInstanceFrom(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes);
            if (handle == null)
            {
                return null;
            }
            return handle.Unwrap();
        }

        [SecuritySafeCritical, Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateInstanceFromAndUnwrap which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public object CreateInstanceFromAndUnwrap(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, System.Security.Policy.Evidence securityAttributes)
        {
            ObjectHandle handle = this.CreateInstanceFrom(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
            if (handle == null)
            {
                return null;
            }
            return handle.Unwrap();
        }

        internal void CreateRemotingData()
        {
            lock (this)
            {
                if (this._RemotingData == null)
                {
                    this._RemotingData = new DomainSpecificRemotingData();
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalDefineDynamicAssembly(name, access, null, null, null, null, null, ref lookForMyCaller, null, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, Obsolete("Assembly level declarative security is obsolete and is no longer enforced by the CLR by default.  See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, System.Security.Policy.Evidence evidence)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalDefineDynamicAssembly(name, access, null, evidence, null, null, null, ref lookForMyCaller, null, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalDefineDynamicAssembly(name, access, dir, null, null, null, null, ref lookForMyCaller, null, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalDefineDynamicAssembly(name, access, null, null, null, null, null, ref lookForMyCaller, assemblyAttributes, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, IEnumerable<CustomAttributeBuilder> assemblyAttributes, SecurityContextSource securityContextSource)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalDefineDynamicAssembly(name, access, null, null, null, null, null, ref lookForMyCaller, assemblyAttributes, securityContextSource);
        }

        [MethodImpl(MethodImplOptions.NoInlining), Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of DefineDynamicAssembly which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkId=155570 for more information."), SecuritySafeCritical]
        public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, System.Security.Policy.Evidence evidence)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalDefineDynamicAssembly(name, access, dir, evidence, null, null, null, ref lookForMyCaller, null, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, Obsolete("Assembly level declarative security is obsolete and is no longer enforced by the CLR by default.  See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, System.Security.PermissionSet requiredPermissions, System.Security.PermissionSet optionalPermissions, System.Security.PermissionSet refusedPermissions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalDefineDynamicAssembly(name, access, null, null, requiredPermissions, optionalPermissions, refusedPermissions, ref lookForMyCaller, null, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, bool isSynchronized, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalDefineDynamicAssembly(name, access, dir, null, null, null, null, ref lookForMyCaller, assemblyAttributes, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, Obsolete("Assembly level declarative security is obsolete and is no longer enforced by the CLR by default. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, System.Security.Policy.Evidence evidence, System.Security.PermissionSet requiredPermissions, System.Security.PermissionSet optionalPermissions, System.Security.PermissionSet refusedPermissions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalDefineDynamicAssembly(name, access, null, evidence, requiredPermissions, optionalPermissions, refusedPermissions, ref lookForMyCaller, null, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), Obsolete("Assembly level declarative security is obsolete and is no longer enforced by the CLR by default. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
        public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, System.Security.PermissionSet requiredPermissions, System.Security.PermissionSet optionalPermissions, System.Security.PermissionSet refusedPermissions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalDefineDynamicAssembly(name, access, dir, null, requiredPermissions, optionalPermissions, refusedPermissions, ref lookForMyCaller, null, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), Obsolete("Assembly level declarative security is obsolete and is no longer enforced by the CLR by default.  Please see http://go.microsoft.com/fwlink/?LinkId=155570 for more information."), SecuritySafeCritical]
        public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, System.Security.Policy.Evidence evidence, System.Security.PermissionSet requiredPermissions, System.Security.PermissionSet optionalPermissions, System.Security.PermissionSet refusedPermissions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalDefineDynamicAssembly(name, access, dir, evidence, requiredPermissions, optionalPermissions, refusedPermissions, ref lookForMyCaller, null, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, Obsolete("Assembly level declarative security is obsolete and is no longer enforced by the CLR by default. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, System.Security.Policy.Evidence evidence, System.Security.PermissionSet requiredPermissions, System.Security.PermissionSet optionalPermissions, System.Security.PermissionSet refusedPermissions, bool isSynchronized)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalDefineDynamicAssembly(name, access, dir, evidence, requiredPermissions, optionalPermissions, refusedPermissions, ref lookForMyCaller, null, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), Obsolete("Assembly level declarative security is obsolete and is no longer enforced by the CLR by default. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
        public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, System.Security.Policy.Evidence evidence, System.Security.PermissionSet requiredPermissions, System.Security.PermissionSet optionalPermissions, System.Security.PermissionSet refusedPermissions, bool isSynchronized, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalDefineDynamicAssembly(name, access, dir, evidence, requiredPermissions, optionalPermissions, refusedPermissions, ref lookForMyCaller, assemblyAttributes, SecurityContextSource.CurrentAssembly);
        }

        [SecurityCritical]
        private static object Deserialize(byte[] blob)
        {
            if (blob == null)
            {
                return null;
            }
            if (blob[0] == 0)
            {
                SecurityElement topElement = new Parser(blob, Tokenizer.ByteTokenEncoding.UTF8Tokens, 1).GetTopElement();
                if (topElement.Tag.Equals("IPermission") || topElement.Tag.Equals("Permission"))
                {
                    IPermission permission = XMLUtil.CreatePermission(topElement, PermissionState.None, false);
                    if (permission == null)
                    {
                        return null;
                    }
                    permission.FromXml(topElement);
                    return permission;
                }
                if (topElement.Tag.Equals("PermissionSet"))
                {
                    System.Security.PermissionSet set = new System.Security.PermissionSet();
                    set.FromXml(topElement, false, false);
                    return set;
                }
                if (topElement.Tag.Equals("PermissionToken"))
                {
                    PermissionToken token = new PermissionToken();
                    token.FromXml(topElement);
                    return token;
                }
                return null;
            }
            using (MemoryStream stream = new MemoryStream(blob, 1, blob.Length - 1))
            {
                return CrossAppDomainSerializer.DeserializeObject(stream);
            }
        }

        [SecuritySafeCritical]
        internal bool DisableFusionUpdatesFromADManager()
        {
            return DisableFusionUpdatesFromADManager(this.GetNativeHandle());
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool DisableFusionUpdatesFromADManager(AppDomainHandle domain);
        public void DoCallBack(CrossAppDomainDelegate callBackDelegate)
        {
            if (callBackDelegate == null)
            {
                throw new ArgumentNullException("callBackDelegate");
            }
            callBackDelegate();
        }

        [SecuritySafeCritical]
        private void EnableResolveAssembliesForIntrospection()
        {
            CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(this.ResolveAssemblyForIntrospection);
        }

        [SecuritySafeCritical]
        public int ExecuteAssembly(string assemblyFile)
        {
            return this.ExecuteAssembly(assemblyFile, (string[]) null);
        }

        [Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of ExecuteAssembly which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
        public int ExecuteAssembly(string assemblyFile, System.Security.Policy.Evidence assemblySecurity)
        {
            return this.ExecuteAssembly(assemblyFile, assemblySecurity, null);
        }

        [SecuritySafeCritical]
        public int ExecuteAssembly(string assemblyFile, string[] args)
        {
            RuntimeAssembly assembly = (RuntimeAssembly) Assembly.LoadFrom(assemblyFile);
            if (args == null)
            {
                args = new string[0];
            }
            return this.nExecuteAssembly(assembly, args);
        }

        [Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of ExecuteAssembly which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
        public int ExecuteAssembly(string assemblyFile, System.Security.Policy.Evidence assemblySecurity, string[] args)
        {
            if ((assemblySecurity != null) && !this.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
            }
            RuntimeAssembly assembly = (RuntimeAssembly) Assembly.LoadFrom(assemblyFile, assemblySecurity);
            if (args == null)
            {
                args = new string[0];
            }
            return this.nExecuteAssembly(assembly, args);
        }

        [SecuritySafeCritical]
        public int ExecuteAssembly(string assemblyFile, string[] args, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
        {
            RuntimeAssembly assembly = (RuntimeAssembly) Assembly.LoadFrom(assemblyFile, hashValue, hashAlgorithm);
            if (args == null)
            {
                args = new string[0];
            }
            return this.nExecuteAssembly(assembly, args);
        }

        [SecuritySafeCritical, Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of ExecuteAssembly which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public int ExecuteAssembly(string assemblyFile, System.Security.Policy.Evidence assemblySecurity, string[] args, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
        {
            if ((assemblySecurity != null) && !this.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
            }
            RuntimeAssembly assembly = (RuntimeAssembly) Assembly.LoadFrom(assemblyFile, assemblySecurity, hashValue, hashAlgorithm);
            if (args == null)
            {
                args = new string[0];
            }
            return this.nExecuteAssembly(assembly, args);
        }

        [SecuritySafeCritical]
        public int ExecuteAssemblyByName(string assemblyName)
        {
            return this.ExecuteAssemblyByName(assemblyName, (string[]) null);
        }

        [SecuritySafeCritical]
        public int ExecuteAssemblyByName(AssemblyName assemblyName, params string[] args)
        {
            RuntimeAssembly assembly = (RuntimeAssembly) Assembly.Load(assemblyName);
            if (args == null)
            {
                args = new string[0];
            }
            return this.nExecuteAssembly(assembly, args);
        }

        [SecuritySafeCritical, Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of ExecuteAssemblyByName which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public int ExecuteAssemblyByName(string assemblyName, System.Security.Policy.Evidence assemblySecurity)
        {
            return this.ExecuteAssemblyByName(assemblyName, assemblySecurity, null);
        }

        [SecuritySafeCritical]
        public int ExecuteAssemblyByName(string assemblyName, params string[] args)
        {
            RuntimeAssembly assembly = (RuntimeAssembly) Assembly.Load(assemblyName);
            if (args == null)
            {
                args = new string[0];
            }
            return this.nExecuteAssembly(assembly, args);
        }

        [SecuritySafeCritical, Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of ExecuteAssemblyByName which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public int ExecuteAssemblyByName(AssemblyName assemblyName, System.Security.Policy.Evidence assemblySecurity, params string[] args)
        {
            if ((assemblySecurity != null) && !this.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
            }
            RuntimeAssembly assembly = (RuntimeAssembly) Assembly.Load(assemblyName, assemblySecurity);
            if (args == null)
            {
                args = new string[0];
            }
            return this.nExecuteAssembly(assembly, args);
        }

        [SecuritySafeCritical, Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of ExecuteAssemblyByName which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public int ExecuteAssemblyByName(string assemblyName, System.Security.Policy.Evidence assemblySecurity, params string[] args)
        {
            if ((assemblySecurity != null) && !this.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
            }
            RuntimeAssembly assembly = (RuntimeAssembly) Assembly.Load(assemblyName, assemblySecurity);
            if (args == null)
            {
                args = new string[0];
            }
            return this.nExecuteAssembly(assembly, args);
        }

        [SecuritySafeCritical]
        internal void GetAppDomainManagerType(out string assembly, out string type)
        {
            string s = null;
            string str2 = null;
            GetAppDomainManagerType(this.GetNativeHandle(), JitHelpers.GetStringHandleOnStack(ref s), JitHelpers.GetStringHandleOnStack(ref str2));
            assembly = s;
            type = str2;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetAppDomainManagerType(AppDomainHandle domain, StringHandleOnStack retAssembly, StringHandleOnStack retType);
        public Assembly[] GetAssemblies()
        {
            return this.nGetAssemblies(false);
        }

        [Obsolete("AppDomain.GetCurrentThreadId has been deprecated because it does not provide a stable Id when managed threads are running on fibers (aka lightweight threads). To get a stable identifier for a managed thread, use the ManagedThreadId property on Thread.  http://go.microsoft.com/fwlink/?linkid=14202", false), DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();
        [SecuritySafeCritical]
        public object GetData(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            switch (AppDomainSetup.Locate(name))
            {
                case 0:
                    return this.FusionStore.ApplicationBase;

                case 1:
                    return this.FusionStore.ConfigurationFile;

                case 2:
                    return this.FusionStore.DynamicBase;

                case 3:
                    return this.FusionStore.DeveloperPath;

                case 4:
                    return this.FusionStore.ApplicationName;

                case 5:
                    return this.FusionStore.PrivateBinPath;

                case 6:
                    return this.FusionStore.PrivateBinPathProbe;

                case 7:
                    return this.FusionStore.ShadowCopyDirectories;

                case 8:
                    return this.FusionStore.ShadowCopyFiles;

                case 9:
                    return this.FusionStore.CachePath;

                case 10:
                    return this.FusionStore.LicenseFile;

                case 11:
                    return this.FusionStore.DisallowPublisherPolicy;

                case 12:
                    return this.FusionStore.DisallowCodeDownload;

                case 13:
                    return this.FusionStore.DisallowBindingRedirects;

                case 14:
                    return this.FusionStore.DisallowApplicationBaseProbing;

                case 15:
                    return this.FusionStore.GetConfigurationBytes();

                case -1:
                    object[] objArray;
                    if (name.Equals(AppDomainSetup.LoaderOptimizationKey))
                    {
                        return this.FusionStore.LoaderOptimization;
                    }
                    lock (((ICollection) this.LocalStore).SyncRoot)
                    {
                        this.LocalStore.TryGetValue(name, out objArray);
                    }
                    if (objArray == null)
                    {
                        return null;
                    }
                    if (objArray[1] != null)
                    {
                        ((IPermission) objArray[1]).Demand();
                    }
                    return objArray[0];
            }
            return null;
        }

        [SecurityCritical]
        internal Context GetDefaultContext()
        {
            if (this._DefaultContext == null)
            {
                this.CreateDefaultContext();
            }
            return this._DefaultContext;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern AppDomain GetDefaultDomain();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern string GetDynamicDir();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern IntPtr GetFusionContext();
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetGrantSet(AppDomainHandle domain, ObjectHandleOnStack retGrantSet);
        internal System.Security.PermissionSet GetHomogenousGrantSet(System.Security.Policy.Evidence evidence)
        {
            if (evidence.GetDelayEvaluatedHostEvidence<StrongName>() != null)
            {
                foreach (StrongName name in this._applicationTrust.FullTrustAssemblies)
                {
                    StrongNameMembershipCondition condition = new StrongNameMembershipCondition(name.PublicKey, name.Name, name.Version);
                    object usedEvidence = null;
                    if (((IReportMatchMembershipCondition) condition).Check(evidence, out usedEvidence))
                    {
                        IDelayEvaluatedEvidence evidence2 = usedEvidence as IDelayEvaluatedEvidence;
                        if (usedEvidence != null)
                        {
                            evidence2.MarkUsed();
                        }
                        return new System.Security.PermissionSet(PermissionState.Unrestricted);
                    }
                }
            }
            return this._applicationTrust.DefaultGrantSet.PermissionSet.Copy();
        }

        internal EvidenceBase GetHostEvidence(Type type)
        {
            if (this._SecurityIdentity != null)
            {
                return this._SecurityIdentity.GetHostEvidence(type);
            }
            return new System.Security.Policy.Evidence(new AppDomainEvidenceFactory(this)).GetHostEvidence(type);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal extern int GetId();
        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static int GetIdForUnload(AppDomain domain)
        {
            if (RemotingServices.IsTransparentProxy(domain))
            {
                return RemotingServices.GetServerDomainIdForProxy(domain);
            }
            return domain.Id;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool GetIsLegacyCasPolicyEnabled(AppDomainHandle domain);
        internal AppDomainHandle GetNativeHandle()
        {
            if (this._pDomain.IsNull())
            {
                throw new InvalidOperationException(Environment.GetResourceString("Argument_InvalidHandle"));
            }
            return new AppDomainHandle(this._pDomain);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern string GetOrInternString(string str);
        private static RuntimeAssembly GetRuntimeAssembly(Assembly asm)
        {
            if (asm != null)
            {
                RuntimeAssembly assembly = asm as RuntimeAssembly;
                if (assembly != null)
                {
                    return assembly;
                }
                AssemblyBuilder builder = asm as AssemblyBuilder;
                if (builder != null)
                {
                    return builder.InternalAssembly;
                }
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern IntPtr GetSecurityDescriptor();
        internal IPrincipal GetThreadPrincipal()
        {
            IPrincipal principal = null;
            IPrincipal principal2;
            lock (this)
            {
                if (this._DefaultPrincipal == null)
                {
                    switch (this._PrincipalPolicy)
                    {
                        case PrincipalPolicy.UnauthenticatedPrincipal:
                            principal = new GenericPrincipal(new GenericIdentity("", ""), new string[] { "" });
                            goto Label_0079;

                        case PrincipalPolicy.NoPrincipal:
                            principal = null;
                            goto Label_0079;

                        case PrincipalPolicy.WindowsPrincipal:
                            principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                            goto Label_0079;
                    }
                    principal = null;
                }
                else
                {
                    principal = this._DefaultPrincipal;
                }
            Label_0079:
                principal2 = principal;
            }
            return principal2;
        }

        public Type GetType()
        {
            return base.GetType();
        }

        [SecuritySafeCritical]
        private void InitializeCompatibilityFlags()
        {
            AppDomainSetup fusionStore = this.FusionStore;
            if (fusionStore.GetCompatibilityFlags() != null)
            {
                this._compatFlags = new Dictionary<string, object>(fusionStore.GetCompatibilityFlags(), StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                this._compatFlags = new Dictionary<string, object>();
            }
        }

        [SecurityCritical]
        private void InitializeDomainSecurity(System.Security.Policy.Evidence providedSecurityInfo, System.Security.Policy.Evidence creatorsSecurityInfo, bool generateDefaultEvidence, IntPtr parentSecurityDescriptor, bool publishAppDomain)
        {
            AppDomainSetup fusionStore = this.FusionStore;
            bool runtimeSuppliedHomogenousGrantSet = false;
            bool? nullable = this.IsCompatibilitySwitchSet("NetFx40_LegacySecurityPolicy");
            if (nullable.HasValue && nullable.Value)
            {
                this.SetLegacyCasPolicyEnabled();
            }
            if ((fusionStore.ApplicationTrust == null) && !this.IsLegacyCasPolicyEnabled)
            {
                fusionStore.ApplicationTrust = new System.Security.Policy.ApplicationTrust(new System.Security.PermissionSet(PermissionState.Unrestricted));
                runtimeSuppliedHomogenousGrantSet = true;
            }
            if (fusionStore.ActivationArguments != null)
            {
                System.ActivationContext activationContext = null;
                System.ApplicationIdentity applicationIdentity = null;
                string[] activationData = null;
                CmsUtils.CreateActivationContext(fusionStore.ActivationArguments.ApplicationFullName, fusionStore.ActivationArguments.ApplicationManifestPaths, fusionStore.ActivationArguments.UseFusionActivationContext, out applicationIdentity, out activationContext);
                activationData = fusionStore.ActivationArguments.ActivationData;
                providedSecurityInfo = CmsUtils.MergeApplicationEvidence(providedSecurityInfo, applicationIdentity, activationContext, activationData, fusionStore.ApplicationTrust);
                this.SetupApplicationHelper(providedSecurityInfo, creatorsSecurityInfo, applicationIdentity, activationContext, activationData);
            }
            else
            {
                System.Security.Policy.ApplicationTrust applicationTrust = fusionStore.ApplicationTrust;
                if (applicationTrust != null)
                {
                    this.SetupDomainSecurityForHomogeneousDomain(applicationTrust, runtimeSuppliedHomogenousGrantSet);
                }
            }
            System.Security.Policy.Evidence inputEvidence = (providedSecurityInfo != null) ? providedSecurityInfo : creatorsSecurityInfo;
            if ((inputEvidence == null) && generateDefaultEvidence)
            {
                inputEvidence = new System.Security.Policy.Evidence(new AppDomainEvidenceFactory(this));
            }
            if (this._domainManager != null)
            {
                System.Security.HostSecurityManager hostSecurityManager = this._domainManager.HostSecurityManager;
                if (hostSecurityManager != null)
                {
                    nSetHostSecurityManagerFlags(hostSecurityManager.Flags);
                    if ((hostSecurityManager.Flags & HostSecurityManagerOptions.HostAppDomainEvidence) == HostSecurityManagerOptions.HostAppDomainEvidence)
                    {
                        inputEvidence = hostSecurityManager.ProvideAppDomainEvidence(inputEvidence);
                        if ((inputEvidence != null) && (inputEvidence.Target == null))
                        {
                            inputEvidence.Target = new AppDomainEvidenceFactory(this);
                        }
                    }
                }
            }
            this._SecurityIdentity = inputEvidence;
            this.SetupDomainSecurity(inputEvidence, parentSecurityDescriptor, publishAppDomain);
            if (this._domainManager != null)
            {
                this.RunDomainManagerPostInitialization(this._domainManager);
            }
        }

        [SecurityCritical]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        private static AppDomain InternalCreateDomain(string imageLocation)
        {
            AppDomainSetup info = InternalCreateDomainSetup(imageLocation);
            return CreateDomain("Validator", null, info);
        }

        [SecurityCritical]
        internal static AppDomain InternalCreateDomain(string friendlyName, System.Security.Policy.Evidence securityInfo, AppDomainSetup info)
        {
            if (friendlyName == null)
            {
                throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_String"));
            }
            AppDomainManager domainManager = CurrentDomain.DomainManager;
            if (domainManager != null)
            {
                return domainManager.CreateDomain(friendlyName, securityInfo, info);
            }
            if (securityInfo != null)
            {
                new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
                CheckDomainCreationEvidence(info, securityInfo);
            }
            return nCreateDomain(friendlyName, info, securityInfo, (securityInfo == null) ? CurrentDomain.InternalEvidence : null, CurrentDomain.GetSecurityDescriptor());
        }

        private static AppDomainSetup InternalCreateDomainSetup(string imageLocation)
        {
            int num = imageLocation.LastIndexOf('\\');
            AppDomainSetup setup = new AppDomainSetup {
                ApplicationBase = imageLocation.Substring(0, num + 1)
            };
            StringBuilder builder = new StringBuilder(imageLocation.Substring(num + 1));
            builder.Append(AppDomainSetup.ConfigurationExtension);
            setup.ConfigurationFile = builder.ToString();
            return setup;
        }

        [SecurityCritical]
        internal ObjectHandle InternalCreateInstanceFromWithNoSecurity(string assemblyName, string typeName)
        {
            System.Security.PermissionSet.s_fullTrust.Assert();
            return this.CreateInstanceFrom(assemblyName, typeName);
        }

        [SecurityCritical]
        internal ObjectHandle InternalCreateInstanceFromWithNoSecurity(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, System.Security.Policy.Evidence securityAttributes)
        {
            System.Security.PermissionSet.s_fullTrust.Assert();
            return this.CreateInstanceFrom(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
        }

        [SecurityCritical]
        internal ObjectHandle InternalCreateInstanceWithNoSecurity(string assemblyName, string typeName)
        {
            System.Security.PermissionSet.s_fullTrust.Assert();
            return this.CreateInstance(assemblyName, typeName);
        }

        [SecurityCritical]
        internal ObjectHandle InternalCreateInstanceWithNoSecurity(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, System.Security.Policy.Evidence securityAttributes)
        {
            System.Security.PermissionSet.s_fullTrust.Assert();
            return this.CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        private AssemblyBuilder InternalDefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, System.Security.Policy.Evidence evidence, System.Security.PermissionSet requiredPermissions, System.Security.PermissionSet optionalPermissions, System.Security.PermissionSet refusedPermissions, ref StackCrawlMark stackMark, IEnumerable<CustomAttributeBuilder> assemblyAttributes, SecurityContextSource securityContextSource)
        {
            return AssemblyBuilder.InternalDefineDynamicAssembly(name, access, dir, evidence, requiredPermissions, optionalPermissions, refusedPermissions, ref stackMark, assemblyAttributes, securityContextSource);
        }

        [SecurityCritical]
        internal void InternalSetCachePath(string path)
        {
            this.FusionStore.CachePath = path;
            if (this.FusionStore.Value[9] != null)
            {
                AppDomainSetup.UpdateContextProperty(this.GetFusionContext(), AppDomainSetup.CachePathKey, this.FusionStore.Value[9]);
            }
        }

        [SecurityCritical]
        private void InternalSetDomainContext(string imageLocation)
        {
            this.SetupFusionStore(InternalCreateDomainSetup(imageLocation), null);
        }

        [SecurityCritical]
        internal void InternalSetDynamicBase(string path)
        {
            this.FusionStore.DynamicBase = path;
            if (this.FusionStore.Value[2] != null)
            {
                AppDomainSetup.UpdateContextProperty(this.GetFusionContext(), AppDomainSetup.DynamicBaseKey, this.FusionStore.Value[2]);
            }
        }

        [SecurityCritical]
        internal void InternalSetPrivateBinPath(string path)
        {
            AppDomainSetup.UpdateContextProperty(this.GetFusionContext(), AppDomainSetup.PrivateBinPathKey, path);
            this.FusionStore.PrivateBinPath = path;
        }

        [SecurityCritical]
        internal void InternalSetShadowCopyFiles()
        {
            AppDomainSetup.UpdateContextProperty(this.GetFusionContext(), AppDomainSetup.ShadowCopyFilesKey, "true");
            this.FusionStore.ShadowCopyFiles = "true";
        }

        [SecurityCritical]
        internal void InternalSetShadowCopyPath(string path)
        {
            if (path != null)
            {
                AppDomainSetup.UpdateContextProperty(this.GetFusionContext(), AppDomainSetup.ShadowCopyDirectoriesKey, path);
            }
            this.FusionStore.ShadowCopyDirectories = path;
        }

        [SecuritySafeCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private bool IsAssemblyOnAptcaVisibleList(RuntimeAssembly assembly)
        {
            if (this._aptcaVisibleAssemblies == null)
            {
                return false;
            }
            string str = assembly.GetName().GetNameWithPublicKey().ToUpperInvariant();
            return (Array.BinarySearch<string>(this._aptcaVisibleAssemblies, str, StringComparer.OrdinalIgnoreCase) >= 0);
        }

        [SecurityCritical]
        private unsafe bool IsAssemblyOnAptcaVisibleListRaw(char* namePtr, int nameLen, byte* keyTokenPtr, int keyTokenLen)
        {
            if (this._aptcaVisibleAssemblies == null)
            {
                return false;
            }
            string str = new string(namePtr, 0, nameLen);
            byte[] publicKeyToken = new byte[keyTokenLen];
            for (int i = 0; i < publicKeyToken.Length; i++)
            {
                publicKeyToken[i] = keyTokenPtr[i];
            }
            AssemblyName name = new AssemblyName {
                Name = str
            };
            name.SetPublicKeyToken(publicKeyToken);
            try
            {
                return (Array.BinarySearch(this._aptcaVisibleAssemblies, name, new CAPTCASearcher()) >= 0);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public bool? IsCompatibilitySwitchSet(string value)
        {
            if (this._compatFlags == null)
            {
                return null;
            }
            return new bool?(this._compatFlags.ContainsKey(value));
        }

        public bool IsDefaultAppDomain()
        {
            return (this.GetId() == 1);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern bool IsDomainIdValid(int id);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public extern bool IsFinalizingForUnload();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern string IsStringInterned(string str);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern bool IsUnloadingForcedFinalize();
        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public Assembly Load(AssemblyName assemblyRef)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.InternalLoadAssemblyName(assemblyRef, null, ref lookForMyCaller, false, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public Assembly Load(string assemblyString)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.InternalLoad(assemblyString, null, ref lookForMyCaller, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public Assembly Load(byte[] rawAssembly)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.nLoadImage(rawAssembly, null, null, ref lookForMyCaller, false, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public Assembly Load(byte[] rawAssembly, byte[] rawSymbolStore)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.nLoadImage(rawAssembly, rawSymbolStore, null, ref lookForMyCaller, false, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of Load which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public Assembly Load(AssemblyName assemblyRef, System.Security.Policy.Evidence assemblySecurity)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.InternalLoadAssemblyName(assemblyRef, assemblySecurity, ref lookForMyCaller, false, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of Load which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
        public Assembly Load(string assemblyString, System.Security.Policy.Evidence assemblySecurity)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.InternalLoad(assemblyString, assemblySecurity, ref lookForMyCaller, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of Load which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkId=155570 for more information."), SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlEvidence=true)]
        public Assembly Load(byte[] rawAssembly, byte[] rawSymbolStore, System.Security.Policy.Evidence securityEvidence)
        {
            if ((securityEvidence != null) && !this.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
            }
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.nLoadImage(rawAssembly, rawSymbolStore, securityEvidence, ref lookForMyCaller, false, SecurityContextSource.CurrentAssembly);
        }

        [SecurityCritical]
        private static byte[] MarshalObject(object o)
        {
            CodeAccessPermission.Assert(true);
            return Serialize(o);
        }

        [SecurityCritical]
        private static byte[] MarshalObjects(object o1, object o2, out byte[] blob2)
        {
            CodeAccessPermission.Assert(true);
            byte[] buffer = Serialize(o1);
            blob2 = Serialize(o2);
            return buffer;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        private extern string nApplyPolicy(AssemblyName an);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void nChangeSecurityPolicy();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern AppDomain nCreateDomain(string friendlyName, AppDomainSetup setup, System.Security.Policy.Evidence providedSecurityInfo, System.Security.Policy.Evidence creatorsSecurityInfo, IntPtr parentSecurityDescriptor);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern ObjRef nCreateInstance(string friendlyName, AppDomainSetup setup, System.Security.Policy.Evidence providedSecurityInfo, System.Security.Policy.Evidence creatorsSecurityInfo, IntPtr parentSecurityDescriptor);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void nEnableMonitoring();
        internal int nExecuteAssembly(RuntimeAssembly assembly, string[] args)
        {
            return this._nExecuteAssembly(assembly, args);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        private extern Assembly[] nGetAssemblies(bool forIntrospection);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern string nGetFriendlyName();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern long nGetLastSurvivedMemorySize();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern long nGetLastSurvivedProcessMemorySize();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern long nGetTotalAllocatedMemorySize();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern long nGetTotalProcessorTime();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern bool nIsDefaultAppDomainForEvidence();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool nMonitoringIsEnabled();
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void nSetDisableInterfaceCache();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void nSetHostSecurityManagerFlags(HostSecurityManagerOptions flags);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void nSetupFriendlyName(string friendlyName);
        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.MayFail), SecurityCritical]
        internal static extern void nUnload(int domainInternal);
        private void OnAssemblyLoadEvent(RuntimeAssembly LoadedAssembly)
        {
            AssemblyLoadEventHandler assemblyLoad = this.AssemblyLoad;
            if (assemblyLoad != null)
            {
                AssemblyLoadEventArgs args = new AssemblyLoadEventArgs(LoadedAssembly);
                assemblyLoad(this, args);
            }
        }

        private RuntimeAssembly OnAssemblyResolveEvent(RuntimeAssembly assembly, string assemblyFullName)
        {
            ResolveEventHandler assemblyResolve = this.AssemblyResolve;
            if (assemblyResolve != null)
            {
                Delegate[] invocationList = assemblyResolve.GetInvocationList();
                int length = invocationList.Length;
                for (int i = 0; i < length; i++)
                {
                    RuntimeAssembly runtimeAssembly = GetRuntimeAssembly(((ResolveEventHandler) invocationList[i])(this, new ResolveEventArgs(assemblyFullName, assembly)));
                    if (runtimeAssembly != null)
                    {
                        return runtimeAssembly;
                    }
                }
            }
            return null;
        }

        private RuntimeAssembly OnReflectionOnlyAssemblyResolveEvent(RuntimeAssembly assembly, string assemblyFullName)
        {
            ResolveEventHandler reflectionOnlyAssemblyResolve = this.ReflectionOnlyAssemblyResolve;
            if (reflectionOnlyAssemblyResolve != null)
            {
                Delegate[] invocationList = reflectionOnlyAssemblyResolve.GetInvocationList();
                int length = invocationList.Length;
                for (int i = 0; i < length; i++)
                {
                    RuntimeAssembly runtimeAssembly = GetRuntimeAssembly(((ResolveEventHandler) invocationList[i])(this, new ResolveEventArgs(assemblyFullName, assembly)));
                    if (runtimeAssembly != null)
                    {
                        return runtimeAssembly;
                    }
                }
            }
            return null;
        }

        private RuntimeAssembly OnResourceResolveEvent(RuntimeAssembly assembly, string resourceName)
        {
            ResolveEventHandler resourceResolve = this.ResourceResolve;
            if (resourceResolve != null)
            {
                Delegate[] invocationList = resourceResolve.GetInvocationList();
                int length = invocationList.Length;
                for (int i = 0; i < length; i++)
                {
                    RuntimeAssembly runtimeAssembly = GetRuntimeAssembly(((ResolveEventHandler) invocationList[i])(this, new ResolveEventArgs(resourceName, assembly)));
                    if (runtimeAssembly != null)
                    {
                        return runtimeAssembly;
                    }
                }
            }
            return null;
        }

        private RuntimeAssembly OnTypeResolveEvent(RuntimeAssembly assembly, string typeName)
        {
            ResolveEventHandler typeResolve = this.TypeResolve;
            if (typeResolve != null)
            {
                Delegate[] invocationList = typeResolve.GetInvocationList();
                int length = invocationList.Length;
                for (int i = 0; i < length; i++)
                {
                    RuntimeAssembly runtimeAssembly = GetRuntimeAssembly(((ResolveEventHandler) invocationList[i])(this, new ResolveEventArgs(typeName, assembly)));
                    if (runtimeAssembly != null)
                    {
                        return runtimeAssembly;
                    }
                }
            }
            return null;
        }

        [SecurityCritical]
        private static object PrepareDataForSetup(string friendlyName, AppDomainSetup setup, System.Security.Policy.Evidence providedSecurityInfo, System.Security.Policy.Evidence creatorsSecurityInfo, IntPtr parentSecurityDescriptor, string securityZone, string[] propertyNames, string[] propertyValues)
        {
            byte[] buffer = null;
            bool flag = false;
            EvidenceCollection evidences = null;
            if ((providedSecurityInfo != null) || (creatorsSecurityInfo != null))
            {
                System.Security.HostSecurityManager manager = (CurrentDomain.DomainManager != null) ? CurrentDomain.DomainManager.HostSecurityManager : null;
                if (((manager == null) || (manager.GetType() == typeof(System.Security.HostSecurityManager))) || ((manager.Flags & HostSecurityManagerOptions.HostAppDomainEvidence) != HostSecurityManagerOptions.HostAppDomainEvidence))
                {
                    if (((providedSecurityInfo != null) && providedSecurityInfo.IsUnmodified) && ((providedSecurityInfo.Target != null) && (providedSecurityInfo.Target is AppDomainEvidenceFactory)))
                    {
                        providedSecurityInfo = null;
                        flag = true;
                    }
                    if (((creatorsSecurityInfo != null) && creatorsSecurityInfo.IsUnmodified) && ((creatorsSecurityInfo.Target != null) && (creatorsSecurityInfo.Target is AppDomainEvidenceFactory)))
                    {
                        creatorsSecurityInfo = null;
                        flag = true;
                    }
                }
            }
            if ((providedSecurityInfo != null) || (creatorsSecurityInfo != null))
            {
                evidences = new EvidenceCollection {
                    ProvidedSecurityInfo = providedSecurityInfo,
                    CreatorsSecurityInfo = creatorsSecurityInfo
                };
            }
            if (evidences != null)
            {
                buffer = CrossAppDomainSerializer.SerializeObject(evidences).GetBuffer();
            }
            AppDomainInitializerInfo info = null;
            if ((setup != null) && (setup.AppDomainInitializer != null))
            {
                info = new AppDomainInitializerInfo(setup.AppDomainInitializer);
            }
            AppDomainSetup setup2 = new AppDomainSetup(setup, false);
            return new object[] { friendlyName, setup2, parentSecurityDescriptor, flag, buffer, info, securityZone, propertyNames, propertyValues };
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void PublishAnonymouslyHostedDynamicMethodsAssembly(RuntimeAssembly assemblyHandle);
        public Assembly[] ReflectionOnlyGetAssemblies()
        {
            return this.nGetAssemblies(true);
        }

        private Assembly ResolveAssemblyForIntrospection(object sender, ResolveEventArgs args)
        {
            return Assembly.ReflectionOnlyLoad(this.ApplyPolicy(args.Name));
        }

        [SecurityCritical]
        private void RunDomainManagerPostInitialization(AppDomainManager domainManager)
        {
            HostExecutionContextManager hostExecutionContextManager = domainManager.HostExecutionContextManager;
            if (this.IsLegacyCasPolicyEnabled)
            {
                System.Security.HostSecurityManager hostSecurityManager = domainManager.HostSecurityManager;
                if ((hostSecurityManager != null) && ((hostSecurityManager.Flags & HostSecurityManagerOptions.HostPolicyLevel) == HostSecurityManagerOptions.HostPolicyLevel))
                {
                    PolicyLevel domainPolicy = hostSecurityManager.DomainPolicy;
                    if (domainPolicy != null)
                    {
                        this.SetAppDomainPolicy(domainPolicy);
                    }
                }
            }
        }

        private static void RunInitializer(AppDomainSetup setup)
        {
            if (setup.AppDomainInitializer != null)
            {
                string[] args = null;
                if (setup.AppDomainInitializerArguments != null)
                {
                    args = (string[]) setup.AppDomainInitializerArguments.Clone();
                }
                setup.AppDomainInitializer(args);
            }
        }

        [SecurityCritical]
        private static byte[] Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }
            if (o is ISecurityEncodable)
            {
                SecurityElement element = ((ISecurityEncodable) o).ToXml();
                MemoryStream stream = new MemoryStream(0x1000);
                stream.WriteByte(0);
                StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
                element.ToWriter(writer);
                writer.Flush();
                return stream.ToArray();
            }
            MemoryStream stm = new MemoryStream();
            stm.WriteByte(1);
            CrossAppDomainSerializer.SerializeObject(o, stm);
            return stm.ToArray();
        }

        [SecuritySafeCritical]
        private void SetAppDomainManagerType(string assembly, string type)
        {
            SetAppDomainManagerType(this.GetNativeHandle(), assembly, type);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void SetAppDomainManagerType(AppDomainHandle domain, string assembly, string type);
        [SecurityCritical, Obsolete("AppDomain policy levels are obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public void SetAppDomainPolicy(PolicyLevel domainPolicy)
        {
            if (domainPolicy == null)
            {
                throw new ArgumentNullException("domainPolicy");
            }
            if (!this.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyExplicit"));
            }
            lock (this)
            {
                if (this._HasSetPolicy)
                {
                    throw new PolicyException(Environment.GetResourceString("Policy_PolicyAlreadySet"));
                }
                this._HasSetPolicy = true;
                this.nChangeSecurityPolicy();
            }
            SecurityManager.PolicyManager.AddLevel(domainPolicy);
        }

        [Obsolete("AppDomain.SetCachePath has been deprecated. Please investigate the use of AppDomainSetup.CachePath instead. http://go.microsoft.com/fwlink/?linkid=14202"), SecurityCritical]
        public void SetCachePath(string path)
        {
            this.InternalSetCachePath(path);
        }

        [SecurityCritical]
        private void SetCanonicalConditionalAptcaList(string canonicalList)
        {
            SetCanonicalConditionalAptcaList(this.GetNativeHandle(), canonicalList);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void SetCanonicalConditionalAptcaList(AppDomainHandle appDomain, string canonicalList);
        [SecurityCritical]
        public void SetData(string name, object data)
        {
            this.SetDataHelper(name, data, null);
        }

        [SecurityCritical]
        public void SetData(string name, object data, IPermission permission)
        {
            this.SetDataHelper(name, data, permission);
        }

        [SecurityCritical]
        private void SetDataHelper(string name, object data, IPermission permission)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Equals("IgnoreSystemPolicy"))
            {
                lock (this)
                {
                    if (!this._HasSetPolicy)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SetData"));
                    }
                }
                new System.Security.PermissionSet(PermissionState.Unrestricted).Demand();
            }
            int index = AppDomainSetup.Locate(name);
            if (index == -1)
            {
                lock (((ICollection) this.LocalStore).SyncRoot)
                {
                    this.LocalStore[name] = new object[] { data, permission };
                    return;
                }
            }
            if (permission != null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SetData"));
            }
            switch (index)
            {
                case 2:
                    this.FusionStore.DynamicBase = (string) data;
                    return;

                case 3:
                    this.FusionStore.DeveloperPath = (string) data;
                    return;

                case 7:
                    this.FusionStore.ShadowCopyDirectories = (string) data;
                    return;

                case 11:
                    if (data == null)
                    {
                        this.FusionStore.DisallowPublisherPolicy = false;
                        return;
                    }
                    this.FusionStore.DisallowPublisherPolicy = true;
                    return;

                case 12:
                    if (data == null)
                    {
                        this.FusionStore.DisallowCodeDownload = false;
                        return;
                    }
                    this.FusionStore.DisallowCodeDownload = true;
                    return;

                case 13:
                    if (data == null)
                    {
                        this.FusionStore.DisallowBindingRedirects = false;
                        return;
                    }
                    this.FusionStore.DisallowBindingRedirects = true;
                    return;

                case 14:
                    if (data == null)
                    {
                        this.FusionStore.DisallowApplicationBaseProbing = false;
                        return;
                    }
                    this.FusionStore.DisallowApplicationBaseProbing = true;
                    return;

                case 15:
                    this.FusionStore.SetConfigurationBytes((byte[]) data);
                    return;
            }
            this.FusionStore.Value[index] = (string) data;
        }

        [SecurityCritical, Obsolete("AppDomain.SetDynamicBase has been deprecated. Please investigate the use of AppDomainSetup.DynamicBase instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public void SetDynamicBase(string path)
        {
            this.InternalSetDynamicBase(path);
        }

        [SecurityCritical]
        private void SetLegacyCasPolicyEnabled()
        {
            SetLegacyCasPolicyEnabled(this.GetNativeHandle());
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void SetLegacyCasPolicyEnabled(AppDomainHandle domain);
        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public void SetPrincipalPolicy(PrincipalPolicy policy)
        {
            this._PrincipalPolicy = policy;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void SetSecurityHomogeneousFlag(AppDomainHandle domain, [MarshalAs(UnmanagedType.Bool)] bool runtimeSuppliedHomogenousGrantSet);
        [Obsolete("AppDomain.SetShadowCopyFiles has been deprecated. Please investigate the use of AppDomainSetup.ShadowCopyFiles instead. http://go.microsoft.com/fwlink/?linkid=14202"), SecurityCritical]
        public void SetShadowCopyFiles()
        {
            this.InternalSetShadowCopyFiles();
        }

        [Obsolete("AppDomain.SetShadowCopyPath has been deprecated. Please investigate the use of AppDomainSetup.ShadowCopyDirectories instead. http://go.microsoft.com/fwlink/?linkid=14202"), SecurityCritical]
        public void SetShadowCopyPath(string path)
        {
            this.InternalSetShadowCopyPath(path);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public void SetThreadPrincipal(IPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }
            lock (this)
            {
                if (this._DefaultPrincipal != null)
                {
                    throw new PolicyException(Environment.GetResourceString("Policy_PrincipalTwice"));
                }
                this._DefaultPrincipal = principal;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        private static object Setup(object arg)
        {
            object[] objArray = (object[]) arg;
            string friendlyName = (string) objArray[0];
            AppDomainSetup copy = (AppDomainSetup) objArray[1];
            IntPtr parentSecurityDescriptor = (IntPtr) objArray[2];
            bool generateDefaultEvidence = (bool) objArray[3];
            byte[] buffer = (byte[]) objArray[4];
            AppDomainInitializerInfo info = (AppDomainInitializerInfo) objArray[5];
            string text1 = (string) objArray[6];
            string[] strArray = (string[]) objArray[7];
            string[] strArray2 = (string[]) objArray[8];
            System.Security.Policy.Evidence providedSecurityInfo = null;
            System.Security.Policy.Evidence creatorsSecurityInfo = null;
            AppDomain currentDomain = CurrentDomain;
            AppDomainSetup setup2 = new AppDomainSetup(copy, false);
            if ((strArray != null) && (strArray2 != null))
            {
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (strArray[i] == "APPBASE")
                    {
                        if (strArray2[i] == null)
                        {
                            throw new ArgumentNullException("APPBASE");
                        }
                        if (Path.IsRelative(strArray2[i]))
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_AbsolutePathRequired"));
                        }
                        setup2.ApplicationBase = Path.NormalizePath(strArray2[i], true);
                    }
                    else if ((strArray[i] == "LOCATION_URI") && (providedSecurityInfo == null))
                    {
                        providedSecurityInfo = new System.Security.Policy.Evidence();
                        providedSecurityInfo.AddHostEvidence<Url>(new Url(strArray2[i]));
                        currentDomain.SetDataHelper(strArray[i], strArray2[i], null);
                    }
                    else if (strArray[i] == "LOADER_OPTIMIZATION")
                    {
                        if (strArray2[i] == null)
                        {
                            throw new ArgumentNullException("LOADER_OPTIMIZATION");
                        }
                        string str2 = strArray2[i];
                        if (str2 == null)
                        {
                            goto Label_01B5;
                        }
                        if (!(str2 == "SingleDomain"))
                        {
                            if (str2 == "MultiDomain")
                            {
                                goto Label_0197;
                            }
                            if (str2 == "MultiDomainHost")
                            {
                                goto Label_01A1;
                            }
                            if (str2 == "NotSpecified")
                            {
                                goto Label_01AB;
                            }
                            goto Label_01B5;
                        }
                        setup2.LoaderOptimization = LoaderOptimization.SingleDomain;
                    }
                    continue;
                Label_0197:
                    setup2.LoaderOptimization = LoaderOptimization.MultiDomain;
                    continue;
                Label_01A1:
                    setup2.LoaderOptimization = LoaderOptimization.MultiDomainHost;
                    continue;
                Label_01AB:
                    setup2.LoaderOptimization = LoaderOptimization.NotSpecified;
                    continue;
                Label_01B5:
                    throw new ArgumentException(Environment.GetResourceString("Argument_UnrecognizedLoaderOptimization"), "LOADER_OPTIMIZATION");
                }
            }
            currentDomain.SetupFusionStore(setup2, null);
            AppDomainSetup fusionStore = currentDomain.FusionStore;
            if (buffer != null)
            {
                EvidenceCollection evidences = (EvidenceCollection) CrossAppDomainSerializer.DeserializeObject(new MemoryStream(buffer));
                providedSecurityInfo = evidences.ProvidedSecurityInfo;
                creatorsSecurityInfo = evidences.CreatorsSecurityInfo;
            }
            currentDomain.nSetupFriendlyName(friendlyName);
            if ((copy != null) && copy.SandboxInterop)
            {
                currentDomain.nSetDisableInterfaceCache();
            }
            if ((fusionStore.AppDomainManagerAssembly != null) && (fusionStore.AppDomainManagerType != null))
            {
                currentDomain.SetAppDomainManagerType(fusionStore.AppDomainManagerAssembly, fusionStore.AppDomainManagerType);
            }
            currentDomain.PartialTrustVisibleAssemblies = fusionStore.PartialTrustVisibleAssemblies;
            currentDomain.CreateAppDomainManager();
            currentDomain.InitializeDomainSecurity(providedSecurityInfo, creatorsSecurityInfo, generateDefaultEvidence, parentSecurityDescriptor, true);
            if (info != null)
            {
                fusionStore.AppDomainInitializer = info.Unwrap();
            }
            RunInitializer(fusionStore);
            ObjectHandle handle = null;
            if ((fusionStore.ActivationArguments != null) && fusionStore.ActivationArguments.ActivateInstance)
            {
                handle = Activator.CreateInstance(currentDomain.ActivationContext);
            }
            return RemotingServices.MarshalInternal(handle, null, null);
        }

        [SecurityCritical]
        private void SetupApplicationHelper(System.Security.Policy.Evidence providedSecurityInfo, System.Security.Policy.Evidence creatorsSecurityInfo, System.ApplicationIdentity appIdentity, System.ActivationContext activationContext, string[] activationData)
        {
            System.Security.Policy.ApplicationTrust appTrust = CurrentDomain.HostSecurityManager.DetermineApplicationTrust(providedSecurityInfo, creatorsSecurityInfo, new TrustManagerContext());
            if ((appTrust == null) || !appTrust.IsApplicationTrustedToRun)
            {
                throw new PolicyException(Environment.GetResourceString("Policy_NoExecutionPermission"), -2146233320, null);
            }
            if (activationContext != null)
            {
                this.SetupDomainForApplication(activationContext, activationData);
            }
            this.SetupDomainSecurityForApplication(appIdentity, appTrust);
        }

        private void SetupDefaultClickOnceDomain(string fullName, string[] manifestPaths, string[] activationData)
        {
            this.FusionStore.ActivationArguments = new ActivationArguments(fullName, manifestPaths, activationData);
        }

        [SecurityCritical]
        private void SetupDomain(bool allowRedirects, string path, string configFile, string[] propertyNames, string[] propertyValues)
        {
            lock (this)
            {
                if (this._FusionStore == null)
                {
                    AppDomainSetup info = new AppDomainSetup();
                    info.SetupDefaults(RuntimeEnvironment.GetModuleFileName());
                    if (path != null)
                    {
                        info.Value[0] = path;
                    }
                    if (configFile != null)
                    {
                        info.Value[1] = configFile;
                    }
                    if (!allowRedirects)
                    {
                        info.DisallowBindingRedirects = true;
                    }
                    if (propertyNames != null)
                    {
                        for (int i = 0; i < propertyNames.Length; i++)
                        {
                            if (string.Equals(propertyNames[i], "PARTIAL_TRUST_VISIBLE_ASSEMBLIES", StringComparison.Ordinal) && (propertyValues[i] != null))
                            {
                                if (propertyValues[i].Length > 0)
                                {
                                    info.PartialTrustVisibleAssemblies = propertyValues[i].Split(new char[] { ';' });
                                }
                                else
                                {
                                    info.PartialTrustVisibleAssemblies = new string[0];
                                }
                            }
                        }
                    }
                    this.PartialTrustVisibleAssemblies = info.PartialTrustVisibleAssemblies;
                    this.SetupFusionStore(info, null);
                }
            }
        }

        [SecurityCritical]
        private void SetupDomainForApplication(System.ActivationContext activationContext, string[] activationData)
        {
            if (this.IsDefaultAppDomain())
            {
                AppDomainSetup fusionStore = this.FusionStore;
                fusionStore.ActivationArguments = new ActivationArguments(activationContext, activationData);
                string entryPointFullPath = CmsUtils.GetEntryPointFullPath(activationContext);
                if (!string.IsNullOrEmpty(entryPointFullPath))
                {
                    fusionStore.SetupDefaults(entryPointFullPath);
                }
                else
                {
                    fusionStore.ApplicationBase = activationContext.ApplicationDirectory;
                }
                this.SetupFusionStore(fusionStore, null);
            }
            activationContext.PrepareForExecution();
            activationContext.SetApplicationState(System.ActivationContext.ApplicationState.Starting);
            activationContext.SetApplicationState(System.ActivationContext.ApplicationState.Running);
            IPermission permission = null;
            string dataDirectory = activationContext.DataDirectory;
            if ((dataDirectory != null) && (dataDirectory.Length > 0))
            {
                permission = new FileIOPermission(FileIOPermissionAccess.PathDiscovery, dataDirectory);
            }
            this.SetData("DataDirectory", dataDirectory, permission);
            this._activationContext = activationContext;
        }

        [SecurityCritical]
        private void SetupDomainSecurity(System.Security.Policy.Evidence appDomainEvidence, IntPtr creatorsSecurityDescriptor, bool publishAppDomain)
        {
            System.Security.Policy.Evidence o = appDomainEvidence;
            SetupDomainSecurity(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<System.Security.Policy.Evidence>(ref o), creatorsSecurityDescriptor, publishAppDomain);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void SetupDomainSecurity(AppDomainHandle appDomain, ObjectHandleOnStack appDomainEvidence, IntPtr creatorsSecurityDescriptor, [MarshalAs(UnmanagedType.Bool)] bool publishAppDomain);
        [SecurityCritical]
        private void SetupDomainSecurityForApplication(System.ApplicationIdentity appIdentity, System.Security.Policy.ApplicationTrust appTrust)
        {
            this._applicationIdentity = appIdentity;
            this.SetupDomainSecurityForHomogeneousDomain(appTrust, false);
        }

        [SecurityCritical]
        private void SetupDomainSecurityForHomogeneousDomain(System.Security.Policy.ApplicationTrust appTrust, bool runtimeSuppliedHomogenousGrantSet)
        {
            if (runtimeSuppliedHomogenousGrantSet)
            {
                this._FusionStore.ApplicationTrust = null;
            }
            this._applicationTrust = appTrust;
            SetSecurityHomogeneousFlag(this.GetNativeHandle(), runtimeSuppliedHomogenousGrantSet);
        }

        [SecurityCritical]
        private void SetupFusionStore(AppDomainSetup info, AppDomainSetup oldInfo)
        {
            if (oldInfo == null)
            {
                if ((info.Value[0] == null) || (info.Value[1] == null))
                {
                    AppDomain defaultDomain = GetDefaultDomain();
                    if (this == defaultDomain)
                    {
                        info.SetupDefaults(RuntimeEnvironment.GetModuleFileName());
                    }
                    else
                    {
                        if (info.Value[1] == null)
                        {
                            info.ConfigurationFile = defaultDomain.FusionStore.Value[1];
                        }
                        if (info.Value[0] == null)
                        {
                            info.ApplicationBase = defaultDomain.FusionStore.Value[0];
                        }
                        if (info.Value[4] == null)
                        {
                            info.ApplicationName = defaultDomain.FusionStore.Value[4];
                        }
                    }
                }
                if (info.Value[5] == null)
                {
                    info.PrivateBinPath = Environment.nativeGetEnvironmentVariable(AppDomainSetup.PrivateBinPathEnvironmentVariable);
                }
                if (info.DeveloperPath == null)
                {
                    info.DeveloperPath = RuntimeEnvironment.GetDeveloperPath();
                }
            }
            IntPtr fusionContext = this.GetFusionContext();
            info.SetupFusionContext(fusionContext, oldInfo);
            if ((info.LoaderOptimization != LoaderOptimization.NotSpecified) || ((oldInfo != null) && (info.LoaderOptimization != oldInfo.LoaderOptimization)))
            {
                this.UpdateLoaderOptimization(info.LoaderOptimization);
            }
            this._FusionStore = info;
        }

        [SecurityCritical]
        private void SetupLoaderOptimization(LoaderOptimization policy)
        {
            if (policy != LoaderOptimization.NotSpecified)
            {
                this.FusionStore.LoaderOptimization = policy;
                this.UpdateLoaderOptimization(this.FusionStore.LoaderOptimization);
            }
        }

        void _AppDomain.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _AppDomain.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _AppDomain.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _AppDomain.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            string str = this.nGetFriendlyName();
            if (str != null)
            {
                builder.Append(Environment.GetResourceString("Loader_Name") + str);
                builder.Append(Environment.NewLine);
            }
            if ((this._Policies == null) || (this._Policies.Length == 0))
            {
                builder.Append(Environment.GetResourceString("Loader_NoContextPolicies") + Environment.NewLine);
            }
            else
            {
                builder.Append(Environment.GetResourceString("Loader_ContextPolicies") + Environment.NewLine);
                for (int i = 0; i < this._Policies.Length; i++)
                {
                    builder.Append(this._Policies[i]);
                    builder.Append(Environment.NewLine);
                }
            }
            return builder.ToString();
        }

        private void TurnOnBindingRedirects()
        {
            this._FusionStore.DisallowBindingRedirects = false;
        }

        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.MayFail), SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlAppDomain=true)]
        public static void Unload(AppDomain domain)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }
            try
            {
                int idForUnload = GetIdForUnload(domain);
                if (idForUnload == 0)
                {
                    throw new CannotUnloadAppDomainException();
                }
                nUnload(idForUnload);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [SecurityCritical]
        private static object UnmarshalObject(byte[] blob)
        {
            CodeAccessPermission.Assert(true);
            return Deserialize(blob);
        }

        [SecurityCritical]
        private static object UnmarshalObjects(byte[] blob1, byte[] blob2, out object o2)
        {
            CodeAccessPermission.Assert(true);
            object obj2 = Deserialize(blob1);
            o2 = Deserialize(blob2);
            return obj2;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern void UpdateLoaderOptimization(LoaderOptimization optimization);

        public System.ActivationContext ActivationContext
        {
            [SecurityCritical]
            get
            {
                return this._activationContext;
            }
        }

        public System.ApplicationIdentity ApplicationIdentity
        {
            [SecurityCritical]
            get
            {
                return this._applicationIdentity;
            }
        }

        public System.Security.Policy.ApplicationTrust ApplicationTrust
        {
            [SecurityCritical]
            get
            {
                return this._applicationTrust;
            }
        }

        public string BaseDirectory
        {
            [SecuritySafeCritical]
            get
            {
                return this.FusionStore.ApplicationBase;
            }
        }

        public static AppDomain CurrentDomain
        {
            get
            {
                return Thread.GetDomain();
            }
        }

        public AppDomainManager DomainManager
        {
            [SecurityCritical]
            get
            {
                return this._domainManager;
            }
        }

        public string DynamicDirectory
        {
            [SecuritySafeCritical]
            get
            {
                string dynamicDir = this.GetDynamicDir();
                if (dynamicDir != null)
                {
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery, dynamicDir).Demand();
                }
                return dynamicDir;
            }
        }

        public System.Security.Policy.Evidence Evidence
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlEvidence=true)]
            get
            {
                return this.EvidenceNoDemand;
            }
        }

        internal System.Security.Policy.Evidence EvidenceNoDemand
        {
            [SecurityCritical]
            get
            {
                if (this._SecurityIdentity != null)
                {
                    return this._SecurityIdentity.Clone();
                }
                if (!this.IsDefaultAppDomain() && this.nIsDefaultAppDomainForEvidence())
                {
                    return GetDefaultDomain().Evidence;
                }
                return new System.Security.Policy.Evidence(new AppDomainEvidenceFactory(this));
            }
        }

        public string FriendlyName
        {
            [SecuritySafeCritical]
            get
            {
                return this.nGetFriendlyName();
            }
        }

        internal AppDomainSetup FusionStore
        {
            get
            {
                return this._FusionStore;
            }
        }

        internal System.Security.HostSecurityManager HostSecurityManager
        {
            [SecurityCritical]
            get
            {
                System.Security.HostSecurityManager hostSecurityManager = null;
                AppDomainManager domainManager = CurrentDomain.DomainManager;
                if (domainManager != null)
                {
                    hostSecurityManager = domainManager.HostSecurityManager;
                }
                if (hostSecurityManager == null)
                {
                    hostSecurityManager = new System.Security.HostSecurityManager();
                }
                return hostSecurityManager;
            }
        }

        public int Id
        {
            [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this.GetId();
            }
        }

        internal System.Security.Policy.Evidence InternalEvidence
        {
            get
            {
                return this._SecurityIdentity;
            }
        }

        public bool IsFullyTrusted
        {
            [SecuritySafeCritical]
            get
            {
                System.Security.PermissionSet o = null;
                GetGrantSet(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<System.Security.PermissionSet>(ref o));
                if (o != null)
                {
                    return o.IsUnrestricted();
                }
                return true;
            }
        }

        public bool IsHomogenous
        {
            get
            {
                return (this._applicationTrust != null);
            }
        }

        internal bool IsLegacyCasPolicyEnabled
        {
            [SecuritySafeCritical]
            get
            {
                return GetIsLegacyCasPolicyEnabled(this.GetNativeHandle());
            }
        }

        private Dictionary<string, object[]> LocalStore
        {
            get
            {
                if (this._LocalStore == null)
                {
                    this._LocalStore = new Dictionary<string, object[]>();
                }
                return this._LocalStore;
            }
        }

        public static bool MonitoringIsEnabled
        {
            [SecurityCritical]
            get
            {
                return nMonitoringIsEnabled();
            }
            [SecurityCritical]
            set
            {
                if (!value)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_MustBeTrue"));
                }
                nEnableMonitoring();
            }
        }

        public long MonitoringSurvivedMemorySize
        {
            [SecurityCritical]
            get
            {
                long num = this.nGetLastSurvivedMemorySize();
                if (num == -1L)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WithoutARM"));
                }
                return num;
            }
        }

        public static long MonitoringSurvivedProcessMemorySize
        {
            [SecurityCritical]
            get
            {
                long num = nGetLastSurvivedProcessMemorySize();
                if (num == -1L)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WithoutARM"));
                }
                return num;
            }
        }

        public long MonitoringTotalAllocatedMemorySize
        {
            [SecurityCritical]
            get
            {
                long num = this.nGetTotalAllocatedMemorySize();
                if (num == -1L)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WithoutARM"));
                }
                return num;
            }
        }

        public TimeSpan MonitoringTotalProcessorTime
        {
            [SecurityCritical]
            get
            {
                long ticks = this.nGetTotalProcessorTime();
                if (ticks == -1L)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WithoutARM"));
                }
                return new TimeSpan(ticks);
            }
        }

        internal string[] PartialTrustVisibleAssemblies
        {
            get
            {
                return this._aptcaVisibleAssemblies;
            }
            [SecuritySafeCritical]
            set
            {
                this._aptcaVisibleAssemblies = value;
                string canonicalList = null;
                if (value != null)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (value[i] != null)
                        {
                            builder.Append(value[i].ToUpperInvariant());
                            if (i != (value.Length - 1))
                            {
                                builder.Append(';');
                            }
                        }
                    }
                    canonicalList = builder.ToString();
                }
                this.SetCanonicalConditionalAptcaList(canonicalList);
            }
        }

        public System.Security.PermissionSet PermissionSet
        {
            [SecurityCritical]
            get
            {
                System.Security.PermissionSet o = null;
                GetGrantSet(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<System.Security.PermissionSet>(ref o));
                if (o != null)
                {
                    return o.Copy();
                }
                return new System.Security.PermissionSet(PermissionState.Unrestricted);
            }
        }

        public string RelativeSearchPath
        {
            [SecuritySafeCritical]
            get
            {
                return this.FusionStore.PrivateBinPath;
            }
        }

        internal DomainSpecificRemotingData RemotingData
        {
            get
            {
                if (this._RemotingData == null)
                {
                    this.CreateRemotingData();
                }
                return this._RemotingData;
            }
        }

        public AppDomainSetup SetupInformation
        {
            get
            {
                return new AppDomainSetup(this.FusionStore, true);
            }
        }

        public bool ShadowCopyFiles
        {
            get
            {
                string shadowCopyFiles = this.FusionStore.ShadowCopyFiles;
                return ((shadowCopyFiles != null) && (string.Compare(shadowCopyFiles, "true", StringComparison.OrdinalIgnoreCase) == 0));
            }
        }

        private class CAPTCASearcher : IComparer
        {
            int IComparer.Compare(object lhs, object rhs)
            {
                AssemblyName name = new AssemblyName((string) lhs);
                AssemblyName name2 = (AssemblyName) rhs;
                int num = string.Compare(name.Name, name2.Name, StringComparison.OrdinalIgnoreCase);
                if (num != 0)
                {
                    return num;
                }
                byte[] publicKeyToken = name.GetPublicKeyToken();
                byte[] buffer2 = name2.GetPublicKeyToken();
                if (publicKeyToken == null)
                {
                    return -1;
                }
                if (buffer2 == null)
                {
                    return 1;
                }
                if (publicKeyToken.Length < buffer2.Length)
                {
                    return -1;
                }
                if (publicKeyToken.Length > buffer2.Length)
                {
                    return 1;
                }
                for (int i = 0; i < publicKeyToken.Length; i++)
                {
                    byte num3 = publicKeyToken[i];
                    byte num4 = buffer2[i];
                    if (num3 < num4)
                    {
                        return -1;
                    }
                    if (num3 > num4)
                    {
                        return 1;
                    }
                }
                return 0;
            }
        }

        [Serializable]
        private class EvidenceCollection
        {
            public Evidence CreatorsSecurityInfo;
            public Evidence ProvidedSecurityInfo;
        }
    }
}


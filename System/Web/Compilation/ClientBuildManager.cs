namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Util;

    [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true), PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public sealed class ClientBuildManager : MarshalByRefObject, IDisposable
    {
        private string _appId;
        private ClientBuildManagerTypeDescriptionProviderBridge _cbmTdpBridge;
        private string _codeGenDir;
        private BuildManagerHost _host;
        private Exception _hostCreationException;
        private bool _hostCreationPending;
        private HostingEnvironmentParameters _hostingParameters;
        private string _installPath;
        private object _lock;
        private WaitCallback _onAppDomainShutdown;
        private WaitCallback _onAppDomainUnloadedCallback;
        private string _physicalPath;
        private ApplicationShutdownReason _reason;
        private VirtualPath _virtualPath;
        private bool _waitForCallBack;
        private const string IISExpressPrefix = "/IISExpress/";

        public event BuildManagerHostUnloadEventHandler AppDomainShutdown;

        public event EventHandler AppDomainStarted;

        public event BuildManagerHostUnloadEventHandler AppDomainUnloaded;

        public ClientBuildManager(string appVirtualDir, string appPhysicalSourceDir) : this(appVirtualDir, appPhysicalSourceDir, null, null)
        {
        }

        public ClientBuildManager(string appVirtualDir, string appPhysicalSourceDir, string appPhysicalTargetDir) : this(appVirtualDir, appPhysicalSourceDir, appPhysicalTargetDir, null)
        {
        }

        public ClientBuildManager(string appVirtualDir, string appPhysicalSourceDir, string appPhysicalTargetDir, ClientBuildManagerParameter parameter) : this(appVirtualDir, appPhysicalSourceDir, appPhysicalTargetDir, parameter, null)
        {
        }

        public ClientBuildManager(string appVirtualDir, string appPhysicalSourceDir, string appPhysicalTargetDir, ClientBuildManagerParameter parameter, TypeDescriptionProvider typeDescriptionProvider)
        {
            this._lock = new object();
            if (parameter == null)
            {
                parameter = new ClientBuildManagerParameter();
            }
            this.InitializeCBMTDPBridge(typeDescriptionProvider);
            if (!string.IsNullOrEmpty(appPhysicalTargetDir))
            {
                parameter.PrecompilationFlags |= PrecompilationFlags.Clean;
            }
            this._hostingParameters = new HostingEnvironmentParameters();
            this._hostingParameters.HostingFlags = HostingEnvironmentFlags.ClientBuildManager | HostingEnvironmentFlags.DontCallAppInitialize;
            this._hostingParameters.ClientBuildManagerParameter = parameter;
            this._hostingParameters.PrecompilationTargetPhysicalDirectory = appPhysicalTargetDir;
            if (typeDescriptionProvider != null)
            {
                this._hostingParameters.HostingFlags |= HostingEnvironmentFlags.SupportsMultiTargeting;
            }
            if (appVirtualDir[0] != '/')
            {
                appVirtualDir = "/" + appVirtualDir;
            }
            if (((appPhysicalSourceDir == null) && appVirtualDir.StartsWith("/IISExpress/", StringComparison.OrdinalIgnoreCase)) && (appVirtualDir.Length > "/IISExpress/".Length))
            {
                int index = appVirtualDir.IndexOf('/', "/IISExpress/".Length);
                if (index > 0)
                {
                    this._hostingParameters.IISExpressVersion = appVirtualDir.Substring("/IISExpress/".Length, index - "/IISExpress/".Length);
                    appVirtualDir = appVirtualDir.Substring(index);
                }
            }
            this.Initialize(VirtualPath.CreateNonRelative(appVirtualDir), appPhysicalSourceDir);
        }

        public void CompileApplicationDependencies()
        {
            this.EnsureHostCreated();
            this._host.CompileApplicationDependencies();
        }

        public void CompileFile(string virtualPath)
        {
            this.CompileFile(virtualPath, null);
        }

        public void CompileFile(string virtualPath, ClientBuildManagerCallback callback)
        {
            if (virtualPath == null)
            {
                throw new ArgumentNullException("virtualPath");
            }
            try
            {
                this.EnsureHostCreated();
                this._host.GetCompiledTypeAndAssemblyName(VirtualPath.Create(virtualPath), callback);
            }
            finally
            {
                if (callback != null)
                {
                    RemotingServices.Disconnect(callback);
                }
            }
        }

        private void CreateHost()
        {
            this._hostCreationPending = true;
            BuildManagerHost host = null;
            try
            {
                string str;
                host = (BuildManagerHost) ApplicationManager.GetApplicationManager().CreateObjectWithDefaultAppHostAndAppId(this._physicalPath, this._virtualPath, typeof(BuildManagerHost), false, this._hostingParameters, out str);
                host.AddPendingCall();
                host.Configure(this);
                this._host = host;
                this._appId = str;
                this._hostCreationException = this._host.InitializationException;
            }
            catch (Exception exception)
            {
                this._hostCreationException = exception;
                this._host = host;
            }
            finally
            {
                this._hostCreationPending = false;
                if (host != null)
                {
                    if (this.AppDomainStarted != null)
                    {
                        this.AppDomainStarted(this, EventArgs.Empty);
                    }
                    host.RemovePendingCall();
                }
            }
        }

        public IRegisteredObject CreateObject(Type type, bool failIfExists)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.EnsureHostCreated();
            this._host.RegisterAssembly(type.Assembly.FullName, type.Assembly.Location);
            return ApplicationManager.GetApplicationManager().CreateObjectInternal(this._appId, type, this._host.ApplicationHost, failIfExists);
        }

        private void EnsureHostCreated()
        {
            if (this._host == null)
            {
                lock (this._lock)
                {
                    if (this._host == null)
                    {
                        this.CreateHost();
                    }
                }
            }
            if (this._hostCreationException != null)
            {
                throw new HttpException(this._hostCreationException.Message, this._hostCreationException);
            }
        }

        public string GenerateCode(string virtualPath, string virtualFileString, out IDictionary linePragmasTable)
        {
            if (virtualPath == null)
            {
                throw new ArgumentNullException("virtualPath");
            }
            this.EnsureHostCreated();
            return this._host.GenerateCode(VirtualPath.Create(virtualPath), virtualFileString, out linePragmasTable);
        }

        public CodeCompileUnit GenerateCodeCompileUnit(string virtualPath, out Type codeDomProviderType, out CompilerParameters compilerParameters, out IDictionary linePragmasTable)
        {
            return this.GenerateCodeCompileUnit(virtualPath, null, out codeDomProviderType, out compilerParameters, out linePragmasTable);
        }

        public CodeCompileUnit GenerateCodeCompileUnit(string virtualPath, string virtualFileString, out Type codeDomProviderType, out CompilerParameters compilerParameters, out IDictionary linePragmasTable)
        {
            if (virtualPath == null)
            {
                throw new ArgumentNullException("virtualPath");
            }
            this.EnsureHostCreated();
            return this._host.GenerateCodeCompileUnit(VirtualPath.Create(virtualPath), virtualFileString, out codeDomProviderType, out compilerParameters, out linePragmasTable);
        }

        public string[] GetAppDomainShutdownDirectories()
        {
            return FileChangesMonitor.s_dirsToMonitor;
        }

        public IDictionary GetBrowserDefinitions()
        {
            this.EnsureHostCreated();
            return this._host.GetBrowserDefinitions();
        }

        public void GetCodeDirectoryInformation(string virtualCodeDir, out Type codeDomProviderType, out CompilerParameters compilerParameters, out string generatedFilesDir)
        {
            if (virtualCodeDir == null)
            {
                throw new ArgumentNullException("virtualCodeDir");
            }
            this.EnsureHostCreated();
            this._host.GetCodeDirectoryInformation(VirtualPath.CreateTrailingSlash(virtualCodeDir), out codeDomProviderType, out compilerParameters, out generatedFilesDir);
        }

        public Type GetCompiledType(string virtualPath)
        {
            if (virtualPath == null)
            {
                throw new ArgumentNullException("virtualPath");
            }
            this.EnsureHostCreated();
            string[] compiledTypeAndAssemblyName = this._host.GetCompiledTypeAndAssemblyName(VirtualPath.Create(virtualPath), null);
            if (compiledTypeAndAssemblyName == null)
            {
                return null;
            }
            return Assembly.LoadFrom(compiledTypeAndAssemblyName[1]).GetType(compiledTypeAndAssemblyName[0]);
        }

        public void GetCompilerParameters(string virtualPath, out Type codeDomProviderType, out CompilerParameters compilerParameters)
        {
            if (virtualPath == null)
            {
                throw new ArgumentNullException("virtualPath");
            }
            this.EnsureHostCreated();
            this._host.GetCompilerParams(VirtualPath.Create(virtualPath), out codeDomProviderType, out compilerParameters);
        }

        public string GetGeneratedFileVirtualPath(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }
            this.EnsureHostCreated();
            return this._host.GetGeneratedFileVirtualPath(filePath);
        }

        public string GetGeneratedSourceFile(string virtualPath)
        {
            if (virtualPath == null)
            {
                throw new ArgumentNullException("virtualPath");
            }
            this.EnsureHostCreated();
            return this._host.GetGeneratedSourceFile(VirtualPath.CreateTrailingSlash(virtualPath));
        }

        public string[] GetTopLevelAssemblyReferences(string virtualPath)
        {
            if (virtualPath == null)
            {
                throw new ArgumentNullException("virtualPath");
            }
            this.EnsureHostCreated();
            return this._host.GetTopLevelAssemblyReferences(VirtualPath.Create(virtualPath));
        }

        public string[] GetVirtualCodeDirectories()
        {
            this.EnsureHostCreated();
            return this._host.GetVirtualCodeDirectories();
        }

        internal void Initialize(VirtualPath virtualPath, string physicalPath)
        {
            this._virtualPath = virtualPath;
            this._physicalPath = FileUtil.FixUpPhysicalDirectory(physicalPath);
            this._onAppDomainUnloadedCallback = new WaitCallback(this.OnAppDomainUnloadedCallback);
            this._onAppDomainShutdown = new WaitCallback(this.OnAppDomainShutdownCallback);
            this._installPath = RuntimeEnvironment.GetRuntimeDirectory();
        }

        private void InitializeCBMTDPBridge(TypeDescriptionProvider typeDescriptionProvider)
        {
            if (typeDescriptionProvider != null)
            {
                this._cbmTdpBridge = new ClientBuildManagerTypeDescriptionProviderBridge(typeDescriptionProvider);
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public bool IsCodeAssembly(string assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }
            this.EnsureHostCreated();
            return this._host.IsCodeAssembly(assemblyName);
        }

        internal void OnAppDomainShutdown(ApplicationShutdownReason reason)
        {
            ThreadPool.QueueUserWorkItem(this._onAppDomainShutdown, reason);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private void OnAppDomainShutdownCallback(object o)
        {
            if (this.AppDomainShutdown != null)
            {
                this.AppDomainShutdown(this, new BuildManagerHostUnloadEventArgs((ApplicationShutdownReason) o));
            }
        }

        internal void OnAppDomainUnloaded(ApplicationShutdownReason reason)
        {
            this._host = null;
            this._hostCreationException = null;
            this._reason = reason;
            this._waitForCallBack = false;
            ThreadPool.QueueUserWorkItem(this._onAppDomainUnloadedCallback);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private void OnAppDomainUnloadedCallback(object unused)
        {
            if (this.AppDomainUnloaded != null)
            {
                this.AppDomainUnloaded(this, new BuildManagerHostUnloadEventArgs(this._reason));
            }
        }

        public void PrecompileApplication()
        {
            this.PrecompileApplication(null);
        }

        public void PrecompileApplication(ClientBuildManagerCallback callback)
        {
            this.PrecompileApplication(callback, false);
        }

        public void PrecompileApplication(ClientBuildManagerCallback callback, bool forceCleanBuild)
        {
            PrecompilationFlags precompilationFlags = this._hostingParameters.ClientBuildManagerParameter.PrecompilationFlags;
            if (forceCleanBuild)
            {
                this._waitForCallBack = this._host != null;
                this.Unload();
                this._hostingParameters.ClientBuildManagerParameter.PrecompilationFlags = precompilationFlags | PrecompilationFlags.Clean;
                this.WaitForCallBack();
            }
            try
            {
                this.EnsureHostCreated();
                this._host.PrecompileApp(callback);
            }
            finally
            {
                if (forceCleanBuild)
                {
                    this._hostingParameters.ClientBuildManagerParameter.PrecompilationFlags = precompilationFlags;
                }
                if (callback != null)
                {
                    RemotingServices.Disconnect(callback);
                }
            }
        }

        void IDisposable.Dispose()
        {
            this.Unload();
        }

        public bool Unload()
        {
            BuildManagerHost host = this._host;
            if (host != null)
            {
                this._host = null;
                return host.UnloadAppDomain();
            }
            return false;
        }

        private void WaitForCallBack()
        {
            for (int i = 0; this._waitForCallBack && (i <= 50); i++)
            {
                Thread.Sleep(200);
            }
            bool flag1 = this._waitForCallBack;
        }

        internal ClientBuildManagerTypeDescriptionProviderBridge CBMTypeDescriptionProviderBridge
        {
            get
            {
                return this._cbmTdpBridge;
            }
        }

        public string CodeGenDir
        {
            get
            {
                if (this._codeGenDir == null)
                {
                    this.EnsureHostCreated();
                    this._codeGenDir = this._host.CodeGenDir;
                }
                return this._codeGenDir;
            }
        }

        public bool IsHostCreated
        {
            get
            {
                return (this._host != null);
            }
        }
    }
}


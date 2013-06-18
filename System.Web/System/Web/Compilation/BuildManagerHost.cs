namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;

    internal class BuildManagerHost : MarshalByRefObject, IRegisteredObject
    {
        private IDictionary _assemblyCollection;
        private BuildManager _buildManager;
        private ClientBuildManager _client;
        private bool _ignorePendingCalls;
        private static bool _inClientBuildManager;
        private object _lock = new object();
        private EventHandler _onAppDomainUnload;
        private int _pendingCallsCount;
        private ClientVirtualPathProvider _virtualPathProvider;

        public BuildManagerHost()
        {
            HostingEnvironment.RegisterObject(this);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(this.ResolveAssembly);
        }

        internal void AddPendingCall()
        {
            Interlocked.Increment(ref this._pendingCallsCount);
        }

        internal void CompileApplicationDependencies()
        {
            this.AddPendingCall();
            try
            {
                this._buildManager.EnsureTopLevelFilesCompiled();
            }
            finally
            {
                this.RemovePendingCall();
            }
        }

        internal void Configure(ClientBuildManager client)
        {
            this.AddPendingCall();
            try
            {
                this._virtualPathProvider = new ClientVirtualPathProvider();
                HostingEnvironment.RegisterVirtualPathProviderInternal(this._virtualPathProvider);
                this._client = client;
                if (this._client.CBMTypeDescriptionProviderBridge != null)
                {
                    TargetFrameworkUtil.CBMTypeDescriptionProviderBridge = this._client.CBMTypeDescriptionProviderBridge;
                }
                this._onAppDomainUnload = new EventHandler(this.OnAppDomainUnload);
                Thread.GetDomain().DomainUnload += this._onAppDomainUnload;
                this._buildManager = BuildManager.TheBuildManager;
                HttpRuntime.AppDomainShutdown += new BuildManagerHostUnloadEventHandler(this.OnAppDomainShutdown);
            }
            finally
            {
                this.RemovePendingCall();
            }
        }

        private void FixupReferencedAssemblies(VirtualPath virtualPath, CompilerParameters compilerParameters)
        {
            Util.AddAssembliesToStringCollection(BuildManager.GetReferencedAssemblies(MTConfigUtil.GetCompilationConfig(virtualPath)), compilerParameters.ReferencedAssemblies);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal string GenerateCode(VirtualPath virtualPath, string virtualFileString, out IDictionary linePragmasTable)
        {
            string str2;
            this.AddPendingCall();
            try
            {
                Type type;
                CompilerParameters parameters;
                string str = null;
                CodeCompileUnit compileUnit = this.GenerateCodeCompileUnit(virtualPath, virtualFileString, out type, out parameters, out linePragmasTable);
                if ((compileUnit != null) && (type != null))
                {
                    CodeDomProvider provider = CompilationUtil.CreateCodeDomProvider(type);
                    CodeGeneratorOptions options = new CodeGeneratorOptions {
                        BlankLinesBetweenMembers = false,
                        IndentString = string.Empty
                    };
                    StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
                    provider.GenerateCodeFromCompileUnit(compileUnit, writer, options);
                    str = writer.ToString();
                }
                str2 = str;
            }
            finally
            {
                this.RemovePendingCall();
            }
            return str2;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal CodeCompileUnit GenerateCodeCompileUnit(VirtualPath virtualPath, string virtualFileString, out Type codeDomProviderType, out CompilerParameters compilerParameters, out IDictionary linePragmasTable)
        {
            CodeCompileUnit codeCompileUnit;
            this.AddPendingCall();
            try
            {
                BuildManager.SkipTopLevelCompilationExceptions = true;
                this._buildManager.EnsureTopLevelFilesCompiled();
                if (virtualFileString == null)
                {
                    using (Stream stream = virtualPath.OpenFile())
                    {
                        virtualFileString = Util.ReaderFromStream(stream, virtualPath).ReadToEnd();
                    }
                }
                this._virtualPathProvider.RegisterVirtualFile(virtualPath, virtualFileString);
                string cacheKey = BuildManager.GetCacheKeyFromVirtualPath(virtualPath) + "_CBMResult";
                BuildResultCodeCompileUnit buildResultFromCache = (BuildResultCodeCompileUnit) BuildManager.GetBuildResultFromCache(cacheKey, virtualPath);
                if (buildResultFromCache == null)
                {
                    lock (this._lock)
                    {
                        DateTime utcNow = DateTime.UtcNow;
                        System.Web.Compilation.BuildProvider provider = this.GetCompilerParamsAndBuildProvider(virtualPath, out codeDomProviderType, out compilerParameters);
                        if (provider == null)
                        {
                            linePragmasTable = null;
                            return null;
                        }
                        CodeCompileUnit unit2 = provider.GetCodeCompileUnit(out linePragmasTable);
                        buildResultFromCache = new BuildResultCodeCompileUnit(codeDomProviderType, unit2, compilerParameters, linePragmasTable) {
                            VirtualPath = virtualPath
                        };
                        buildResultFromCache.SetCacheKey(cacheKey);
                        this.FixupReferencedAssemblies(virtualPath, compilerParameters);
                        if (unit2 != null)
                        {
                            foreach (string str2 in compilerParameters.ReferencedAssemblies)
                            {
                                unit2.ReferencedAssemblies.Add(str2);
                            }
                        }
                        ICollection virtualPathDependencies = provider.VirtualPathDependencies;
                        if (virtualPathDependencies != null)
                        {
                            buildResultFromCache.AddVirtualPathDependencies(virtualPathDependencies);
                        }
                        BuildManager.CacheBuildResult(cacheKey, buildResultFromCache, utcNow);
                        return unit2;
                    }
                }
                codeDomProviderType = buildResultFromCache.CodeDomProviderType;
                compilerParameters = buildResultFromCache.CompilerParameters;
                linePragmasTable = buildResultFromCache.LinePragmasTable;
                this.FixupReferencedAssemblies(virtualPath, compilerParameters);
                codeCompileUnit = buildResultFromCache.CodeCompileUnit;
            }
            finally
            {
                if (virtualFileString != null)
                {
                    this._virtualPathProvider.RevertVirtualFile(virtualPath);
                }
                BuildManager.SkipTopLevelCompilationExceptions = false;
                this.RemovePendingCall();
            }
            return codeCompileUnit;
        }

        internal IDictionary GetBrowserDefinitions()
        {
            IDictionary browserElements;
            this.AddPendingCall();
            try
            {
                browserElements = BrowserCapabilitiesCompiler.BrowserCapabilitiesFactory.InternalGetBrowserElements();
            }
            finally
            {
                this.RemovePendingCall();
            }
            return browserElements;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal void GetCodeDirectoryInformation(VirtualPath virtualCodeDir, out Type codeDomProviderType, out CompilerParameters compParams, out string generatedFilesDir)
        {
            this.AddPendingCall();
            try
            {
                BuildManager.SkipTopLevelCompilationExceptions = true;
                this._buildManager.EnsureTopLevelFilesCompiled();
                virtualCodeDir = virtualCodeDir.CombineWithAppRoot();
                this._buildManager.GetCodeDirectoryInformation(virtualCodeDir, out codeDomProviderType, out compParams, out generatedFilesDir);
            }
            finally
            {
                BuildManager.SkipTopLevelCompilationExceptions = false;
                this.RemovePendingCall();
            }
        }

        internal string[] GetCompiledTypeAndAssemblyName(VirtualPath virtualPath, ClientBuildManagerCallback callback)
        {
            string[] strArray;
            this.AddPendingCall();
            try
            {
                virtualPath.CombineWithAppRoot();
                Type compiledType = BuildManager.GetCompiledType(virtualPath, callback);
                if (compiledType == null)
                {
                    return null;
                }
                string assemblyPathFromType = Util.GetAssemblyPathFromType(compiledType);
                strArray = new string[] { compiledType.FullName, assemblyPathFromType };
            }
            finally
            {
                this.RemovePendingCall();
            }
            return strArray;
        }

        internal void GetCompilerParams(VirtualPath virtualPath, out Type codeDomProviderType, out CompilerParameters compParams)
        {
            this.AddPendingCall();
            try
            {
                BuildManager.SkipTopLevelCompilationExceptions = true;
                this._buildManager.EnsureTopLevelFilesCompiled();
                this.GetCompilerParamsAndBuildProvider(virtualPath, out codeDomProviderType, out compParams);
                if (compParams != null)
                {
                    this.FixupReferencedAssemblies(virtualPath, compParams);
                }
            }
            finally
            {
                BuildManager.SkipTopLevelCompilationExceptions = false;
                this.RemovePendingCall();
            }
        }

        private System.Web.Compilation.BuildProvider GetCompilerParamsAndBuildProvider(VirtualPath virtualPath, out Type codeDomProviderType, out CompilerParameters compilerParameters)
        {
            virtualPath.CombineWithAppRoot();
            CompilationSection compilationConfig = MTConfigUtil.GetCompilationConfig(virtualPath);
            ICollection referencedAssemblies = BuildManager.GetReferencedAssemblies(compilationConfig);
            System.Web.Compilation.BuildProvider provider = null;
            if (StringUtil.EqualsIgnoreCase(virtualPath.VirtualPathString, BuildManager.GlobalAsaxVirtualPath.VirtualPathString))
            {
                ApplicationBuildProvider provider2 = new ApplicationBuildProvider();
                provider2.SetVirtualPath(virtualPath);
                provider2.SetReferencedAssemblies(referencedAssemblies);
                provider = provider2;
            }
            else
            {
                provider = BuildManager.CreateBuildProvider(virtualPath, compilationConfig, referencedAssemblies, true);
            }
            provider.IgnoreParseErrors = true;
            provider.IgnoreControlProperties = true;
            provider.ThrowOnFirstParseError = false;
            CompilerType codeCompilerType = provider.CodeCompilerType;
            if (codeCompilerType == null)
            {
                codeDomProviderType = null;
                compilerParameters = null;
                return null;
            }
            codeDomProviderType = codeCompilerType.CodeDomProviderType;
            compilerParameters = codeCompilerType.CompilerParameters;
            IAssemblyDependencyParser assemblyDependencyParser = provider.AssemblyDependencyParser;
            if ((assemblyDependencyParser != null) && (assemblyDependencyParser.AssemblyDependencies != null))
            {
                Util.AddAssembliesToStringCollection(assemblyDependencyParser.AssemblyDependencies, compilerParameters.ReferencedAssemblies);
            }
            AssemblyBuilder.FixUpCompilerParameters(codeDomProviderType, compilerParameters);
            return provider;
        }

        internal string GetGeneratedFileVirtualPath(string filePath)
        {
            string str;
            this.AddPendingCall();
            try
            {
                Dictionary<string, string>.Enumerator enumerator = BuildManager.GenerateFileTable.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyValuePair<string, string> current = enumerator.Current;
                    if (filePath.Equals(current.Value, StringComparison.Ordinal))
                    {
                        return current.Key;
                    }
                }
                str = null;
            }
            finally
            {
                this.RemovePendingCall();
            }
            return str;
        }

        internal string GetGeneratedSourceFile(VirtualPath virtualPath)
        {
            string str2;
            this.AddPendingCall();
            try
            {
                Type type;
                CompilerParameters parameters;
                string str;
                if (!virtualPath.DirectoryExists())
                {
                    throw new ArgumentException(System.Web.SR.GetString("GetGeneratedSourceFile_Directory_Only", new object[] { virtualPath.VirtualPathString }), "virtualPath");
                }
                this.GetCodeDirectoryInformation(virtualPath, out type, out parameters, out str);
                str2 = BuildManager.GenerateFileTable[virtualPath.VirtualPathStringNoTrailingSlash];
            }
            finally
            {
                this.RemovePendingCall();
            }
            return str2;
        }

        internal string[] GetTopLevelAssemblyReferences(VirtualPath virtualPath)
        {
            this.AddPendingCall();
            List<Assembly> fromList = new List<Assembly>();
            try
            {
                virtualPath.CombineWithAppRoot();
                foreach (AssemblyInfo info in MTConfigUtil.GetCompilationConfig(virtualPath).Assemblies)
                {
                    Assembly[] assemblyInternal = info.AssemblyInternal;
                    for (int i = 0; i < assemblyInternal.Length; i++)
                    {
                        if (assemblyInternal[i] != null)
                        {
                            fromList.Add(assemblyInternal[i]);
                        }
                    }
                }
            }
            finally
            {
                this.RemovePendingCall();
            }
            StringCollection toList = new StringCollection();
            Util.AddAssembliesToStringCollection(fromList, toList);
            string[] array = new string[toList.Count];
            toList.CopyTo(array, 0);
            return array;
        }

        internal string[] GetVirtualCodeDirectories()
        {
            string[] codeDirectories;
            this.AddPendingCall();
            try
            {
                codeDirectories = this._buildManager.GetCodeDirectories();
            }
            finally
            {
                this.RemovePendingCall();
            }
            return codeDirectories;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        internal bool IsCodeAssembly(string assemblyName)
        {
            return (BuildManager.GetNormalizedCodeAssemblyName(assemblyName) != null);
        }

        private void OnAppDomainShutdown(object o, BuildManagerHostUnloadEventArgs args)
        {
            this._client.OnAppDomainShutdown(args.Reason);
        }

        private void OnAppDomainUnload(object unusedObject, EventArgs unusedEventArgs)
        {
            Thread.GetDomain().DomainUnload -= this._onAppDomainUnload;
            if (this._client != null)
            {
                this._client.OnAppDomainUnloaded(HttpRuntime.ShutdownReason);
                this._client = null;
            }
        }

        internal void PrecompileApp(ClientBuildManagerCallback callback)
        {
            this.AddPendingCall();
            try
            {
                this._buildManager.PrecompileApp(callback);
            }
            finally
            {
                this.RemovePendingCall();
            }
        }

        internal void RegisterAssembly(string assemblyName, string assemblyLocation)
        {
            if (this._assemblyCollection == null)
            {
                lock (this._lock)
                {
                    if (this._assemblyCollection == null)
                    {
                        this._assemblyCollection = Hashtable.Synchronized(new Hashtable());
                    }
                }
            }
            AssemblyName name = new AssemblyName(assemblyName);
            this._assemblyCollection[name.FullName] = assemblyLocation;
        }

        internal void RemovePendingCall()
        {
            Interlocked.Decrement(ref this._pendingCallsCount);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private Assembly ResolveAssembly(object sender, ResolveEventArgs e)
        {
            if (this._assemblyCollection == null)
            {
                return null;
            }
            string assemblyFile = (string) this._assemblyCollection[e.Name];
            if (assemblyFile == null)
            {
                return null;
            }
            return Assembly.LoadFrom(assemblyFile);
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            this.WaitForPendingCallsToFinish();
            HostingEnvironment.UnregisterObject(this);
        }

        internal bool UnloadAppDomain()
        {
            this._ignorePendingCalls = true;
            HttpRuntime.SetUserForcedShutdown();
            return HttpRuntime.ShutdownAppDomain(ApplicationShutdownReason.UnloadAppDomainCalled, "CBM called UnloadAppDomain");
        }

        private void WaitForPendingCallsToFinish()
        {
            while (this._pendingCallsCount > 0)
            {
                if (this._ignorePendingCalls)
                {
                    return;
                }
                Thread.Sleep(250);
            }
            return;
        }

        internal IApplicationHost ApplicationHost
        {
            get
            {
                return HostingEnvironment.ApplicationHostInternal;
            }
        }

        internal string CodeGenDir
        {
            get
            {
                string codegenDirInternal;
                this.AddPendingCall();
                try
                {
                    codegenDirInternal = HttpRuntime.CodegenDirInternal;
                }
                finally
                {
                    this.RemovePendingCall();
                }
                return codegenDirInternal;
            }
        }

        internal static bool InClientBuildManager
        {
            get
            {
                return _inClientBuildManager;
            }
            set
            {
                _inClientBuildManager = true;
            }
        }

        internal Exception InitializationException
        {
            get
            {
                return HostingEnvironment.InitializationException;
            }
        }

        internal static bool SupportsMultiTargeting
        {
            [CompilerGenerated]
            get
            {
                return <SupportsMultiTargeting>k__BackingField;
            }
            [CompilerGenerated]
            set
            {
                <SupportsMultiTargeting>k__BackingField = value;
            }
        }

        internal class ClientVirtualPathProvider : VirtualPathProvider
        {
            private IDictionary _stringDictionary = new HybridDictionary(true);

            internal ClientVirtualPathProvider()
            {
            }

            public override bool FileExists(string virtualPath)
            {
                return (this._stringDictionary.Contains(virtualPath) || base.FileExists(virtualPath));
            }

            public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
            {
                if (virtualPath != null)
                {
                    virtualPath = UrlPath.MakeVirtualPathAppAbsolute(virtualPath);
                    if (this._stringDictionary.Contains(virtualPath))
                    {
                        return null;
                    }
                }
                return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
            }

            public override VirtualFile GetFile(string virtualPath)
            {
                string virtualFileString = (string) this._stringDictionary[virtualPath];
                if (virtualFileString == null)
                {
                    return base.GetFile(virtualPath);
                }
                return new ClientVirtualFile(virtualPath, virtualFileString);
            }

            public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
            {
                HashCodeCombiner combiner = null;
                ArrayList list = new ArrayList();
                foreach (string str in virtualPathDependencies)
                {
                    if (this._stringDictionary.Contains(str))
                    {
                        if (combiner == null)
                        {
                            combiner = new HashCodeCombiner();
                        }
                        combiner.AddInt(this._stringDictionary[str].GetHashCode());
                    }
                    else
                    {
                        list.Add(str);
                    }
                }
                if (combiner == null)
                {
                    return base.GetFileHash(virtualPath, virtualPathDependencies);
                }
                combiner.AddObject(base.GetFileHash(virtualPath, list));
                return combiner.CombinedHashString;
            }

            internal void RegisterVirtualFile(VirtualPath virtualPath, string virtualFileString)
            {
                this._stringDictionary[virtualPath.VirtualPathString] = virtualFileString;
            }

            internal void RevertVirtualFile(VirtualPath virtualPath)
            {
                this._stringDictionary.Remove(virtualPath.VirtualPathString);
            }

            internal class ClientVirtualFile : VirtualFile
            {
                private string _virtualFileString;

                internal ClientVirtualFile(string virtualPath, string virtualFileString) : base(virtualPath)
                {
                    this._virtualFileString = virtualFileString;
                }

                public override Stream Open()
                {
                    Stream stream = new MemoryStream();
                    StreamWriter writer = new StreamWriter(stream, Encoding.Unicode);
                    writer.Write(this._virtualFileString);
                    writer.Flush();
                    stream.Seek(0L, SeekOrigin.Begin);
                    return stream;
                }
            }
        }
    }
}


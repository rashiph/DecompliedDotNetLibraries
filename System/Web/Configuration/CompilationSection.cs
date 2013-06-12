namespace System.Web.Configuration
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Web.Util;

    public sealed class CompilationSection : ConfigurationSection
    {
        private static readonly Lazy<ConcurrentDictionary<Assembly, string>> _assemblyNames = new Lazy<ConcurrentDictionary<Assembly, string>>();
        private Type _assemblyPostProcessorType;
        private Hashtable _compilerExtensions;
        private Hashtable _compilerLanguages;
        private bool _isRuntimeObject;
        private static readonly ConfigurationProperty _propAssemblies = new ConfigurationProperty("assemblies", typeof(AssemblyCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propAssemblyPreprocessorType = new ConfigurationProperty("assemblyPostProcessorType", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propBatch = new ConfigurationProperty("batch", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propBatchTimeout = new ConfigurationProperty("batchTimeout", typeof(TimeSpan), TimeSpan.FromMinutes(15.0), StdValidatorsAndConverters.TimeSpanSecondsOrInfiniteConverter, StdValidatorsAndConverters.PositiveTimeSpanValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propBuildProviders = new ConfigurationProperty("buildProviders", typeof(BuildProviderCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propCodeSubDirs = new ConfigurationProperty("codeSubDirectories", typeof(CodeSubDirectoriesCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propCompilers = new ConfigurationProperty("compilers", typeof(CompilerCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propDebug = new ConfigurationProperty("debug", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDefaultLanguage = new ConfigurationProperty("defaultLanguage", typeof(string), "vb", ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propExplicit = new ConfigurationProperty("explicit", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propExpressionBuilders = new ConfigurationProperty("expressionBuilders", typeof(ExpressionBuilderCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propFolderLevelBuildProviders = new ConfigurationProperty("folderLevelBuildProviders", typeof(FolderLevelBuildProviderCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propMaxBatchGeneratedFileSize = new ConfigurationProperty("maxBatchGeneratedFileSize", typeof(int), 0x3e8, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaxBatchSize = new ConfigurationProperty("maxBatchSize", typeof(int), 0x3e8, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propNumRecompilesBeforeAppRestart = new ConfigurationProperty("numRecompilesBeforeAppRestart", typeof(int), 15, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propOptimizeCompilations = new ConfigurationProperty("optimizeCompilations", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propStrict = new ConfigurationProperty("strict", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propTargetFramework = new ConfigurationProperty("targetFramework", typeof(string), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propTempDirectory = new ConfigurationProperty("tempDirectory", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propUrlLinePragmas = new ConfigurationProperty("urlLinePragmas", typeof(bool), false, ConfigurationPropertyOptions.None);
        private long _recompilationHash = -1L;
        private bool _referenceSet;
        private const string assemblyPostProcessorTypeAttributeName = "assemblyPostProcessorType";
        private const char fieldSeparator = ';';
        private const string tempDirectoryAttributeName = "tempDirectory";

        static CompilationSection()
        {
            _properties.Add(_propTempDirectory);
            _properties.Add(_propDebug);
            _properties.Add(_propStrict);
            _properties.Add(_propExplicit);
            _properties.Add(_propBatch);
            _properties.Add(_propOptimizeCompilations);
            _properties.Add(_propBatchTimeout);
            _properties.Add(_propMaxBatchSize);
            _properties.Add(_propMaxBatchGeneratedFileSize);
            _properties.Add(_propNumRecompilesBeforeAppRestart);
            _properties.Add(_propDefaultLanguage);
            _properties.Add(_propTargetFramework);
            _properties.Add(_propCompilers);
            _properties.Add(_propAssemblies);
            _properties.Add(_propBuildProviders);
            _properties.Add(_propFolderLevelBuildProviders);
            _properties.Add(_propExpressionBuilders);
            _properties.Add(_propUrlLinePragmas);
            _properties.Add(_propCodeSubDirs);
            _properties.Add(_propAssemblyPreprocessorType);
        }

        private void EnsureCompilerCacheInit()
        {
            if (this._compilerLanguages == null)
            {
                lock (this)
                {
                    if (this._compilerLanguages == null)
                    {
                        Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
                        this._compilerExtensions = new Hashtable(StringComparer.OrdinalIgnoreCase);
                        foreach (Compiler compiler in this.Compilers)
                        {
                            string[] strArray = compiler.Language.Split(new char[] { ';' });
                            string[] strArray2 = compiler.Extension.Split(new char[] { ';' });
                            foreach (string str in strArray)
                            {
                                hashtable[str] = compiler;
                            }
                            foreach (string str2 in strArray2)
                            {
                                this._compilerExtensions[str2] = compiler;
                            }
                        }
                        this._compilerLanguages = hashtable;
                    }
                }
            }
        }

        private void EnsureReferenceSet()
        {
            if (!this._referenceSet)
            {
                foreach (AssemblyInfo info in this.GetAssembliesCollection())
                {
                    info.SetCompilationReference(this);
                }
                this._referenceSet = true;
            }
        }

        private AssemblyCollection GetAssembliesCollection()
        {
            return (AssemblyCollection) base[_propAssemblies];
        }

        internal CompilerType GetCompilerInfoFromExtension(string extension, bool throwOnFail)
        {
            CompilerType compilerTypeInternal;
            this.EnsureCompilerCacheInit();
            object obj2 = this._compilerExtensions[extension];
            Compiler compiler = obj2 as Compiler;
            if (compiler != null)
            {
                compilerTypeInternal = compiler.CompilerTypeInternal;
                this._compilerExtensions[extension] = compilerTypeInternal;
            }
            else
            {
                compilerTypeInternal = obj2 as CompilerType;
            }
            if ((compilerTypeInternal == null) && CodeDomProvider.IsDefinedExtension(extension))
            {
                CompilerInfo compilerInfo = CodeDomProvider.GetCompilerInfo(CodeDomProvider.GetLanguageFromExtension(extension));
                compilerTypeInternal = new CompilerType(compilerInfo.CodeDomProviderType, compilerInfo.CreateDefaultCompilerParameters());
                this._compilerExtensions[extension] = compilerTypeInternal;
            }
            if (compilerTypeInternal == null)
            {
                if (throwOnFail)
                {
                    throw new HttpException(System.Web.SR.GetString("Invalid_lang_extension", new object[] { extension }));
                }
                return null;
            }
            compilerTypeInternal = compilerTypeInternal.Clone();
            compilerTypeInternal.CompilerParameters.IncludeDebugInformation = this.Debug;
            return compilerTypeInternal;
        }

        internal CompilerType GetCompilerInfoFromLanguage(string language)
        {
            CompilerType compilerTypeInternal;
            this.EnsureCompilerCacheInit();
            object obj2 = this._compilerLanguages[language];
            Compiler compiler = obj2 as Compiler;
            if (compiler != null)
            {
                compilerTypeInternal = compiler.CompilerTypeInternal;
                this._compilerLanguages[language] = compilerTypeInternal;
            }
            else
            {
                compilerTypeInternal = obj2 as CompilerType;
            }
            if ((compilerTypeInternal == null) && CodeDomProvider.IsDefinedLanguage(language))
            {
                CompilerInfo compilerInfo = CodeDomProvider.GetCompilerInfo(language);
                compilerTypeInternal = new CompilerType(compilerInfo.CodeDomProviderType, compilerInfo.CreateDefaultCompilerParameters());
                this._compilerLanguages[language] = compilerTypeInternal;
            }
            if (compilerTypeInternal == null)
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_lang", new object[] { language }));
            }
            CompilationUtil.CheckCompilerOptionsAllowed(compilerTypeInternal.CompilerParameters.CompilerOptions, true, null, 0);
            compilerTypeInternal = compilerTypeInternal.Clone();
            compilerTypeInternal.CompilerParameters.IncludeDebugInformation = this.Debug;
            return compilerTypeInternal;
        }

        private FolderLevelBuildProviderCollection GetFolderLevelBuildProviders()
        {
            return (FolderLevelBuildProviderCollection) base[_propFolderLevelBuildProviders];
        }

        internal static string GetOriginalAssemblyName(Assembly a)
        {
            string fullName = null;
            if (!_assemblyNames.Value.TryGetValue(a, out fullName))
            {
                fullName = a.FullName;
            }
            return fullName;
        }

        protected override object GetRuntimeObject()
        {
            this._isRuntimeObject = true;
            return base.GetRuntimeObject();
        }

        internal void GetTempDirectoryErrorInfo(out string tempDirAttribName, out string configFileName, out int configLineNumber)
        {
            tempDirAttribName = "tempDirectory";
            configFileName = base.ElementInformation.Properties["tempDirectory"].Source;
            configLineNumber = base.ElementInformation.Properties["tempDirectory"].LineNumber;
        }

        internal Assembly[] LoadAllAssembliesFromAppDomainBinDirectory()
        {
            string binDirectoryInternal = HttpRuntime.BinDirectoryInternal;
            Assembly assembly = null;
            Assembly[] assemblyArray = null;
            if (System.Web.Util.FileUtil.DirectoryExists(binDirectoryInternal))
            {
                FileInfo[] files = new DirectoryInfo(binDirectoryInternal).GetFiles("*.dll");
                if (files.Length > 0)
                {
                    ArrayList list = new ArrayList(files.Length);
                    for (int i = 0; i < files.Length; i++)
                    {
                        string assemblyNameFromFileName = Util.GetAssemblyNameFromFileName(files[i].Name);
                        if (!assemblyNameFromFileName.StartsWith("App_Web_", StringComparison.Ordinal))
                        {
                            if (!this.GetAssembliesCollection().IsRemoved(assemblyNameFromFileName))
                            {
                                assembly = this.LoadAssemblyHelper(assemblyNameFromFileName, true);
                            }
                            if (assembly != null)
                            {
                                list.Add(assembly);
                            }
                        }
                    }
                    assemblyArray = (Assembly[]) list.ToArray(typeof(Assembly));
                }
            }
            if (assemblyArray == null)
            {
                assemblyArray = new Assembly[0];
            }
            return assemblyArray;
        }

        internal static Assembly LoadAndRecordAssembly(AssemblyName name)
        {
            Assembly a = Assembly.Load(name);
            RecordAssembly(name.FullName, a);
            return a;
        }

        internal Assembly[] LoadAssembly(AssemblyInfo ai)
        {
            Assembly[] assemblyArray = null;
            if (ai.Assembly == "*")
            {
                return this.LoadAllAssembliesFromAppDomainBinDirectory();
            }
            Assembly a = this.LoadAssemblyHelper(ai.Assembly, false);
            if (a != null)
            {
                assemblyArray = new Assembly[] { a };
                RecordAssembly(ai.Assembly, a);
            }
            return assemblyArray;
        }

        internal Assembly LoadAssembly(string assemblyName, bool throwOnFail)
        {
            try
            {
                Assembly a = Assembly.Load(assemblyName);
                RecordAssembly(assemblyName, a);
                return a;
            }
            catch
            {
                AssemblyName name = new AssemblyName(assemblyName);
                byte[] publicKeyToken = name.GetPublicKeyToken();
                if (((publicKeyToken == null) || (publicKeyToken.Length == 0)) && (name.Version == null))
                {
                    this.EnsureReferenceSet();
                    foreach (AssemblyInfo info in this.GetAssembliesCollection())
                    {
                        Assembly[] assemblyInternal = info.AssemblyInternal;
                        if (assemblyInternal != null)
                        {
                            for (int i = 0; i < assemblyInternal.Length; i++)
                            {
                                if (System.Web.Util.StringUtil.EqualsIgnoreCase(name.Name, new AssemblyName(assemblyInternal[i].FullName).Name))
                                {
                                    return assemblyInternal[i];
                                }
                            }
                        }
                    }
                }
                if (throwOnFail)
                {
                    throw;
                }
            }
            return null;
        }

        private Assembly LoadAssemblyHelper(string assemblyName, bool starDirective)
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch (Exception exception)
            {
                bool flag = false;
                if (starDirective && (Marshal.GetHRForException(exception) == -2146234344))
                {
                    flag = true;
                }
                if (flag)
                {
                    return assembly;
                }
                string message = exception.Message;
                if (string.IsNullOrEmpty(message))
                {
                    if (exception is FileLoadException)
                    {
                        message = System.Web.SR.GetString("Config_base_file_load_exception_no_message", new object[] { "assembly" });
                    }
                    else if (exception is BadImageFormatException)
                    {
                        message = System.Web.SR.GetString("Config_base_bad_image_exception_no_message", new object[] { assemblyName });
                    }
                    else
                    {
                        message = System.Web.SR.GetString("Config_base_report_exception_type", new object[] { exception.GetType().ToString() });
                    }
                }
                string source = base.ElementInformation.Properties["assemblies"].Source;
                int lineNumber = base.ElementInformation.Properties["assemblies"].LineNumber;
                if (starDirective)
                {
                    assemblyName = "*";
                }
                if (this.Assemblies[assemblyName] != null)
                {
                    source = this.Assemblies[assemblyName].ElementInformation.Source;
                    lineNumber = this.Assemblies[assemblyName].ElementInformation.LineNumber;
                }
                throw new ConfigurationErrorsException(message, exception, source, lineNumber);
            }
            return assembly;
        }

        protected override void PostDeserialize()
        {
            WebContext hostingContext = base.EvaluationContext.HostingContext as WebContext;
            if ((hostingContext != null) && (hostingContext.ApplicationLevel == WebApplicationLevel.BelowApplication))
            {
                if (this.CodeSubDirectories.ElementInformation.IsPresent)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_element_below_app_illegal", new object[] { _propCodeSubDirs.Name }), this.CodeSubDirectories.ElementInformation.Source, this.CodeSubDirectories.ElementInformation.LineNumber);
                }
                if (this.BuildProviders.ElementInformation.IsPresent)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_element_below_app_illegal", new object[] { _propBuildProviders.Name }), this.BuildProviders.ElementInformation.Source, this.BuildProviders.ElementInformation.LineNumber);
                }
                if (this.FolderLevelBuildProviders.ElementInformation.IsPresent)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_element_below_app_illegal", new object[] { _propFolderLevelBuildProviders.Name }), this.FolderLevelBuildProviders.ElementInformation.Source, this.FolderLevelBuildProviders.ElementInformation.LineNumber);
                }
            }
        }

        internal static void RecordAssembly(string assemblyName, Assembly a)
        {
            if (!_assemblyNames.Value.ContainsKey(a))
            {
                _assemblyNames.Value.TryAdd(a, assemblyName);
            }
        }

        [ConfigurationProperty("assemblies")]
        public AssemblyCollection Assemblies
        {
            get
            {
                if (this._isRuntimeObject || BuildManagerHost.InClientBuildManager)
                {
                    this.EnsureReferenceSet();
                }
                return this.GetAssembliesCollection();
            }
        }

        [ConfigurationProperty("assemblyPostProcessorType", DefaultValue="")]
        public string AssemblyPostProcessorType
        {
            get
            {
                return (string) base[_propAssemblyPreprocessorType];
            }
            set
            {
                base[_propAssemblyPreprocessorType] = value;
            }
        }

        internal Type AssemblyPostProcessorTypeInternal
        {
            get
            {
                if ((this._assemblyPostProcessorType == null) && !string.IsNullOrEmpty(this.AssemblyPostProcessorType))
                {
                    lock (this)
                    {
                        if (this._assemblyPostProcessorType == null)
                        {
                            if (!HttpRuntime.HasUnmanagedPermission())
                            {
                                throw new ConfigurationErrorsException(System.Web.SR.GetString("Insufficient_trust_for_attribute", new object[] { "assemblyPostProcessorType" }), base.ElementInformation.Properties["assemblyPostProcessorType"].Source, base.ElementInformation.Properties["assemblyPostProcessorType"].LineNumber);
                            }
                            Type userBaseType = ConfigUtil.GetType(this.AssemblyPostProcessorType, "assemblyPostProcessorType", this);
                            ConfigUtil.CheckBaseType(typeof(IAssemblyPostProcessor), userBaseType, "assemblyPostProcessorType", this);
                            this._assemblyPostProcessorType = userBaseType;
                        }
                    }
                }
                return this._assemblyPostProcessorType;
            }
        }

        [ConfigurationProperty("batch", DefaultValue=true)]
        public bool Batch
        {
            get
            {
                return (bool) base[_propBatch];
            }
            set
            {
                base[_propBatch] = value;
            }
        }

        [ConfigurationProperty("batchTimeout", DefaultValue="00:15:00"), TimeSpanValidator(MinValueString="00:00:00", MaxValueString="10675199.02:48:05.4775807"), TypeConverter(typeof(TimeSpanSecondsOrInfiniteConverter))]
        public TimeSpan BatchTimeout
        {
            get
            {
                return (TimeSpan) base[_propBatchTimeout];
            }
            set
            {
                base[_propBatchTimeout] = value;
            }
        }

        [ConfigurationProperty("buildProviders")]
        public BuildProviderCollection BuildProviders
        {
            get
            {
                return (BuildProviderCollection) base[_propBuildProviders];
            }
        }

        [ConfigurationProperty("codeSubDirectories")]
        public CodeSubDirectoriesCollection CodeSubDirectories
        {
            get
            {
                return (CodeSubDirectoriesCollection) base[_propCodeSubDirs];
            }
        }

        [ConfigurationProperty("compilers")]
        public CompilerCollection Compilers
        {
            get
            {
                return (CompilerCollection) base[_propCompilers];
            }
        }

        [ConfigurationProperty("debug", DefaultValue=false)]
        public bool Debug
        {
            get
            {
                return (bool) base[_propDebug];
            }
            set
            {
                base[_propDebug] = value;
            }
        }

        [ConfigurationProperty("defaultLanguage", DefaultValue="vb")]
        public string DefaultLanguage
        {
            get
            {
                return (string) base[_propDefaultLanguage];
            }
            set
            {
                base[_propDefaultLanguage] = value;
            }
        }

        [ConfigurationProperty("explicit", DefaultValue=true)]
        public bool Explicit
        {
            get
            {
                return (bool) base[_propExplicit];
            }
            set
            {
                base[_propExplicit] = value;
            }
        }

        [ConfigurationProperty("expressionBuilders")]
        public ExpressionBuilderCollection ExpressionBuilders
        {
            get
            {
                return (ExpressionBuilderCollection) base[_propExpressionBuilders];
            }
        }

        [ConfigurationProperty("folderLevelBuildProviders")]
        public FolderLevelBuildProviderCollection FolderLevelBuildProviders
        {
            get
            {
                return this.GetFolderLevelBuildProviders();
            }
        }

        [ConfigurationProperty("maxBatchGeneratedFileSize", DefaultValue=0x3e8)]
        public int MaxBatchGeneratedFileSize
        {
            get
            {
                return (int) base[_propMaxBatchGeneratedFileSize];
            }
            set
            {
                base[_propMaxBatchGeneratedFileSize] = value;
            }
        }

        [ConfigurationProperty("maxBatchSize", DefaultValue=0x3e8)]
        public int MaxBatchSize
        {
            get
            {
                return (int) base[_propMaxBatchSize];
            }
            set
            {
                base[_propMaxBatchSize] = value;
            }
        }

        [ConfigurationProperty("numRecompilesBeforeAppRestart", DefaultValue=15)]
        public int NumRecompilesBeforeAppRestart
        {
            get
            {
                return (int) base[_propNumRecompilesBeforeAppRestart];
            }
            set
            {
                base[_propNumRecompilesBeforeAppRestart] = value;
            }
        }

        [ConfigurationProperty("optimizeCompilations", DefaultValue=false)]
        public bool OptimizeCompilations
        {
            get
            {
                return (bool) base[_propOptimizeCompilations];
            }
            set
            {
                base[_propOptimizeCompilations] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        internal long RecompilationHash
        {
            get
            {
                if (this._recompilationHash == -1L)
                {
                    lock (this)
                    {
                        if (this._recompilationHash == -1L)
                        {
                            this._recompilationHash = CompilationUtil.GetRecompilationHash(this);
                        }
                    }
                }
                return this._recompilationHash;
            }
        }

        [ConfigurationProperty("strict", DefaultValue=false)]
        public bool Strict
        {
            get
            {
                return (bool) base[_propStrict];
            }
            set
            {
                base[_propStrict] = value;
            }
        }

        [ConfigurationProperty("targetFramework", DefaultValue=null)]
        public string TargetFramework
        {
            get
            {
                return (string) base[_propTargetFramework];
            }
            set
            {
                base[_propTargetFramework] = value;
            }
        }

        [ConfigurationProperty("tempDirectory", DefaultValue="")]
        public string TempDirectory
        {
            get
            {
                return (string) base[_propTempDirectory];
            }
            set
            {
                base[_propTempDirectory] = value;
            }
        }

        [ConfigurationProperty("urlLinePragmas", DefaultValue=false)]
        public bool UrlLinePragmas
        {
            get
            {
                return (bool) base[_propUrlLinePragmas];
            }
            set
            {
                base[_propUrlLinePragmas] = value;
            }
        }
    }
}


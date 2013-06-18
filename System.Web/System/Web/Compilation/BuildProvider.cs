namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;

    public abstract class BuildProvider
    {
        private BuildProviderSet _buildProviderDependencies;
        private ICollection _referencedAssemblies;
        private System.Web.VirtualPath _virtualPath;
        internal const int contributedCode = 0x20;
        internal const int dontThrowOnFirstParseError = 0x10;
        internal SimpleBitVector32 flags;
        internal const int ignoreControlProperties = 8;
        internal const int ignoreParseErrors = 4;
        internal const int isDependedOn = 1;
        internal const int noBuildResult = 2;
        private static Dictionary<string, BuildProviderInfo> s_dynamicallyRegisteredProviders = new Dictionary<string, BuildProviderInfo>();

        protected BuildProvider()
        {
        }

        internal void AddBuildProviderDependency(System.Web.Compilation.BuildProvider dependentBuildProvider)
        {
            if (this._buildProviderDependencies == null)
            {
                this._buildProviderDependencies = new BuildProviderSet();
            }
            this._buildProviderDependencies.Add(dependentBuildProvider);
            dependentBuildProvider.flags[1] = true;
        }

        internal virtual BuildResult CreateBuildResult(CompilerResults results)
        {
            BuildResult result;
            if (this.flags[2])
            {
                return null;
            }
            if (!BuildManagerHost.InClientBuildManager && (results != null))
            {
                Assembly compiledAssembly = results.CompiledAssembly;
            }
            Type generatedType = this.GetGeneratedType(results);
            if (generatedType != null)
            {
                BuildResultCompiledType type2 = this.CreateBuildResult(generatedType);
                if (!type2.IsDelayLoadType && ((results == null) || (generatedType.Assembly != results.CompiledAssembly)))
                {
                    type2.UsesExistingAssembly = true;
                }
                result = type2;
            }
            else
            {
                string customString = this.GetCustomString(results);
                if (customString != null)
                {
                    result = new BuildResultCustomString(this.flags[0x20] ? results.CompiledAssembly : null, customString);
                }
                else
                {
                    if (results == null)
                    {
                        return null;
                    }
                    result = new BuildResultCompiledAssembly(results.CompiledAssembly);
                }
            }
            int resultFlags = (int) this.GetResultFlags(results);
            if (resultFlags != 0)
            {
                resultFlags &= 0xffff;
                result.Flags |= resultFlags;
            }
            return result;
        }

        internal virtual BuildResultCompiledType CreateBuildResult(Type t)
        {
            return new BuildResultCompiledType(t);
        }

        public virtual void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
        }

        internal static BuildProviderInfo GetBuildProviderInfo(CompilationSection config, string extension)
        {
            System.Web.Configuration.BuildProvider provider = config.BuildProviders[extension];
            if (provider != null)
            {
                return provider.BuildProviderInfo;
            }
            BuildProviderInfo info = null;
            s_dynamicallyRegisteredProviders.TryGetValue(extension, out info);
            return info;
        }

        internal BuildResult GetBuildResult(CompilerResults results)
        {
            BuildResult result = this.CreateBuildResult(results);
            if (result == null)
            {
                return null;
            }
            result.VirtualPath = this.VirtualPathObject;
            this.SetBuildResultDependencies(result);
            return result;
        }

        internal virtual ICollection GetBuildResultVirtualPathDependencies()
        {
            return null;
        }

        protected internal virtual CodeCompileUnit GetCodeCompileUnit(out IDictionary linePragmasTable)
        {
            CodeSnippetCompileUnit unit = new CodeSnippetCompileUnit(Util.StringFromVirtualPath(this.VirtualPathObject));
            LinePragmaCodeInfo info = new LinePragmaCodeInfo(1, 1, 1, -1, false);
            linePragmasTable = new Hashtable();
            linePragmasTable[1] = info;
            return unit;
        }

        internal static CompilerType GetCompilerTypeFromBuildProvider(System.Web.Compilation.BuildProvider buildProvider)
        {
            HttpContext context = null;
            CompilerType type2;
            if (EtwTrace.IsTraceEnabled(5, 1) && ((context = HttpContext.Current) != null))
            {
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_PARSE_ENTER, context.WorkerRequest);
            }
            try
            {
                CompilerType codeCompilerType = buildProvider.CodeCompilerType;
                if (codeCompilerType != null)
                {
                    CompilationUtil.CheckCompilerOptionsAllowed(codeCompilerType.CompilerParameters.CompilerOptions, false, null, 0);
                }
                type2 = codeCompilerType;
            }
            finally
            {
                if (EtwTrace.IsTraceEnabled(5, 1) && (context != null))
                {
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_PARSE_LEAVE, context.WorkerRequest);
                }
            }
            return type2;
        }

        internal virtual ICollection GetCompileWithDependencies()
        {
            return null;
        }

        internal string GetCultureName()
        {
            return Util.GetCultureName(this.VirtualPath);
        }

        public virtual string GetCustomString(CompilerResults results)
        {
            return null;
        }

        protected CompilerType GetDefaultCompilerType()
        {
            return CompilationUtil.GetDefaultLanguageCompilerInfo(null, this.VirtualPathObject);
        }

        protected CompilerType GetDefaultCompilerTypeForLanguage(string language)
        {
            return CompilationUtil.GetCompilerInfoFromLanguage(this.VirtualPathObject, language);
        }

        internal static string GetDisplayName(System.Web.Compilation.BuildProvider buildProvider)
        {
            if (buildProvider.VirtualPath != null)
            {
                return buildProvider.VirtualPath;
            }
            return buildProvider.GetType().Name;
        }

        public virtual Type GetGeneratedType(CompilerResults results)
        {
            return null;
        }

        internal virtual ICollection GetGeneratedTypeNames()
        {
            return null;
        }

        public virtual BuildProviderResultFlags GetResultFlags(CompilerResults results)
        {
            return BuildProviderResultFlags.Default;
        }

        protected TextReader OpenReader()
        {
            return this.OpenReader(this.VirtualPathObject);
        }

        protected TextReader OpenReader(string virtualPath)
        {
            return this.OpenReader(System.Web.VirtualPath.Create(virtualPath));
        }

        internal TextReader OpenReader(System.Web.VirtualPath virtualPath)
        {
            return Util.ReaderFromStream(this.OpenStream(virtualPath), virtualPath);
        }

        protected Stream OpenStream()
        {
            return this.OpenStream(this.VirtualPath);
        }

        protected Stream OpenStream(string virtualPath)
        {
            return VirtualPathProvider.OpenFile(virtualPath);
        }

        internal Stream OpenStream(System.Web.VirtualPath virtualPath)
        {
            return virtualPath.OpenFile();
        }

        public virtual void ProcessCompileErrors(CompilerResults results)
        {
        }

        public static void RegisterBuildProvider(string extension, Type providerType)
        {
            if (string.IsNullOrEmpty(extension))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("extension");
            }
            if (providerType == null)
            {
                throw new ArgumentNullException("providerType");
            }
            if (!typeof(System.Web.Compilation.BuildProvider).IsAssignableFrom(providerType))
            {
                throw ExceptionUtil.ParameterInvalid("providerType");
            }
            BuildManager.ThrowIfPreAppStartNotRunning();
            s_dynamicallyRegisteredProviders[extension] = new CompilationBuildProviderInfo(providerType);
        }

        internal void SetBuildResultDependencies(BuildResult result)
        {
            result.AddVirtualPathDependencies(this.VirtualPathDependencies);
        }

        internal void SetContributedCode()
        {
            this.flags[0x20] = true;
        }

        internal void SetNoBuildResult()
        {
            this.flags[2] = true;
        }

        internal void SetReferencedAssemblies(ICollection referencedAssemblies)
        {
            this._referencedAssemblies = referencedAssemblies;
        }

        internal void SetVirtualPath(System.Web.VirtualPath virtualPath)
        {
            this._virtualPath = virtualPath;
        }

        internal virtual IAssemblyDependencyParser AssemblyDependencyParser
        {
            get
            {
                return null;
            }
        }

        internal BuildProviderSet BuildProviderDependencies
        {
            get
            {
                return this._buildProviderDependencies;
            }
        }

        public virtual CompilerType CodeCompilerType
        {
            get
            {
                return null;
            }
        }

        internal bool IgnoreControlProperties
        {
            get
            {
                return this.flags[8];
            }
            set
            {
                this.flags[8] = value;
            }
        }

        internal virtual bool IgnoreParseErrors
        {
            get
            {
                return this.flags[4];
            }
            set
            {
                this.flags[4] = value;
            }
        }

        internal bool IsDependedOn
        {
            get
            {
                return this.flags[1];
            }
        }

        protected ICollection ReferencedAssemblies
        {
            get
            {
                return this._referencedAssemblies;
            }
        }

        internal bool ThrowOnFirstParseError
        {
            get
            {
                return !this.flags[0x10];
            }
            set
            {
                this.flags[0x10] = !value;
            }
        }

        protected internal string VirtualPath
        {
            get
            {
                return System.Web.VirtualPath.GetVirtualPathString(this._virtualPath);
            }
        }

        public virtual ICollection VirtualPathDependencies
        {
            get
            {
                return new SingleObjectCollection(this.VirtualPath);
            }
        }

        internal System.Web.VirtualPath VirtualPathObject
        {
            get
            {
                return this._virtualPath;
            }
        }

        private class CompilationBuildProviderInfo : BuildProviderInfo
        {
            private readonly System.Type _type;

            public CompilationBuildProviderInfo(System.Type type)
            {
                this._type = type;
            }

            internal override System.Type Type
            {
                get
                {
                    return this._type;
                }
            }
        }
    }
}


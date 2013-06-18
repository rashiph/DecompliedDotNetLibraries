namespace System.Web.Compilation
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Configuration;

    internal class BuildProvidersCompiler
    {
        private AssemblyBuilder _assemblyBuilder;
        private ICollection _buildProviders;
        private CompilationSection _compConfig;
        private VirtualPath _configPath;
        private string _generatedFilesDir;
        private string _outputAssemblyName;
        private ICollection _referencedAssemblies;
        private IDictionary _satelliteAssemblyBuilders;
        private bool _supportLocalization;

        internal BuildProvidersCompiler(VirtualPath configPath, string outputAssemblyName) : this(configPath, false, outputAssemblyName)
        {
        }

        internal BuildProvidersCompiler(VirtualPath configPath, bool supportLocalization, string outputAssemblyName)
        {
            this._configPath = configPath;
            this._supportLocalization = supportLocalization;
            this._compConfig = MTConfigUtil.GetCompilationConfig(this._configPath);
            this._referencedAssemblies = BuildManager.GetReferencedAssemblies(this.CompConfig);
            this._outputAssemblyName = outputAssemblyName;
        }

        internal BuildProvidersCompiler(VirtualPath configPath, bool supportLocalization, string generatedFilesDir, int index)
        {
            this._configPath = configPath;
            this._supportLocalization = supportLocalization;
            this._compConfig = MTConfigUtil.GetCompilationConfig(this._configPath);
            this._referencedAssemblies = BuildManager.GetReferencedAssemblies(this.CompConfig, index);
            this._generatedFilesDir = generatedFilesDir;
        }

        internal void GenerateSources(out Type codeDomProviderType, out CompilerParameters compilerParameters)
        {
            this.ProcessBuildProviders();
            if (this._assemblyBuilder == null)
            {
                this._assemblyBuilder = CompilerType.GetDefaultAssemblyBuilder(this.CompConfig, this._referencedAssemblies, this._configPath, this._generatedFilesDir, null);
            }
            codeDomProviderType = this._assemblyBuilder.CodeDomProviderType;
            compilerParameters = this._assemblyBuilder.GetCompilerParameters();
        }

        internal CompilerResults PerformBuild()
        {
            this.ProcessBuildProviders();
            if (this._satelliteAssemblyBuilders != null)
            {
                foreach (AssemblyBuilder builder in this._satelliteAssemblyBuilders.Values)
                {
                    builder.Compile();
                }
            }
            if (this._assemblyBuilder != null)
            {
                return this._assemblyBuilder.Compile();
            }
            return null;
        }

        private void ProcessBuildProviders()
        {
            CompilerType type = null;
            System.Web.Compilation.BuildProvider buildProvider = null;
            if (this.OutputAssemblyName != null)
            {
                StandardDiskBuildResultCache.RemoveSatelliteAssemblies(this.OutputAssemblyName);
            }
            ArrayList list = null;
            foreach (System.Web.Compilation.BuildProvider provider2 in this._buildProviders)
            {
                provider2.SetReferencedAssemblies(this._referencedAssemblies);
                if (!BuildManager.ThrowOnFirstParseError)
                {
                    InternalBuildProvider provider3 = provider2 as InternalBuildProvider;
                    if (provider3 != null)
                    {
                        provider3.ThrowOnFirstParseError = false;
                    }
                }
                CompilerType compilerTypeFromBuildProvider = System.Web.Compilation.BuildProvider.GetCompilerTypeFromBuildProvider(provider2);
                string cultureName = null;
                if (this._supportLocalization)
                {
                    cultureName = provider2.GetCultureName();
                }
                if (compilerTypeFromBuildProvider != null)
                {
                    if (cultureName != null)
                    {
                        throw new HttpException(System.Web.SR.GetString("Both_culture_and_language", new object[] { System.Web.Compilation.BuildProvider.GetDisplayName(provider2) }));
                    }
                    if (type != null)
                    {
                        if (!compilerTypeFromBuildProvider.Equals(type))
                        {
                            throw new HttpException(System.Web.SR.GetString("Inconsistent_language", new object[] { System.Web.Compilation.BuildProvider.GetDisplayName(provider2), System.Web.Compilation.BuildProvider.GetDisplayName(buildProvider) }));
                        }
                    }
                    else
                    {
                        buildProvider = provider2;
                        this._assemblyBuilder = compilerTypeFromBuildProvider.CreateAssemblyBuilder(this.CompConfig, this._referencedAssemblies, this._generatedFilesDir, this.OutputAssemblyName);
                    }
                }
                else
                {
                    if (cultureName != null)
                    {
                        if (!this.CbmGenerateOnlyMode)
                        {
                            if (this._satelliteAssemblyBuilders == null)
                            {
                                this._satelliteAssemblyBuilders = new Hashtable(StringComparer.OrdinalIgnoreCase);
                            }
                            AssemblyBuilder builder = (AssemblyBuilder) this._satelliteAssemblyBuilders[cultureName];
                            if (builder == null)
                            {
                                builder = CompilerType.GetDefaultAssemblyBuilder(this.CompConfig, this._referencedAssemblies, this._configPath, this.OutputAssemblyName);
                                builder.CultureName = cultureName;
                                this._satelliteAssemblyBuilders[cultureName] = builder;
                            }
                            builder.AddBuildProvider(provider2);
                        }
                        continue;
                    }
                    if (this._assemblyBuilder == null)
                    {
                        if (list == null)
                        {
                            list = new ArrayList();
                        }
                        list.Add(provider2);
                        continue;
                    }
                }
                this._assemblyBuilder.AddBuildProvider(provider2);
            }
            if ((this._assemblyBuilder == null) && (list != null))
            {
                this._assemblyBuilder = CompilerType.GetDefaultAssemblyBuilder(this.CompConfig, this._referencedAssemblies, this._configPath, this._generatedFilesDir, this.OutputAssemblyName);
            }
            if ((this._assemblyBuilder != null) && (list != null))
            {
                foreach (System.Web.Compilation.BuildProvider provider4 in list)
                {
                    this._assemblyBuilder.AddBuildProvider(provider4);
                }
            }
        }

        internal void SetBuildProviders(ICollection buildProviders)
        {
            this._buildProviders = buildProviders;
        }

        private bool CbmGenerateOnlyMode
        {
            get
            {
                return (this._generatedFilesDir != null);
            }
        }

        internal CompilationSection CompConfig
        {
            get
            {
                return this._compConfig;
            }
        }

        internal string OutputAssemblyName
        {
            get
            {
                return this._outputAssemblyName;
            }
        }

        internal ICollection ReferencedAssemblies
        {
            get
            {
                return this._referencedAssemblies;
            }
        }
    }
}


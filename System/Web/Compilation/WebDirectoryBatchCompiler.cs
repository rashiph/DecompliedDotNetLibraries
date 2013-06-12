namespace System.Web.Compilation
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Hosting;

    internal class WebDirectoryBatchCompiler
    {
        private IDictionary _buildProviders = new Hashtable(StringComparer.OrdinalIgnoreCase);
        private CompilationSection _compConfig;
        private HttpParseException _firstException;
        private bool _ignoreProvidersWithErrors;
        private ArrayList[] _nonDependentBuckets;
        private ParserErrorCollection _parserErrors;
        private ICollection _referencedAssemblies;
        private DateTime _utcStart;
        private VirtualDirectory _vdir;

        internal WebDirectoryBatchCompiler(VirtualDirectory vdir)
        {
            this._vdir = vdir;
            this._utcStart = DateTime.UtcNow;
            this._compConfig = MTConfigUtil.GetCompilationConfig(this._vdir.VirtualPath);
            this._referencedAssemblies = BuildManager.GetReferencedAssemblies(this._compConfig);
        }

        private void AddBuildProviders(bool retryIfDeletionHappens)
        {
            DiskBuildResultCache.ResetAssemblyDeleted();
            foreach (VirtualFile file in this._vdir.Files)
            {
                BuildResult vPathBuildResultFromCache = null;
                try
                {
                    vPathBuildResultFromCache = BuildManager.GetVPathBuildResultFromCache(file.VirtualPathObject);
                }
                catch
                {
                    if (!BuildManager.PerformingPrecompilation)
                    {
                        continue;
                    }
                }
                if (vPathBuildResultFromCache == null)
                {
                    System.Web.Compilation.BuildProvider provider = BuildManager.CreateBuildProvider(file.VirtualPathObject, this._compConfig, this._referencedAssemblies, false);
                    if (provider != null)
                    {
                        this._buildProviders[file.VirtualPath] = provider;
                    }
                }
            }
            if ((DiskBuildResultCache.InUseAssemblyWasDeleted && retryIfDeletionHappens) && BuildManager.PerformingPrecompilation)
            {
                this.AddBuildProviders(false);
            }
        }

        private void CacheAssemblyResults(AssemblyBuilder assemblyBuilder, CompilerResults results)
        {
            foreach (System.Web.Compilation.BuildProvider provider in assemblyBuilder.BuildProviders)
            {
                BuildResult buildResult = provider.GetBuildResult(results);
                if ((buildResult != null) && !BuildManager.CacheVPathBuildResult(provider.VirtualPathObject, buildResult, this._utcStart))
                {
                    break;
                }
            }
        }

        private void CacheCompileErrors(AssemblyBuilder assemblyBuilder, CompilerResults results)
        {
            System.Web.Compilation.BuildProvider provider = null;
            foreach (CompilerError error in results.Errors)
            {
                if (!error.IsWarning)
                {
                    System.Web.Compilation.BuildProvider buildProviderFromLinePragma = assemblyBuilder.GetBuildProviderFromLinePragma(error.FileName);
                    if (((buildProviderFromLinePragma != null) && (buildProviderFromLinePragma is BaseTemplateBuildProvider)) && (buildProviderFromLinePragma != provider))
                    {
                        provider = buildProviderFromLinePragma;
                        CompilerResults results2 = new CompilerResults(null);
                        foreach (string str in results.Output)
                        {
                            results2.Output.Add(str);
                        }
                        results2.PathToAssembly = results.PathToAssembly;
                        results2.NativeCompilerReturnValue = results.NativeCompilerReturnValue;
                        results2.Errors.Add(error);
                        HttpCompileException compileException = new HttpCompileException(results2, assemblyBuilder.GetGeneratedSourceFromBuildProvider(buildProviderFromLinePragma));
                        BuildResult result = new BuildResultCompileError(buildProviderFromLinePragma.VirtualPathObject, compileException);
                        buildProviderFromLinePragma.SetBuildResultDependencies(result);
                        BuildManager.CacheVPathBuildResult(buildProviderFromLinePragma.VirtualPathObject, result, this._utcStart);
                    }
                }
            }
        }

        private void CompileAssemblyBuilder(AssemblyBuilder builder)
        {
            CompilerResults results;
            try
            {
                results = builder.Compile();
            }
            catch (HttpCompileException exception)
            {
                this.CacheCompileErrors(builder, exception.Results);
                throw;
            }
            this.CacheAssemblyResults(builder, results);
        }

        private bool CompileNonDependentBuildProviders(ICollection buildProviders)
        {
            IDictionary dictionary = new Hashtable();
            ArrayList list = null;
            AssemblyBuilder builder = null;
            bool flag = false;
            foreach (System.Web.Compilation.BuildProvider provider in buildProviders)
            {
                ICollection is2;
                if (this.IsBuildProviderSkipable(provider))
                {
                    continue;
                }
                if (!BuildManager.ThrowOnFirstParseError)
                {
                    InternalBuildProvider provider2 = provider as InternalBuildProvider;
                    if (provider2 != null)
                    {
                        provider2.ThrowOnFirstParseError = false;
                    }
                }
                CompilerType compilerTypeFromBuildProvider = null;
                try
                {
                    compilerTypeFromBuildProvider = System.Web.Compilation.BuildProvider.GetCompilerTypeFromBuildProvider(provider);
                }
                catch (HttpParseException exception)
                {
                    if (!this._ignoreProvidersWithErrors)
                    {
                        flag = true;
                        if (this._firstException == null)
                        {
                            this._firstException = exception;
                        }
                        if (this._parserErrors == null)
                        {
                            this._parserErrors = new ParserErrorCollection();
                        }
                        this._parserErrors.AddRange(exception.ParserErrors);
                    }
                    continue;
                }
                catch
                {
                    if (!this._ignoreProvidersWithErrors)
                    {
                        throw;
                    }
                    continue;
                }
                AssemblyBuilder builder2 = builder;
                if (compilerTypeFromBuildProvider == null)
                {
                    if (builder != null)
                    {
                        goto Label_00E6;
                    }
                    if (list == null)
                    {
                        list = new ArrayList();
                    }
                    list.Add(provider);
                    continue;
                }
                builder2 = (AssemblyBuilder) dictionary[compilerTypeFromBuildProvider];
            Label_00E6:
                is2 = provider.GetGeneratedTypeNames();
                if (((builder2 == null) || builder2.IsBatchFull) || builder2.ContainsTypeNames(is2))
                {
                    if (builder2 != null)
                    {
                        this.CompileAssemblyBuilder(builder2);
                    }
                    AssemblyBuilder builder3 = compilerTypeFromBuildProvider.CreateAssemblyBuilder(this._compConfig, this._referencedAssemblies);
                    dictionary[compilerTypeFromBuildProvider] = builder3;
                    if ((builder == null) || (builder == builder2))
                    {
                        builder = builder3;
                    }
                    builder2 = builder3;
                }
                builder2.AddTypeNames(is2);
                builder2.AddBuildProvider(provider);
            }
            if (flag)
            {
                return false;
            }
            if (list != null)
            {
                bool flag2 = builder == null;
                foreach (System.Web.Compilation.BuildProvider provider3 in list)
                {
                    ICollection generatedTypeNames = provider3.GetGeneratedTypeNames();
                    if (((builder == null) || builder.IsBatchFull) || builder.ContainsTypeNames(generatedTypeNames))
                    {
                        if (builder != null)
                        {
                            this.CompileAssemblyBuilder(builder);
                        }
                        builder = CompilerType.GetDefaultAssemblyBuilder(this._compConfig, this._referencedAssemblies, this._vdir.VirtualPathObject, null);
                        flag2 = true;
                    }
                    builder.AddTypeNames(generatedTypeNames);
                    builder.AddBuildProvider(provider3);
                }
                if (flag2)
                {
                    this.CompileAssemblyBuilder(builder);
                }
            }
            foreach (AssemblyBuilder builder4 in dictionary.Values)
            {
                this.CompileAssemblyBuilder(builder4);
            }
            return true;
        }

        private void GetBuildResultDependencies()
        {
            foreach (System.Web.Compilation.BuildProvider provider in this._buildProviders.Values)
            {
                ICollection buildResultVirtualPathDependencies = provider.GetBuildResultVirtualPathDependencies();
                if (buildResultVirtualPathDependencies != null)
                {
                    foreach (string str in buildResultVirtualPathDependencies)
                    {
                        System.Web.Compilation.BuildProvider dependentBuildProvider = (System.Web.Compilation.BuildProvider) this._buildProviders[str];
                        if (dependentBuildProvider != null)
                        {
                            provider.AddBuildProviderDependency(dependentBuildProvider);
                        }
                    }
                }
            }
        }

        private bool IsBuildProviderSkipable(System.Web.Compilation.BuildProvider buildProvider)
        {
            if (buildProvider.IsDependedOn)
            {
                return false;
            }
            return ((buildProvider is SourceFileBuildProvider) || (buildProvider is ResXBuildProvider));
        }

        internal void Process()
        {
            this.AddBuildProviders(true);
            if (this._buildProviders.Count != 0)
            {
                BuildManager.ReportDirectoryCompilationProgress(this._vdir.VirtualPathObject);
                this.GetBuildResultDependencies();
                this.ProcessDependencies();
                ArrayList[] listArray = this._nonDependentBuckets;
                for (int i = 0; i < listArray.Length; i++)
                {
                    ICollection buildProviders = listArray[i];
                    if (!this.CompileNonDependentBuildProviders(buildProviders))
                    {
                        break;
                    }
                }
                if ((this._parserErrors != null) && (this._parserErrors.Count > 0))
                {
                    HttpParseException exception = new HttpParseException(this._firstException.Message, this._firstException, this._firstException.VirtualPath, this._firstException.Source, this._firstException.Line);
                    for (int j = 1; j < this._parserErrors.Count; j++)
                    {
                        exception.ParserErrors.Add(this._parserErrors[j]);
                    }
                    throw exception;
                }
            }
        }

        private void ProcessDependencies()
        {
            int num = 0;
            Hashtable hashtable = new Hashtable();
            Stack stack = new Stack();
            foreach (System.Web.Compilation.BuildProvider provider in this._buildProviders.Values)
            {
                stack.Push(provider);
                while (stack.Count > 0)
                {
                    System.Web.Compilation.BuildProvider provider2 = (System.Web.Compilation.BuildProvider) stack.Peek();
                    bool flag = false;
                    int num2 = 0;
                    if (provider2.BuildProviderDependencies != null)
                    {
                        foreach (System.Web.Compilation.BuildProvider provider3 in (IEnumerable) provider2.BuildProviderDependencies)
                        {
                            if (hashtable.ContainsKey(provider3))
                            {
                                if (num2 > ((int) hashtable[provider3]))
                                {
                                    if (((int) hashtable[provider3]) == -1)
                                    {
                                        throw new HttpException(System.Web.SR.GetString("File_Circular_Reference", new object[] { provider3.VirtualPath }));
                                    }
                                }
                                else
                                {
                                    num2 = ((int) hashtable[provider3]) + 1;
                                }
                            }
                            else
                            {
                                flag = true;
                                stack.Push(provider3);
                            }
                        }
                    }
                    if (flag)
                    {
                        hashtable[provider2] = -1;
                    }
                    else
                    {
                        stack.Pop();
                        hashtable[provider2] = num2;
                        if (num <= num2)
                        {
                            num = num2 + 1;
                        }
                    }
                }
            }
            this._nonDependentBuckets = new ArrayList[num];
            IDictionaryEnumerator enumerator = hashtable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                int index = (int) enumerator.Value;
                if (this._nonDependentBuckets[index] == null)
                {
                    this._nonDependentBuckets[index] = new ArrayList();
                }
                this._nonDependentBuckets[index].Add(enumerator.Key);
            }
        }

        internal void SetIgnoreErrors()
        {
            this._ignoreProvidersWithErrors = true;
        }
    }
}


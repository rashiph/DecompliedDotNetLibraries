namespace System.Web.Compilation
{
    using System;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;

    internal class CodeDirectoryCompiler
    {
        private BuildProvidersCompiler _bpc;
        private BuildProviderSet _buildProviders = new BuildProviderSet();
        private CodeDirectoryType _dirType;
        private StringSet _excludedSubdirectories;
        internal static BuildResultMainCodeAssembly _mainCodeBuildResult;
        private bool _onlyBuildLocalizedResources;
        private VirtualPath _virtualDir;
        internal const string sourcesDirectoryPrefix = "Sources_";

        private CodeDirectoryCompiler(VirtualPath virtualDir, CodeDirectoryType dirType, StringSet excludedSubdirectories)
        {
            this._virtualDir = virtualDir;
            this._dirType = dirType;
            this._excludedSubdirectories = excludedSubdirectories;
        }

        private void AddFolderLevelBuildProviders(VirtualDirectory vdir, FolderLevelBuildProviderAppliesTo appliesTo)
        {
            BuildManager.AddFolderLevelBuildProviders(this._buildProviders, vdir.VirtualPathObject, appliesTo, this._bpc.CompConfig, this._bpc.ReferencedAssemblies);
        }

        internal static void CallAppInitializeMethod()
        {
            if (_mainCodeBuildResult != null)
            {
                _mainCodeBuildResult.CallAppInitializeMethod();
            }
        }

        private void FindBuildProviders()
        {
            if ((this._dirType == CodeDirectoryType.MainCode) && ProfileBuildProvider.HasCompilableProfile)
            {
                this._buildProviders.Add(ProfileBuildProvider.Create());
            }
            VirtualDirectory vdir = HostingEnvironment.VirtualPathProvider.GetDirectory(this._virtualDir);
            this.ProcessDirectoryRecursive(vdir, true);
        }

        internal static Assembly GetCodeDirectoryAssembly(VirtualPath virtualDir, CodeDirectoryType dirType, string assemblyName, StringSet excludedSubdirectories, bool isDirectoryAllowed)
        {
            string path = virtualDir.MapPath();
            if (!isDirectoryAllowed && Directory.Exists(path))
            {
                throw new HttpException(System.Web.SR.GetString("Bar_dir_in_precompiled_app", new object[] { virtualDir }));
            }
            bool supportLocalization = IsResourceCodeDirectoryType(dirType);
            string cacheKey = assemblyName;
            BuildResult buildResultFromCache = BuildManager.GetBuildResultFromCache(cacheKey);
            Assembly a = null;
            if ((buildResultFromCache != null) && (buildResultFromCache is BuildResultCompiledAssembly))
            {
                if (buildResultFromCache is BuildResultMainCodeAssembly)
                {
                    _mainCodeBuildResult = (BuildResultMainCodeAssembly) buildResultFromCache;
                }
                a = ((BuildResultCompiledAssembly) buildResultFromCache).ResultAssembly;
                if (!supportLocalization)
                {
                    return a;
                }
                if (!isDirectoryAllowed)
                {
                    return a;
                }
                BuildResultResourceAssembly assembly2 = (BuildResultResourceAssembly) buildResultFromCache;
                if (HashCodeCombiner.GetDirectoryHash(virtualDir) == assembly2.ResourcesDependenciesHash)
                {
                    return a;
                }
            }
            if (!isDirectoryAllowed)
            {
                return null;
            }
            if ((dirType != CodeDirectoryType.LocalResources) && !StringUtil.StringStartsWithIgnoreCase(path, HttpRuntime.AppDomainAppPathInternal))
            {
                throw new HttpException(System.Web.SR.GetString("Virtual_codedir", new object[] { virtualDir.VirtualPathString }));
            }
            if (!Directory.Exists(path))
            {
                if (dirType != CodeDirectoryType.MainCode)
                {
                    return null;
                }
                if (!ProfileBuildProvider.HasCompilableProfile)
                {
                    return null;
                }
            }
            BuildManager.ReportDirectoryCompilationProgress(virtualDir);
            DateTime utcNow = DateTime.UtcNow;
            CodeDirectoryCompiler compiler = new CodeDirectoryCompiler(virtualDir, dirType, excludedSubdirectories);
            string outputAssemblyName = null;
            if (a != null)
            {
                outputAssemblyName = a.GetName().Name;
                compiler._onlyBuildLocalizedResources = true;
            }
            else
            {
                outputAssemblyName = BuildManager.GenerateRandomAssemblyName(assemblyName);
            }
            BuildProvidersCompiler compiler2 = new BuildProvidersCompiler(virtualDir, supportLocalization, outputAssemblyName);
            compiler._bpc = compiler2;
            compiler.FindBuildProviders();
            compiler2.SetBuildProviders(compiler._buildProviders);
            CompilerResults results = compiler2.PerformBuild();
            if (results != null)
            {
                DateTime time2 = DateTime.UtcNow.AddMilliseconds(3000.0);
                do
                {
                    if (UnsafeNativeMethods.GetModuleHandle(results.PathToAssembly) == IntPtr.Zero)
                    {
                        a = results.CompiledAssembly;
                        goto Label_01E6;
                    }
                    Thread.Sleep(250);
                }
                while (DateTime.UtcNow <= time2);
                throw new HttpException(System.Web.SR.GetString("Assembly_already_loaded", new object[] { results.PathToAssembly }));
            }
        Label_01E6:
            if (a == null)
            {
                return null;
            }
            if (dirType == CodeDirectoryType.MainCode)
            {
                _mainCodeBuildResult = new BuildResultMainCodeAssembly(a);
                buildResultFromCache = _mainCodeBuildResult;
            }
            else if (supportLocalization)
            {
                buildResultFromCache = new BuildResultResourceAssembly(a);
            }
            else
            {
                buildResultFromCache = new BuildResultCompiledAssembly(a);
            }
            buildResultFromCache.VirtualPath = virtualDir;
            if (BuildManager.OptimizeCompilations && (dirType != CodeDirectoryType.LocalResources))
            {
                buildResultFromCache.AddVirtualPathDependencies(new SingleObjectCollection(virtualDir.AppRelativeVirtualPathString));
            }
            if (dirType != CodeDirectoryType.LocalResources)
            {
                buildResultFromCache.CacheToMemory = false;
            }
            BuildManager.CacheBuildResult(cacheKey, buildResultFromCache, utcNow);
            return a;
        }

        internal static void GetCodeDirectoryInformation(VirtualPath virtualDir, CodeDirectoryType dirType, StringSet excludedSubdirectories, int index, out Type codeDomProviderType, out CompilerParameters compilerParameters, out string generatedFilesDir)
        {
            generatedFilesDir = HttpRuntime.CodegenDirInternal + @"\Sources_" + virtualDir.FileName;
            bool supportLocalization = IsResourceCodeDirectoryType(dirType);
            BuildProvidersCompiler compiler = new BuildProvidersCompiler(virtualDir, supportLocalization, generatedFilesDir, index);
            CodeDirectoryCompiler compiler2 = new CodeDirectoryCompiler(virtualDir, dirType, excludedSubdirectories) {
                _bpc = compiler
            };
            compiler2.FindBuildProviders();
            compiler.SetBuildProviders(compiler2._buildProviders);
            compiler.GenerateSources(out codeDomProviderType, out compilerParameters);
        }

        internal static bool IsResourceCodeDirectoryType(CodeDirectoryType dirType)
        {
            if (dirType != CodeDirectoryType.AppResources)
            {
                return (dirType == CodeDirectoryType.LocalResources);
            }
            return true;
        }

        private void ProcessDirectoryRecursive(VirtualDirectory vdir, bool topLevel)
        {
            if (this._dirType == CodeDirectoryType.WebReferences)
            {
                BuildProvider o = new WebReferencesBuildProvider(vdir);
                o.SetVirtualPath(vdir.VirtualPathObject);
                this._buildProviders.Add(o);
                this.AddFolderLevelBuildProviders(vdir, FolderLevelBuildProviderAppliesTo.WebReferences);
            }
            else if (this._dirType == CodeDirectoryType.AppResources)
            {
                this.AddFolderLevelBuildProviders(vdir, FolderLevelBuildProviderAppliesTo.GlobalResources);
            }
            else if (this._dirType == CodeDirectoryType.LocalResources)
            {
                this.AddFolderLevelBuildProviders(vdir, FolderLevelBuildProviderAppliesTo.LocalResources);
            }
            else if ((this._dirType == CodeDirectoryType.MainCode) || (this._dirType == CodeDirectoryType.SubCode))
            {
                this.AddFolderLevelBuildProviders(vdir, FolderLevelBuildProviderAppliesTo.Code);
            }
            foreach (VirtualFileBase base2 in vdir.Children)
            {
                if (base2.IsDirectory)
                {
                    if (((!topLevel || (this._excludedSubdirectories == null)) || !this._excludedSubdirectories.Contains(base2.Name)) && !(base2.Name == "_vti_cnf"))
                    {
                        this.ProcessDirectoryRecursive(base2 as VirtualDirectory, false);
                    }
                }
                else if ((this._dirType != CodeDirectoryType.WebReferences) && ((!IsResourceCodeDirectoryType(this._dirType) || !this._onlyBuildLocalizedResources) || (Util.GetCultureName(base2.VirtualPath) != null)))
                {
                    BuildProvider provider2 = BuildManager.CreateBuildProvider(base2.VirtualPathObject, IsResourceCodeDirectoryType(this._dirType) ? BuildProviderAppliesTo.Resources : BuildProviderAppliesTo.Code, this._bpc.CompConfig, this._bpc.ReferencedAssemblies, false);
                    if (provider2 != null)
                    {
                        if ((this._dirType == CodeDirectoryType.LocalResources) && (provider2 is BaseResourcesBuildProvider))
                        {
                            ((BaseResourcesBuildProvider) provider2).DontGenerateStronglyTypedClass();
                        }
                        this._buildProviders.Add(provider2);
                    }
                }
            }
        }
    }
}


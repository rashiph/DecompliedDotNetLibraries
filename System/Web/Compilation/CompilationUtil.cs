namespace System.Web.Compilation
{
    using Microsoft.CSharp;
    using Microsoft.VisualBasic;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;

    internal static class CompilationUtil
    {
        internal const string CodeDomProviderOptionPath = "system.codedom/compilers/compiler/ProviderOption/";
        private const string CompilerDirectoryPath = "CompilerDirectoryPath";

        internal static void CheckCompilerDirectoryPathAllowed(IDictionary<string, string> providerOptions)
        {
            if (((providerOptions != null) && providerOptions.ContainsKey("CompilerDirectoryPath")) && !HttpRuntime.HasUnmanagedPermission())
            {
                throw new HttpException(System.Web.SR.GetString("Insufficient_trust_for_attribute", new object[] { "CompilerDirectoryPath" }));
            }
        }

        internal static void CheckCompilerOptionsAllowed(string compilerOptions, bool config, string file, int line)
        {
            if (!string.IsNullOrEmpty(compilerOptions) && !HttpRuntime.HasUnmanagedPermission())
            {
                string message = System.Web.SR.GetString("Insufficient_trust_for_attribute", new object[] { "compilerOptions" });
                if (config)
                {
                    throw new ConfigurationErrorsException(message, file, line);
                }
                throw new HttpException(message);
            }
        }

        internal static CodeDomProvider CreateCodeDomProvider(Type codeDomProviderType)
        {
            CodeDomProvider provider = CreateCodeDomProviderWithPropertyOptions(codeDomProviderType);
            if (provider != null)
            {
                return provider;
            }
            return (CodeDomProvider) Activator.CreateInstance(codeDomProviderType);
        }

        internal static CodeDomProvider CreateCodeDomProviderNonPublic(Type codeDomProviderType)
        {
            CodeDomProvider provider = CreateCodeDomProviderWithPropertyOptions(codeDomProviderType);
            if (provider != null)
            {
                return provider;
            }
            return (CodeDomProvider) HttpRuntime.CreateNonPublicInstance(codeDomProviderType);
        }

        [ReflectionPermission(SecurityAction.Assert, Unrestricted=true)]
        private static CodeDomProvider CreateCodeDomProviderWithPropertyOptions(Type codeDomProviderType)
        {
            IDictionary<string, string> providerOptions = GetProviderOptions(codeDomProviderType);
            IDictionary<string, string> dictionary2 = null;
            if (providerOptions != null)
            {
                dictionary2 = new Dictionary<string, string>(providerOptions);
            }
            CheckCompilerDirectoryPathAllowed(dictionary2);
            bool flag = false;
            if (MultiTargetingUtil.IsTargetFramework20)
            {
                if (dictionary2 == null)
                {
                    dictionary2 = new Dictionary<string, string>();
                }
                dictionary2["CompilerVersion"] = "v2.0";
            }
            else if (MultiTargetingUtil.IsTargetFramework35)
            {
                dictionary2["CompilerVersion"] = "v3.5";
            }
            else
            {
                Version versionFromVString = GetVersionFromVString(GetCompilerVersion(codeDomProviderType));
                if ((versionFromVString != null) && (versionFromVString < MultiTargetingUtil.Version40))
                {
                    dictionary2["CompilerVersion"] = "v4.0";
                }
            }
            if ((dictionary2 == null) || (dictionary2.Count <= 0))
            {
                return null;
            }
            ConstructorInfo constructor = codeDomProviderType.GetConstructor(new Type[] { typeof(IDictionary<string, string>) });
            CodeDomProvider provider = null;
            if (constructor != null)
            {
                CodeDomProvider provider2 = (CodeDomProvider) Activator.CreateInstance(codeDomProviderType);
                provider = CodeDomProvider.CreateProvider(provider2.FileExtension, dictionary2);
            }
            if (flag)
            {
                dictionary2.Remove("CompilerDirectoryPath");
            }
            return provider;
        }

        internal static Type GetBuildProviderTypeFromExtension(CompilationSection config, string extension, BuildProviderAppliesTo neededFor, bool failIfUnknown)
        {
            BuildProviderInfo buildProviderInfo = System.Web.Compilation.BuildProvider.GetBuildProviderInfo(config, extension);
            Type c = null;
            if (((buildProviderInfo != null) && (buildProviderInfo.Type != typeof(IgnoreFileBuildProvider))) && (buildProviderInfo.Type != typeof(ForceCopyBuildProvider)))
            {
                c = buildProviderInfo.Type;
            }
            if (((neededFor == BuildProviderAppliesTo.Web) && BuildManager.PrecompilingForUpdatableDeployment) && !typeof(BaseTemplateBuildProvider).IsAssignableFrom(c))
            {
                c = null;
            }
            if (c != null)
            {
                if ((neededFor & buildProviderInfo.AppliesTo) != 0)
                {
                    return c;
                }
            }
            else if ((neededFor != BuildProviderAppliesTo.Resources) && (config.GetCompilerInfoFromExtension(extension, false) != null))
            {
                return typeof(SourceFileBuildProvider);
            }
            if (failIfUnknown)
            {
                throw new HttpException(System.Web.SR.GetString("Unknown_buildprovider_extension", new object[] { extension, neededFor.ToString() }));
            }
            return null;
        }

        internal static Type GetBuildProviderTypeFromExtension(VirtualPath configPath, string extension, BuildProviderAppliesTo neededFor, bool failIfUnknown)
        {
            return GetBuildProviderTypeFromExtension(MTConfigUtil.GetCompilationConfig(configPath), extension, neededFor, failIfUnknown);
        }

        internal static CompilerType GetCodeDefaultLanguageCompilerInfo()
        {
            return new CompilerType(typeof(VBCodeProvider), null);
        }

        internal static CodeSubDirectoriesCollection GetCodeSubDirectories()
        {
            CodeSubDirectoriesCollection codeSubDirectories = MTConfigUtil.GetCompilationAppConfig().CodeSubDirectories;
            if (codeSubDirectories != null)
            {
                codeSubDirectories.EnsureRuntimeValidation();
            }
            return codeSubDirectories;
        }

        private static CompilerType GetCompilerInfoFromExtension(VirtualPath configPath, string extension)
        {
            return MTConfigUtil.GetCompilationConfig(configPath).GetCompilerInfoFromExtension(extension, true);
        }

        internal static CompilerType GetCompilerInfoFromLanguage(VirtualPath configPath, string language)
        {
            return MTConfigUtil.GetCompilationConfig(configPath).GetCompilerInfoFromLanguage(language);
        }

        internal static CompilerType GetCompilerInfoFromVirtualPath(VirtualPath virtualPath)
        {
            string extension = virtualPath.Extension;
            if (extension.Length == 0)
            {
                throw new HttpException(System.Web.SR.GetString("Empty_extension", new object[] { virtualPath }));
            }
            return GetCompilerInfoFromExtension(virtualPath, extension);
        }

        internal static string GetCompilerVersion(Type codeDomProviderType)
        {
            return GetProviderOption(codeDomProviderType, "CompilerVersion");
        }

        internal static CompilerType GetCSharpCompilerInfo(CompilationSection compConfig, VirtualPath configPath)
        {
            if (compConfig == null)
            {
                compConfig = MTConfigUtil.GetCompilationConfig(configPath);
            }
            if (compConfig.DefaultLanguage == null)
            {
                return new CompilerType(typeof(CSharpCodeProvider), null);
            }
            return compConfig.GetCompilerInfoFromLanguage("c#");
        }

        internal static CompilerType GetDefaultLanguageCompilerInfo(CompilationSection compConfig, VirtualPath configPath)
        {
            if (compConfig == null)
            {
                compConfig = MTConfigUtil.GetCompilationConfig(configPath);
            }
            if (compConfig.DefaultLanguage == null)
            {
                return GetCodeDefaultLanguageCompilerInfo();
            }
            return compConfig.GetCompilerInfoFromLanguage(compConfig.DefaultLanguage);
        }

        internal static List<Type> GetFolderLevelBuildProviderTypes(CompilationSection config, FolderLevelBuildProviderAppliesTo appliesTo)
        {
            return config.FolderLevelBuildProviders.GetBuildProviderTypes(appliesTo);
        }

        internal static string GetProviderOption(Type codeDomProviderType, string providerOption)
        {
            string str;
            IDictionary<string, string> providerOptions = GetProviderOptions(codeDomProviderType);
            if ((providerOptions != null) && providerOptions.TryGetValue(providerOption, out str))
            {
                return str;
            }
            return null;
        }

        [ReflectionPermission(SecurityAction.Assert, Unrestricted=true)]
        private static IDictionary<string, string> GetProviderOptions(CompilerInfo ci)
        {
            PropertyInfo property = ci.GetType().GetProperty("ProviderOptions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null)
            {
                return (IDictionary<string, string>) property.GetValue(ci, null);
            }
            return null;
        }

        [ReflectionPermission(SecurityAction.Assert, Unrestricted=true)]
        internal static IDictionary<string, string> GetProviderOptions(Type codeDomProviderType)
        {
            CodeDomProvider provider = (CodeDomProvider) Activator.CreateInstance(codeDomProviderType);
            string fileExtension = provider.FileExtension;
            if (CodeDomProvider.IsDefinedExtension(fileExtension))
            {
                return GetProviderOptions(CodeDomProvider.GetCompilerInfo(CodeDomProvider.GetLanguageFromExtension(fileExtension)));
            }
            return null;
        }

        internal static long GetRecompilationHash(CompilationSection ps)
        {
            HashCodeCombiner combiner = new HashCodeCombiner();
            combiner.AddObject(ps.Debug);
            combiner.AddObject(ps.TargetFramework);
            combiner.AddObject(ps.Strict);
            combiner.AddObject(ps.Explicit);
            combiner.AddObject(ps.Batch);
            combiner.AddObject(ps.OptimizeCompilations);
            combiner.AddObject(ps.BatchTimeout);
            combiner.AddObject(ps.MaxBatchGeneratedFileSize);
            combiner.AddObject(ps.MaxBatchSize);
            combiner.AddObject(ps.NumRecompilesBeforeAppRestart);
            combiner.AddObject(ps.DefaultLanguage);
            combiner.AddObject(ps.UrlLinePragmas);
            if (ps.AssemblyPostProcessorTypeInternal != null)
            {
                combiner.AddObject(ps.AssemblyPostProcessorTypeInternal.FullName);
            }
            foreach (Compiler compiler in ps.Compilers)
            {
                combiner.AddObject(compiler.Language);
                combiner.AddObject(compiler.Extension);
                combiner.AddObject(compiler.Type);
                combiner.AddObject(compiler.WarningLevel);
                combiner.AddObject(compiler.CompilerOptions);
            }
            foreach (System.Web.Configuration.ExpressionBuilder builder in ps.ExpressionBuilders)
            {
                combiner.AddObject(builder.ExpressionPrefix);
                combiner.AddObject(builder.Type);
            }
            AssemblyCollection assemblies = ps.Assemblies;
            if (assemblies.Count == 0)
            {
                combiner.AddObject("__clearassemblies");
            }
            else
            {
                foreach (AssemblyInfo info in assemblies)
                {
                    combiner.AddObject(info.Assembly);
                }
            }
            BuildProviderCollection buildProviders = ps.BuildProviders;
            if (buildProviders.Count == 0)
            {
                combiner.AddObject("__clearbuildproviders");
            }
            else
            {
                foreach (System.Web.Configuration.BuildProvider provider in buildProviders)
                {
                    combiner.AddObject(provider.Type);
                    combiner.AddObject(provider.Extension);
                }
            }
            FolderLevelBuildProviderCollection folderLevelBuildProviders = ps.FolderLevelBuildProviders;
            if (folderLevelBuildProviders.Count == 0)
            {
                combiner.AddObject("__clearfolderlevelbuildproviders");
            }
            else
            {
                foreach (FolderLevelBuildProvider provider2 in folderLevelBuildProviders)
                {
                    combiner.AddObject(provider2.Type);
                    combiner.AddObject(provider2.Name);
                }
            }
            CodeSubDirectoriesCollection codeSubDirectories = ps.CodeSubDirectories;
            if (codeSubDirectories.Count == 0)
            {
                combiner.AddObject("__clearcodesubdirs");
            }
            else
            {
                foreach (CodeSubDirectory directory in codeSubDirectories)
                {
                    combiner.AddObject(directory.DirectoryName);
                }
            }
            CompilerInfo[] allCompilerInfo = CodeDomProvider.GetAllCompilerInfo();
            if (allCompilerInfo != null)
            {
                foreach (CompilerInfo info2 in allCompilerInfo)
                {
                    if (info2.IsCodeDomProviderTypeValid)
                    {
                        string compilerOptions = info2.CreateDefaultCompilerParameters().CompilerOptions;
                        if (!string.IsNullOrEmpty(compilerOptions))
                        {
                            Type codeDomProviderType = info2.CodeDomProviderType;
                            if (codeDomProviderType != null)
                            {
                                combiner.AddObject(codeDomProviderType.FullName);
                            }
                            combiner.AddObject(compilerOptions);
                        }
                        if (info2.CodeDomProviderType != null)
                        {
                            IDictionary<string, string> providerOptions = GetProviderOptions(info2);
                            if ((providerOptions != null) && (providerOptions.Count > 0))
                            {
                                string fullName = info2.CodeDomProviderType.FullName;
                                foreach (string str3 in providerOptions.Keys)
                                {
                                    string str4 = providerOptions[str3];
                                    combiner.AddObject(fullName + ":" + str3 + "=" + str4);
                                }
                            }
                        }
                    }
                }
            }
            return combiner.CombinedHash;
        }

        internal static int GetRecompilationsBeforeAppRestarts()
        {
            return MTConfigUtil.GetCompilationAppConfig().NumRecompilesBeforeAppRestart;
        }

        internal static Version GetVersionFromVString(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return null;
            }
            return new Version(version.Substring(1));
        }

        internal static bool IsBatchingEnabled(string configPath)
        {
            return MTConfigUtil.GetCompilationConfig(configPath).Batch;
        }

        internal static bool IsCompilerVersion35(string compilerVersion)
        {
            return (compilerVersion == "v3.5");
        }

        internal static bool IsCompilerVersion35(Type codeDomProviderType)
        {
            return IsCompilerVersion35(GetCompilerVersion(codeDomProviderType));
        }

        internal static bool IsCompilerVersion35OrAbove(Type codeDomProviderType)
        {
            if (!IsCompilerVersion35(GetCompilerVersion(codeDomProviderType)) && MultiTargetingUtil.IsTargetFramework20)
            {
                return false;
            }
            return true;
        }

        internal static bool IsDebuggingEnabled(HttpContext context)
        {
            return MTConfigUtil.GetCompilationConfig(context).Debug;
        }

        internal static Type LoadTypeWithChecks(string typeName, Type requiredBaseType, Type requiredBaseType2, ConfigurationElement elem, string propertyName)
        {
            Type type = ConfigUtil.GetType(typeName, propertyName, elem);
            if (requiredBaseType2 == null)
            {
                ConfigUtil.CheckAssignableType(requiredBaseType, type, elem, propertyName);
                return type;
            }
            ConfigUtil.CheckAssignableType(requiredBaseType, requiredBaseType2, type, elem, propertyName);
            return type;
        }

        internal static bool NeedToCopyFile(VirtualPath virtualPath, bool updatable, out bool createStub)
        {
            createStub = false;
            CompilationSection compilationConfig = MTConfigUtil.GetCompilationConfig(virtualPath);
            string extension = virtualPath.Extension;
            BuildProviderInfo buildProviderInfo = System.Web.Compilation.BuildProvider.GetBuildProviderInfo(compilationConfig, extension);
            if (buildProviderInfo != null)
            {
                if ((BuildProviderAppliesTo.Web & buildProviderInfo.AppliesTo) == 0)
                {
                    return true;
                }
                if (buildProviderInfo.Type == typeof(ForceCopyBuildProvider))
                {
                    return true;
                }
                if ((buildProviderInfo.Type != typeof(IgnoreFileBuildProvider)) && BuildManager.PrecompilingForUpdatableDeployment)
                {
                    return true;
                }
                createStub = true;
                if (((buildProviderInfo.Type == typeof(UserControlBuildProvider)) || (buildProviderInfo.Type == typeof(MasterPageBuildProvider))) || (buildProviderInfo.Type == typeof(IgnoreFileBuildProvider)))
                {
                    createStub = false;
                }
                return false;
            }
            if (compilationConfig.GetCompilerInfoFromExtension(extension, false) != null)
            {
                return false;
            }
            if (System.Web.Util.StringUtil.EqualsIgnoreCase(extension, ".asax"))
            {
                return false;
            }
            if (!updatable && System.Web.Util.StringUtil.EqualsIgnoreCase(extension, ".skin"))
            {
                return false;
            }
            return true;
        }

        internal static bool WarnAsError(Type codeDomProviderType)
        {
            bool flag;
            string providerOption = GetProviderOption(codeDomProviderType, "WarnAsError");
            return (((providerOption != null) && bool.TryParse(providerOption, out flag)) && flag);
        }
    }
}


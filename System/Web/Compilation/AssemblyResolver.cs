namespace System.Web.Compilation
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Tasks;
    using Microsoft.Build.Utilities;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;

    internal class AssemblyResolver
    {
        private static Dictionary<Assembly, string> s_assemblyLocations;
        private static Dictionary<Assembly, AssemblyResolutionResult> s_assemblyResults;
        private static Dictionary<Assembly, ReferenceAssemblyType> s_assemblyTypes;
        private static readonly Lazy<ConcurrentDictionary<string, Version>> s_assemblyVersions = new Lazy<ConcurrentDictionary<string, Version>>(() => new ConcurrentDictionary<string, Version>(StringComparer.OrdinalIgnoreCase));
        private static IList<string> s_fullProfileReferenceAssemblyPaths;
        private static IList<string> s_higherFrameworkReferenceAssemblyPaths;
        private static object s_lock = new object();
        private static bool? s_needToCheckFullProfile;
        private static IList<string> s_targetFrameworkReferenceAssemblyPaths;
        private static bool? s_warnAsError = null;
        private static object s_warnAsErrorLock = new object();

        [CompilerGenerated]
        private static ConcurrentDictionary<string, Version> <.cctor>b__3()
        {
            return new ConcurrentDictionary<string, Version>(StringComparer.OrdinalIgnoreCase);
        }

        private static void CheckOutOfRangeDependencies(string assemblyName)
        {
            string fullName = null;
            Assembly assembly = Assembly.Load(assemblyName);
            AssemblyName name = new AssemblyName(assemblyName);
            if (assembly.GetName().Version == name.Version)
            {
                foreach (AssemblyName name2 in assembly.GetReferencedAssemblies())
                {
                    try
                    {
                        string str2;
                        ReferenceAssemblyType type = GetPathToReferenceAssembly(CompilationSection.LoadAndRecordAssembly(name2), out str2, null, null, false);
                        Version assemblyVersion = GetAssemblyVersion(str2);
                        if ((assemblyVersion != null) && (((type == ReferenceAssemblyType.FrameworkAssembly) && (assemblyVersion < name2.Version)) || (type == ReferenceAssemblyType.FrameworkAssemblyOnlyPresentInHigherVersion)))
                        {
                            if (fullName == null)
                            {
                                fullName = name2.FullName;
                            }
                            else
                            {
                                fullName = fullName + "; " + name2.FullName;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                if (fullName != null)
                {
                    ReportWarningOrError(System.Web.SR.GetString("Higher_dependencies", new object[] { assemblyName, fullName }));
                }
            }
        }

        private static void FixMscorlibPath(Assembly a, ref string path)
        {
            if ((!MultiTargetingUtil.IsTargetFramework20 && !MultiTargetingUtil.IsTargetFramework35) && (a.FullName == typeof(string).Assembly.FullName))
            {
                path = a.Location;
            }
        }

        private static Version GetAssemblyVersion(string path)
        {
            Version version = null;
            ConcurrentDictionary<string, Version> assemblyVersions = AssemblyVersions;
            if (!assemblyVersions.TryGetValue(path, out version))
            {
                try
                {
                    version = AssemblyName.GetAssemblyName(path).Version;
                }
                catch
                {
                }
                assemblyVersions.TryAdd(path, version);
            }
            return version;
        }

        private static IList<string> GetPathToReferenceAssemblies(FrameworkName frameworkName)
        {
            return ToolLocationHelper.GetPathToReferenceAssemblies(frameworkName);
        }

        internal static ReferenceAssemblyType GetPathToReferenceAssembly(Assembly a, out string path)
        {
            return GetPathToReferenceAssembly(a, out path, null, null);
        }

        internal static ReferenceAssemblyType GetPathToReferenceAssembly(Assembly a, out string path, ICollection<BuildErrorEventArgs> errors, ICollection<BuildWarningEventArgs> warnings)
        {
            return GetPathToReferenceAssembly(a, out path, errors, warnings, true);
        }

        internal static ReferenceAssemblyType GetPathToReferenceAssembly(Assembly a, out string path, ICollection<BuildErrorEventArgs> errors, ICollection<BuildWarningEventArgs> warnings, bool checkDependencies)
        {
            lock (s_lock)
            {
                if (AssemblyLocations.TryGetValue(a, out path))
                {
                    return ReferenceAssemblyTypes[a];
                }
            }
            if ((TargetFrameworkReferenceAssemblyPaths == null) || (TargetFrameworkReferenceAssemblyPaths.Count == 0))
            {
                path = Util.GetAssemblyCodeBase(a);
                return ReferenceAssemblyType.FrameworkAssembly;
            }
            AssemblyResolutionResult result = null;
            ReferenceAssemblyType nonFrameworkAssembly = ReferenceAssemblyType.NonFrameworkAssembly;
            if (BuildResultCompiledAssemblyBase.AssemblyIsInCodegenDir(a))
            {
                path = Util.GetAssemblyCodeBase(a);
            }
            else
            {
                nonFrameworkAssembly = GetPathToReferenceAssembly(a, out path, errors, warnings, checkDependencies, true, out result);
            }
            StoreResults(a, path, result, nonFrameworkAssembly);
            return nonFrameworkAssembly;
        }

        private static ReferenceAssemblyType GetPathToReferenceAssembly(Assembly a, out string path, ICollection<BuildErrorEventArgs> errors, ICollection<BuildWarningEventArgs> warnings, bool checkDependencies, bool useFullName, out AssemblyResolutionResult result)
        {
            string originalAssemblyName;
            string name = a.GetName().Name;
            if (useFullName)
            {
                originalAssemblyName = CompilationSection.GetOriginalAssemblyName(a);
            }
            else
            {
                originalAssemblyName = name;
            }
            result = ResolveAssembly(originalAssemblyName, TargetFrameworkReferenceAssemblyPaths, TargetFrameworkReferenceAssemblyPaths, false);
            if ((result.ResolvedFiles != null) && (result.ResolvedFiles.Count > 0))
            {
                path = result.ResolvedFiles.FirstOrDefault<string>();
                FixMscorlibPath(a, ref path);
                return ReferenceAssemblyType.FrameworkAssembly;
            }
            result = ResolveAssembly(originalAssemblyName, HigherFrameworkReferenceAssemblyPaths, HigherFrameworkReferenceAssemblyPaths, false);
            if ((result.ResolvedFiles != null) && (result.ResolvedFiles.Count > 0))
            {
                path = result.ResolvedFiles.FirstOrDefault<string>();
                return ReferenceAssemblyType.FrameworkAssemblyOnlyPresentInHigherVersion;
            }
            if (NeedToCheckFullProfile)
            {
                result = ResolveAssembly(originalAssemblyName, FullProfileReferenceAssemblyPaths, FullProfileReferenceAssemblyPaths, false);
                if ((result.ResolvedFiles != null) && (result.ResolvedFiles.Count > 0))
                {
                    path = result.ResolvedFiles.FirstOrDefault<string>();
                    string str3 = "";
                    if (!string.IsNullOrEmpty(MultiTargetingUtil.TargetFrameworkName.Profile))
                    {
                        str3 = " '" + MultiTargetingUtil.TargetFrameworkName.Profile + "'";
                    }
                    ReportWarningOrError(System.Web.SR.GetString("Assembly_not_found_in_profile", new object[] { originalAssemblyName, str3 }));
                    return ReferenceAssemblyType.FrameworkAssemblyOnlyPresentInHigherVersion;
                }
            }
            List<string> searchPaths = new List<string>();
            searchPaths.AddRange(TargetFrameworkReferenceAssemblyPaths);
            searchPaths.Add(Path.GetDirectoryName(a.Location));
            if (useFullName)
            {
                searchPaths.Add("{GAC}");
            }
            if (!useFullName)
            {
                originalAssemblyName = a.GetName().FullName;
            }
            result = ResolveAssembly(originalAssemblyName, searchPaths, TargetFrameworkReferenceAssemblyPaths, checkDependencies);
            path = result.ResolvedFiles.FirstOrDefault<string>();
            if (string.IsNullOrEmpty(path))
            {
                path = Util.GetAssemblyCodeBase(a);
            }
            if (useFullName)
            {
                AssemblyResolutionResult result2 = ResolveAssembly(name, HigherFrameworkReferenceAssemblyPaths, HigherFrameworkReferenceAssemblyPaths, false);
                if ((result2.ResolvedFiles != null) && (result2.ResolvedFiles.Count > 0))
                {
                    return ReferenceAssemblyType.FrameworkAssembly;
                }
            }
            return ReferenceAssemblyType.NonFrameworkAssembly;
        }

        private static void ReportWarningOrError(string message)
        {
            if (WarnAsError)
            {
                throw new HttpCompileException(message);
            }
            CompilerError error = new CompilerError {
                ErrorText = message,
                IsWarning = true
            };
            if (BuildManager.CBMCallback != null)
            {
                BuildManager.CBMCallback.ReportCompilerError(error);
            }
        }

        private static AssemblyResolutionResult ResolveAssembly(string assemblyName, IList<string> searchPaths, IList<string> targetFrameworkDirectories, bool checkDependencies)
        {
            ResolveAssemblyReference reference = new ResolveAssemblyReference();
            MockEngine engine = new MockEngine();
            reference.BuildEngine = engine;
            if (searchPaths != null)
            {
                reference.SearchPaths = searchPaths.ToArray<string>();
            }
            if (targetFrameworkDirectories != null)
            {
                reference.TargetFrameworkDirectories = targetFrameworkDirectories.ToArray<string>();
            }
            reference.Assemblies = new ITaskItem[] { new TaskItem(assemblyName) };
            reference.Silent = true;
            reference.Execute();
            AssemblyResolutionResult result = new AssemblyResolutionResult();
            List<string> list = new List<string>();
            foreach (ITaskItem item in reference.ResolvedFiles)
            {
                list.Add(item.ItemSpec);
            }
            if (checkDependencies)
            {
                CheckOutOfRangeDependencies(assemblyName);
            }
            result.ResolvedFiles = list.ToArray();
            result.Warnings = engine.Warnings;
            result.Errors = engine.Errors;
            return result;
        }

        private static void StoreResults(Assembly a, string path, AssemblyResolutionResult result, ReferenceAssemblyType assemblyType)
        {
            lock (s_lock)
            {
                if (!AssemblyLocations.ContainsKey(a))
                {
                    AssemblyLocations.Add(a, path);
                    AssemblyResolutionResults.Add(a, result);
                    ReferenceAssemblyTypes.Add(a, assemblyType);
                }
            }
        }

        private static Dictionary<Assembly, string> AssemblyLocations
        {
            get
            {
                if (s_assemblyLocations == null)
                {
                    s_assemblyLocations = new Dictionary<Assembly, string>();
                }
                return s_assemblyLocations;
            }
        }

        private static Dictionary<Assembly, AssemblyResolutionResult> AssemblyResolutionResults
        {
            get
            {
                if (s_assemblyResults == null)
                {
                    s_assemblyResults = new Dictionary<Assembly, AssemblyResolutionResult>();
                }
                return s_assemblyResults;
            }
        }

        private static ConcurrentDictionary<string, Version> AssemblyVersions
        {
            get
            {
                return s_assemblyVersions.Value;
            }
        }

        private static IList<string> FullProfileReferenceAssemblyPaths
        {
            get
            {
                if (s_fullProfileReferenceAssemblyPaths == null)
                {
                    List<string> list = new List<string>();
                    FrameworkName targetFrameworkName = MultiTargetingUtil.TargetFrameworkName;
                    FrameworkName frameworkName = new FrameworkName(targetFrameworkName.Identifier, targetFrameworkName.Version);
                    list.AddRange(GetPathToReferenceAssemblies(frameworkName));
                    s_fullProfileReferenceAssemblyPaths = list;
                }
                return s_fullProfileReferenceAssemblyPaths;
            }
        }

        private static IList<string> HigherFrameworkReferenceAssemblyPaths
        {
            get
            {
                if (s_higherFrameworkReferenceAssemblyPaths == null)
                {
                    List<string> list = new List<string>();
                    FrameworkName targetFrameworkName = MultiTargetingUtil.TargetFrameworkName;
                    foreach (FrameworkName name2 in MultiTargetingUtil.KnownFrameworkNames)
                    {
                        if (string.Equals(name2.Identifier, targetFrameworkName.Identifier, StringComparison.OrdinalIgnoreCase) && string.Equals(name2.Profile, targetFrameworkName.Profile, StringComparison.OrdinalIgnoreCase))
                        {
                            Version version = name2.Version;
                            if (targetFrameworkName.Version < version)
                            {
                                list.AddRange(GetPathToReferenceAssemblies(name2));
                            }
                        }
                    }
                    s_higherFrameworkReferenceAssemblyPaths = list;
                }
                return s_higherFrameworkReferenceAssemblyPaths;
            }
        }

        private static bool NeedToCheckFullProfile
        {
            get
            {
                if (!s_needToCheckFullProfile.HasValue)
                {
                    if (FullProfileReferenceAssemblyPaths.Except<string>(TargetFrameworkReferenceAssemblyPaths, StringComparer.OrdinalIgnoreCase).Count<string>() == 0)
                    {
                        s_needToCheckFullProfile = false;
                    }
                    else
                    {
                        s_needToCheckFullProfile = true;
                    }
                }
                return s_needToCheckFullProfile.Value;
            }
        }

        private static Dictionary<Assembly, ReferenceAssemblyType> ReferenceAssemblyTypes
        {
            get
            {
                if (s_assemblyTypes == null)
                {
                    s_assemblyTypes = new Dictionary<Assembly, ReferenceAssemblyType>();
                }
                return s_assemblyTypes;
            }
        }

        private static IList<string> TargetFrameworkReferenceAssemblyPaths
        {
            get
            {
                if (s_targetFrameworkReferenceAssemblyPaths == null)
                {
                    IList<string> pathToReferenceAssemblies = GetPathToReferenceAssemblies(MultiTargetingUtil.TargetFrameworkName);
                    int count = pathToReferenceAssemblies.Count;
                    if (MultiTargetingUtil.IsTargetFramework20 || MultiTargetingUtil.IsTargetFramework35)
                    {
                        if (string.IsNullOrEmpty(ToolLocationHelper.GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version35)))
                        {
                            throw new HttpException(System.Web.SR.GetString("Downlevel_requires_35"));
                        }
                        IList<string> list2 = GetPathToReferenceAssemblies(MultiTargetingUtil.FrameworkNameV30);
                        IList<string> list3 = GetPathToReferenceAssemblies(MultiTargetingUtil.FrameworkNameV20);
                        bool flag = MultiTargetingUtil.IsTargetFramework35 && ((list2.Count == count) || (list3.Count == count));
                        if ((count == 0) || flag)
                        {
                            throw new HttpException(System.Web.SR.GetString("Reference_assemblies_not_found"));
                        }
                    }
                    else if (BuildManagerHost.SupportsMultiTargeting && (count == 0))
                    {
                        throw new HttpException(System.Web.SR.GetString("Reference_assemblies_not_found"));
                    }
                    s_targetFrameworkReferenceAssemblyPaths = pathToReferenceAssemblies;
                }
                return s_targetFrameworkReferenceAssemblyPaths;
            }
        }

        private static bool WarnAsError
        {
            get
            {
                if (!s_warnAsError.HasValue)
                {
                    lock (s_warnAsErrorLock)
                    {
                        if (!s_warnAsError.HasValue)
                        {
                            s_warnAsError = false;
                            foreach (CompilerInfo info in CodeDomProvider.GetAllCompilerInfo())
                            {
                                if (((info != null) && info.IsCodeDomProviderTypeValid) && CompilationUtil.WarnAsError(info.CodeDomProviderType))
                                {
                                    s_warnAsError = true;
                                    goto Label_0086;
                                }
                            }
                        }
                    }
                }
            Label_0086:
                return s_warnAsError.Value;
            }
        }
    }
}


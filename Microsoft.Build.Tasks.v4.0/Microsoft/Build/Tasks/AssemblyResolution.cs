namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal static class AssemblyResolution
    {
        internal static Microsoft.Build.Tasks.Resolver[] CompileDirectories(IEnumerable<string> directories, Microsoft.Build.Shared.FileExists fileExists, GetAssemblyName getAssemblyName, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntimeVersion)
        {
            List<Microsoft.Build.Tasks.Resolver> list = new List<Microsoft.Build.Tasks.Resolver>();
            foreach (string str in directories)
            {
                list.Add(new DirectoryResolver(str, getAssemblyName, fileExists, getRuntimeVersion, targetedRuntimeVersion));
            }
            return list.ToArray();
        }

        public static Microsoft.Build.Tasks.Resolver[] CompileSearchPaths(string[] searchPaths, string[] candidateAssemblyFiles, ProcessorArchitecture targetProcessorArchitecture, string[] frameworkPaths, Microsoft.Build.Shared.FileExists fileExists, GetAssemblyName getAssemblyName, GetRegistrySubKeyNames getRegistrySubKeyNames, GetRegistrySubKeyDefaultValue getRegistrySubKeyDefaultValue, OpenBaseKey openBaseKey, InstalledAssemblies installedAssemblies, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntimeVersion)
        {
            Microsoft.Build.Tasks.Resolver[] resolverArray = new Microsoft.Build.Tasks.Resolver[searchPaths.Length];
            for (int i = 0; i < searchPaths.Length; i++)
            {
                string strA = searchPaths[i];
                if (string.Compare(strA, "{hintpathfromitem}", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    resolverArray[i] = new HintPathResolver(searchPaths[i], getAssemblyName, fileExists, getRuntimeVersion, targetedRuntimeVersion);
                }
                else if (string.Compare(strA, "{targetframeworkdirectory}", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    resolverArray[i] = new FrameworkPathResolver(frameworkPaths, installedAssemblies, searchPaths[i], getAssemblyName, fileExists, getRuntimeVersion, targetedRuntimeVersion);
                }
                else if (string.Compare(strA, "{rawfilename}", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    resolverArray[i] = new RawFilenameResolver(searchPaths[i], getAssemblyName, fileExists, getRuntimeVersion, targetedRuntimeVersion);
                }
                else if (string.Compare(strA, "{candidateassemblyfiles}", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    resolverArray[i] = new CandidateAssemblyFilesResolver(candidateAssemblyFiles, searchPaths[i], getAssemblyName, fileExists, getRuntimeVersion, targetedRuntimeVersion);
                }
                else if (string.Compare(strA, "{gac}", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    resolverArray[i] = new GacResolver(targetProcessorArchitecture, searchPaths[i], getAssemblyName, fileExists, getRuntimeVersion, targetedRuntimeVersion);
                }
                else if (string.Compare(strA, "{assemblyfolders}", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    resolverArray[i] = new AssemblyFoldersResolver(searchPaths[i], getAssemblyName, fileExists, getRuntimeVersion, targetedRuntimeVersion);
                }
                else if (string.Compare(strA, 0, "{registry:", 0, "{registry:".Length, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    resolverArray[i] = new AssemblyFoldersExResolver(searchPaths[i], getAssemblyName, fileExists, getRegistrySubKeyNames, getRegistrySubKeyDefaultValue, getRuntimeVersion, openBaseKey, targetedRuntimeVersion, targetProcessorArchitecture, true);
                }
                else
                {
                    resolverArray[i] = new DirectoryResolver(searchPaths[i], getAssemblyName, fileExists, getRuntimeVersion, targetedRuntimeVersion);
                }
            }
            return resolverArray;
        }

        internal static string ResolveReference(IEnumerable<Microsoft.Build.Tasks.Resolver[]> jaggedResolvers, AssemblyNameExtension assemblyName, string rawFileNameCandidate, bool isPrimaryProjectReference, bool wantSpecificVersion, string[] executableExtensions, string hintPath, string assemblyFolderKey, ArrayList assembliesConsideredAndRejected, out string resolvedSearchPath, out bool userRequestedSpecificFile)
        {
            userRequestedSpecificFile = false;
            resolvedSearchPath = string.Empty;
            foreach (Microsoft.Build.Tasks.Resolver[] resolverArray in jaggedResolvers)
            {
                if (resolverArray == null)
                {
                    break;
                }
                foreach (Microsoft.Build.Tasks.Resolver resolver in resolverArray)
                {
                    string str;
                    if (resolver.Resolve(assemblyName, rawFileNameCandidate, isPrimaryProjectReference, wantSpecificVersion, executableExtensions, hintPath, assemblyFolderKey, assembliesConsideredAndRejected, out str, out userRequestedSpecificFile))
                    {
                        resolvedSearchPath = resolver.SearchPath;
                        return str;
                    }
                }
            }
            return null;
        }
    }
}


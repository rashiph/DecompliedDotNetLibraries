namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    internal class FrameworkPathResolver : Microsoft.Build.Tasks.Resolver
    {
        private string[] frameworkPaths;
        private InstalledAssemblies installedAssemblies;

        public FrameworkPathResolver(string[] frameworkPaths, InstalledAssemblies installedAssemblies, string searchPathElement, GetAssemblyName getAssemblyName, Microsoft.Build.Shared.FileExists fileExists, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntimeVesion) : base(searchPathElement, getAssemblyName, fileExists, getRuntimeVersion, targetedRuntimeVesion, ProcessorArchitecture.None, false)
        {
            this.frameworkPaths = frameworkPaths;
            this.installedAssemblies = installedAssemblies;
        }

        internal static AssemblyNameExtension GetHighestVersionInRedist(InstalledAssemblies installedAssemblies, AssemblyNameExtension assemblyName)
        {
            AssemblyNameExtension extension = assemblyName;
            if ((extension.Version == null) && (installedAssemblies != null))
            {
                AssemblyEntry[] entryArray = installedAssemblies.FindAssemblyNameFromSimpleName(assemblyName.Name);
                if (entryArray.Length <= 1)
                {
                    return extension;
                }
                for (int i = 0; i < entryArray.Length; i++)
                {
                    AssemblyNameExtension that = new AssemblyNameExtension(entryArray[i].FullName);
                    if (((that.Version != null) && (that.Version.CompareTo(extension.Version) > 0)) && assemblyName.PartialNameCompare(that, PartialComparisonFlags.PublicKeyToken | PartialComparisonFlags.Culture))
                    {
                        extension = that;
                    }
                }
            }
            return extension;
        }

        public override bool Resolve(AssemblyNameExtension assemblyName, string rawFileNameCandidate, bool isPrimaryProjectReference, bool wantSpecificVersion, string[] executableExtensions, string hintPath, string assemblyFolderKey, ArrayList assembliesConsideredAndRejected, out string foundPath, out bool userRequestedSpecificFile)
        {
            foundPath = null;
            userRequestedSpecificFile = false;
            if (assemblyName != null)
            {
                AssemblyNameExtension highestVersionInRedist = GetHighestVersionInRedist(this.installedAssemblies, assemblyName);
                foreach (string str in this.frameworkPaths)
                {
                    string str2 = base.ResolveFromDirectory(highestVersionInRedist, isPrimaryProjectReference, wantSpecificVersion, executableExtensions, str, assembliesConsideredAndRejected);
                    if (str2 != null)
                    {
                        foundPath = str2;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}


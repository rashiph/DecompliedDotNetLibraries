namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Runtime.InteropServices;

    internal class InstalledAssemblies
    {
        private RedistList redistList;

        internal InstalledAssemblies(RedistList redistList)
        {
            this.redistList = redistList;
        }

        internal AssemblyEntry[] FindAssemblyNameFromSimpleName(string simpleName)
        {
            if (this.redistList == null)
            {
                return new AssemblyEntry[0];
            }
            return this.redistList.FindAssemblyNameFromSimpleName(simpleName);
        }

        internal AssemblyEntry FindHighestVersionInRedistList(AssemblyNameExtension assemblyName)
        {
            foreach (AssemblyEntry entry in this.redistList.FindAssemblyNameFromSimpleName(assemblyName.Name))
            {
                if (entry.AssemblyNameExtension.EqualsIgnoreVersion(assemblyName))
                {
                    return entry;
                }
            }
            return null;
        }

        internal void GetInfo(AssemblyNameExtension assemblyName, out Version unifiedVersion, out bool isPrerequisite, out bool? isRedistRoot, out string redistName)
        {
            unifiedVersion = assemblyName.Version;
            isPrerequisite = false;
            isRedistRoot = 0;
            redistName = null;
            if ((this.redistList != null) && (assemblyName.Version != null))
            {
                AssemblyEntry entry = this.FindHighestVersionInRedistList(assemblyName);
                if ((entry != null) && (assemblyName.Version <= entry.AssemblyNameExtension.Version))
                {
                    unifiedVersion = entry.AssemblyNameExtension.Version;
                    isPrerequisite = this.redistList.IsPrerequisiteAssembly(entry.FullName);
                    isRedistRoot = this.redistList.IsRedistRoot(entry.FullName);
                    redistName = this.redistList.RedistName(entry.FullName);
                }
            }
        }
    }
}


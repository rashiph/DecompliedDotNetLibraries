namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    internal class AssemblyFoldersResolver : Microsoft.Build.Tasks.Resolver
    {
        public AssemblyFoldersResolver(string searchPathElement, GetAssemblyName getAssemblyName, Microsoft.Build.Shared.FileExists fileExists, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntimeVesion) : base(searchPathElement, getAssemblyName, fileExists, getRuntimeVersion, targetedRuntimeVesion, ProcessorArchitecture.None, false)
        {
        }

        public override bool Resolve(AssemblyNameExtension assemblyName, string rawFileNameCandidate, bool isPrimaryProjectReference, bool wantSpecificVersion, string[] executableExtensions, string hintPath, string assemblyFolderKey, ArrayList assembliesConsideredAndRejected, out string foundPath, out bool userRequestedSpecificFile)
        {
            foundPath = null;
            userRequestedSpecificFile = false;
            if (assemblyName != null)
            {
                ICollection assemblyFolders = AssemblyFolder.GetAssemblyFolders(assemblyFolderKey);
                if (assemblyFolders != null)
                {
                    foreach (string str in assemblyFolders)
                    {
                        string str2 = base.ResolveFromDirectory(assemblyName, isPrimaryProjectReference, wantSpecificVersion, executableExtensions, str, assembliesConsideredAndRejected);
                        if (str2 != null)
                        {
                            foundPath = str2;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}


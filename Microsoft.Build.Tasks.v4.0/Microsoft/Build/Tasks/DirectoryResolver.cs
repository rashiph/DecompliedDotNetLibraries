namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    internal class DirectoryResolver : Microsoft.Build.Tasks.Resolver
    {
        public DirectoryResolver(string searchPathElement, GetAssemblyName getAssemblyName, Microsoft.Build.Shared.FileExists fileExists, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntimeVesion) : base(searchPathElement, getAssemblyName, fileExists, getRuntimeVersion, targetedRuntimeVesion, ProcessorArchitecture.None, false)
        {
        }

        public override bool Resolve(AssemblyNameExtension assemblyName, string rawFileNameCandidate, bool isPrimaryProjectReference, bool wantSpecificVersion, string[] executableExtensions, string hintPath, string assemblyFolderKey, ArrayList assembliesConsideredAndRejected, out string foundPath, out bool userRequestedSpecificFile)
        {
            foundPath = null;
            userRequestedSpecificFile = false;
            string str = base.ResolveFromDirectory(assemblyName, isPrimaryProjectReference, wantSpecificVersion, executableExtensions, base.searchPathElement, assembliesConsideredAndRejected);
            if (str != null)
            {
                foundPath = str;
                return true;
            }
            return false;
        }
    }
}


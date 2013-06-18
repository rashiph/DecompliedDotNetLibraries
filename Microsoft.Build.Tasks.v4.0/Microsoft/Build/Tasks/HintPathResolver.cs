namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    internal class HintPathResolver : Microsoft.Build.Tasks.Resolver
    {
        public HintPathResolver(string searchPathElement, GetAssemblyName getAssemblyName, Microsoft.Build.Shared.FileExists fileExists, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntimeVesion) : base(searchPathElement, getAssemblyName, fileExists, getRuntimeVersion, targetedRuntimeVesion, ProcessorArchitecture.None, false)
        {
        }

        public override bool Resolve(AssemblyNameExtension assemblyName, string rawFileNameCandidate, bool isPrimaryProjectReference, bool wantSpecificVersion, string[] executableExtensions, string hintPath, string assemblyFolderKey, ArrayList assembliesConsideredAndRejected, out string foundPath, out bool userRequestedSpecificFile)
        {
            if (((hintPath != null) && (hintPath.Length > 0)) && base.ResolveAsFile(hintPath, assemblyName, isPrimaryProjectReference, wantSpecificVersion, true, assembliesConsideredAndRejected))
            {
                userRequestedSpecificFile = true;
                foundPath = hintPath;
                return true;
            }
            foundPath = null;
            userRequestedSpecificFile = false;
            return false;
        }
    }
}


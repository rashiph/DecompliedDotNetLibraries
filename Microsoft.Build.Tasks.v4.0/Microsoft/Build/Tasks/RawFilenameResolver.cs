namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    internal class RawFilenameResolver : Microsoft.Build.Tasks.Resolver
    {
        public RawFilenameResolver(string searchPathElement, GetAssemblyName getAssemblyName, Microsoft.Build.Shared.FileExists fileExists, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntimeVesion) : base(searchPathElement, getAssemblyName, fileExists, getRuntimeVersion, targetedRuntimeVesion, ProcessorArchitecture.None, false)
        {
        }

        public override bool Resolve(AssemblyNameExtension assemblyName, string rawFileNameCandidate, bool isPrimaryProjectReference, bool wantSpecificVersion, string[] executableExtensions, string hintPath, string assemblyFolderKey, ArrayList assembliesConsideredAndRejected, out string foundPath, out bool userRequestedSpecificFile)
        {
            foundPath = null;
            userRequestedSpecificFile = false;
            if (rawFileNameCandidate != null)
            {
                if (base.fileExists(rawFileNameCandidate))
                {
                    userRequestedSpecificFile = true;
                    foundPath = rawFileNameCandidate;
                    return true;
                }
                if (assembliesConsideredAndRejected != null)
                {
                    ResolutionSearchLocation location = null;
                    location = new ResolutionSearchLocation {
                        FileNameAttempted = rawFileNameCandidate,
                        SearchPath = base.searchPathElement,
                        Reason = NoMatchReason.NotAFileNameOnDisk
                    };
                    assembliesConsideredAndRejected.Add(location);
                }
            }
            return false;
        }
    }
}


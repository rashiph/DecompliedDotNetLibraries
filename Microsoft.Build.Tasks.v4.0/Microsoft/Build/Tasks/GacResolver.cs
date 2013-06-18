namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class GacResolver : Microsoft.Build.Tasks.Resolver
    {
        public GacResolver(ProcessorArchitecture targetProcessorArchitecture, string searchPathElement, GetAssemblyName getAssemblyName, Microsoft.Build.Shared.FileExists fileExists, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntimeVesion) : base(searchPathElement, getAssemblyName, fileExists, getRuntimeVersion, targetedRuntimeVesion, targetProcessorArchitecture, true)
        {
        }

        public override bool Resolve(AssemblyNameExtension assemblyName, string rawFileNameCandidate, bool isPrimaryProjectReference, bool wantSpecificVersion, string[] executableExtensions, string hintPath, string assemblyFolderKey, ArrayList assembliesConsideredAndRejected, out string foundPath, out bool userRequestedSpecificFile)
        {
            foundPath = null;
            userRequestedSpecificFile = false;
            if (assemblyName != null)
            {
                string path = GlobalAssemblyCache.GetLocation(assemblyName, base.targetProcessorArchitecture, base.getRuntimeVersion, base.targetedRuntimeVersion, false, base.fileExists, null, null, wantSpecificVersion);
                if (((path != null) && (path.Length > 0)) && base.fileExists(path))
                {
                    foundPath = path;
                    return true;
                }
                if (assembliesConsideredAndRejected != null)
                {
                    ResolutionSearchLocation location = new ResolutionSearchLocation {
                        FileNameAttempted = assemblyName.FullName,
                        SearchPath = base.searchPathElement,
                        AssemblyName = assemblyName,
                        Reason = NoMatchReason.NotInGac
                    };
                    assembliesConsideredAndRejected.Add(location);
                }
            }
            return false;
        }
    }
}


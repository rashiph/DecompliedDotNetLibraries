namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    internal class CandidateAssemblyFilesResolver : Microsoft.Build.Tasks.Resolver
    {
        private string[] candidateAssemblyFiles;

        public CandidateAssemblyFilesResolver(string[] candidateAssemblyFiles, string searchPathElement, GetAssemblyName getAssemblyName, Microsoft.Build.Shared.FileExists fileExists, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntimeVesion) : base(searchPathElement, getAssemblyName, fileExists, getRuntimeVersion, targetedRuntimeVesion, ProcessorArchitecture.None, false)
        {
            this.candidateAssemblyFiles = candidateAssemblyFiles;
        }

        public override bool Resolve(AssemblyNameExtension assemblyName, string rawFileNameCandidate, bool isPrimaryProjectReference, bool wantSpecificVersion, string[] executableExtensions, string hintPath, string assemblyFolderKey, ArrayList assembliesConsideredAndRejected, out string foundPath, out bool userRequestedSpecificFile)
        {
            foundPath = null;
            userRequestedSpecificFile = false;
            if (assemblyName != null)
            {
                foreach (string str in this.candidateAssemblyFiles)
                {
                    if (Microsoft.Build.Shared.FileUtilities.HasExtension(str, executableExtensions))
                    {
                        bool flag2 = false;
                        ResolutionSearchLocation searchLocation = null;
                        if (assembliesConsideredAndRejected != null)
                        {
                            searchLocation = new ResolutionSearchLocation {
                                FileNameAttempted = str,
                                SearchPath = base.searchPathElement
                            };
                        }
                        if (base.FileMatchesAssemblyName(assemblyName, isPrimaryProjectReference, wantSpecificVersion, false, str, searchLocation))
                        {
                            flag2 = true;
                        }
                        else if (assembliesConsideredAndRejected != null)
                        {
                            assembliesConsideredAndRejected.Add(searchLocation);
                        }
                        if (flag2)
                        {
                            foundPath = str;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}


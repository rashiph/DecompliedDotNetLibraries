namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    internal class AssemblyFoldersExResolver : Microsoft.Build.Tasks.Resolver
    {
        private static readonly Regex crackAssemblyFoldersExSentinel = new Regex("{registry:(?<REGISTRYKEYROOT>[^,]*),(?<TARGETRUNTIMEVERSION>[^,]*),(?<REGISTRYKEYSUFFIX>[^,]*)([,]*)(?<CONDITIONS>.*)}", RegexOptions.IgnoreCase);
        private GetRegistrySubKeyDefaultValue getRegistrySubKeyDefaultValue;
        private GetRegistrySubKeyNames getRegistrySubKeyNames;
        private bool isInitialized;
        private OpenBaseKey openBaseKey;
        private string osVersion;
        private string platform;
        private string registryKeyRoot;
        private string registryKeySuffix;
        private string targetRuntimeVersion;
        private bool wasMatch;

        public AssemblyFoldersExResolver(string searchPathElement, GetAssemblyName getAssemblyName, Microsoft.Build.Shared.FileExists fileExists, GetRegistrySubKeyNames getRegistrySubKeyNames, GetRegistrySubKeyDefaultValue getRegistrySubKeyDefaultValue, GetAssemblyRuntimeVersion getRuntimeVersion, OpenBaseKey openBaseKey, Version targetedRuntimeVesion, ProcessorArchitecture targetProcessorArchitecture, bool compareProcessorArchitecture) : base(searchPathElement, getAssemblyName, fileExists, getRuntimeVersion, targetedRuntimeVesion, targetProcessorArchitecture, compareProcessorArchitecture)
        {
            this.getRegistrySubKeyNames = getRegistrySubKeyNames;
            this.getRegistrySubKeyDefaultValue = getRegistrySubKeyDefaultValue;
            this.openBaseKey = openBaseKey;
        }

        private void LazyInitialize()
        {
            if (!this.isInitialized)
            {
                this.isInitialized = true;
                Match match = crackAssemblyFoldersExSentinel.Match(base.searchPathElement);
                this.wasMatch = false;
                if (match.Success)
                {
                    this.registryKeyRoot = match.Groups["REGISTRYKEYROOT"].Value.Trim();
                    this.targetRuntimeVersion = match.Groups["TARGETRUNTIMEVERSION"].Value.Trim();
                    this.registryKeySuffix = match.Groups["REGISTRYKEYSUFFIX"].Value.Trim();
                    this.osVersion = null;
                    this.platform = null;
                    Group group = match.Groups["CONDITIONS"];
                    if (((this.registryKeyRoot.Length != 0) && (this.targetRuntimeVersion.Length != 0)) && (this.registryKeySuffix.Length != 0))
                    {
                        if (!this.targetRuntimeVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                        {
                            this.targetRuntimeVersion = this.targetRuntimeVersion.Insert(0, "v");
                        }
                        if (((group != null) && (group.Value != null)) && ((group.Length > 0) && (group.Value.Length > 0)))
                        {
                            foreach (string str2 in group.Value.Trim().Split(new char[] { ':' }))
                            {
                                if (string.Compare(str2, 0, "OSVERSION=", 0, 10, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    this.osVersion = str2.Substring(10);
                                }
                                else if (string.Compare(str2, 0, "PLATFORM=", 0, 9, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    this.platform = str2.Substring(9);
                                }
                            }
                        }
                        this.wasMatch = true;
                    }
                }
            }
        }

        public override bool Resolve(AssemblyNameExtension assemblyName, string rawFileNameCandidate, bool isPrimaryProjectReference, bool wantSpecificVersion, string[] executableExtensions, string hintPath, string assemblyFolderKey, ArrayList assembliesConsideredAndRejected, out string foundPath, out bool userRequestedSpecificFile)
        {
            foundPath = null;
            userRequestedSpecificFile = false;
            if (assemblyName != null)
            {
                this.LazyInitialize();
                if (this.wasMatch)
                {
                    AssemblyFoldersEx ex = new AssemblyFoldersEx(this.registryKeyRoot, this.targetRuntimeVersion, this.registryKeySuffix, this.osVersion, this.platform, this.getRegistrySubKeyNames, this.getRegistrySubKeyDefaultValue, base.targetProcessorArchitecture, this.openBaseKey);
                    string str = null;
                    foreach (string str2 in ex)
                    {
                        string path = base.ResolveFromDirectory(assemblyName, isPrimaryProjectReference, wantSpecificVersion, executableExtensions, str2, assembliesConsideredAndRejected);
                        if (path != null)
                        {
                            if (str == null)
                            {
                                str = path;
                            }
                            if ((base.targetProcessorArchitecture != ProcessorArchitecture.MSIL) && (base.targetProcessorArchitecture != ProcessorArchitecture.None))
                            {
                                foundPath = path;
                                return true;
                            }
                            AssemblyNameExtension extension = base.getAssemblyName(path);
                            if ((extension != null) && ((extension.AssemblyName.ProcessorArchitecture == ProcessorArchitecture.MSIL) || (extension.AssemblyName.ProcessorArchitecture == ProcessorArchitecture.None)))
                            {
                                foundPath = path;
                                return true;
                            }
                        }
                    }
                    if (str != null)
                    {
                        foundPath = str;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}


namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
    using System;

    public sealed class RequiresFramework35SP1Assembly : TaskExtension
    {
        private ITaskItem[] assemblies;
        private bool? createDesktopShortcut;
        private ITaskItem deploymentManifestEntryPoint;
        private ITaskItem entryPoint;
        private string errorReportUrl;
        private ITaskItem[] files;
        private bool outputRequiresMinimumFramework35SP1;
        private ITaskItem[] referencedAssemblies;
        private bool signingManifests;
        private string suiteName;
        private string targetFrameworkVersion = "v2.0";

        private static int CompareFrameworkVersions(string versionA, string versionB)
        {
            Version version = ConvertFrameworkVersionToString(versionA);
            Version version2 = ConvertFrameworkVersionToString(versionB);
            return version.CompareTo(version2);
        }

        private static Version ConvertFrameworkVersionToString(string version)
        {
            if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                return new Version(version.Substring(1));
            }
            return new Version(version);
        }

        private bool ExcludeReferenceFromHashing()
        {
            if ((!HasExcludedFileOrSP1File(this.referencedAssemblies) && !HasExcludedFileOrSP1File(this.assemblies)) && ((!HasExcludedFileOrSP1File(this.files) && !IsExcludedFileOrSP1File(this.deploymentManifestEntryPoint)) && !IsExcludedFileOrSP1File(this.entryPoint)))
            {
                return false;
            }
            return true;
        }

        public override bool Execute()
        {
            this.outputRequiresMinimumFramework35SP1 = false;
            if ((this.HasErrorUrl() || this.HasCreatedShortcut()) || ((this.UncheckedSigning() || this.ExcludeReferenceFromHashing()) || this.HasSuiteName()))
            {
                this.outputRequiresMinimumFramework35SP1 = true;
            }
            return true;
        }

        private bool HasCreatedShortcut()
        {
            return this.CreateDesktopShortcut;
        }

        private bool HasErrorUrl()
        {
            if (string.IsNullOrEmpty(this.ErrorReportUrl))
            {
                return false;
            }
            return true;
        }

        private static bool HasExcludedFileOrSP1File(ITaskItem[] candidateFiles)
        {
            if (candidateFiles != null)
            {
                foreach (ITaskItem item in candidateFiles)
                {
                    if (IsExcludedFileOrSP1File(item))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool HasSuiteName()
        {
            return !string.IsNullOrEmpty(this.SuiteName);
        }

        private static bool IsExcludedFileOrSP1File(ITaskItem candidateFile)
        {
            if ((candidateFile == null) || ((!string.Equals(candidateFile.GetMetadata("IncludeHash"), "false", StringComparison.OrdinalIgnoreCase) && !string.Equals(candidateFile.ItemSpec, Constants.NET35SP1AssemblyIdentity[0], StringComparison.OrdinalIgnoreCase)) && !string.Equals(candidateFile.ItemSpec, Constants.NET35ClientAssemblyIdentity[0], StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            return true;
        }

        private bool UncheckedSigning()
        {
            return !this.SigningManifests;
        }

        public ITaskItem[] Assemblies
        {
            get
            {
                return this.assemblies;
            }
            set
            {
                this.assemblies = value;
            }
        }

        public bool CreateDesktopShortcut
        {
            get
            {
                if (!this.createDesktopShortcut.HasValue)
                {
                    return false;
                }
                if (CompareFrameworkVersions(this.TargetFrameworkVersion, "v3.5") < 0)
                {
                    return false;
                }
                return this.createDesktopShortcut.Value;
            }
            set
            {
                this.createDesktopShortcut = new bool?(value);
            }
        }

        public ITaskItem DeploymentManifestEntryPoint
        {
            get
            {
                return this.deploymentManifestEntryPoint;
            }
            set
            {
                this.deploymentManifestEntryPoint = value;
            }
        }

        public ITaskItem EntryPoint
        {
            get
            {
                return this.entryPoint;
            }
            set
            {
                this.entryPoint = value;
            }
        }

        public string ErrorReportUrl
        {
            get
            {
                return this.errorReportUrl;
            }
            set
            {
                this.errorReportUrl = value;
            }
        }

        public ITaskItem[] Files
        {
            get
            {
                return this.files;
            }
            set
            {
                this.files = value;
            }
        }

        public ITaskItem[] ReferencedAssemblies
        {
            get
            {
                return this.referencedAssemblies;
            }
            set
            {
                this.referencedAssemblies = value;
            }
        }

        [Output]
        public bool RequiresMinimumFramework35SP1
        {
            get
            {
                return this.outputRequiresMinimumFramework35SP1;
            }
            set
            {
                this.outputRequiresMinimumFramework35SP1 = value;
            }
        }

        public bool SigningManifests
        {
            get
            {
                return this.signingManifests;
            }
            set
            {
                this.signingManifests = value;
            }
        }

        public string SuiteName
        {
            get
            {
                return this.suiteName;
            }
            set
            {
                this.suiteName = value;
            }
        }

        public string TargetFrameworkVersion
        {
            get
            {
                if (string.IsNullOrEmpty(this.targetFrameworkVersion))
                {
                    return "v3.5";
                }
                return this.targetFrameworkVersion;
            }
            set
            {
                this.targetFrameworkVersion = value;
            }
        }
    }
}


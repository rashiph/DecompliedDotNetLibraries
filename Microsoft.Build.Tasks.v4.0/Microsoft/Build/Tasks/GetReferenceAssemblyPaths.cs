namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    public class GetReferenceAssemblyPaths : TaskExtension
    {
        private bool bypassFrameworkInstallChecks;
        private static bool? net35SP1SentinelAssemblyFound;
        private static readonly string NET35SP1SentinelAssemblyName = "System.Data.Entity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089, processorArchitecture=MSIL";
        private string rootPath;
        private string targetFrameworkMoniker;
        private IList<string> tfmPaths;
        private IList<string> tfmPathsNoProfile;

        public override bool Execute()
        {
            FrameworkName frameworkmoniker = null;
            FrameworkName name2 = null;
            bool flag = false;
            try
            {
                frameworkmoniker = new FrameworkName(this.TargetFrameworkMoniker);
                flag = !string.IsNullOrEmpty(frameworkmoniker.Profile);
                if (flag)
                {
                    name2 = new FrameworkName(frameworkmoniker.Identifier, frameworkmoniker.Version);
                }
                if ((!this.bypassFrameworkInstallChecks && frameworkmoniker.Identifier.Equals(".NETFramework", StringComparison.OrdinalIgnoreCase)) && (frameworkmoniker.Version.Major < 4))
                {
                    if (!net35SP1SentinelAssemblyFound.HasValue)
                    {
                        AssemblyNameExtension strongName = new AssemblyNameExtension(NET35SP1SentinelAssemblyName);
                        net35SP1SentinelAssemblyFound = new bool?(!string.IsNullOrEmpty(GlobalAssemblyCache.GetLocation(strongName, System.Reflection.ProcessorArchitecture.MSIL, runtimeVersion => "v2.0.50727", new Version("2.0.57027"), false, new Microsoft.Build.Shared.FileExists(Microsoft.Build.Shared.FileUtilities.FileExistsNoThrow), GlobalAssemblyCache.pathFromFusionName, GlobalAssemblyCache.gacEnumerator, false)));
                    }
                    if (!net35SP1SentinelAssemblyFound.Value)
                    {
                        base.Log.LogErrorWithCodeFromResources("GetReferenceAssemblyPaths.NETFX35SP1NotIntstalled", new object[] { this.TargetFrameworkMoniker });
                    }
                }
            }
            catch (ArgumentException exception)
            {
                base.Log.LogErrorWithCodeFromResources("GetReferenceAssemblyPaths.InvalidTargetFrameworkMoniker", new object[] { this.TargetFrameworkMoniker, exception.Message });
                return false;
            }
            try
            {
                this.tfmPaths = this.GetPaths(this.rootPath, frameworkmoniker);
                if ((this.tfmPaths != null) && (this.tfmPaths.Count > 0))
                {
                    this.TargetFrameworkMonikerDisplayName = ToolLocationHelper.GetDisplayNameForTargetFrameworkDirectory(this.tfmPaths[0], frameworkmoniker);
                }
                if (flag && (this.tfmPaths != null))
                {
                    this.tfmPathsNoProfile = this.GetPaths(this.rootPath, name2);
                }
                if (!flag)
                {
                    this.tfmPathsNoProfile = this.tfmPaths;
                }
            }
            catch (Exception exception2)
            {
                base.Log.LogErrorWithCodeFromResources("GetReferenceAssemblyPaths.ProblemGeneratingReferencePaths", new object[] { this.TargetFrameworkMoniker, exception2.Message });
                if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception2))
                {
                    throw;
                }
                this.tfmPathsNoProfile = null;
                this.TargetFrameworkMonikerDisplayName = null;
            }
            return !base.Log.HasLoggedErrors;
        }

        private IList<string> GetPaths(string rootPath, FrameworkName frameworkmoniker)
        {
            IList<string> pathToReferenceAssemblies = null;
            if (string.IsNullOrEmpty(rootPath))
            {
                pathToReferenceAssemblies = ToolLocationHelper.GetPathToReferenceAssemblies(frameworkmoniker);
            }
            else
            {
                pathToReferenceAssemblies = ToolLocationHelper.GetPathToReferenceAssemblies(rootPath, frameworkmoniker);
            }
            if (pathToReferenceAssemblies.Count == 0)
            {
                base.Log.LogWarningWithCodeFromResources("GetReferenceAssemblyPaths.NoReferenceAssemblyDirectoryFound", new object[] { frameworkmoniker.ToString() });
            }
            return pathToReferenceAssemblies;
        }

        public bool BypassFrameworkInstallChecks
        {
            get
            {
                return this.bypassFrameworkInstallChecks;
            }
            set
            {
                this.bypassFrameworkInstallChecks = value;
            }
        }

        [Output]
        public string[] FullFrameworkReferenceAssemblyPaths
        {
            get
            {
                if (this.tfmPathsNoProfile != null)
                {
                    string[] array = new string[this.tfmPathsNoProfile.Count];
                    this.tfmPathsNoProfile.CopyTo(array, 0);
                    return array;
                }
                return new string[0];
            }
        }

        [Output]
        public string[] ReferenceAssemblyPaths
        {
            get
            {
                if (this.tfmPaths != null)
                {
                    string[] array = new string[this.tfmPaths.Count];
                    this.tfmPaths.CopyTo(array, 0);
                    return array;
                }
                return new string[0];
            }
        }

        public string RootPath
        {
            get
            {
                return this.rootPath;
            }
            set
            {
                this.rootPath = value;
            }
        }

        public string TargetFrameworkMoniker
        {
            get
            {
                return this.targetFrameworkMoniker;
            }
            set
            {
                this.targetFrameworkMoniker = value;
            }
        }

        [Output]
        public string TargetFrameworkMonikerDisplayName { get; set; }
    }
}


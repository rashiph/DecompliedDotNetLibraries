namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
    using System;
    using System.IO;
    using System.Runtime.Versioning;

    public sealed class GenerateTrustInfo : TaskExtension
    {
        private ITaskItem[] applicationDependencies;
        private ITaskItem baseManifest;
        private const string Custom = "Custom";
        private string excludedPermissions;
        private string targetFrameworkMoniker;
        private string targetZone;
        private ITaskItem trustInfoFile;

        public override bool Execute()
        {
            TrustInfo info = new TrustInfo {
                IsFullTrust = false
            };
            FrameworkName name = null;
            string str = string.Empty;
            if (!string.IsNullOrEmpty(this.TargetFrameworkMoniker))
            {
                name = new FrameworkName(this.TargetFrameworkMoniker);
                str = name.Version.ToString();
            }
            if ((this.BaseManifest != null) && File.Exists(this.BaseManifest.ItemSpec))
            {
                try
                {
                    info.ReadManifest(this.BaseManifest.ItemSpec);
                }
                catch (Exception exception)
                {
                    base.Log.LogErrorWithCodeFromResources("GenerateManifest.ReadInputManifestFailed", new object[] { this.BaseManifest.ItemSpec, exception.Message });
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(this.ExcludedPermissions))
            {
                base.Log.LogWarningFromResources("GenerateManifest.ExcludedPermissionsNotSupported", new object[0]);
            }
            try
            {
                if ((!string.IsNullOrEmpty(this.targetZone) && (info.PermissionSet != null)) && ((info.PermissionSet.Count > 0) && !string.Equals(this.targetZone, "Custom", StringComparison.OrdinalIgnoreCase)))
                {
                    base.Log.LogErrorFromResources("GenerateManifest.KnownTargetZoneCannotHaveAdditionalPermissionType", new object[0]);
                    return false;
                }
                info.PermissionSet = SecurityUtilities.ComputeZonePermissionSetHelper(this.TargetZone, info.PermissionSet, this.applicationDependencies, this.TargetFrameworkMoniker);
                if (info.PermissionSet == null)
                {
                    base.Log.LogErrorWithCodeFromResources("GenerateManifest.NoPermissionSetForTargetZone", new object[] { str });
                    return false;
                }
            }
            catch (ArgumentNullException)
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.NoPermissionSetForTargetZone", new object[] { str });
                return false;
            }
            catch (ArgumentException exception2)
            {
                if (!string.Equals(exception2.ParamName, "TargetZone", StringComparison.OrdinalIgnoreCase))
                {
                    throw;
                }
                base.Log.LogWarningWithCodeFromResources("GenerateManifest.InvalidItemValue", new object[] { "TargetZone", this.TargetZone });
            }
            info.Write(this.TrustInfoFile.ItemSpec);
            return true;
        }

        private static string[] StringToIdentityList(string s)
        {
            string[] strArray = s.Split(new char[] { ';' });
            for (int i = 0; i < strArray.Length; i++)
            {
                strArray[i] = strArray[i].Trim();
            }
            return strArray;
        }

        public ITaskItem[] ApplicationDependencies
        {
            get
            {
                return this.applicationDependencies;
            }
            set
            {
                this.applicationDependencies = value;
            }
        }

        public ITaskItem BaseManifest
        {
            get
            {
                return this.baseManifest;
            }
            set
            {
                this.baseManifest = value;
            }
        }

        public string ExcludedPermissions
        {
            get
            {
                return this.excludedPermissions;
            }
            set
            {
                this.excludedPermissions = value;
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

        public string TargetZone
        {
            get
            {
                return this.targetZone;
            }
            set
            {
                this.targetZone = value;
            }
        }

        [Required, Output]
        public ITaskItem TrustInfoFile
        {
            get
            {
                return this.trustInfoFile;
            }
            set
            {
                this.trustInfoFile = value;
            }
        }
    }
}


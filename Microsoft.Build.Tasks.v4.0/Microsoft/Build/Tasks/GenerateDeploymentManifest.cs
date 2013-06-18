namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
    using System;
    using System.IO;

    public sealed class GenerateDeploymentManifest : GenerateManifestBase
    {
        private bool? createDesktopShortcut = null;
        private string deploymentUrl;
        private bool? disallowUrlActivation = null;
        private string errorReportUrl;
        private bool? install = null;
        private bool? mapFileExtensions = null;
        private string minimumRequiredVersion;
        private string product;
        private string publisher;
        private string specifiedUpdateMode;
        private string specifiedUpdateUnit;
        private string suiteName;
        private string supportUrl;
        private bool? trustUrlParameters = null;
        private bool? updateEnabled = null;
        private int? updateInterval = null;
        private Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateMode? updateMode = null;
        private Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateUnit? updateUnit = null;

        private bool BuildDeployManifest(DeployManifest manifest)
        {
            if (manifest.EntryPoint == null)
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.NoEntryPoint", new object[0]);
                return false;
            }
            if (this.SupportUrl != null)
            {
                manifest.SupportUrl = this.SupportUrl;
            }
            if (this.DeploymentUrl != null)
            {
                manifest.DeploymentUrl = this.DeploymentUrl;
            }
            if (this.install.HasValue)
            {
                manifest.Install = this.install.Value;
            }
            if (this.updateEnabled.HasValue)
            {
                manifest.UpdateEnabled = this.updateEnabled.Value;
            }
            if (this.updateInterval.HasValue)
            {
                manifest.UpdateInterval = this.updateInterval.Value;
            }
            if (this.updateMode.HasValue)
            {
                manifest.UpdateMode = this.updateMode.Value;
            }
            if (this.updateUnit.HasValue)
            {
                manifest.UpdateUnit = this.updateUnit.Value;
            }
            if (this.MinimumRequiredVersion != null)
            {
                manifest.MinimumRequiredVersion = this.MinimumRequiredVersion;
            }
            if (manifest.Install && this.disallowUrlActivation.HasValue)
            {
                manifest.DisallowUrlActivation = this.disallowUrlActivation.Value;
            }
            if (this.mapFileExtensions.HasValue)
            {
                manifest.MapFileExtensions = this.mapFileExtensions.Value;
            }
            if (this.trustUrlParameters.HasValue)
            {
                manifest.TrustUrlParameters = this.trustUrlParameters.Value;
            }
            if (this.createDesktopShortcut.HasValue)
            {
                manifest.CreateDesktopShortcut = this.CreateDesktopShortcut;
            }
            if (this.SuiteName != null)
            {
                manifest.SuiteName = this.SuiteName;
            }
            if (this.ErrorReportUrl != null)
            {
                manifest.ErrorReportUrl = this.ErrorReportUrl;
            }
            return true;
        }

        private bool BuildResolvedSettings(DeployManifest manifest)
        {
            if (this.Product != null)
            {
                manifest.Product = this.Product;
            }
            else if (string.IsNullOrEmpty(manifest.Product))
            {
                manifest.Product = Path.GetFileNameWithoutExtension(manifest.AssemblyIdentity.Name);
            }
            if (this.Publisher != null)
            {
                manifest.Publisher = this.Publisher;
            }
            else if (string.IsNullOrEmpty(manifest.Publisher))
            {
                string registeredOrganization = Util.GetRegisteredOrganization();
                if (!string.IsNullOrEmpty(registeredOrganization))
                {
                    manifest.Publisher = registeredOrganization;
                }
                else
                {
                    manifest.Publisher = manifest.Product;
                }
            }
            return true;
        }

        protected override Type GetObjectType()
        {
            return typeof(DeployManifest);
        }

        protected override bool OnManifestLoaded(Manifest manifest)
        {
            return this.BuildDeployManifest(manifest as DeployManifest);
        }

        protected override bool OnManifestResolved(Manifest manifest)
        {
            return this.BuildResolvedSettings(manifest as DeployManifest);
        }

        protected internal override bool ValidateInputs()
        {
            bool flag = base.ValidateInputs();
            if (!string.IsNullOrEmpty(this.minimumRequiredVersion) && !Util.IsValidVersion(this.minimumRequiredVersion, 4))
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.InvalidValue", new object[] { "MinimumRequiredVersion" });
                flag = false;
            }
            if (this.specifiedUpdateMode != null)
            {
                try
                {
                    this.updateMode = new Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateMode?((Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateMode) Enum.Parse(typeof(Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateMode), this.specifiedUpdateMode, true));
                }
                catch (FormatException)
                {
                    base.Log.LogErrorWithCodeFromResources("GenerateManifest.InvalidValue", new object[] { "UpdateMode" });
                    flag = false;
                }
                catch (ArgumentException)
                {
                    base.Log.LogErrorWithCodeFromResources("GenerateManifest.InvalidValue", new object[] { "UpdateMode" });
                    flag = false;
                }
            }
            if (this.specifiedUpdateUnit != null)
            {
                try
                {
                    this.updateUnit = new Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateUnit?((Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateUnit) Enum.Parse(typeof(Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateUnit), this.specifiedUpdateUnit, true));
                }
                catch (FormatException)
                {
                    base.Log.LogErrorWithCodeFromResources("GenerateManifest.InvalidValue", new object[] { "UpdateUnit" });
                    flag = false;
                }
                catch (ArgumentException)
                {
                    base.Log.LogErrorWithCodeFromResources("GenerateManifest.InvalidValue", new object[] { "UpdateUnit" });
                    flag = false;
                }
            }
            return flag;
        }

        public bool CreateDesktopShortcut
        {
            get
            {
                if (!this.createDesktopShortcut.HasValue)
                {
                    return false;
                }
                if (Util.CompareFrameworkVersions(base.TargetFrameworkVersion, "v3.5") < 0)
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

        public string DeploymentUrl
        {
            get
            {
                return this.deploymentUrl;
            }
            set
            {
                this.deploymentUrl = value;
            }
        }

        public bool DisallowUrlActivation
        {
            get
            {
                return this.disallowUrlActivation.Value;
            }
            set
            {
                this.disallowUrlActivation = new bool?(value);
            }
        }

        public string ErrorReportUrl
        {
            get
            {
                if (Util.CompareFrameworkVersions(base.TargetFrameworkVersion, "v3.5") < 0)
                {
                    return null;
                }
                return this.errorReportUrl;
            }
            set
            {
                this.errorReportUrl = value;
            }
        }

        public bool Install
        {
            get
            {
                return this.install.Value;
            }
            set
            {
                this.install = new bool?(value);
            }
        }

        public bool MapFileExtensions
        {
            get
            {
                return this.mapFileExtensions.Value;
            }
            set
            {
                this.mapFileExtensions = new bool?(value);
            }
        }

        public string MinimumRequiredVersion
        {
            get
            {
                return this.minimumRequiredVersion;
            }
            set
            {
                this.minimumRequiredVersion = value;
            }
        }

        public string Product
        {
            get
            {
                return this.product;
            }
            set
            {
                this.product = value;
            }
        }

        public string Publisher
        {
            get
            {
                return this.publisher;
            }
            set
            {
                this.publisher = value;
            }
        }

        public string SuiteName
        {
            get
            {
                if (Util.CompareFrameworkVersions(base.TargetFrameworkVersion, "v3.5") < 0)
                {
                    return null;
                }
                return this.suiteName;
            }
            set
            {
                this.suiteName = value;
            }
        }

        public string SupportUrl
        {
            get
            {
                return this.supportUrl;
            }
            set
            {
                this.supportUrl = value;
            }
        }

        public bool TrustUrlParameters
        {
            get
            {
                return this.trustUrlParameters.Value;
            }
            set
            {
                this.trustUrlParameters = new bool?(value);
            }
        }

        public bool UpdateEnabled
        {
            get
            {
                return this.updateEnabled.Value;
            }
            set
            {
                this.updateEnabled = new bool?(value);
            }
        }

        public int UpdateInterval
        {
            get
            {
                return this.updateInterval.Value;
            }
            set
            {
                this.updateInterval = new int?(value);
            }
        }

        public string UpdateMode
        {
            get
            {
                return this.specifiedUpdateMode;
            }
            set
            {
                this.specifiedUpdateMode = value;
            }
        }

        public string UpdateUnit
        {
            get
            {
                return this.specifiedUpdateUnit;
            }
            set
            {
                this.specifiedUpdateUnit = value;
            }
        }
    }
}


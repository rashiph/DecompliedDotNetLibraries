namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Xml;
    using System.Xml.Serialization;

    [ComVisible(false), XmlRoot("DeployManifest")]
    public sealed class DeployManifest : Manifest
    {
        private const string _redistListFile = "FrameworkList.xml";
        private const string _redistListFolder = "RedistList";
        private CompatibleFrameworkCollection compatibleFrameworkList;
        private List<CompatibleFramework> compatibleFrameworks;
        private string createDesktopShortcut;
        private string deploymentUrl;
        private string disallowUrlActivation;
        private AssemblyReference entryPoint;
        private string errorReportUrl;
        private string install;
        private string mapFileExtensions;
        private string minimumRequiredVersion;
        private string product;
        private string publisher;
        private string suiteName;
        private string supportUrl;
        private string targetFrameworkMoniker;
        private string trustUrlParameters;
        private string updateEnabled;
        private string updateInterval;
        private string updateMode;
        private string updateUnit;

        public DeployManifest()
        {
            this.install = "true";
            this.updateInterval = "0";
            this.updateUnit = "days";
            this.compatibleFrameworks = new List<CompatibleFramework>();
        }

        public DeployManifest(string targetFrameworkMoniker)
        {
            this.install = "true";
            this.updateInterval = "0";
            this.updateUnit = "days";
            this.compatibleFrameworks = new List<CompatibleFramework>();
            this.DiscoverCompatFrameworks(targetFrameworkMoniker);
        }

        private void DiscoverCompatFrameworks(string moniker)
        {
            if (!string.IsNullOrEmpty(moniker))
            {
                FrameworkName frameworkName = new FrameworkName(moniker);
                if (frameworkName.Version.Major >= 4)
                {
                    this.compatibleFrameworks.Clear();
                    this.DiscoverCompatibleFrameworks(frameworkName);
                }
            }
        }

        private void DiscoverCompatibleFrameworks(FrameworkName frameworkName)
        {
            FrameworkName installableFrameworkName = this.GetInstallableFrameworkName(frameworkName);
            if (string.IsNullOrEmpty(installableFrameworkName.Profile))
            {
                this.compatibleFrameworks.Add(this.GetFullCompatFramework(installableFrameworkName));
            }
            else
            {
                this.compatibleFrameworks.Add(this.GetSubsetCompatFramework(installableFrameworkName));
                this.compatibleFrameworks.Add(this.GetFullCompatFramework(installableFrameworkName));
            }
        }

        private CompatibleFramework GetFullCompatFramework(FrameworkName frameworkName)
        {
            return new CompatibleFramework { Version = frameworkName.Version.ToString(), SupportedRuntime = this.PatchCLRVersion(Util.GetClrVersion(frameworkName.Version.ToString())), Profile = "Full" };
        }

        private string GetInstallableFramework(string redistListFilePath)
        {
            string str = null;
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(redistListFilePath);
                XmlNode documentElement = document.DocumentElement;
                if (documentElement != null)
                {
                    XmlAttribute attribute = documentElement.Attributes["InstallableFramework"];
                    if ((attribute != null) && !string.IsNullOrEmpty(attribute.Value))
                    {
                        str = attribute.Value;
                    }
                }
            }
            catch (Exception)
            {
            }
            return str;
        }

        private FrameworkName GetInstallableFrameworkName(FrameworkName frameworkName)
        {
            string installableFramework = null;
            IList<string> pathToReferenceAssemblies = this.GetPathToReferenceAssemblies(frameworkName);
            if ((pathToReferenceAssemblies != null) && (pathToReferenceAssemblies.Count > 0))
            {
                string referenceAssemblyPath = pathToReferenceAssemblies[0];
                string redistListFilePath = this.GetRedistListFilePath(referenceAssemblyPath);
                if (File.Exists(redistListFilePath))
                {
                    installableFramework = this.GetInstallableFramework(redistListFilePath);
                }
            }
            if (installableFramework == null)
            {
                return frameworkName;
            }
            try
            {
                return new FrameworkName(installableFramework);
            }
            catch (ArgumentException)
            {
                return frameworkName;
            }
        }

        private IList<string> GetPathToReferenceAssemblies(FrameworkName targetFrameworkMoniker)
        {
            IList<string> pathToReferenceAssemblies = null;
            try
            {
                pathToReferenceAssemblies = ToolLocationHelper.GetPathToReferenceAssemblies(targetFrameworkMoniker);
            }
            catch (InvalidOperationException)
            {
            }
            return pathToReferenceAssemblies;
        }

        private string GetRedistListFilePath(string referenceAssemblyPath)
        {
            return Path.Combine(Path.Combine(referenceAssemblyPath, "RedistList"), "FrameworkList.xml");
        }

        private CompatibleFramework GetSubsetCompatFramework(FrameworkName frameworkName)
        {
            CompatibleFramework fullCompatFramework = this.GetFullCompatFramework(frameworkName);
            fullCompatFramework.Profile = frameworkName.Profile;
            return fullCompatFramework;
        }

        internal override void OnAfterLoad()
        {
            base.OnAfterLoad();
            if (((this.entryPoint == null) && (base.AssemblyReferences != null)) && (base.AssemblyReferences.Count > 0))
            {
                this.entryPoint = base.AssemblyReferences[0];
                this.entryPoint.ReferenceType = AssemblyReferenceType.ClickOnceManifest;
            }
        }

        internal override void OnBeforeSave()
        {
            base.OnBeforeSave();
            if ((base.AssemblyIdentity != null) && string.IsNullOrEmpty(base.AssemblyIdentity.PublicKeyToken))
            {
                base.AssemblyIdentity.PublicKeyToken = "0000000000000000";
            }
        }

        private string PatchCLRVersion(string version)
        {
            try
            {
                Version version2 = new Version(version);
                Version version3 = new Version(version2.Major, version2.Minor, version2.Build);
                return version3.ToString();
            }
            catch (ArgumentException)
            {
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }
            return version;
        }

        public override void Validate()
        {
            base.Validate();
            this.ValidateDeploymentProvider();
            this.ValidateMinimumRequiredVersion();
            base.ValidatePlatform();
            this.ValidateEntryPoint();
        }

        private void ValidateDeploymentProvider()
        {
            if (!string.IsNullOrEmpty(this.deploymentUrl) && PathUtil.IsLocalPath(this.deploymentUrl))
            {
                base.OutputMessages.AddWarningMessage("GenerateManifest.InvalidDeploymentProvider", new string[0]);
            }
        }

        private void ValidateEntryPoint()
        {
            if (this.entryPoint != null)
            {
                if (!string.IsNullOrEmpty(this.entryPoint.TargetPath) && !this.entryPoint.TargetPath.EndsWith(".manifest", StringComparison.OrdinalIgnoreCase))
                {
                    base.OutputMessages.AddErrorMessage("GenerateManifest.InvalidEntryPoint", new string[] { this.entryPoint.ToString() });
                }
                string resolvedPath = this.entryPoint.ResolvedPath;
                if (resolvedPath == null)
                {
                    resolvedPath = Path.Combine(Path.GetDirectoryName(base.SourcePath), this.entryPoint.TargetPath);
                }
                if (File.Exists(resolvedPath))
                {
                    ApplicationManifest manifest = ManifestReader.ReadManifest(resolvedPath, false) as ApplicationManifest;
                    if (manifest != null)
                    {
                        if (this.Install)
                        {
                            if (manifest.HostInBrowser)
                            {
                                base.OutputMessages.AddErrorMessage("GenerateManifest.HostInBrowserNotOnlineOnly", new string[0]);
                            }
                        }
                        else if ((manifest.FileAssociations != null) && (manifest.FileAssociations.Count > 0))
                        {
                            base.OutputMessages.AddErrorMessage("GenerateManifest.FileAssociationsNotInstalled", new string[0]);
                        }
                    }
                }
            }
        }

        private void ValidateMinimumRequiredVersion()
        {
            if (!string.IsNullOrEmpty(this.minimumRequiredVersion))
            {
                Version version = new Version(this.minimumRequiredVersion);
                Version version2 = new Version(base.AssemblyIdentity.Version);
                if (version > version2)
                {
                    base.OutputMessages.AddErrorMessage("GenerateManifest.GreaterMinimumRequiredVersion", new string[0]);
                }
            }
        }

        [XmlIgnore]
        public CompatibleFrameworkCollection CompatibleFrameworks
        {
            get
            {
                if ((this.compatibleFrameworkList == null) && (this.compatibleFrameworks != null))
                {
                    this.compatibleFrameworkList = new CompatibleFrameworkCollection(this.compatibleFrameworks.ToArray());
                }
                return this.compatibleFrameworkList;
            }
        }

        [XmlIgnore]
        public bool CreateDesktopShortcut
        {
            get
            {
                return ConvertUtil.ToBoolean(this.createDesktopShortcut);
            }
            set
            {
                this.createDesktopShortcut = value ? "true" : null;
            }
        }

        [XmlIgnore]
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

        [XmlIgnore]
        public bool DisallowUrlActivation
        {
            get
            {
                return ConvertUtil.ToBoolean(this.disallowUrlActivation);
            }
            set
            {
                this.disallowUrlActivation = value ? "true" : null;
            }
        }

        [XmlIgnore]
        public override AssemblyReference EntryPoint
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

        [XmlIgnore]
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

        [XmlIgnore]
        public bool Install
        {
            get
            {
                return ConvertUtil.ToBoolean(this.install);
            }
            set
            {
                this.install = Convert.ToString(value, CultureInfo.InvariantCulture);
            }
        }

        [XmlIgnore]
        public bool MapFileExtensions
        {
            get
            {
                return ConvertUtil.ToBoolean(this.mapFileExtensions);
            }
            set
            {
                this.mapFileExtensions = value ? "true" : null;
            }
        }

        [XmlIgnore]
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

        [XmlIgnore]
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

        [XmlIgnore]
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

        [XmlIgnore]
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

        [XmlIgnore]
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

        [XmlIgnore]
        public string TargetFrameworkMoniker
        {
            get
            {
                return this.targetFrameworkMoniker;
            }
            set
            {
                this.targetFrameworkMoniker = value;
                this.DiscoverCompatFrameworks(this.targetFrameworkMoniker);
            }
        }

        [XmlIgnore]
        public bool TrustUrlParameters
        {
            get
            {
                return ConvertUtil.ToBoolean(this.trustUrlParameters);
            }
            set
            {
                this.trustUrlParameters = value ? "true" : null;
            }
        }

        [XmlIgnore]
        public bool UpdateEnabled
        {
            get
            {
                return ConvertUtil.ToBoolean(this.updateEnabled);
            }
            set
            {
                this.updateEnabled = Convert.ToString(value, CultureInfo.InvariantCulture);
            }
        }

        [XmlIgnore]
        public int UpdateInterval
        {
            get
            {
                try
                {
                    return Convert.ToInt32(this.updateInterval, CultureInfo.InvariantCulture);
                }
                catch (ArgumentException)
                {
                    return 1;
                }
                catch (FormatException)
                {
                    return 1;
                }
            }
            set
            {
                this.updateInterval = Convert.ToString(value, CultureInfo.InvariantCulture);
            }
        }

        [XmlIgnore]
        public Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateMode UpdateMode
        {
            get
            {
                try
                {
                    return (Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateMode) Enum.Parse(typeof(Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateMode), this.updateMode, true);
                }
                catch (FormatException)
                {
                    return Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateMode.Foreground;
                }
                catch (ArgumentException)
                {
                    return Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateMode.Foreground;
                }
            }
            set
            {
                this.updateMode = value.ToString();
            }
        }

        [XmlIgnore]
        public Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateUnit UpdateUnit
        {
            get
            {
                try
                {
                    return (Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateUnit) Enum.Parse(typeof(Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateUnit), this.updateUnit, true);
                }
                catch (FormatException)
                {
                    return Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateUnit.Days;
                }
                catch (ArgumentException)
                {
                    return Microsoft.Build.Tasks.Deployment.ManifestUtilities.UpdateUnit.Days;
                }
            }
            set
            {
                this.updateUnit = value.ToString();
            }
        }

        [XmlArray("CompatibleFrameworks"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public CompatibleFramework[] XmlCompatibleFrameworks
        {
            get
            {
                if (this.compatibleFrameworks.Count <= 0)
                {
                    return null;
                }
                return this.compatibleFrameworks.ToArray();
            }
            set
            {
                this.compatibleFrameworks = new List<CompatibleFramework>(value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("CreateDesktopShortcut")]
        public string XmlCreateDesktopShortcut
        {
            get
            {
                if (this.createDesktopShortcut == null)
                {
                    return null;
                }
                return this.createDesktopShortcut.ToLowerInvariant();
            }
            set
            {
                this.createDesktopShortcut = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("DeploymentUrl"), Browsable(false)]
        public string XmlDeploymentUrl
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

        [XmlAttribute("DisallowUrlActivation"), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public string XmlDisallowUrlActivation
        {
            get
            {
                if (this.disallowUrlActivation == null)
                {
                    return null;
                }
                return this.disallowUrlActivation.ToLowerInvariant();
            }
            set
            {
                this.disallowUrlActivation = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("ErrorReportUrl")]
        public string XmlErrorReportUrl
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

        [XmlAttribute("Install"), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public string XmlInstall
        {
            get
            {
                if (string.IsNullOrEmpty(this.install))
                {
                    return "true";
                }
                return this.install.ToLower(CultureInfo.InvariantCulture);
            }
            set
            {
                this.install = value;
            }
        }

        [XmlAttribute("MapFileExtensions"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlMapFileExtensions
        {
            get
            {
                if (this.mapFileExtensions == null)
                {
                    return null;
                }
                return this.mapFileExtensions.ToLowerInvariant();
            }
            set
            {
                this.mapFileExtensions = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("MinimumRequiredVersion")]
        public string XmlMinimumRequiredVersion
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

        [XmlAttribute("Product"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlProduct
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

        [XmlAttribute("Publisher"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlPublisher
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

        [Browsable(false), XmlAttribute("SuiteName"), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlSuiteName
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

        [Browsable(false), XmlAttribute("SupportUrl"), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlSupportUrl
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

        [XmlAttribute("TrustUrlParameters"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlTrustUrlParameters
        {
            get
            {
                if (this.trustUrlParameters == null)
                {
                    return null;
                }
                return this.trustUrlParameters.ToLowerInvariant();
            }
            set
            {
                this.trustUrlParameters = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), XmlAttribute("UpdateEnabled")]
        public string XmlUpdateEnabled
        {
            get
            {
                if (this.updateEnabled == null)
                {
                    return null;
                }
                return this.updateEnabled.ToLower(CultureInfo.InvariantCulture);
            }
            set
            {
                this.updateEnabled = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), XmlAttribute("UpdateInterval")]
        public string XmlUpdateInterval
        {
            get
            {
                return this.updateInterval;
            }
            set
            {
                this.updateInterval = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("UpdateMode")]
        public string XmlUpdateMode
        {
            get
            {
                return this.updateMode;
            }
            set
            {
                this.updateMode = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), XmlAttribute("UpdateUnit")]
        public string XmlUpdateUnit
        {
            get
            {
                if (this.updateUnit == null)
                {
                    return null;
                }
                return this.updateUnit.ToLower(CultureInfo.InvariantCulture);
            }
            set
            {
                this.updateUnit = value;
            }
        }
    }
}


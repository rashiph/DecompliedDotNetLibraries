namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml;

    public sealed class GenerateApplicationManifest : GenerateManifestBase
    {
        private string clrVersion;
        private ITaskItem configFile;
        private ITaskItem[] dependencies;
        private string errorReportUrl;
        private ITaskItem[] fileAssociations;
        private ITaskItem[] files;
        private bool hostInBrowser;
        private ITaskItem iconFile;
        private ITaskItem[] isolatedComReferences;
        private _ManifestType manifestType = _ManifestType.ClickOnce;
        private string osVersion;
        private string product;
        private string publisher;
        private bool requiresMinimumFramework35SP1;
        private string specifiedManifestType;
        private string suiteName;
        private string supportUrl;
        private string targetFrameworkProfile = string.Empty;
        private string targetFrameworkSubset = string.Empty;
        private ITaskItem trustInfoFile;
        private bool useApplicationTrust;

        private bool AddClickOnceFileAssociations(ApplicationManifest manifest)
        {
            if (this.FileAssociations != null)
            {
                foreach (ITaskItem item in this.FileAssociations)
                {
                    FileAssociation fileAssociation = new FileAssociation {
                        DefaultIcon = item.GetMetadata("DefaultIcon"),
                        Description = item.GetMetadata("Description"),
                        Extension = item.ItemSpec,
                        ProgId = item.GetMetadata("Progid")
                    };
                    manifest.FileAssociations.Add(fileAssociation);
                }
            }
            return true;
        }

        private bool AddClickOnceFiles(ApplicationManifest manifest)
        {
            int tickCount = Environment.TickCount;
            if ((this.ConfigFile != null) && !string.IsNullOrEmpty(this.ConfigFile.ItemSpec))
            {
                manifest.ConfigFile = base.FindFileFromItem(this.ConfigFile).TargetPath;
            }
            if ((this.IconFile != null) && !string.IsNullOrEmpty(this.IconFile.ItemSpec))
            {
                manifest.IconFile = base.FindFileFromItem(this.IconFile).TargetPath;
            }
            if ((this.TrustInfoFile != null) && !string.IsNullOrEmpty(this.TrustInfoFile.ItemSpec))
            {
                manifest.TrustInfo = new TrustInfo();
                manifest.TrustInfo.Read(this.TrustInfoFile.ItemSpec);
            }
            if (manifest.TrustInfo == null)
            {
                manifest.TrustInfo = new TrustInfo();
            }
            if (this.OSVersion != null)
            {
                manifest.OSVersion = this.osVersion;
            }
            if (this.ClrVersion != null)
            {
                AssemblyReference assembly = manifest.AssemblyReferences.Find("Microsoft.Windows.CommonLanguageRuntime");
                if (assembly == null)
                {
                    assembly = new AssemblyReference {
                        IsPrerequisite = true
                    };
                    manifest.AssemblyReferences.Add(assembly);
                }
                assembly.AssemblyIdentity = new AssemblyIdentity("Microsoft.Windows.CommonLanguageRuntime", this.ClrVersion);
            }
            if (Util.CompareFrameworkVersions(base.TargetFrameworkVersion, "v3.0") == 0)
            {
                this.EnsureAssemblyReferenceExists(manifest, this.CreateAssemblyIdentity(Constants.NET30AssemblyIdentity));
            }
            else if (Util.CompareFrameworkVersions(base.TargetFrameworkVersion, "v3.5") == 0)
            {
                this.EnsureAssemblyReferenceExists(manifest, this.CreateAssemblyIdentity(Constants.NET30AssemblyIdentity));
                this.EnsureAssemblyReferenceExists(manifest, this.CreateAssemblyIdentity(Constants.NET35AssemblyIdentity));
                if ((!string.IsNullOrEmpty(this.TargetFrameworkSubset) && this.TargetFrameworkSubset.Equals("Client", StringComparison.OrdinalIgnoreCase)) || (!string.IsNullOrEmpty(this.TargetFrameworkProfile) && this.TargetFrameworkProfile.Equals("Client", StringComparison.OrdinalIgnoreCase)))
                {
                    this.EnsureAssemblyReferenceExists(manifest, this.CreateAssemblyIdentity(Constants.NET35ClientAssemblyIdentity));
                }
                else if (this.RequiresMinimumFramework35SP1)
                {
                    this.EnsureAssemblyReferenceExists(manifest, this.CreateAssemblyIdentity(Constants.NET35SP1AssemblyIdentity));
                }
            }
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "GenerateApplicationManifest.AddClickOnceFiles t={0}", new object[] { Environment.TickCount - tickCount }));
            return true;
        }

        private bool AddIsolatedComReferences(ApplicationManifest manifest)
        {
            int tickCount = Environment.TickCount;
            bool flag = true;
            if (this.IsolatedComReferences != null)
            {
                foreach (ITaskItem item in this.IsolatedComReferences)
                {
                    string metadata = item.GetMetadata("Name");
                    if (string.IsNullOrEmpty(metadata))
                    {
                        metadata = Path.GetFileName(item.ItemSpec);
                    }
                    if (!base.AddFileFromItem(item).ImportComComponent(item.ItemSpec, manifest.OutputMessages, metadata))
                    {
                        flag = false;
                    }
                }
            }
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "GenerateApplicationManifest.AddIsolatedComReferences t={0}", new object[] { Environment.TickCount - tickCount }));
            return flag;
        }

        private bool BuildApplicationManifest(ApplicationManifest manifest)
        {
            if (this.Dependencies != null)
            {
                foreach (ITaskItem item in this.Dependencies)
                {
                    base.AddAssemblyFromItem(item);
                }
            }
            if (this.Files != null)
            {
                foreach (ITaskItem item2 in this.Files)
                {
                    base.AddFileFromItem(item2);
                }
            }
            manifest.IsClickOnceManifest = this.manifestType == _ManifestType.ClickOnce;
            if (manifest.IsClickOnceManifest)
            {
                if ((manifest.EntryPoint == null) && (Util.CompareFrameworkVersions(base.TargetFrameworkVersion, "v3.5") < 0))
                {
                    base.Log.LogErrorWithCodeFromResources("GenerateManifest.NoEntryPoint", new object[0]);
                    return false;
                }
                if (!this.AddClickOnceFiles(manifest))
                {
                    return false;
                }
                if (!this.AddClickOnceFileAssociations(manifest))
                {
                    return false;
                }
            }
            if (this.HostInBrowser && (Util.CompareFrameworkVersions(base.TargetFrameworkVersion, "v3.0") < 0))
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.HostInBrowserInvalidFrameworkVersion", new object[0]);
                return false;
            }
            if (!this.AddIsolatedComReferences(manifest))
            {
                return false;
            }
            manifest.MaxTargetPath = base.MaxTargetPath;
            manifest.HostInBrowser = this.HostInBrowser;
            manifest.UseApplicationTrust = this.UseApplicationTrust;
            if (this.UseApplicationTrust && (this.SupportUrl != null))
            {
                manifest.SupportUrl = this.SupportUrl;
            }
            if (this.UseApplicationTrust && (this.SuiteName != null))
            {
                manifest.SuiteName = this.SuiteName;
            }
            if (this.UseApplicationTrust && (this.ErrorReportUrl != null))
            {
                manifest.ErrorReportUrl = this.ErrorReportUrl;
            }
            return true;
        }

        private bool BuildResolvedSettings(ApplicationManifest manifest)
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

        private AssemblyIdentity CreateAssemblyIdentity(string[] values)
        {
            if (values.Length != 5)
            {
                return null;
            }
            return new AssemblyIdentity(values[0], values[1], values[2], values[3], values[4]);
        }

        private void EnsureAssemblyReferenceExists(ApplicationManifest manifest, AssemblyIdentity identity)
        {
            if (manifest.AssemblyReferences.Find(identity) == null)
            {
                AssemblyReference assembly = new AssemblyReference {
                    IsPrerequisite = true,
                    AssemblyIdentity = identity
                };
                manifest.AssemblyReferences.Add(assembly);
            }
        }

        protected override Type GetObjectType()
        {
            return typeof(ApplicationManifest);
        }

        private bool GetRequestedExecutionLevel(out string requestedExecutionLevel)
        {
            bool flag;
            requestedExecutionLevel = "asInvoker";
            if (((base.InputManifest == null) || string.IsNullOrEmpty(base.InputManifest.ItemSpec)) || (string.CompareOrdinal(base.InputManifest.ItemSpec, "NoManifest") == 0))
            {
                return false;
            }
            try
            {
                using (Stream stream = File.Open(base.InputManifest.ItemSpec, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(stream);
                    XmlNamespaceManager namespaceManager = XmlNamespaces.GetNamespaceManager(document.NameTable);
                    XmlNode node = (XmlElement) document.SelectSingleNode("/asmv1:assembly/asmv2:trustInfo/asmv2:security/asmv3:requestedPrivileges/asmv3:requestedExecutionLevel", namespaceManager);
                    if (node != null)
                    {
                        XmlAttribute namedItem = (XmlAttribute) node.Attributes.GetNamedItem("level");
                        if (namedItem != null)
                        {
                            requestedExecutionLevel = namedItem.Value;
                        }
                    }
                    flag = true;
                }
            }
            catch (Exception exception)
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.ReadInputManifestFailed", new object[] { base.InputManifest.ItemSpec, exception.Message });
                flag = false;
            }
            return flag;
        }

        protected override bool OnManifestLoaded(Manifest manifest)
        {
            return this.BuildApplicationManifest(manifest as ApplicationManifest);
        }

        protected override bool OnManifestResolved(Manifest manifest)
        {
            if (this.UseApplicationTrust)
            {
                return this.BuildResolvedSettings(manifest as ApplicationManifest);
            }
            return true;
        }

        protected internal override bool ValidateInputs()
        {
            string str;
            bool flag = base.ValidateInputs();
            if (this.specifiedManifestType != null)
            {
                try
                {
                    this.manifestType = (_ManifestType) Enum.Parse(typeof(_ManifestType), this.specifiedManifestType, true);
                }
                catch (FormatException)
                {
                    base.Log.LogErrorWithCodeFromResources("GenerateManifest.InvalidValue", new object[] { "ManifestType" });
                    flag = false;
                }
                catch (ArgumentException)
                {
                    base.Log.LogErrorWithCodeFromResources("GenerateManifest.InvalidValue", new object[] { "ManifestType" });
                    flag = false;
                }
                if (this.manifestType == _ManifestType.Native)
                {
                    base.EntryPoint = null;
                }
            }
            if ((this.ClrVersion != null) && !Util.IsValidVersion(this.ClrVersion, 4))
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.InvalidValue", new object[] { "ClrVersion" });
                flag = false;
            }
            if ((this.OSVersion != null) && !Util.IsValidVersion(this.OSVersion, 4))
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.InvalidValue", new object[] { "OSVersion" });
                flag = false;
            }
            if (!Util.IsValidFrameworkVersion(base.TargetFrameworkVersion) || (Util.CompareFrameworkVersions(base.TargetFrameworkVersion, "v2.0") < 0))
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.InvalidValue", new object[] { "TargetFrameworkVersion" });
                flag = false;
            }
            if (((this.manifestType == _ManifestType.ClickOnce) && this.GetRequestedExecutionLevel(out str)) && (string.CompareOrdinal(str, "asInvoker") != 0))
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.InvalidRequestedExecutionLevel", new object[] { str });
                flag = false;
            }
            return flag;
        }

        public string ClrVersion
        {
            get
            {
                return this.clrVersion;
            }
            set
            {
                this.clrVersion = value;
            }
        }

        public ITaskItem ConfigFile
        {
            get
            {
                return this.configFile;
            }
            set
            {
                this.configFile = value;
            }
        }

        public ITaskItem[] Dependencies
        {
            get
            {
                return this.dependencies;
            }
            set
            {
                this.dependencies = Util.SortItems(value);
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

        public ITaskItem[] FileAssociations
        {
            get
            {
                if (Util.CompareFrameworkVersions(base.TargetFrameworkVersion, "v3.5") < 0)
                {
                    return null;
                }
                return this.fileAssociations;
            }
            set
            {
                this.fileAssociations = value;
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
                this.files = Util.SortItems(value);
            }
        }

        public bool HostInBrowser
        {
            get
            {
                return this.hostInBrowser;
            }
            set
            {
                this.hostInBrowser = value;
            }
        }

        public ITaskItem IconFile
        {
            get
            {
                return this.iconFile;
            }
            set
            {
                this.iconFile = value;
            }
        }

        public ITaskItem[] IsolatedComReferences
        {
            get
            {
                return this.isolatedComReferences;
            }
            set
            {
                this.isolatedComReferences = Util.SortItems(value);
            }
        }

        public string ManifestType
        {
            get
            {
                return this.specifiedManifestType;
            }
            set
            {
                this.specifiedManifestType = value;
            }
        }

        public string OSVersion
        {
            get
            {
                return this.osVersion;
            }
            set
            {
                this.osVersion = value;
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

        public bool RequiresMinimumFramework35SP1
        {
            get
            {
                return this.requiresMinimumFramework35SP1;
            }
            set
            {
                this.requiresMinimumFramework35SP1 = value;
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

        public string TargetFrameworkProfile
        {
            get
            {
                return this.targetFrameworkProfile;
            }
            set
            {
                this.targetFrameworkProfile = value;
            }
        }

        public string TargetFrameworkSubset
        {
            get
            {
                return this.targetFrameworkSubset;
            }
            set
            {
                this.targetFrameworkSubset = value;
            }
        }

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

        public bool UseApplicationTrust
        {
            get
            {
                if (Util.CompareFrameworkVersions(base.TargetFrameworkVersion, "v3.5") < 0)
                {
                    return false;
                }
                return this.useApplicationTrust;
            }
            set
            {
                this.useApplicationTrust = value;
            }
        }

        private enum _ManifestType
        {
            Native,
            ClickOnce
        }
    }
}


namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Serialization;

    [XmlRoot("ApplicationManifest"), ComVisible(false)]
    public sealed class ApplicationManifest : AssemblyManifest
    {
        private string configFile;
        private AssemblyReference entryPoint;
        private AssemblyIdentity entryPointIdentity;
        private string entryPointParameters;
        private string entryPointPath;
        private string errorReportUrl;
        private FileAssociationCollection fileAssociationList;
        private FileAssociation[] fileAssociations;
        private bool hostInBrowser;
        private string iconFile;
        private bool isClickOnceManifest;
        private int maxTargetPath;
        private string oSBuild;
        private string oSDescription;
        private string oSMajor;
        private string oSMinor;
        private string oSRevision;
        private string oSSupportUrl;
        private string product;
        private string publisher;
        private string suiteName;
        private string supportUrl;
        private string targetFrameworkVersion;
        private Microsoft.Build.Tasks.Deployment.ManifestUtilities.TrustInfo trustInfo;
        private bool useApplicationTrust;

        public ApplicationManifest()
        {
            this.isClickOnceManifest = true;
        }

        public ApplicationManifest(string targetFrameworkVersion)
        {
            this.isClickOnceManifest = true;
            this.targetFrameworkVersion = targetFrameworkVersion;
        }

        private void FixupClrVersion()
        {
            AssemblyReference assembly = base.AssemblyReferences.Find("Microsoft.Windows.CommonLanguageRuntime");
            if (assembly == null)
            {
                assembly = new AssemblyReference {
                    IsPrerequisite = true
                };
                base.AssemblyReferences.Add(assembly);
            }
            if ((assembly.AssemblyIdentity == null) || string.IsNullOrEmpty(assembly.AssemblyIdentity.Version))
            {
                assembly.AssemblyIdentity = new AssemblyIdentity("Microsoft.Windows.CommonLanguageRuntime", Util.GetClrVersion(this.targetFrameworkVersion));
            }
        }

        private void FixupEntryPoint()
        {
            if (this.entryPoint == null)
            {
                this.entryPoint = base.AssemblyReferences.Find(this.entryPointIdentity);
            }
        }

        internal override void OnBeforeSave()
        {
            this.FixupEntryPoint();
            if (this.isClickOnceManifest)
            {
                this.FixupClrVersion();
            }
            base.OnBeforeSave();
            if ((this.isClickOnceManifest && (base.AssemblyIdentity != null)) && string.IsNullOrEmpty(base.AssemblyIdentity.PublicKeyToken))
            {
                base.AssemblyIdentity.PublicKeyToken = "0000000000000000";
            }
            this.UpdateEntryPoint();
            base.AssemblyIdentity.Type = "win32";
            if (string.IsNullOrEmpty(this.OSVersion))
            {
                if (!this.WinXPRequired)
                {
                    this.OSVersion = "4.10.0.0";
                }
                else
                {
                    this.OSVersion = "5.1.2600.0";
                }
            }
            if (this.fileAssociationList != null)
            {
                this.fileAssociations = this.fileAssociationList.ToArray();
            }
        }

        private void UpdateEntryPoint()
        {
            if (this.entryPoint != null)
            {
                this.entryPointIdentity = new AssemblyIdentity(this.entryPoint.AssemblyIdentity);
                this.entryPointPath = this.entryPoint.TargetPath;
            }
            else
            {
                this.entryPointIdentity = null;
                this.entryPointPath = null;
            }
        }

        public override void Validate()
        {
            base.Validate();
            if (this.isClickOnceManifest)
            {
                this.ValidateReferencesForClickOnceApplication();
                base.ValidatePlatform();
                this.ValidateConfig();
                this.ValidateEntryPoint();
                this.ValidateFileAssociations();
            }
            else
            {
                this.ValidateReferencesForNativeApplication();
            }
            this.ValidateCom();
        }

        private void ValidateCom()
        {
            int tickCount = Environment.TickCount;
            string fileName = Path.GetFileName(base.SourcePath);
            Dictionary<string, ComInfo> dictionary = new Dictionary<string, ComInfo>();
            Dictionary<string, ComInfo> dictionary2 = new Dictionary<string, ComInfo>();
            foreach (AssemblyReference reference in base.AssemblyReferences)
            {
                if (((reference.ReferenceType == AssemblyReferenceType.NativeAssembly) && !reference.IsPrerequisite) && !string.IsNullOrEmpty(reference.ResolvedPath))
                {
                    ComInfo[] comInfo = ManifestReader.GetComInfo(reference.ResolvedPath);
                    if (comInfo != null)
                    {
                        foreach (ComInfo info in comInfo)
                        {
                            if (!string.IsNullOrEmpty(info.ClsId))
                            {
                                string key = info.ClsId.ToLowerInvariant();
                                if (!dictionary.ContainsKey(key))
                                {
                                    dictionary.Add(key, info);
                                }
                                else
                                {
                                    base.OutputMessages.AddErrorMessage("GenerateManifest.DuplicateComDefinition", new string[] { "clsid", info.ComponentFileName, info.ClsId, info.ManifestFileName, dictionary[key].ManifestFileName });
                                }
                            }
                            if (!string.IsNullOrEmpty(info.TlbId))
                            {
                                string str3 = info.TlbId.ToLowerInvariant();
                                if (!dictionary2.ContainsKey(str3))
                                {
                                    dictionary2.Add(str3, info);
                                }
                                else
                                {
                                    base.OutputMessages.AddErrorMessage("GenerateManifest.DuplicateComDefinition", new string[] { "tlbid", info.ComponentFileName, info.TlbId, info.ManifestFileName, dictionary2[str3].ManifestFileName });
                                }
                            }
                        }
                    }
                }
            }
            foreach (FileReference reference2 in base.FileReferences)
            {
                if (reference2.ComClasses != null)
                {
                    foreach (ComClass class2 in reference2.ComClasses)
                    {
                        string str4 = class2.ClsId.ToLowerInvariant();
                        if (!dictionary.ContainsKey(str4))
                        {
                            dictionary.Add(str4, new ComInfo(fileName, reference2.TargetPath, class2.ClsId, null));
                        }
                        else
                        {
                            base.OutputMessages.AddErrorMessage("GenerateManifest.DuplicateComDefinition", new string[] { "clsid", reference2.ToString(), class2.ClsId, fileName, dictionary[str4].ManifestFileName });
                        }
                    }
                }
                if (reference2.TypeLibs != null)
                {
                    foreach (TypeLib lib in reference2.TypeLibs)
                    {
                        string str5 = lib.TlbId.ToLowerInvariant();
                        if (!dictionary2.ContainsKey(str5))
                        {
                            dictionary2.Add(str5, new ComInfo(fileName, reference2.TargetPath, null, lib.TlbId));
                        }
                        else
                        {
                            base.OutputMessages.AddErrorMessage("GenerateManifest.DuplicateComDefinition", new string[] { "tlbid", reference2.ToString(), lib.TlbId, fileName, dictionary2[str5].ManifestFileName });
                        }
                    }
                }
            }
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "GenerateManifest.CheckForComDuplicates t={0}", new object[] { Environment.TickCount - tickCount }));
        }

        private void ValidateConfig()
        {
            if (!string.IsNullOrEmpty(this.ConfigFile))
            {
                FileReference reference = base.FileReferences.FindTargetPath(this.ConfigFile);
                if ((reference != null) && !this.TrustInfo.IsFullTrust)
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(reference.ResolvedPath);
                    XmlNamespaceManager namespaceManager = XmlNamespaces.GetNamespaceManager(document.NameTable);
                    if (document.SelectNodes("configuration/runtime/asmv1:assemblyBinding/asmv1:dependentAssembly/asmv1:bindingRedirect", namespaceManager).Count > 0)
                    {
                        base.OutputMessages.AddWarningMessage("GenerateManifest.ConfigBindingRedirectsWithPartialTrust", new string[0]);
                    }
                }
            }
        }

        private void ValidateEntryPoint()
        {
            if ((this.entryPoint != null) && !(!string.IsNullOrEmpty(this.entryPoint.TargetPath) && this.entryPoint.TargetPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)))
            {
                base.OutputMessages.AddErrorMessage("GenerateManifest.InvalidEntryPoint", new string[] { this.entryPoint.ToString() });
            }
        }

        private void ValidateFileAssociations()
        {
            if (this.FileAssociations.Count > 0)
            {
                if (this.FileAssociations.Count > 8)
                {
                    string[] arguments = new string[] { 8.ToString(CultureInfo.CurrentUICulture) };
                    base.OutputMessages.AddErrorMessage("GenerateManifest.FileAssociationsCountExceedsMaximum", arguments);
                }
                Dictionary<string, FileAssociation> dictionary = new Dictionary<string, FileAssociation>(StringComparer.OrdinalIgnoreCase);
                foreach (FileAssociation association in this.FileAssociations)
                {
                    if ((string.IsNullOrEmpty(association.Extension) || string.IsNullOrEmpty(association.Description)) || (string.IsNullOrEmpty(association.ProgId) || string.IsNullOrEmpty(association.DefaultIcon)))
                    {
                        base.OutputMessages.AddErrorMessage("GenerateManifest.FileAssociationMissingAttribute", new string[0]);
                    }
                    if (!string.IsNullOrEmpty(association.Extension))
                    {
                        if (association.Extension[0] != '.')
                        {
                            base.OutputMessages.AddErrorMessage("GenerateManifest.FileAssociationExtensionMissingLeadDot", new string[0]);
                        }
                        if (association.Extension.Length > 0x18)
                        {
                            string[] strArray2 = new string[] { association.Extension, 0x18.ToString(CultureInfo.CurrentUICulture) };
                            base.OutputMessages.AddErrorMessage("GenerateManifest.FileAssociationExtensionTooLong", strArray2);
                        }
                        if (!dictionary.ContainsKey(association.Extension))
                        {
                            dictionary.Add(association.Extension, association);
                        }
                        else
                        {
                            base.OutputMessages.AddErrorMessage("GenerateManifest.FileAssociationsDuplicateExtensions", new string[] { association.Extension });
                        }
                    }
                    if (!string.IsNullOrEmpty(association.DefaultIcon))
                    {
                        FileReference reference = null;
                        foreach (FileReference reference2 in base.FileReferences)
                        {
                            if (reference2.TargetPath.Equals(association.DefaultIcon, StringComparison.Ordinal))
                            {
                                reference = reference2;
                                break;
                            }
                        }
                        if ((reference == null) || !string.IsNullOrEmpty(reference.Group))
                        {
                            base.OutputMessages.AddErrorMessage("GenerateManifest.FileAssociationDefaultIconNotInstalled", new string[] { association.DefaultIcon });
                        }
                    }
                }
                if (!this.TrustInfo.IsFullTrust)
                {
                    base.OutputMessages.AddErrorMessage("GenerateManifest.FileAssociationsApplicationNotFullTrust", new string[0]);
                }
                if (this.EntryPoint == null)
                {
                    base.OutputMessages.AddErrorMessage("GenerateManifest.FileAssociationsNoEntryPoint", new string[0]);
                }
            }
        }

        private void ValidateReferenceForPartialTrust(AssemblyReference assembly, Microsoft.Build.Tasks.Deployment.ManifestUtilities.TrustInfo trustInfo)
        {
            if (!trustInfo.IsFullTrust)
            {
                string resolvedPath = assembly.ResolvedPath;
                AssemblyAttributeFlags flags = new AssemblyAttributeFlags(resolvedPath);
                if (Util.CompareFrameworkVersions(this.TargetFrameworkVersion, "v3.5") <= 0)
                {
                    if ((assembly.IsPrimary && flags.IsSigned) && !flags.HasAllowPartiallyTrustedCallersAttribute)
                    {
                        base.OutputMessages.AddWarningMessage("GenerateManifest.AllowPartiallyTrustedCallers", new string[] { Path.GetFileNameWithoutExtension(resolvedPath) });
                    }
                }
                else if ((assembly.AssemblyIdentity != null) && assembly.AssemblyIdentity.IsFrameworkAssembly)
                {
                    if ((assembly.IsPrimary && !flags.HasAllowPartiallyTrustedCallersAttribute) && !flags.HasSecurityTransparentAttribute)
                    {
                        base.OutputMessages.AddWarningMessage("GenerateManifest.AllowPartiallyTrustedCallers", new string[] { Path.GetFileNameWithoutExtension(resolvedPath) });
                    }
                }
                else if ((assembly.IsPrimary && flags.IsSigned) && (!flags.HasAllowPartiallyTrustedCallersAttribute && !flags.HasSecurityTransparentAttribute))
                {
                    base.OutputMessages.AddWarningMessage("GenerateManifest.AllowPartiallyTrustedCallers", new string[] { Path.GetFileNameWithoutExtension(resolvedPath) });
                }
                if (flags.HasPrimaryInteropAssemblyAttribute || flags.HasImportedFromTypeLibAttribute)
                {
                    base.OutputMessages.AddWarningMessage("GenerateManifest.UnmanagedCodePermission", new string[] { Path.GetFileNameWithoutExtension(resolvedPath) });
                }
            }
        }

        private void ValidateReferencesForClickOnceApplication()
        {
            int tickCount = Environment.TickCount;
            bool flag = !this.TrustInfo.IsFullTrust;
            Dictionary<string, NGen<bool>> dictionary = new Dictionary<string, NGen<bool>>();
            foreach (AssemblyReference reference in base.AssemblyReferences)
            {
                if ((flag && (reference != this.EntryPoint)) && !string.IsNullOrEmpty(reference.ResolvedPath))
                {
                    this.ValidateReferenceForPartialTrust(reference, this.TrustInfo);
                }
                if (!reference.IsPrerequisite && !string.IsNullOrEmpty(reference.TargetPath))
                {
                    if ((this.maxTargetPath > 0) && (reference.TargetPath.Length > this.maxTargetPath))
                    {
                        base.OutputMessages.AddWarningMessage("GenerateManifest.TargetPathTooLong", new string[] { reference.ToString(), this.maxTargetPath.ToString(CultureInfo.CurrentCulture) });
                    }
                    string key = reference.TargetPath.ToLowerInvariant();
                    if (!dictionary.ContainsKey(key))
                    {
                        dictionary.Add(key, 0);
                    }
                    else if (dictionary[key] == 0)
                    {
                        base.OutputMessages.AddWarningMessage("GenerateManifest.DuplicateTargetPath", new string[] { reference.ToString() });
                        dictionary[key] = 1;
                    }
                }
                else if ((this.maxTargetPath > 0) && (reference.AssemblyIdentity.Name.Length > this.maxTargetPath))
                {
                    base.OutputMessages.AddWarningMessage("GenerateManifest.TargetPathTooLong", new string[] { reference.AssemblyIdentity.Name, this.maxTargetPath.ToString(CultureInfo.CurrentCulture) });
                }
                if ((reference.IsPrerequisite && !reference.AssemblyIdentity.IsStrongName) && !reference.IsVirtual)
                {
                    base.OutputMessages.AddErrorMessage("GenerateManifest.PrerequisiteNotSigned", new string[] { reference.ToString() });
                }
            }
            foreach (FileReference reference2 in base.FileReferences)
            {
                if (!string.IsNullOrEmpty(reference2.ResolvedPath) && PathUtil.IsAssembly(reference2.ResolvedPath))
                {
                    base.OutputMessages.AddWarningMessage("GenerateManifest.AssemblyAsFile", new string[] { reference2.ToString() });
                }
                if (!string.IsNullOrEmpty(reference2.TargetPath))
                {
                    if ((this.maxTargetPath > 0) && (reference2.TargetPath.Length > this.maxTargetPath))
                    {
                        base.OutputMessages.AddWarningMessage("GenerateManifest.TargetPathTooLong", new string[] { reference2.TargetPath, this.maxTargetPath.ToString(CultureInfo.CurrentCulture) });
                    }
                    string str2 = reference2.TargetPath.ToLowerInvariant();
                    if (!dictionary.ContainsKey(str2))
                    {
                        dictionary.Add(str2, 0);
                    }
                    else if (dictionary[str2] == 0)
                    {
                        base.OutputMessages.AddWarningMessage("GenerateManifest.DuplicateTargetPath", new string[] { reference2.TargetPath });
                        dictionary[str2] = 1;
                    }
                }
            }
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "GenerateManifest.CheckManifestReferences t={0}", new object[] { Environment.TickCount - tickCount }));
        }

        private void ValidateReferencesForNativeApplication()
        {
            foreach (AssemblyReference reference in base.AssemblyReferences)
            {
                if (!reference.IsPrerequisite && !string.Equals(reference.AssemblyIdentity.Name, Path.GetFileNameWithoutExtension(reference.TargetPath), StringComparison.OrdinalIgnoreCase))
                {
                    base.OutputMessages.AddErrorMessage("GenerateManifest.IdentityFileNameMismatch", new string[] { reference.ToString(), reference.AssemblyIdentity.Name, reference.AssemblyIdentity.Name + Path.GetExtension(reference.TargetPath) });
                }
            }
        }

        [XmlIgnore]
        public string ConfigFile
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

        [XmlIgnore]
        public override AssemblyReference EntryPoint
        {
            get
            {
                this.FixupEntryPoint();
                return this.entryPoint;
            }
            set
            {
                this.entryPoint = value;
                this.UpdateEntryPoint();
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
        public FileAssociationCollection FileAssociations
        {
            get
            {
                if (this.fileAssociationList == null)
                {
                    this.fileAssociationList = new FileAssociationCollection(this.fileAssociations);
                }
                return this.fileAssociationList;
            }
        }

        [XmlIgnore]
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

        [XmlIgnore]
        public string IconFile
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

        [XmlIgnore]
        public bool IsClickOnceManifest
        {
            get
            {
                return this.isClickOnceManifest;
            }
            set
            {
                this.isClickOnceManifest = value;
            }
        }

        [XmlIgnore]
        public int MaxTargetPath
        {
            get
            {
                return this.maxTargetPath;
            }
            set
            {
                this.maxTargetPath = value;
            }
        }

        [XmlIgnore]
        public string OSDescription
        {
            get
            {
                return this.oSDescription;
            }
            set
            {
                this.oSDescription = value;
            }
        }

        [XmlIgnore]
        public string OSSupportUrl
        {
            get
            {
                return this.oSSupportUrl;
            }
            set
            {
                this.oSSupportUrl = value;
            }
        }

        [XmlIgnore]
        public string OSVersion
        {
            get
            {
                if (string.IsNullOrEmpty(this.oSMajor))
                {
                    return null;
                }
                Version version = null;
                try
                {
                    version = new Version(string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", new object[] { this.oSMajor, this.oSMinor, this.oSBuild, this.oSRevision }));
                }
                catch (FormatException)
                {
                    return null;
                }
                return version.ToString();
            }
            set
            {
                if (value == null)
                {
                    this.oSMajor = null;
                    this.oSMinor = null;
                    this.oSBuild = null;
                    this.oSRevision = null;
                }
                else
                {
                    Version version = new Version(value);
                    if ((version.Build < 0) || (version.Revision < 0))
                    {
                        throw new FormatException();
                    }
                    this.oSMajor = version.Major.ToString("G", CultureInfo.InvariantCulture);
                    this.oSMinor = version.Minor.ToString("G", CultureInfo.InvariantCulture);
                    this.oSBuild = version.Build.ToString("G", CultureInfo.InvariantCulture);
                    this.oSRevision = version.Revision.ToString("G", CultureInfo.InvariantCulture);
                }
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
        public string TargetFrameworkVersion
        {
            get
            {
                return this.targetFrameworkVersion;
            }
            set
            {
                this.targetFrameworkVersion = value;
            }
        }

        [XmlIgnore]
        public Microsoft.Build.Tasks.Deployment.ManifestUtilities.TrustInfo TrustInfo
        {
            get
            {
                return this.trustInfo;
            }
            set
            {
                this.trustInfo = value;
            }
        }

        [XmlIgnore]
        public bool UseApplicationTrust
        {
            get
            {
                return this.useApplicationTrust;
            }
            set
            {
                this.useApplicationTrust = value;
            }
        }

        private bool WinXPRequired
        {
            get
            {
                foreach (FileReference reference in base.FileReferences)
                {
                    if (((reference.ComClasses != null) || (reference.TypeLibs != null)) || (reference.ProxyStubs != null))
                    {
                        return true;
                    }
                }
                foreach (AssemblyReference reference2 in base.AssemblyReferences)
                {
                    if (reference2.ReferenceType == AssemblyReferenceType.NativeAssembly)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        [XmlAttribute("ConfigFile"), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public string XmlConfigFile
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

        [EditorBrowsable(EditorBrowsableState.Never), XmlElement("EntryPointIdentity"), Browsable(false)]
        public AssemblyIdentity XmlEntryPointIdentity
        {
            get
            {
                return this.entryPointIdentity;
            }
            set
            {
                this.entryPointIdentity = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("EntryPointParameters"), Browsable(false)]
        public string XmlEntryPointParameters
        {
            get
            {
                return this.entryPointParameters;
            }
            set
            {
                this.entryPointParameters = value;
            }
        }

        [Browsable(false), XmlAttribute("EntryPointPath"), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlEntryPointPath
        {
            get
            {
                return this.entryPointPath;
            }
            set
            {
                this.entryPointPath = value;
            }
        }

        [XmlAttribute("ErrorReportUrl"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [XmlArray("FileAssociations"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public FileAssociation[] XmlFileAssociations
        {
            get
            {
                return this.fileAssociations;
            }
            set
            {
                this.fileAssociations = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("HostInBrowser")]
        public string XmlHostInBrowser
        {
            get
            {
                return Convert.ToString(this.hostInBrowser, CultureInfo.InvariantCulture).ToLowerInvariant();
            }
            set
            {
                this.hostInBrowser = ConvertUtil.ToBoolean(value);
            }
        }

        [Browsable(false), XmlAttribute("IconFile"), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlIconFile
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

        [Browsable(false), XmlAttribute("IsClickOnceManifest"), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlIsClickOnceManifest
        {
            get
            {
                return Convert.ToString(this.isClickOnceManifest, CultureInfo.InvariantCulture).ToLowerInvariant();
            }
            set
            {
                this.isClickOnceManifest = ConvertUtil.ToBoolean(value);
            }
        }

        [XmlAttribute("OSBuild"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlOSBuild
        {
            get
            {
                return this.oSBuild;
            }
            set
            {
                this.oSBuild = value;
            }
        }

        [Browsable(false), XmlAttribute("OSDescription"), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlOSDescription
        {
            get
            {
                return this.oSDescription;
            }
            set
            {
                this.oSDescription = value;
            }
        }

        [XmlAttribute("OSMajor"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlOSMajor
        {
            get
            {
                return this.oSMajor;
            }
            set
            {
                this.oSMajor = value;
            }
        }

        [Browsable(false), XmlAttribute("OSMinor"), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlOSMinor
        {
            get
            {
                return this.oSMinor;
            }
            set
            {
                this.oSMinor = value;
            }
        }

        [Browsable(false), XmlAttribute("OSRevision"), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlOSRevision
        {
            get
            {
                return this.oSRevision;
            }
            set
            {
                this.oSRevision = value;
            }
        }

        [XmlAttribute("OSSupportUrl"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlOSSupportUrl
        {
            get
            {
                return this.oSSupportUrl;
            }
            set
            {
                this.oSSupportUrl = value;
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

        [Browsable(false), XmlAttribute("Publisher"), EditorBrowsable(EditorBrowsableState.Never)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("SuiteName")]
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

        [XmlAttribute("SupportUrl"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("UseApplicationTrust"), Browsable(false)]
        public string XmlUseApplicationTrust
        {
            get
            {
                return Convert.ToString(this.useApplicationTrust, CultureInfo.InvariantCulture).ToLowerInvariant();
            }
            set
            {
                this.useApplicationTrust = ConvertUtil.ToBoolean(value);
            }
        }

        private class AssemblyAttributeFlags
        {
            public readonly bool HasAllowPartiallyTrustedCallersAttribute;
            public readonly bool HasImportedFromTypeLibAttribute;
            public readonly bool HasPrimaryInteropAssemblyAttribute;
            public readonly bool HasSecurityRulesAttribute;
            public readonly bool HasSecurityTransparentAttribute;
            public readonly bool IsSigned;

            public AssemblyAttributeFlags(string path)
            {
                using (MetadataReader reader = MetadataReader.Create(path))
                {
                    if (reader != null)
                    {
                        this.IsSigned = !string.IsNullOrEmpty(reader.PublicKeyToken);
                        this.HasAllowPartiallyTrustedCallersAttribute = reader.HasAssemblyAttribute("System.Security.AllowPartiallyTrustedCallersAttribute");
                        this.HasSecurityTransparentAttribute = reader.HasAssemblyAttribute("System.Security.SecurityTransparentAttribute");
                        this.HasPrimaryInteropAssemblyAttribute = reader.HasAssemblyAttribute("System.Runtime.InteropServices.PrimaryInteropAssemblyAttribute");
                        this.HasImportedFromTypeLibAttribute = reader.HasAssemblyAttribute("System.Runtime.InteropServices.ImportedFromTypeLibAttribute");
                        this.HasSecurityRulesAttribute = reader.HasAssemblyAttribute("System.Security.SecurityRulesAttribute");
                    }
                }
            }
        }
    }
}


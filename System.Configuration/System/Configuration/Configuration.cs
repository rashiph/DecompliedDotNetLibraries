namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration.Internal;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Versioning;
    using System.Security.Permissions;

    public sealed class Configuration
    {
        private Func<string, string> _AssemblyStringTransformer;
        private bool _AssemblyStringTransformerIsSet;
        private MgmtConfigurationRecord _configRecord;
        private InternalConfigRoot _configRoot;
        private ContextInformation _evalContext;
        private object[] _hostInitConfigurationParams;
        private ConfigurationLocationCollection _locations;
        private ConfigurationSectionGroup _rootSectionGroup;
        private Stack _SectionsStack;
        private FrameworkName _TargetFramework;
        private Type _typeConfigHost;
        private Func<string, string> _TypeStringTransformer;
        private bool _TypeStringTransformerIsSet;

        internal Configuration(string locationSubPath, Type typeConfigHost, params object[] hostInitConfigurationParams)
        {
            string str;
            string str2;
            this._typeConfigHost = typeConfigHost;
            this._hostInitConfigurationParams = hostInitConfigurationParams;
            this._configRoot = new InternalConfigRoot(this);
            IInternalConfigHost host = (IInternalConfigHost) System.Configuration.TypeUtil.CreateInstanceWithReflectionPermission(typeConfigHost);
            IInternalConfigHost host2 = new UpdateConfigHost(host);
            ((IInternalConfigRoot) this._configRoot).Init(host2, true);
            host.InitForConfiguration(ref locationSubPath, out str, out str2, this._configRoot, hostInitConfigurationParams);
            if (!string.IsNullOrEmpty(locationSubPath) && !host2.SupportsLocation)
            {
                throw ExceptionUtil.UnexpectedError("Configuration::ctor");
            }
            if (string.IsNullOrEmpty(locationSubPath) != string.IsNullOrEmpty(str2))
            {
                throw ExceptionUtil.UnexpectedError("Configuration::ctor");
            }
            this._configRecord = (MgmtConfigurationRecord) this._configRoot.GetConfigRecord(str);
            if (!string.IsNullOrEmpty(locationSubPath))
            {
                this._configRecord = MgmtConfigurationRecord.Create(this._configRoot, this._configRecord, str2, locationSubPath);
            }
            this._configRecord.ThrowIfInitErrors();
        }

        private void ForceGroupsRecursive(ConfigurationSectionGroup group)
        {
            foreach (ConfigurationSection section in group.Sections)
            {
                ConfigurationSection section1 = group.Sections[section.SectionInformation.Name];
            }
            foreach (ConfigurationSectionGroup group2 in group.SectionGroups)
            {
                this.ForceGroupsRecursive(group.SectionGroups[group2.Name]);
            }
        }

        public ConfigurationSection GetSection(string sectionName)
        {
            return (ConfigurationSection) this._configRecord.GetSection(sectionName);
        }

        public ConfigurationSectionGroup GetSectionGroup(string sectionGroupName)
        {
            return this._configRecord.GetSectionGroup(sectionGroupName);
        }

        internal System.Configuration.Configuration OpenLocationConfiguration(string locationSubPath)
        {
            return new System.Configuration.Configuration(locationSubPath, this._typeConfigHost, this._hostInitConfigurationParams);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Save()
        {
            this.SaveAsImpl(null, ConfigurationSaveMode.Modified, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Save(ConfigurationSaveMode saveMode)
        {
            this.SaveAsImpl(null, saveMode, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Save(ConfigurationSaveMode saveMode, bool forceSaveAll)
        {
            this.SaveAsImpl(null, saveMode, forceSaveAll);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void SaveAs(string filename)
        {
            this.SaveAs(filename, ConfigurationSaveMode.Modified, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void SaveAs(string filename, ConfigurationSaveMode saveMode)
        {
            this.SaveAs(filename, saveMode, false);
        }

        public void SaveAs(string filename, ConfigurationSaveMode saveMode, bool forceSaveAll)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("filename");
            }
            this.SaveAsImpl(filename, saveMode, forceSaveAll);
        }

        private void SaveAsImpl(string filename, ConfigurationSaveMode saveMode, bool forceSaveAll)
        {
            if (string.IsNullOrEmpty(filename))
            {
                filename = null;
            }
            else
            {
                filename = Path.GetFullPath(filename);
            }
            if (forceSaveAll)
            {
                this.ForceGroupsRecursive(this.RootSectionGroup);
            }
            this._configRecord.SaveAs(filename, saveMode, forceSaveAll);
        }

        public AppSettingsSection AppSettings
        {
            get
            {
                return (AppSettingsSection) this.GetSection("appSettings");
            }
        }

        public Func<string, string> AssemblyStringTransformer
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._AssemblyStringTransformer;
            }
            [ConfigurationPermission(SecurityAction.Demand, Unrestricted=true)]
            set
            {
                if (this._AssemblyStringTransformer != value)
                {
                    this._AssemblyStringTransformerIsSet = value != null;
                    this._AssemblyStringTransformer = value;
                }
            }
        }

        internal bool AssemblyStringTransformerIsSet
        {
            get
            {
                return this._AssemblyStringTransformerIsSet;
            }
        }

        public ConnectionStringsSection ConnectionStrings
        {
            get
            {
                return (ConnectionStringsSection) this.GetSection("connectionStrings");
            }
        }

        public ContextInformation EvaluationContext
        {
            get
            {
                if (this._evalContext == null)
                {
                    this._evalContext = new ContextInformation(this._configRecord);
                }
                return this._evalContext;
            }
        }

        public string FilePath
        {
            get
            {
                return this._configRecord.ConfigurationFilePath;
            }
        }

        public bool HasFile
        {
            get
            {
                return this._configRecord.HasStream;
            }
        }

        public ConfigurationLocationCollection Locations
        {
            get
            {
                if (this._locations == null)
                {
                    this._locations = this._configRecord.GetLocationCollection(this);
                }
                return this._locations;
            }
        }

        public bool NamespaceDeclared
        {
            get
            {
                return this._configRecord.NamespacePresent;
            }
            set
            {
                this._configRecord.NamespacePresent = value;
            }
        }

        public ConfigurationSectionGroup RootSectionGroup
        {
            get
            {
                if (this._rootSectionGroup == null)
                {
                    this._rootSectionGroup = new ConfigurationSectionGroup();
                    this._rootSectionGroup.RootAttachToConfigurationRecord(this._configRecord);
                }
                return this._rootSectionGroup;
            }
        }

        public ConfigurationSectionGroupCollection SectionGroups
        {
            get
            {
                return this.RootSectionGroup.SectionGroups;
            }
        }

        public ConfigurationSectionCollection Sections
        {
            get
            {
                return this.RootSectionGroup.Sections;
            }
        }

        internal Stack SectionsStack
        {
            get
            {
                if (this._SectionsStack == null)
                {
                    this._SectionsStack = new Stack();
                }
                return this._SectionsStack;
            }
        }

        public FrameworkName TargetFramework
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._TargetFramework;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), ConfigurationPermission(SecurityAction.Demand, Unrestricted=true)]
            set
            {
                this._TargetFramework = value;
            }
        }

        public Func<string, string> TypeStringTransformer
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._TypeStringTransformer;
            }
            [ConfigurationPermission(SecurityAction.Demand, Unrestricted=true)]
            set
            {
                if (this._TypeStringTransformer != value)
                {
                    this._TypeStringTransformerIsSet = value != null;
                    this._TypeStringTransformer = value;
                }
            }
        }

        internal bool TypeStringTransformerIsSet
        {
            get
            {
                return this._TypeStringTransformerIsSet;
            }
        }
    }
}


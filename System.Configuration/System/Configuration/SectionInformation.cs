namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Runtime;

    public sealed class SectionInformation
    {
        private ConfigurationAllowDefinition _allowDefinition;
        private ConfigurationAllowExeDefinition _allowExeDefinition;
        private string _configKey = string.Empty;
        private MgmtConfigurationRecord _configRecord;
        private string _configSource;
        private string _configSourceStreamName;
        private ConfigurationSection _configurationSection;
        private SafeBitVector32 _flags;
        private string _group = string.Empty;
        private SimpleBitVector32 _modifiedFlags;
        private string _name = string.Empty;
        private System.Configuration.OverrideModeSetting _overrideMode;
        private System.Configuration.OverrideModeSetting _overrideModeDefault;
        private ProtectedConfigurationProvider _protectionProvider;
        private string _protectionProviderName;
        private string _rawXml;
        private string _typeName;
        private const int Flag_AllowDefinitionModified = 0x20000;
        private const int Flag_AllowExeDefinitionModified = 0x10000;
        private const int Flag_AllowLocation = 8;
        private const int Flag_Attached = 1;
        private const int Flag_ChildrenLocked = 0x80;
        private const int Flag_ChildrenLockWithoutFileInput = 0x4000;
        private const int Flag_ConfigSourceModified = 0x40000;
        private const int Flag_DeclarationRequired = 4;
        private const int Flag_Declared = 2;
        private const int Flag_ForceSave = 0x1000;
        private const int Flag_InheritInChildApps = 0x100;
        private const int Flag_IsParentSection = 0x200;
        private const int Flag_IsUndeclared = 0x2000;
        private const int Flag_LocationLocked = 0x40;
        private const int Flag_OverrideModeDefaultModified = 0x100000;
        private const int Flag_OverrideModeModified = 0x200000;
        private const int Flag_ProtectionProviderDetermined = 0x800;
        private const int Flag_ProtectionProviderModified = 0x80000;
        private const int Flag_Removed = 0x400;
        private const int Flag_RequirePermission = 0x20;
        private const int Flag_RestartOnExternalChanges = 0x10;

        internal SectionInformation(ConfigurationSection associatedConfigurationSection)
        {
            this._configurationSection = associatedConfigurationSection;
            this._allowDefinition = ConfigurationAllowDefinition.Everywhere;
            this._allowExeDefinition = ConfigurationAllowExeDefinition.MachineToApplication;
            this._overrideModeDefault = System.Configuration.OverrideModeSetting.SectionDefault;
            this._overrideMode = System.Configuration.OverrideModeSetting.LocationDefault;
            this._flags[8] = true;
            this._flags[0x10] = true;
            this._flags[0x20] = true;
            this._flags[0x100] = true;
            this._flags[0x1000] = false;
            this._modifiedFlags = new SimpleBitVector32();
        }

        internal void AttachToConfigurationRecord(MgmtConfigurationRecord configRecord, FactoryRecord factoryRecord, SectionRecord sectionRecord)
        {
            this.SetRuntimeConfigurationInformation(configRecord, factoryRecord, sectionRecord);
            this._configRecord = configRecord;
        }

        internal void DetachFromConfigurationRecord()
        {
            this.RevertToParent();
            this._flags[1] = false;
            this._configRecord = null;
        }

        private FactoryRecord FindParentFactoryRecord(bool permitErrors)
        {
            FactoryRecord record = null;
            if ((this._configRecord != null) && !this._configRecord.Parent.IsRootConfig)
            {
                record = this._configRecord.Parent.FindFactoryRecord(this._configKey, permitErrors);
            }
            return record;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void ForceDeclaration()
        {
            this.ForceDeclaration(true);
        }

        public void ForceDeclaration(bool force)
        {
            this.VerifyIsEditable();
            if (force || !this._flags[4])
            {
                if (force && BaseConfigurationRecord.IsImplicitSection(this.SectionName))
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Cannot_declare_or_remove_implicit_section"));
                }
                if (force && this._flags[0x2000])
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_cannot_edit_configurationsection_when_it_is_undeclared"));
                }
                this._flags[2] = force;
            }
        }

        public ConfigurationSection GetParentSection()
        {
            this.VerifyDesigntime();
            if (this._flags[0x200])
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_getparentconfigurationsection_first_instance"));
            }
            ConfigurationSection section = null;
            if (this._configRecord != null)
            {
                section = this._configRecord.FindAndCloneImmediateParentSection(this._configurationSection);
                if (section != null)
                {
                    section.SectionInformation._flags[0x200] = true;
                    section.SetReadOnly();
                }
            }
            return section;
        }

        public string GetRawXml()
        {
            this.VerifyDesigntime();
            this.VerifyNotParentSection();
            if (this.RawXml != null)
            {
                return this.RawXml;
            }
            if (this._configRecord != null)
            {
                return this._configRecord.GetRawXml(this._configKey);
            }
            return null;
        }

        internal bool IsModifiedFlags()
        {
            return (this._modifiedFlags.Data != 0);
        }

        public void ProtectSection(string protectionProvider)
        {
            ProtectedConfigurationProvider protectionProviderFromName = null;
            this.VerifyIsEditable();
            if (!this.AllowLocation || (this._configKey == "configProtectedData"))
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_not_allowed_to_encrypt_this_section"));
            }
            if (this._configRecord == null)
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Must_add_to_config_before_protecting_it"));
            }
            if (string.IsNullOrEmpty(protectionProvider))
            {
                protectionProvider = this._configRecord.DefaultProviderName;
            }
            protectionProviderFromName = this._configRecord.GetProtectionProviderFromName(protectionProvider, true);
            this._protectionProviderName = protectionProvider;
            this._protectionProvider = protectionProviderFromName;
            this._flags[0x800] = true;
            this._modifiedFlags[0x80000] = true;
        }

        internal void ResetModifiedFlags()
        {
            this._modifiedFlags = new SimpleBitVector32();
        }

        public void RevertToParent()
        {
            this.VerifyIsEditable();
            this.VerifyIsAttachedToConfigRecord();
            this._configRecord.RevertToParent(this._configurationSection);
        }

        public void SetRawXml(string rawXml)
        {
            this.VerifyIsEditable();
            if (this._configRecord != null)
            {
                this._configRecord.SetRawXml(this._configurationSection, rawXml);
            }
            else
            {
                this.RawXml = string.IsNullOrEmpty(rawXml) ? null : rawXml;
            }
        }

        internal void SetRuntimeConfigurationInformation(BaseConfigurationRecord configRecord, FactoryRecord factoryRecord, SectionRecord sectionRecord)
        {
            this._flags[1] = true;
            this._configKey = factoryRecord.ConfigKey;
            this._group = factoryRecord.Group;
            this._name = factoryRecord.Name;
            this._typeName = factoryRecord.FactoryTypeName;
            this._allowDefinition = factoryRecord.AllowDefinition;
            this._allowExeDefinition = factoryRecord.AllowExeDefinition;
            this._flags[8] = factoryRecord.AllowLocation;
            this._flags[0x10] = factoryRecord.RestartOnExternalChanges;
            this._flags[0x20] = factoryRecord.RequirePermission;
            this._overrideModeDefault = factoryRecord.OverrideModeDefault;
            if (factoryRecord.IsUndeclared)
            {
                this._flags[0x2000] = true;
                this._flags[2] = false;
                this._flags[4] = false;
            }
            else
            {
                this._flags[0x2000] = false;
                this._flags[2] = configRecord.GetFactoryRecord(factoryRecord.ConfigKey, false) != null;
                this._flags[4] = configRecord.IsRootDeclaration(factoryRecord.ConfigKey, false);
            }
            this._flags[0x40] = sectionRecord.Locked;
            this._flags[0x80] = sectionRecord.LockChildren;
            this._flags[0x4000] = sectionRecord.LockChildrenWithoutFileInput;
            if (sectionRecord.HasFileInput)
            {
                SectionInput fileInput = sectionRecord.FileInput;
                this._flags[0x800] = fileInput.IsProtectionProviderDetermined;
                this._protectionProvider = fileInput.ProtectionProvider;
                SectionXmlInfo sectionXmlInfo = fileInput.SectionXmlInfo;
                this._configSource = sectionXmlInfo.ConfigSource;
                this._configSourceStreamName = sectionXmlInfo.ConfigSourceStreamName;
                this._overrideMode = sectionXmlInfo.OverrideModeSetting;
                this._flags[0x100] = !sectionXmlInfo.SkipInChildApps;
                this._protectionProviderName = sectionXmlInfo.ProtectionProviderName;
            }
            else
            {
                this._flags[0x800] = false;
                this._protectionProvider = null;
            }
            this._configurationSection.AssociateContext(configRecord);
        }

        public void UnprotectSection()
        {
            this.VerifyIsEditable();
            this._protectionProvider = null;
            this._protectionProviderName = null;
            this._flags[0x800] = true;
            this._modifiedFlags[0x80000] = true;
        }

        private void VerifyDesigntime()
        {
            if (this.IsRuntime)
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_operation_not_runtime"));
            }
        }

        private void VerifyIsAttachedToConfigRecord()
        {
            if (this._configRecord == null)
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_cannot_edit_configurationsection_when_not_attached"));
            }
        }

        internal void VerifyIsEditable()
        {
            this.VerifyDesigntime();
            if (this.IsLocked)
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_cannot_edit_configurationsection_when_locked"));
            }
            if (this._flags[0x200])
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_cannot_edit_configurationsection_parentsection"));
            }
            if ((!this._flags[8] && (this._configRecord != null)) && this._configRecord.IsLocationConfig)
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_cannot_edit_configurationsection_when_location_locked"));
            }
        }

        internal void VerifyIsEditableFactory()
        {
            if ((this._configRecord != null) && this._configRecord.IsLocationConfig)
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_cannot_edit_configurationsection_in_location_config"));
            }
            if (BaseConfigurationRecord.IsImplicitSection(this.ConfigKey))
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_cannot_edit_configurationsection_when_it_is_implicit"));
            }
            if (this._flags[0x2000])
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_cannot_edit_configurationsection_when_it_is_undeclared"));
            }
        }

        private void VerifyNotParentSection()
        {
            if (this._flags[0x200])
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_configsection_parentnotvalid"));
            }
        }

        private void VerifySupportsLocation()
        {
            if ((this._configRecord != null) && !this._configRecord.RecordSupportsLocation)
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_cannot_edit_locationattriubtes"));
            }
        }

        public ConfigurationAllowDefinition AllowDefinition
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._allowDefinition;
            }
            set
            {
                this.VerifyIsEditable();
                this.VerifyIsEditableFactory();
                FactoryRecord record = this.FindParentFactoryRecord(false);
                if ((record != null) && (record.AllowDefinition != value))
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_tag_name_already_defined", new object[] { this._configKey }));
                }
                this._allowDefinition = value;
                this._modifiedFlags[0x20000] = true;
            }
        }

        internal bool AllowDefinitionModified
        {
            get
            {
                return this._modifiedFlags[0x20000];
            }
        }

        public ConfigurationAllowExeDefinition AllowExeDefinition
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._allowExeDefinition;
            }
            set
            {
                this.VerifyIsEditable();
                this.VerifyIsEditableFactory();
                FactoryRecord record = this.FindParentFactoryRecord(false);
                if ((record != null) && (record.AllowExeDefinition != value))
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_tag_name_already_defined", new object[] { this._configKey }));
                }
                this._allowExeDefinition = value;
                this._modifiedFlags[0x10000] = true;
            }
        }

        internal bool AllowExeDefinitionModified
        {
            get
            {
                return this._modifiedFlags[0x10000];
            }
        }

        public bool AllowLocation
        {
            get
            {
                return this._flags[8];
            }
            set
            {
                this.VerifyIsEditable();
                this.VerifyIsEditableFactory();
                FactoryRecord record = this.FindParentFactoryRecord(false);
                if ((record != null) && (record.AllowLocation != value))
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_tag_name_already_defined", new object[] { this._configKey }));
                }
                this._flags[8] = value;
                this._modifiedFlags[8] = true;
            }
        }

        internal bool AllowLocationModified
        {
            get
            {
                return this._modifiedFlags[8];
            }
        }

        public bool AllowOverride
        {
            get
            {
                return this._overrideMode.AllowOverride;
            }
            set
            {
                this.VerifyIsEditable();
                this.VerifySupportsLocation();
                this._overrideMode.AllowOverride = value;
                this._modifiedFlags[0x200000] = true;
            }
        }

        internal bool Attached
        {
            get
            {
                return this._flags[1];
            }
        }

        internal string ConfigKey
        {
            get
            {
                return this._configKey;
            }
        }

        public string ConfigSource
        {
            get
            {
                if (this._configSource != null)
                {
                    return this._configSource;
                }
                return string.Empty;
            }
            set
            {
                string str;
                this.VerifyIsEditable();
                if (!string.IsNullOrEmpty(value))
                {
                    str = BaseConfigurationRecord.NormalizeConfigSource(value, null);
                }
                else
                {
                    str = null;
                }
                if (str != this._configSource)
                {
                    if (this._configRecord != null)
                    {
                        this._configRecord.ChangeConfigSource(this, this._configSource, this._configSourceStreamName, str);
                    }
                    this._configSource = str;
                    this._modifiedFlags[0x40000] = true;
                }
            }
        }

        internal bool ConfigSourceModified
        {
            get
            {
                return this._modifiedFlags[0x40000];
            }
        }

        internal string ConfigSourceStreamName
        {
            get
            {
                return this._configSourceStreamName;
            }
            set
            {
                this._configSourceStreamName = value;
            }
        }

        public bool ForceSave
        {
            get
            {
                return this._flags[0x1000];
            }
            set
            {
                this.VerifyIsEditable();
                this._flags[0x1000] = value;
            }
        }

        public bool InheritInChildApplications
        {
            get
            {
                return this._flags[0x100];
            }
            set
            {
                this.VerifyIsEditable();
                this.VerifySupportsLocation();
                this._flags[0x100] = value;
            }
        }

        public bool IsDeclarationRequired
        {
            get
            {
                this.VerifyNotParentSection();
                return this._flags[4];
            }
        }

        public bool IsDeclared
        {
            get
            {
                this.VerifyNotParentSection();
                return this._flags[2];
            }
        }

        private bool IsDefinitionAllowed
        {
            get
            {
                return ((this._configRecord == null) || this._configRecord.IsDefinitionAllowed(this._allowDefinition, this._allowExeDefinition));
            }
        }

        public bool IsLocked
        {
            get
            {
                if (!this._flags[0x40] && this.IsDefinitionAllowed)
                {
                    return this._configurationSection.ElementInformation.IsLocked;
                }
                return true;
            }
        }

        public bool IsProtected
        {
            get
            {
                return (this.ProtectionProvider != null);
            }
        }

        private bool IsRuntime
        {
            get
            {
                return (this._flags[1] && (this._configRecord == null));
            }
        }

        internal bool LocationAttributesAreDefault
        {
            get
            {
                return (this._overrideMode.IsDefaultForLocationTag && this._flags[0x100]);
            }
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._name;
            }
        }

        public System.Configuration.OverrideMode OverrideMode
        {
            get
            {
                return this._overrideMode.OverrideMode;
            }
            set
            {
                this.VerifyIsEditable();
                this.VerifySupportsLocation();
                this._overrideMode.OverrideMode = value;
                this._modifiedFlags[0x200000] = true;
                switch (value)
                {
                    case System.Configuration.OverrideMode.Inherit:
                        this._flags[0x80] = this._flags[0x4000];
                        return;

                    case System.Configuration.OverrideMode.Allow:
                        this._flags[0x80] = false;
                        return;

                    case System.Configuration.OverrideMode.Deny:
                        this._flags[0x80] = true;
                        return;
                }
            }
        }

        public System.Configuration.OverrideMode OverrideModeDefault
        {
            get
            {
                return this._overrideModeDefault.OverrideMode;
            }
            set
            {
                this.VerifyIsEditable();
                this.VerifyIsEditableFactory();
                FactoryRecord record = this.FindParentFactoryRecord(false);
                if ((record != null) && (record.OverrideModeDefault.OverrideMode != value))
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_tag_name_already_defined", new object[] { this._configKey }));
                }
                if (value == System.Configuration.OverrideMode.Inherit)
                {
                    value = System.Configuration.OverrideMode.Allow;
                }
                this._overrideModeDefault.OverrideMode = value;
                this._modifiedFlags[0x100000] = true;
            }
        }

        internal bool OverrideModeDefaultModified
        {
            get
            {
                return this._modifiedFlags[0x100000];
            }
        }

        internal System.Configuration.OverrideModeSetting OverrideModeDefaultSetting
        {
            get
            {
                return this._overrideModeDefault;
            }
        }

        public System.Configuration.OverrideMode OverrideModeEffective
        {
            get
            {
                if (!this._flags[0x80])
                {
                    return System.Configuration.OverrideMode.Allow;
                }
                return System.Configuration.OverrideMode.Deny;
            }
        }

        internal System.Configuration.OverrideModeSetting OverrideModeSetting
        {
            get
            {
                return this._overrideMode;
            }
        }

        public ProtectedConfigurationProvider ProtectionProvider
        {
            get
            {
                if (!this._flags[0x800] && (this._configRecord != null))
                {
                    this._protectionProvider = this._configRecord.GetProtectionProviderFromName(this._protectionProviderName, false);
                    this._flags[0x800] = true;
                }
                return this._protectionProvider;
            }
        }

        internal string ProtectionProviderName
        {
            get
            {
                return this._protectionProviderName;
            }
        }

        internal string RawXml
        {
            get
            {
                return this._rawXml;
            }
            set
            {
                this._rawXml = value;
            }
        }

        internal bool Removed
        {
            get
            {
                return this._flags[0x400];
            }
            set
            {
                this._flags[0x400] = value;
            }
        }

        public bool RequirePermission
        {
            get
            {
                return this._flags[0x20];
            }
            set
            {
                this.VerifyIsEditable();
                this.VerifyIsEditableFactory();
                FactoryRecord record = this.FindParentFactoryRecord(false);
                if ((record != null) && (record.RequirePermission != value))
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_tag_name_already_defined", new object[] { this._configKey }));
                }
                this._flags[0x20] = value;
                this._modifiedFlags[0x20] = true;
            }
        }

        internal bool RequirePermissionModified
        {
            get
            {
                return this._modifiedFlags[0x20];
            }
        }

        public bool RestartOnExternalChanges
        {
            get
            {
                return this._flags[0x10];
            }
            set
            {
                this.VerifyIsEditable();
                this.VerifyIsEditableFactory();
                FactoryRecord record = this.FindParentFactoryRecord(false);
                if ((record != null) && (record.RestartOnExternalChanges != value))
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_tag_name_already_defined", new object[] { this._configKey }));
                }
                this._flags[0x10] = value;
                this._modifiedFlags[0x10] = true;
            }
        }

        internal bool RestartOnExternalChangesModified
        {
            get
            {
                return this._modifiedFlags[0x10];
            }
        }

        public string SectionName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._configKey;
            }
        }

        public string Type
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._typeName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw ExceptionUtil.PropertyNullOrEmpty("Type");
                }
                this.VerifyIsEditable();
                this.VerifyIsEditableFactory();
                FactoryRecord record = this.FindParentFactoryRecord(false);
                if (record != null)
                {
                    IInternalConfigHost host = null;
                    if (this._configRecord != null)
                    {
                        host = this._configRecord.Host;
                    }
                    if (!record.IsEquivalentType(host, value))
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_tag_name_already_defined", new object[] { this._configKey }));
                    }
                }
                this._typeName = value;
            }
        }
    }
}


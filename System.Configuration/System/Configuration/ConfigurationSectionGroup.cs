namespace System.Configuration
{
    using System;
    using System.Runtime;
    using System.Runtime.Versioning;

    public class ConfigurationSectionGroup
    {
        private string _configKey = string.Empty;
        private MgmtConfigurationRecord _configRecord;
        private ConfigurationSectionGroupCollection _configSectionGroups;
        private ConfigurationSectionCollection _configSections;
        private bool _declarationRequired;
        private bool _declared;
        private string _group = string.Empty;
        private bool _isRoot;
        private string _name = string.Empty;
        private string _typeName;

        internal void AttachToConfigurationRecord(MgmtConfigurationRecord configRecord, FactoryRecord factoryRecord)
        {
            this._configRecord = configRecord;
            this._configKey = factoryRecord.ConfigKey;
            this._group = factoryRecord.Group;
            this._name = factoryRecord.Name;
            this._typeName = factoryRecord.FactoryTypeName;
            if (this._typeName != null)
            {
                FactoryRecord record = null;
                if (!configRecord.Parent.IsRootConfig)
                {
                    record = configRecord.Parent.FindFactoryRecord(factoryRecord.ConfigKey, true);
                }
                this._declarationRequired = (record == null) || (record.FactoryTypeName == null);
                this._declared = configRecord.GetFactoryRecord(factoryRecord.ConfigKey, true) != null;
            }
        }

        internal void DetachFromConfigurationRecord()
        {
            if (this._configSections != null)
            {
                this._configSections.DetachFromConfigurationRecord();
            }
            if (this._configSectionGroups != null)
            {
                this._configSectionGroups.DetachFromConfigurationRecord();
            }
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
            if (this._isRoot)
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_root_section_group_cannot_be_edited"));
            }
            if ((this._configRecord != null) && this._configRecord.IsLocationConfig)
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_cannot_edit_configurationsectiongroup_in_location_config"));
            }
            if (force || !this._declarationRequired)
            {
                this._declared = force;
            }
        }

        internal void RootAttachToConfigurationRecord(MgmtConfigurationRecord configRecord)
        {
            this._configRecord = configRecord;
            this._isRoot = true;
        }

        protected internal virtual bool ShouldSerializeSectionGroupInTargetVersion(FrameworkName targetFramework)
        {
            return true;
        }

        private void VerifyIsAttachedToConfigRecord()
        {
            if (this._configRecord == null)
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_cannot_edit_configurationsectiongroup_when_not_attached"));
            }
        }

        internal bool Attached
        {
            get
            {
                return (this._configRecord != null);
            }
        }

        public bool IsDeclarationRequired
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._declarationRequired;
            }
        }

        public bool IsDeclared
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._declared;
            }
        }

        internal bool IsRoot
        {
            get
            {
                return this._isRoot;
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

        public string SectionGroupName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._configKey;
            }
        }

        public ConfigurationSectionGroupCollection SectionGroups
        {
            get
            {
                if (this._configSectionGroups == null)
                {
                    this.VerifyIsAttachedToConfigRecord();
                    this._configSectionGroups = new ConfigurationSectionGroupCollection(this._configRecord, this);
                }
                return this._configSectionGroups;
            }
        }

        public ConfigurationSectionCollection Sections
        {
            get
            {
                if (this._configSections == null)
                {
                    this.VerifyIsAttachedToConfigRecord();
                    this._configSections = new ConfigurationSectionCollection(this._configRecord, this);
                }
                return this._configSections;
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
                if (this._isRoot)
                {
                    throw new InvalidOperationException(System.Configuration.SR.GetString("Config_root_section_group_cannot_be_edited"));
                }
                string str = value;
                if (string.IsNullOrEmpty(str))
                {
                    str = null;
                }
                if (this._configRecord != null)
                {
                    if (this._configRecord.IsLocationConfig)
                    {
                        throw new InvalidOperationException(System.Configuration.SR.GetString("Config_cannot_edit_configurationsectiongroup_in_location_config"));
                    }
                    if (str != null)
                    {
                        FactoryRecord record = this.FindParentFactoryRecord(false);
                        if ((record != null) && !record.IsEquivalentType(this._configRecord.Host, str))
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_tag_name_already_defined", new object[] { this._configKey }));
                        }
                    }
                }
                this._typeName = str;
            }
        }
    }
}


namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    internal sealed class RuntimeConfigurationRecord : BaseConfigurationRecord
    {
        private static readonly SimpleBitVector32 RuntimeClassFlags = new SimpleBitVector32(0x2f);

        private RuntimeConfigurationRecord()
        {
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        protected override string CallHostDecryptSection(string encryptedXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfig)
        {
            return base.CallHostDecryptSection(encryptedXml, protectionProvider, protectedConfig);
        }

        internal static IInternalConfigRecord Create(InternalConfigRoot configRoot, IInternalConfigRecord parent, string configPath)
        {
            RuntimeConfigurationRecord record = new RuntimeConfigurationRecord();
            record.Init(configRoot, (BaseConfigurationRecord) parent, configPath, null);
            return record;
        }

        protected override object CreateSection(bool inputIsTrusted, FactoryRecord factoryRecord, SectionRecord sectionRecord, object parentConfig, ConfigXmlReader reader)
        {
            RuntimeConfigurationFactory factory = (RuntimeConfigurationFactory) factoryRecord.Factory;
            return factory.CreateSection(inputIsTrusted, this, factoryRecord, sectionRecord, parentConfig, reader);
        }

        protected override object CreateSectionFactory(FactoryRecord factoryRecord)
        {
            return new RuntimeConfigurationFactory(this, factoryRecord);
        }

        protected override object GetRuntimeObject(object result)
        {
            object obj2;
            ConfigurationSection section = result as ConfigurationSection;
            if (section == null)
            {
                return result;
            }
            try
            {
                using (base.Impersonate())
                {
                    if (this._flags[0x2000])
                    {
                        return this.GetRuntimeObjectWithFullTrust(section);
                    }
                    return this.GetRuntimeObjectWithRestrictedPermissions(section);
                }
            }
            catch (Exception exception)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_exception_in_config_section_handler", new object[] { section.SectionInformation.SectionName }), exception);
            }
            return obj2;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private object GetRuntimeObjectWithFullTrust(ConfigurationSection section)
        {
            return section.GetRuntimeObject();
        }

        private object GetRuntimeObjectWithRestrictedPermissions(ConfigurationSection section)
        {
            object runtimeObject;
            bool flag = false;
            try
            {
                PermissionSet restrictedPermissions = base.GetRestrictedPermissions();
                if (restrictedPermissions != null)
                {
                    restrictedPermissions.PermitOnly();
                    flag = true;
                }
                runtimeObject = section.GetRuntimeObject();
            }
            finally
            {
                if (flag)
                {
                    CodeAccessPermission.RevertPermitOnly();
                }
            }
            return runtimeObject;
        }

        protected override object UseParentResult(string configKey, object parentResult, SectionRecord sectionRecord)
        {
            return parentResult;
        }

        protected override SimpleBitVector32 ClassFlags
        {
            get
            {
                return RuntimeClassFlags;
            }
        }

        private class RuntimeConfigurationFactory
        {
            private ConstructorInfo _sectionCtor;
            private IConfigurationSectionHandler _sectionHandler;

            internal RuntimeConfigurationFactory(RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord)
            {
                if (factoryRecord.IsFromTrustedConfigRecord)
                {
                    this.InitWithFullTrust(configRecord, factoryRecord);
                }
                else
                {
                    this.InitWithRestrictedPermissions(configRecord, factoryRecord);
                }
            }

            private static void CheckForLockAttributes(string sectionName, XmlNode xmlNode)
            {
                XmlAttributeCollection attributes = xmlNode.Attributes;
                if (attributes != null)
                {
                    foreach (XmlAttribute attribute in attributes)
                    {
                        if (ConfigurationElement.IsLockAttributeName(attribute.Name))
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_element_locking_not_supported", new object[] { sectionName }), attribute);
                        }
                    }
                }
                foreach (XmlNode node in xmlNode.ChildNodes)
                {
                    if (xmlNode.NodeType == XmlNodeType.Element)
                    {
                        CheckForLockAttributes(sectionName, node);
                    }
                }
            }

            internal object CreateSection(bool inputIsTrusted, RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord, SectionRecord sectionRecord, object parentConfig, ConfigXmlReader reader)
            {
                if (inputIsTrusted)
                {
                    return this.CreateSectionWithFullTrust(configRecord, factoryRecord, sectionRecord, parentConfig, reader);
                }
                return this.CreateSectionWithRestrictedPermissions(configRecord, factoryRecord, sectionRecord, parentConfig, reader);
            }

            private object CreateSectionImpl(RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord, SectionRecord sectionRecord, object parentConfig, ConfigXmlReader reader)
            {
                if (this._sectionCtor != null)
                {
                    ConfigurationSection section = (ConfigurationSection) System.Configuration.TypeUtil.InvokeCtorWithReflectionPermission(this._sectionCtor);
                    section.SectionInformation.SetRuntimeConfigurationInformation(configRecord, factoryRecord, sectionRecord);
                    section.CallInit();
                    ConfigurationSection parentElement = (ConfigurationSection) parentConfig;
                    section.Reset(parentElement);
                    if (reader != null)
                    {
                        section.DeserializeSection(reader);
                    }
                    ConfigurationErrorsException errors = section.GetErrors();
                    if (errors != null)
                    {
                        throw errors;
                    }
                    section.SetReadOnly();
                    section.ResetModified();
                    return section;
                }
                if (reader != null)
                {
                    XmlNode xmlNode = ErrorInfoXmlDocument.CreateSectionXmlNode(reader);
                    CheckForLockAttributes(factoryRecord.ConfigKey, xmlNode);
                    object configContext = configRecord.Host.CreateDeprecatedConfigContext(configRecord.ConfigPath);
                    return this._sectionHandler.Create(parentConfig, configContext, xmlNode);
                }
                return null;
            }

            [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
            private object CreateSectionWithFullTrust(RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord, SectionRecord sectionRecord, object parentConfig, ConfigXmlReader reader)
            {
                return this.CreateSectionImpl(configRecord, factoryRecord, sectionRecord, parentConfig, reader);
            }

            private object CreateSectionWithRestrictedPermissions(RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord, SectionRecord sectionRecord, object parentConfig, ConfigXmlReader reader)
            {
                object obj2;
                bool flag = false;
                try
                {
                    PermissionSet restrictedPermissions = configRecord.GetRestrictedPermissions();
                    if (restrictedPermissions != null)
                    {
                        restrictedPermissions.PermitOnly();
                        flag = true;
                    }
                    obj2 = this.CreateSectionImpl(configRecord, factoryRecord, sectionRecord, parentConfig, reader);
                }
                finally
                {
                    if (flag)
                    {
                        CodeAccessPermission.RevertPermitOnly();
                    }
                }
                return obj2;
            }

            private void Init(RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord)
            {
                Type c = System.Configuration.TypeUtil.GetTypeWithReflectionPermission(configRecord.Host, factoryRecord.FactoryTypeName, true);
                if (typeof(ConfigurationSection).IsAssignableFrom(c))
                {
                    this._sectionCtor = System.Configuration.TypeUtil.GetConstructorWithReflectionPermission(c, typeof(ConfigurationSection), true);
                }
                else
                {
                    System.Configuration.TypeUtil.VerifyAssignableType(typeof(IConfigurationSectionHandler), c, true);
                    this._sectionHandler = (IConfigurationSectionHandler) System.Configuration.TypeUtil.CreateInstanceWithReflectionPermission(c);
                }
            }

            [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
            private void InitWithFullTrust(RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord)
            {
                this.Init(configRecord, factoryRecord);
            }

            private void InitWithRestrictedPermissions(RuntimeConfigurationRecord configRecord, FactoryRecord factoryRecord)
            {
                bool flag = false;
                try
                {
                    PermissionSet restrictedPermissions = configRecord.GetRestrictedPermissions();
                    if (restrictedPermissions != null)
                    {
                        restrictedPermissions.PermitOnly();
                        flag = true;
                    }
                    this.Init(configRecord, factoryRecord);
                }
                finally
                {
                    if (flag)
                    {
                        CodeAccessPermission.RevertPermitOnly();
                    }
                }
            }
        }
    }
}


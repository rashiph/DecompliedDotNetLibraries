namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Xml;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class LocalFileSettingsProvider : SettingsProvider, IApplicationSettingsProvider
    {
        private string _appName = string.Empty;
        private XmlEscaper _escaper;
        private string _prevLocalConfigFileName;
        private string _prevRoamingConfigFileName;
        private ClientSettingsStore _store;

        private Version CreateVersion(string name)
        {
            try
            {
                return new Version(name);
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (OverflowException)
            {
                return null;
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private string GetPreviousConfigFileName(bool isRoaming)
        {
            if (!ConfigurationManagerInternalFactory.Instance.SupportsUserConfig)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("UserSettingsNotSupported"));
            }
            string str = isRoaming ? this._prevRoamingConfigFileName : this._prevLocalConfigFileName;
            if (string.IsNullOrEmpty(str))
            {
                string path = isRoaming ? ConfigurationManagerInternalFactory.Instance.ExeRoamingConfigDirectory : ConfigurationManagerInternalFactory.Instance.ExeLocalConfigDirectory;
                Version version = this.CreateVersion(ConfigurationManagerInternalFactory.Instance.ExeProductVersion);
                Version version2 = null;
                DirectoryInfo info = null;
                string str3 = null;
                if (version == null)
                {
                    return null;
                }
                DirectoryInfo parent = Directory.GetParent(path);
                if (parent.Exists)
                {
                    foreach (DirectoryInfo info3 in parent.GetDirectories())
                    {
                        Version version3 = this.CreateVersion(info3.Name);
                        if ((version3 != null) && (version3 < version))
                        {
                            if (version2 == null)
                            {
                                version2 = version3;
                                info = info3;
                            }
                            else if (version3 > version2)
                            {
                                version2 = version3;
                                info = info3;
                            }
                        }
                    }
                    if (info != null)
                    {
                        str3 = Path.Combine(info.FullName, ConfigurationManagerInternalFactory.Instance.UserConfigFilename);
                    }
                    if (File.Exists(str3))
                    {
                        str = str3;
                    }
                }
                if (isRoaming)
                {
                    this._prevRoamingConfigFileName = str;
                    return str;
                }
                this._prevLocalConfigFileName = str;
            }
            return str;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        public SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property)
        {
            bool isRoaming = IsRoamingSetting(property);
            string previousConfigFileName = this.GetPreviousConfigFileName(isRoaming);
            if (!string.IsNullOrEmpty(previousConfigFileName))
            {
                SettingsPropertyCollection properties = new SettingsPropertyCollection();
                properties.Add(property);
                return this.GetSettingValuesFromFile(previousConfigFileName, this.GetSectionName(context), 1, properties)[property.Name];
            }
            return new SettingsPropertyValue(property) { PropertyValue = null };
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection properties)
        {
            SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();
            string sectionName = this.GetSectionName(context);
            IDictionary dictionary = this.Store.ReadSettings(sectionName, false);
            IDictionary dictionary2 = this.Store.ReadSettings(sectionName, true);
            ConnectionStringSettingsCollection settingss = this.Store.ReadConnectionStrings();
            foreach (SettingsProperty property in properties)
            {
                string name = property.Name;
                SettingsPropertyValue value2 = new SettingsPropertyValue(property);
                SpecialSettingAttribute attribute = property.Attributes[typeof(SpecialSettingAttribute)] as SpecialSettingAttribute;
                if ((attribute != null) ? (attribute.SpecialSetting == SpecialSetting.ConnectionString) : false)
                {
                    string str3 = sectionName + "." + name;
                    if ((settingss != null) && (settingss[str3] != null))
                    {
                        value2.PropertyValue = settingss[str3].ConnectionString;
                    }
                    else if ((property.DefaultValue != null) && (property.DefaultValue is string))
                    {
                        value2.PropertyValue = property.DefaultValue;
                    }
                    else
                    {
                        value2.PropertyValue = string.Empty;
                    }
                    value2.IsDirty = false;
                    values.Add(value2);
                }
                else
                {
                    bool flag2 = this.IsUserSetting(property);
                    if (flag2 && !ConfigurationManagerInternalFactory.Instance.SupportsUserConfig)
                    {
                        throw new ConfigurationErrorsException(System.SR.GetString("UserSettingsNotSupported"));
                    }
                    IDictionary dictionary3 = flag2 ? dictionary2 : dictionary;
                    if (dictionary3.Contains(name))
                    {
                        StoredSetting setting = (StoredSetting) dictionary3[name];
                        string innerXml = setting.Value.InnerXml;
                        if (setting.SerializeAs == SettingsSerializeAs.String)
                        {
                            innerXml = this.Escaper.Unescape(innerXml);
                        }
                        value2.SerializedValue = innerXml;
                    }
                    else if (property.DefaultValue != null)
                    {
                        value2.SerializedValue = property.DefaultValue;
                    }
                    else
                    {
                        value2.PropertyValue = null;
                    }
                    value2.IsDirty = false;
                    values.Add(value2);
                }
            }
            return values;
        }

        private string GetSectionName(SettingsContext context)
        {
            string str = (string) context["GroupName"];
            string str2 = (string) context["SettingsKey"];
            string name = str;
            if (!string.IsNullOrEmpty(str2))
            {
                name = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[] { name, str2 });
            }
            return XmlConvert.EncodeLocalName(name);
        }

        private SettingsPropertyValueCollection GetSettingValuesFromFile(string configFileName, string sectionName, bool userScoped, SettingsPropertyCollection properties)
        {
            SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();
            IDictionary dictionary = ClientSettingsStore.ReadSettingsFromFile(configFileName, sectionName, userScoped);
            foreach (SettingsProperty property in properties)
            {
                string name = property.Name;
                SettingsPropertyValue value2 = new SettingsPropertyValue(property);
                if (dictionary.Contains(name))
                {
                    StoredSetting setting = (StoredSetting) dictionary[name];
                    string innerXml = setting.Value.InnerXml;
                    if (setting.SerializeAs == SettingsSerializeAs.String)
                    {
                        innerXml = this.Escaper.Unescape(innerXml);
                    }
                    value2.SerializedValue = innerXml;
                    value2.IsDirty = true;
                    values.Add(value2);
                }
            }
            return values;
        }

        public override void Initialize(string name, NameValueCollection values)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "LocalFileSettingsProvider";
            }
            base.Initialize(name, values);
        }

        private static bool IsRoamingSetting(SettingsProperty setting)
        {
            bool flag = !ApplicationSettingsBase.IsClickOnceDeployed(AppDomain.CurrentDomain);
            bool flag2 = false;
            if (flag)
            {
                SettingsManageabilityAttribute attribute = setting.Attributes[typeof(SettingsManageabilityAttribute)] as SettingsManageabilityAttribute;
                flag2 = (attribute != null) && (0 == 0);
            }
            return flag2;
        }

        private bool IsUserSetting(SettingsProperty setting)
        {
            bool flag = setting.Attributes[typeof(UserScopedSettingAttribute)] is UserScopedSettingAttribute;
            bool flag2 = setting.Attributes[typeof(ApplicationScopedSettingAttribute)] is ApplicationScopedSettingAttribute;
            if (flag && flag2)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("BothScopeAttributes"));
            }
            if (!flag && !flag2)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("NoScopeAttributes"));
            }
            return flag;
        }

        public void Reset(SettingsContext context)
        {
            string sectionName = this.GetSectionName(context);
            this.Store.RevertToParent(sectionName, true);
            this.Store.RevertToParent(sectionName, false);
        }

        private XmlNode SerializeToXmlElement(SettingsProperty setting, SettingsPropertyValue value)
        {
            XmlElement element = new XmlDocument().CreateElement("value");
            string serializedValue = value.SerializedValue as string;
            if ((serializedValue == null) && (setting.SerializeAs == SettingsSerializeAs.Binary))
            {
                byte[] inArray = value.SerializedValue as byte[];
                if (inArray != null)
                {
                    serializedValue = Convert.ToBase64String(inArray);
                }
            }
            if (serializedValue == null)
            {
                serializedValue = string.Empty;
            }
            if (setting.SerializeAs == SettingsSerializeAs.String)
            {
                serializedValue = this.Escaper.Escape(serializedValue);
            }
            element.InnerXml = serializedValue;
            XmlNode oldChild = null;
            foreach (XmlNode node2 in element.ChildNodes)
            {
                if (node2.NodeType == XmlNodeType.XmlDeclaration)
                {
                    oldChild = node2;
                    break;
                }
            }
            if (oldChild != null)
            {
                element.RemoveChild(oldChild);
            }
            return element;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection values)
        {
            string sectionName = this.GetSectionName(context);
            IDictionary newSettings = new Hashtable();
            IDictionary dictionary2 = new Hashtable();
            foreach (SettingsPropertyValue value2 in values)
            {
                SettingsProperty property = value2.Property;
                bool flag = this.IsUserSetting(property);
                if (value2.IsDirty && flag)
                {
                    bool flag2 = IsRoamingSetting(property);
                    StoredSetting setting = new StoredSetting(property.SerializeAs, this.SerializeToXmlElement(property, value2));
                    if (flag2)
                    {
                        newSettings[property.Name] = setting;
                    }
                    else
                    {
                        dictionary2[property.Name] = setting;
                    }
                    value2.IsDirty = false;
                }
            }
            if (newSettings.Count > 0)
            {
                this.Store.WriteSettings(sectionName, true, newSettings);
            }
            if (dictionary2.Count > 0)
            {
                this.Store.WriteSettings(sectionName, false, dictionary2);
            }
        }

        public void Upgrade(SettingsContext context, SettingsPropertyCollection properties)
        {
            SettingsPropertyCollection propertys = new SettingsPropertyCollection();
            SettingsPropertyCollection propertys2 = new SettingsPropertyCollection();
            foreach (SettingsProperty property in properties)
            {
                if (IsRoamingSetting(property))
                {
                    propertys2.Add(property);
                }
                else
                {
                    propertys.Add(property);
                }
            }
            if (propertys2.Count > 0)
            {
                this.Upgrade(context, propertys2, true);
            }
            if (propertys.Count > 0)
            {
                this.Upgrade(context, propertys, false);
            }
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read)]
        private void Upgrade(SettingsContext context, SettingsPropertyCollection properties, bool isRoaming)
        {
            string previousConfigFileName = this.GetPreviousConfigFileName(isRoaming);
            if (!string.IsNullOrEmpty(previousConfigFileName))
            {
                SettingsPropertyCollection propertys = new SettingsPropertyCollection();
                foreach (SettingsProperty property in properties)
                {
                    if (!(property.Attributes[typeof(NoSettingsVersionUpgradeAttribute)] is NoSettingsVersionUpgradeAttribute))
                    {
                        propertys.Add(property);
                    }
                }
                SettingsPropertyValueCollection collection = this.GetSettingValuesFromFile(previousConfigFileName, this.GetSectionName(context), true, propertys);
                this.SetPropertyValues(context, collection);
            }
        }

        public override string ApplicationName
        {
            get
            {
                return this._appName;
            }
            set
            {
                this._appName = value;
            }
        }

        private XmlEscaper Escaper
        {
            get
            {
                if (this._escaper == null)
                {
                    this._escaper = new XmlEscaper();
                }
                return this._escaper;
            }
        }

        private ClientSettingsStore Store
        {
            get
            {
                if (this._store == null)
                {
                    this._store = new ClientSettingsStore();
                }
                return this._store;
            }
        }

        private class XmlEscaper
        {
            private XmlDocument doc = new XmlDocument();
            private XmlElement temp;

            internal XmlEscaper()
            {
                this.temp = this.doc.CreateElement("temp");
            }

            internal string Escape(string xmlString)
            {
                if (string.IsNullOrEmpty(xmlString))
                {
                    return xmlString;
                }
                this.temp.InnerText = xmlString;
                return this.temp.InnerXml;
            }

            internal string Unescape(string escapedString)
            {
                if (string.IsNullOrEmpty(escapedString))
                {
                    return escapedString;
                }
                this.temp.InnerXml = escapedString;
                return this.temp.InnerText;
            }
        }
    }
}


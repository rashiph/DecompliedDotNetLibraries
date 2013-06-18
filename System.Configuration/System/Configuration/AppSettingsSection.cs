namespace System.Configuration
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Runtime;
    using System.Xml;

    public sealed class AppSettingsSection : ConfigurationSection
    {
        private KeyValueInternalCollection _KeyValueCollection;
        private static ConfigurationProperty s_propAppSettings;
        private static ConfigurationPropertyCollection s_properties;
        private static ConfigurationProperty s_propFile;

        public AppSettingsSection()
        {
            EnsureStaticPropertyBag();
        }

        protected internal override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            string name = reader.Name;
            base.DeserializeElement(reader, serializeCollectionKey);
            if ((this.File != null) && (this.File.Length > 0))
            {
                string file;
                string source = base.ElementInformation.Source;
                if (string.IsNullOrEmpty(source))
                {
                    file = this.File;
                }
                else
                {
                    file = Path.Combine(Path.GetDirectoryName(source), this.File);
                }
                if (System.IO.File.Exists(file))
                {
                    int lineOffset = 0;
                    string rawXml = null;
                    using (Stream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (XmlUtil util = new XmlUtil(stream, file, true))
                        {
                            if (util.Reader.Name != name)
                            {
                                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_name_value_file_section_file_invalid_root", new object[] { name }), util);
                            }
                            lineOffset = util.Reader.LineNumber;
                            rawXml = util.CopySection();
                            while (!util.Reader.EOF)
                            {
                                if (util.Reader.NodeType != XmlNodeType.Comment)
                                {
                                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_source_file_format"), util);
                                }
                                util.Reader.Read();
                            }
                        }
                    }
                    ConfigXmlReader reader2 = new ConfigXmlReader(rawXml, file, lineOffset);
                    reader2.Read();
                    if (reader2.MoveToNextAttribute())
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_unrecognized_attribute", new object[] { reader2.Name }), reader2);
                    }
                    reader2.MoveToElement();
                    base.DeserializeElement(reader2, serializeCollectionKey);
                }
            }
        }

        private static ConfigurationPropertyCollection EnsureStaticPropertyBag()
        {
            if (s_properties == null)
            {
                s_propAppSettings = new ConfigurationProperty(null, typeof(KeyValueConfigurationCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
                s_propFile = new ConfigurationProperty("file", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
                ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                propertys.Add(s_propAppSettings);
                propertys.Add(s_propFile);
                s_properties = propertys;
            }
            return s_properties;
        }

        protected internal override object GetRuntimeObject()
        {
            this.SetReadOnly();
            return this.InternalSettings;
        }

        protected internal override bool IsModified()
        {
            return base.IsModified();
        }

        protected internal override void Reset(ConfigurationElement parentSection)
        {
            this._KeyValueCollection = null;
            base.Reset(parentSection);
            if (!string.IsNullOrEmpty((string) base[s_propFile]))
            {
                base.SetPropertyValue(s_propFile, null, true);
            }
        }

        protected internal override string SerializeSection(ConfigurationElement parentElement, string name, ConfigurationSaveMode saveMode)
        {
            return base.SerializeSection(parentElement, name, saveMode);
        }

        [ConfigurationProperty("file", DefaultValue="")]
        public string File
        {
            get
            {
                string str = (string) base[s_propFile];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base[s_propFile] = value;
            }
        }

        internal NameValueCollection InternalSettings
        {
            get
            {
                if (this._KeyValueCollection == null)
                {
                    this._KeyValueCollection = new KeyValueInternalCollection(this);
                }
                return this._KeyValueCollection;
            }
        }

        protected internal override ConfigurationPropertyCollection Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return EnsureStaticPropertyBag();
            }
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public KeyValueConfigurationCollection Settings
        {
            get
            {
                return (KeyValueConfigurationCollection) base[s_propAppSettings];
            }
        }
    }
}


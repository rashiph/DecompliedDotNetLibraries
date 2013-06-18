namespace System.Configuration
{
    using System;
    using System.Runtime;
    using System.Xml;

    public sealed class DefaultSection : ConfigurationSection
    {
        private bool _isModified;
        private string _rawXml = string.Empty;
        private static ConfigurationPropertyCollection s_properties;

        public DefaultSection()
        {
            EnsureStaticPropertyBag();
        }

        protected internal override void DeserializeSection(XmlReader xmlReader)
        {
            if (!xmlReader.Read() || (xmlReader.NodeType != XmlNodeType.Element))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_expected_to_find_element"), xmlReader);
            }
            this._rawXml = xmlReader.ReadOuterXml();
            this._isModified = true;
        }

        private static ConfigurationPropertyCollection EnsureStaticPropertyBag()
        {
            if (s_properties == null)
            {
                ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                s_properties = propertys;
            }
            return s_properties;
        }

        protected internal override bool IsModified()
        {
            return this._isModified;
        }

        protected internal override void Reset(ConfigurationElement parentSection)
        {
            this._rawXml = string.Empty;
            this._isModified = false;
        }

        protected internal override void ResetModified()
        {
            this._isModified = false;
        }

        protected internal override string SerializeSection(ConfigurationElement parentSection, string name, ConfigurationSaveMode saveMode)
        {
            return this._rawXml;
        }

        protected internal override ConfigurationPropertyCollection Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return EnsureStaticPropertyBag();
            }
        }
    }
}


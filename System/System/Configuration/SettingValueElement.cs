namespace System.Configuration
{
    using System;
    using System.Xml;

    public sealed class SettingValueElement : ConfigurationElement
    {
        private static ConfigurationPropertyCollection _properties;
        private XmlNode _valueXml;
        private static XmlDocument doc = new XmlDocument();
        private bool isModified;

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            this.ValueXml = doc.ReadNode(reader);
        }

        public override bool Equals(object settingValue)
        {
            SettingValueElement element = settingValue as SettingValueElement;
            return ((element != null) && object.Equals(element.ValueXml, this.ValueXml));
        }

        public override int GetHashCode()
        {
            return this.ValueXml.GetHashCode();
        }

        protected override bool IsModified()
        {
            return this.isModified;
        }

        protected override void Reset(ConfigurationElement parentElement)
        {
            base.Reset(parentElement);
            this.ValueXml = ((SettingValueElement) parentElement).ValueXml;
        }

        protected override void ResetModified()
        {
            this.isModified = false;
        }

        protected override bool SerializeToXmlElement(XmlWriter writer, string elementName)
        {
            if (this.ValueXml == null)
            {
                return false;
            }
            if (writer != null)
            {
                this.ValueXml.WriteTo(writer);
            }
            return true;
        }

        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            base.Unmerge(sourceElement, parentElement, saveMode);
            this.ValueXml = ((SettingValueElement) sourceElement).ValueXml;
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = new ConfigurationPropertyCollection();
                }
                return _properties;
            }
        }

        public XmlNode ValueXml
        {
            get
            {
                return this._valueXml;
            }
            set
            {
                this._valueXml = value;
                this.isModified = true;
            }
        }
    }
}


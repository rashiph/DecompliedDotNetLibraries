namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Xml;

    internal class SwitchElement : ConfigurationElement
    {
        private Hashtable _attributes;
        private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), "", ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propValue = new ConfigurationProperty("value", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

        static SwitchElement()
        {
            _properties.Add(_propName);
            _properties.Add(_propValue);
        }

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            this.Attributes.Add(name, value);
            return true;
        }

        protected override void PreSerialize(XmlWriter writer)
        {
            if (this._attributes != null)
            {
                IDictionaryEnumerator enumerator = this._attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string str = (string) enumerator.Value;
                    string key = (string) enumerator.Key;
                    if ((str != null) && (writer != null))
                    {
                        writer.WriteAttributeString(key, str);
                    }
                }
            }
        }

        internal void ResetProperties()
        {
            if (this._attributes != null)
            {
                this._attributes.Clear();
                _properties.Clear();
                _properties.Add(_propName);
                _properties.Add(_propValue);
            }
        }

        protected override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey)
        {
            return (base.SerializeElement(writer, serializeCollectionKey) || ((this._attributes != null) && (this._attributes.Count > 0)));
        }

        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            base.Unmerge(sourceElement, parentElement, saveMode);
            SwitchElement element = sourceElement as SwitchElement;
            if ((element != null) && (element._attributes != null))
            {
                this._attributes = element._attributes;
            }
        }

        public Hashtable Attributes
        {
            get
            {
                if (this._attributes == null)
                {
                    this._attributes = new Hashtable(StringComparer.OrdinalIgnoreCase);
                }
                return this._attributes;
            }
        }

        [ConfigurationProperty("name", DefaultValue="", IsRequired=true, IsKey=true)]
        public string Name
        {
            get
            {
                return (string) base[_propName];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("value", IsRequired=true)]
        public string Value
        {
            get
            {
                return (string) base[_propValue];
            }
        }
    }
}


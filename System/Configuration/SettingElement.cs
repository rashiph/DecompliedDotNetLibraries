namespace System.Configuration
{
    using System;
    using System.Xml;

    public sealed class SettingElement : ConfigurationElement
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), "", ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propSerializeAs = new ConfigurationProperty("serializeAs", typeof(SettingsSerializeAs), SettingsSerializeAs.String, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propValue = new ConfigurationProperty("value", typeof(SettingValueElement), null, ConfigurationPropertyOptions.IsRequired);
        private static XmlDocument doc = new XmlDocument();

        static SettingElement()
        {
            _properties.Add(_propName);
            _properties.Add(_propSerializeAs);
            _properties.Add(_propValue);
        }

        public SettingElement()
        {
        }

        public SettingElement(string name, SettingsSerializeAs serializeAs) : this()
        {
            this.Name = name;
            this.SerializeAs = serializeAs;
        }

        public override bool Equals(object settings)
        {
            SettingElement element = settings as SettingElement;
            return (((element != null) && base.Equals(settings)) && object.Equals(element.Value, this.Value));
        }

        public override int GetHashCode()
        {
            return (base.GetHashCode() ^ this.Value.GetHashCode());
        }

        internal string Key
        {
            get
            {
                return this.Name;
            }
        }

        [ConfigurationProperty("name", IsRequired=true, IsKey=true, DefaultValue="")]
        public string Name
        {
            get
            {
                return (string) base[_propName];
            }
            set
            {
                base[_propName] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("serializeAs", IsRequired=true, DefaultValue=0)]
        public SettingsSerializeAs SerializeAs
        {
            get
            {
                return (SettingsSerializeAs) base[_propSerializeAs];
            }
            set
            {
                base[_propSerializeAs] = value;
            }
        }

        [ConfigurationProperty("value", IsRequired=true, DefaultValue=null)]
        public SettingValueElement Value
        {
            get
            {
                return (SettingValueElement) base[_propValue];
            }
            set
            {
                base[_propValue] = value;
            }
        }
    }
}


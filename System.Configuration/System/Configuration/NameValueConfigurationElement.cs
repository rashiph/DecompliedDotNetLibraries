namespace System.Configuration
{
    using System;
    using System.Runtime;

    public sealed class NameValueConfigurationElement : ConfigurationElement
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), string.Empty, ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _propValue = new ConfigurationProperty("value", typeof(string), string.Empty, ConfigurationPropertyOptions.None);

        static NameValueConfigurationElement()
        {
            _properties.Add(_propName);
            _properties.Add(_propValue);
        }

        internal NameValueConfigurationElement()
        {
        }

        public NameValueConfigurationElement(string name, string value)
        {
            base[_propName] = name;
            base[_propValue] = value;
        }

        [ConfigurationProperty("name", IsKey=true, DefaultValue="")]
        public string Name
        {
            get
            {
                return (string) base[_propName];
            }
        }

        protected internal override ConfigurationPropertyCollection Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("value", DefaultValue="")]
        public string Value
        {
            get
            {
                return (string) base[_propValue];
            }
            set
            {
                base[_propValue] = value;
            }
        }
    }
}


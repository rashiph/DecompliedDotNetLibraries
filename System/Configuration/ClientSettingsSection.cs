namespace System.Configuration
{
    using System;

    public sealed class ClientSettingsSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propSettings = new ConfigurationProperty(null, typeof(SettingElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        static ClientSettingsSection()
        {
            _properties.Add(_propSettings);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public SettingElementCollection Settings
        {
            get
            {
                return (SettingElementCollection) base[_propSettings];
            }
        }
    }
}


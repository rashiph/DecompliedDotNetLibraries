namespace System.Configuration
{
    using System;
    using System.Runtime;

    public class ProtectedProviderSettings : ConfigurationElement
    {
        private ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private readonly ConfigurationProperty _propProviders = new ConfigurationProperty(null, typeof(ProviderSettingsCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        public ProtectedProviderSettings()
        {
            this._properties.Add(this._propProviders);
        }

        protected internal override ConfigurationPropertyCollection Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection=true, Options=ConfigurationPropertyOptions.IsDefaultCollection)]
        public ProviderSettingsCollection Providers
        {
            get
            {
                return (ProviderSettingsCollection) base[this._propProviders];
            }
        }
    }
}


namespace System.Web.Configuration
{
    using System;
    using System.Configuration;

    public sealed class OutputCacheSettingsSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propOutputCacheProfiles = new ConfigurationProperty("outputCacheProfiles", typeof(OutputCacheProfileCollection), null, ConfigurationPropertyOptions.None);

        static OutputCacheSettingsSection()
        {
            _properties.Add(_propOutputCacheProfiles);
        }

        [ConfigurationProperty("outputCacheProfiles")]
        public OutputCacheProfileCollection OutputCacheProfiles
        {
            get
            {
                return (OutputCacheProfileCollection) base[_propOutputCacheProfiles];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }
}


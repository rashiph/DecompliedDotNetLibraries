namespace System.Runtime.Caching.Configuration
{
    using System;
    using System.Configuration;

    public sealed class MemoryCacheSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propNamedCaches = new ConfigurationProperty("namedCaches", typeof(MemoryCacheSettingsCollection), null, ConfigurationPropertyOptions.None);

        static MemoryCacheSection()
        {
            _properties.Add(_propNamedCaches);
        }

        [ConfigurationProperty("namedCaches")]
        public MemoryCacheSettingsCollection NamedCaches
        {
            get
            {
                return (MemoryCacheSettingsCollection) base[_propNamedCaches];
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


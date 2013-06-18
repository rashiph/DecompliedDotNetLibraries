namespace System.Web.Configuration
{
    using System.Configuration;

    public sealed class SystemWebCachingSectionGroup : ConfigurationSectionGroup
    {
        [ConfigurationProperty("cache")]
        public CacheSection Cache
        {
            get
            {
                return (CacheSection) base.Sections["cache"];
            }
        }

        [ConfigurationProperty("outputCache")]
        public OutputCacheSection OutputCache
        {
            get
            {
                return (OutputCacheSection) base.Sections["outputCache"];
            }
        }

        [ConfigurationProperty("outputCacheSettings")]
        public OutputCacheSettingsSection OutputCacheSettings
        {
            get
            {
                return (OutputCacheSettingsSection) base.Sections["outputCacheSettings"];
            }
        }

        [ConfigurationProperty("sqlCacheDependency")]
        public SqlCacheDependencySection SqlCacheDependency
        {
            get
            {
                return (SqlCacheDependencySection) base.Sections["sqlCacheDependency"];
            }
        }
    }
}


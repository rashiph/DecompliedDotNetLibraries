namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web.Util;

    public sealed class UrlMappingsSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _propEnabled = new ConfigurationProperty("enabled", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propMappings = new ConfigurationProperty(null, typeof(UrlMappingCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        static UrlMappingsSection()
        {
            _properties.Add(_propMappings);
            _properties.Add(_propEnabled);
        }

        internal string HttpResolveMapping(string path)
        {
            string mappedUrl = null;
            string str2 = System.Web.Util.UrlPath.MakeVirtualPathAppRelative(path);
            UrlMapping mapping = this.UrlMappings[str2];
            if (mapping != null)
            {
                mappedUrl = mapping.MappedUrl;
            }
            return mappedUrl;
        }

        [ConfigurationProperty("enabled", DefaultValue=true)]
        public bool IsEnabled
        {
            get
            {
                return (bool) base[_propEnabled];
            }
            set
            {
                base[_propEnabled] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public UrlMappingCollection UrlMappings
        {
            get
            {
                return (UrlMappingCollection) base[_propMappings];
            }
        }
    }
}


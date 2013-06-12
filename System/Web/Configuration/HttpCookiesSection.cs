namespace System.Web.Configuration
{
    using System;
    using System.Configuration;

    public sealed class HttpCookiesSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _propDomain = new ConfigurationProperty("domain", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propHttpOnlyCookies = new ConfigurationProperty("httpOnlyCookies", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRequireSSL = new ConfigurationProperty("requireSSL", typeof(bool), false, ConfigurationPropertyOptions.None);

        static HttpCookiesSection()
        {
            _properties.Add(_propHttpOnlyCookies);
            _properties.Add(_propRequireSSL);
            _properties.Add(_propDomain);
        }

        [ConfigurationProperty("domain", DefaultValue="")]
        public string Domain
        {
            get
            {
                return (string) base[_propDomain];
            }
            set
            {
                base[_propDomain] = value;
            }
        }

        [ConfigurationProperty("httpOnlyCookies", DefaultValue=false)]
        public bool HttpOnlyCookies
        {
            get
            {
                return (bool) base[_propHttpOnlyCookies];
            }
            set
            {
                base[_propHttpOnlyCookies] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("requireSSL", DefaultValue=false)]
        public bool RequireSSL
        {
            get
            {
                return (bool) base[_propRequireSSL];
            }
            set
            {
                base[_propRequireSSL] = value;
            }
        }
    }
}


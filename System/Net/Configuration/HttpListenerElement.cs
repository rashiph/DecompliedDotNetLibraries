namespace System.Net.Configuration
{
    using System;
    using System.Configuration;

    public sealed class HttpListenerElement : ConfigurationElement
    {
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty unescapeRequestUrl = new ConfigurationProperty("unescapeRequestUrl", typeof(bool), true, ConfigurationPropertyOptions.None);
        internal const bool UnescapeRequestUrlDefaultValue = true;

        static HttpListenerElement()
        {
            properties.Add(unescapeRequestUrl);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return properties;
            }
        }

        [ConfigurationProperty("unescapeRequestUrl", DefaultValue=true, IsRequired=false)]
        public bool UnescapeRequestUrl
        {
            get
            {
                return (bool) base[unescapeRequestUrl];
            }
        }
    }
}


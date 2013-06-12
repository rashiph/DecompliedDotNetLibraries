namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Net.Cache;
    using System.Xml;

    public sealed class FtpCachePolicyElement : ConfigurationElement
    {
        private readonly ConfigurationProperty policyLevel = new ConfigurationProperty("policyLevel", typeof(RequestCacheLevel), RequestCacheLevel.Default, ConfigurationPropertyOptions.None);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private bool wasReadFromConfig;

        public FtpCachePolicyElement()
        {
            this.properties.Add(this.policyLevel);
        }

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            this.wasReadFromConfig = true;
            base.DeserializeElement(reader, serializeCollectionKey);
        }

        protected override void Reset(ConfigurationElement parentElement)
        {
            if (parentElement != null)
            {
                FtpCachePolicyElement element = (FtpCachePolicyElement) parentElement;
                this.wasReadFromConfig = element.wasReadFromConfig;
            }
            base.Reset(parentElement);
        }

        [ConfigurationProperty("policyLevel", DefaultValue=0)]
        public RequestCacheLevel PolicyLevel
        {
            get
            {
                return (RequestCacheLevel) base[this.policyLevel];
            }
            set
            {
                base[this.policyLevel] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        internal bool WasReadFromConfig
        {
            get
            {
                return this.wasReadFromConfig;
            }
        }
    }
}


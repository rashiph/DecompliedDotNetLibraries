namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Net.Cache;
    using System.Xml;

    public sealed class HttpCachePolicyElement : ConfigurationElement
    {
        private readonly ConfigurationProperty maximumAge = new ConfigurationProperty("maximumAge", typeof(TimeSpan), TimeSpan.MaxValue, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty maximumStale = new ConfigurationProperty("maximumStale", typeof(TimeSpan), TimeSpan.MinValue, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty minimumFresh = new ConfigurationProperty("minimumFresh", typeof(TimeSpan), TimeSpan.MinValue, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty policyLevel = new ConfigurationProperty("policyLevel", typeof(HttpRequestCacheLevel), HttpRequestCacheLevel.Default, ConfigurationPropertyOptions.None);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private bool wasReadFromConfig;

        public HttpCachePolicyElement()
        {
            this.properties.Add(this.maximumAge);
            this.properties.Add(this.maximumStale);
            this.properties.Add(this.minimumFresh);
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
                HttpCachePolicyElement element = (HttpCachePolicyElement) parentElement;
                this.wasReadFromConfig = element.wasReadFromConfig;
            }
            base.Reset(parentElement);
        }

        [ConfigurationProperty("maximumAge", DefaultValue="10675199.02:48:05.4775807")]
        public TimeSpan MaximumAge
        {
            get
            {
                return (TimeSpan) base[this.maximumAge];
            }
            set
            {
                base[this.maximumAge] = value;
            }
        }

        [ConfigurationProperty("maximumStale", DefaultValue="-10675199.02:48:05.4775808")]
        public TimeSpan MaximumStale
        {
            get
            {
                return (TimeSpan) base[this.maximumStale];
            }
            set
            {
                base[this.maximumStale] = value;
            }
        }

        [ConfigurationProperty("minimumFresh", DefaultValue="-10675199.02:48:05.4775808")]
        public TimeSpan MinimumFresh
        {
            get
            {
                return (TimeSpan) base[this.minimumFresh];
            }
            set
            {
                base[this.minimumFresh] = value;
            }
        }

        [ConfigurationProperty("policyLevel", IsRequired=true, DefaultValue=0)]
        public HttpRequestCacheLevel PolicyLevel
        {
            get
            {
                return (HttpRequestCacheLevel) base[this.policyLevel];
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


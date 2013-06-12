namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.Net.Cache;
    using System.Xml;

    public sealed class RequestCachingSection : ConfigurationSection
    {
        private readonly ConfigurationProperty defaultFtpCachePolicy = new ConfigurationProperty("defaultFtpCachePolicy", typeof(FtpCachePolicyElement), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty defaultHttpCachePolicy = new ConfigurationProperty("defaultHttpCachePolicy", typeof(HttpCachePolicyElement), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty defaultPolicyLevel = new ConfigurationProperty("defaultPolicyLevel", typeof(RequestCacheLevel), RequestCacheLevel.BypassCache, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty disableAllCaching = new ConfigurationProperty("disableAllCaching", typeof(bool), false, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty isPrivateCache = new ConfigurationProperty("isPrivateCache", typeof(bool), true, ConfigurationPropertyOptions.None);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private readonly ConfigurationProperty unspecifiedMaximumAge = new ConfigurationProperty("unspecifiedMaximumAge", typeof(TimeSpan), TimeSpan.FromDays(1.0), ConfigurationPropertyOptions.None);

        public RequestCachingSection()
        {
            this.properties.Add(this.disableAllCaching);
            this.properties.Add(this.defaultPolicyLevel);
            this.properties.Add(this.isPrivateCache);
            this.properties.Add(this.defaultHttpCachePolicy);
            this.properties.Add(this.defaultFtpCachePolicy);
            this.properties.Add(this.unspecifiedMaximumAge);
        }

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            bool disableAllCaching = this.DisableAllCaching;
            base.DeserializeElement(reader, serializeCollectionKey);
            if (disableAllCaching)
            {
                this.DisableAllCaching = true;
            }
        }

        protected override void PostDeserialize()
        {
            if (!base.EvaluationContext.IsMachineLevel)
            {
                try
                {
                    ExceptionHelper.WebPermissionUnrestricted.Demand();
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(System.SR.GetString("net_config_section_permission", new object[] { "requestCaching" }), exception);
                }
            }
        }

        [ConfigurationProperty("defaultFtpCachePolicy")]
        public FtpCachePolicyElement DefaultFtpCachePolicy
        {
            get
            {
                return (FtpCachePolicyElement) base[this.defaultFtpCachePolicy];
            }
        }

        [ConfigurationProperty("defaultHttpCachePolicy")]
        public HttpCachePolicyElement DefaultHttpCachePolicy
        {
            get
            {
                return (HttpCachePolicyElement) base[this.defaultHttpCachePolicy];
            }
        }

        [ConfigurationProperty("defaultPolicyLevel", DefaultValue=1)]
        public RequestCacheLevel DefaultPolicyLevel
        {
            get
            {
                return (RequestCacheLevel) base[this.defaultPolicyLevel];
            }
            set
            {
                base[this.defaultPolicyLevel] = value;
            }
        }

        [ConfigurationProperty("disableAllCaching", DefaultValue=false)]
        public bool DisableAllCaching
        {
            get
            {
                return (bool) base[this.disableAllCaching];
            }
            set
            {
                base[this.disableAllCaching] = value;
            }
        }

        [ConfigurationProperty("isPrivateCache", DefaultValue=true)]
        public bool IsPrivateCache
        {
            get
            {
                return (bool) base[this.isPrivateCache];
            }
            set
            {
                base[this.isPrivateCache] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty("unspecifiedMaximumAge", DefaultValue="1.00:00:00")]
        public TimeSpan UnspecifiedMaximumAge
        {
            get
            {
                return (TimeSpan) base[this.unspecifiedMaximumAge];
            }
            set
            {
                base[this.unspecifiedMaximumAge] = value;
            }
        }
    }
}


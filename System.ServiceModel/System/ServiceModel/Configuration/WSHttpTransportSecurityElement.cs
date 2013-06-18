namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Security.Authentication.ExtendedProtection.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public sealed class WSHttpTransportSecurityElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(HttpTransportSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.ClientCredentialType = this.ClientCredentialType;
            security.ProxyCredentialType = this.ProxyCredentialType;
            security.Realm = this.Realm;
            security.ExtendedProtectionPolicy = ChannelBindingUtility.BuildPolicy(this.ExtendedProtectionPolicy);
        }

        internal void InitializeFrom(HttpTransportSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            this.ClientCredentialType = security.ClientCredentialType;
            this.ProxyCredentialType = security.ProxyCredentialType;
            this.Realm = security.Realm;
            ChannelBindingUtility.InitializeFrom(security.ExtendedProtectionPolicy, this.ExtendedProtectionPolicy);
        }

        [ServiceModelEnumValidator(typeof(HttpClientCredentialTypeHelper)), ConfigurationProperty("clientCredentialType", DefaultValue=4)]
        public HttpClientCredentialType ClientCredentialType
        {
            get
            {
                return (HttpClientCredentialType) base["clientCredentialType"];
            }
            set
            {
                base["clientCredentialType"] = value;
            }
        }

        [ConfigurationProperty("extendedProtectionPolicy")]
        public ExtendedProtectionPolicyElement ExtendedProtectionPolicy
        {
            get
            {
                return (ExtendedProtectionPolicyElement) base["extendedProtectionPolicy"];
            }
            private set
            {
                base["extendedProtectionPolicy"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("clientCredentialType", typeof(HttpClientCredentialType), HttpClientCredentialType.Windows, null, new ServiceModelEnumValidator(typeof(HttpClientCredentialTypeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("proxyCredentialType", typeof(HttpProxyCredentialType), HttpProxyCredentialType.None, null, new ServiceModelEnumValidator(typeof(HttpProxyCredentialTypeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("extendedProtectionPolicy", typeof(ExtendedProtectionPolicyElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("realm", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ServiceModelEnumValidator(typeof(HttpProxyCredentialTypeHelper)), ConfigurationProperty("proxyCredentialType", DefaultValue=0)]
        public HttpProxyCredentialType ProxyCredentialType
        {
            get
            {
                return (HttpProxyCredentialType) base["proxyCredentialType"];
            }
            set
            {
                base["proxyCredentialType"] = value;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("realm", DefaultValue="")]
        public string Realm
        {
            get
            {
                return (string) base["realm"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["realm"] = value;
            }
        }
    }
}


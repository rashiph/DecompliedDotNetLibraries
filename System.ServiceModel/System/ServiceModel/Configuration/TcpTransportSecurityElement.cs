namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Net.Security;
    using System.Security.Authentication.ExtendedProtection.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;

    public sealed class TcpTransportSecurityElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(TcpTransportSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.ClientCredentialType = this.ClientCredentialType;
            security.ProtectionLevel = this.ProtectionLevel;
            security.ExtendedProtectionPolicy = ChannelBindingUtility.BuildPolicy(this.ExtendedProtectionPolicy);
        }

        internal void InitializeFrom(TcpTransportSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            this.ClientCredentialType = security.ClientCredentialType;
            this.ProtectionLevel = security.ProtectionLevel;
            ChannelBindingUtility.InitializeFrom(security.ExtendedProtectionPolicy, this.ExtendedProtectionPolicy);
        }

        [ServiceModelEnumValidator(typeof(TcpClientCredentialTypeHelper)), ConfigurationProperty("clientCredentialType", DefaultValue=1)]
        public TcpClientCredentialType ClientCredentialType
        {
            get
            {
                return (TcpClientCredentialType) base["clientCredentialType"];
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
                    propertys.Add(new ConfigurationProperty("clientCredentialType", typeof(TcpClientCredentialType), TcpClientCredentialType.Windows, null, new ServiceModelEnumValidator(typeof(TcpClientCredentialTypeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("protectionLevel", typeof(System.Net.Security.ProtectionLevel), System.Net.Security.ProtectionLevel.EncryptAndSign, null, new ServiceModelEnumValidator(typeof(ProtectionLevelHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("extendedProtectionPolicy", typeof(ExtendedProtectionPolicyElement), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ServiceModelEnumValidator(typeof(ProtectionLevelHelper)), ConfigurationProperty("protectionLevel", DefaultValue=2)]
        public System.Net.Security.ProtectionLevel ProtectionLevel
        {
            get
            {
                return (System.Net.Security.ProtectionLevel) base["protectionLevel"];
            }
            set
            {
                base["protectionLevel"] = value;
            }
        }
    }
}


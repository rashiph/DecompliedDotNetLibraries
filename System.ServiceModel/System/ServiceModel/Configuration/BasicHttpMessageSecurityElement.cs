namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    public sealed class BasicHttpMessageSecurityElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(BasicHttpMessageSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.ClientCredentialType = this.ClientCredentialType;
            if (base.ElementInformation.Properties["algorithmSuite"].ValueOrigin != PropertyValueOrigin.Default)
            {
                security.AlgorithmSuite = this.AlgorithmSuite;
            }
        }

        internal void InitializeFrom(BasicHttpMessageSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            this.ClientCredentialType = security.ClientCredentialType;
            this.AlgorithmSuite = security.AlgorithmSuite;
        }

        [TypeConverter(typeof(SecurityAlgorithmSuiteConverter)), ConfigurationProperty("algorithmSuite", DefaultValue="Default")]
        public SecurityAlgorithmSuite AlgorithmSuite
        {
            get
            {
                return (SecurityAlgorithmSuite) base["algorithmSuite"];
            }
            set
            {
                base["algorithmSuite"] = value;
            }
        }

        [ServiceModelEnumValidator(typeof(BasicHttpMessageCredentialTypeHelper)), ConfigurationProperty("clientCredentialType", DefaultValue=0)]
        public BasicHttpMessageCredentialType ClientCredentialType
        {
            get
            {
                return (BasicHttpMessageCredentialType) base["clientCredentialType"];
            }
            set
            {
                base["clientCredentialType"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("clientCredentialType", typeof(BasicHttpMessageCredentialType), BasicHttpMessageCredentialType.UserName, null, new ServiceModelEnumValidator(typeof(BasicHttpMessageCredentialTypeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("algorithmSuite", typeof(SecurityAlgorithmSuite), "Default", new SecurityAlgorithmSuiteConverter(), null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}


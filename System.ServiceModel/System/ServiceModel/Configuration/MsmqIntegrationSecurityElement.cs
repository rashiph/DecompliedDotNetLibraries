namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.MsmqIntegration;

    public sealed class MsmqIntegrationSecurityElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(MsmqIntegrationSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.Mode = this.Mode;
            this.Transport.ApplyConfiguration(security.Transport);
        }

        internal void InitializeFrom(MsmqIntegrationSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            this.Mode = security.Mode;
            this.Transport.InitializeFrom(security.Transport);
        }

        [ConfigurationProperty("mode", DefaultValue=1), ServiceModelEnumValidator(typeof(MsmqIntegrationSecurityModeHelper))]
        public MsmqIntegrationSecurityMode Mode
        {
            get
            {
                return (MsmqIntegrationSecurityMode) base["mode"];
            }
            set
            {
                base["mode"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("mode", typeof(MsmqIntegrationSecurityMode), MsmqIntegrationSecurityMode.Transport, null, new ServiceModelEnumValidator(typeof(MsmqIntegrationSecurityModeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("transport", typeof(MsmqTransportSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("transport")]
        public MsmqTransportSecurityElement Transport
        {
            get
            {
                return (MsmqTransportSecurityElement) base["transport"];
            }
        }
    }
}


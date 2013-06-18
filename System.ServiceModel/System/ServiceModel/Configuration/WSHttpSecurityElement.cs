namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    public sealed class WSHttpSecurityElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(WSHttpSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.Mode = this.Mode;
            this.Transport.ApplyConfiguration(security.Transport);
            this.Message.ApplyConfiguration(security.Message);
        }

        internal void InitializeFrom(WSHttpSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            this.Mode = security.Mode;
            this.Transport.InitializeFrom(security.Transport);
            this.Message.InitializeFrom(security.Message);
        }

        [ConfigurationProperty("message")]
        public NonDualMessageSecurityOverHttpElement Message
        {
            get
            {
                return (NonDualMessageSecurityOverHttpElement) base["message"];
            }
        }

        [ServiceModelEnumValidator(typeof(SecurityModeHelper)), ConfigurationProperty("mode", DefaultValue=2)]
        public SecurityMode Mode
        {
            get
            {
                return (SecurityMode) base["mode"];
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
                    propertys.Add(new ConfigurationProperty("mode", typeof(SecurityMode), SecurityMode.Message, null, new ServiceModelEnumValidator(typeof(SecurityModeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("transport", typeof(WSHttpTransportSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("message", typeof(NonDualMessageSecurityOverHttpElement), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("transport")]
        public WSHttpTransportSecurityElement Transport
        {
            get
            {
                return (WSHttpTransportSecurityElement) base["transport"];
            }
        }
    }
}


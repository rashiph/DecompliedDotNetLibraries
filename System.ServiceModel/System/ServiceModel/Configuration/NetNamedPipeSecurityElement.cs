namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    public sealed class NetNamedPipeSecurityElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(NetNamedPipeSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.Mode = this.Mode;
            this.Transport.ApplyConfiguration(security.Transport);
        }

        internal void InitializeFrom(NetNamedPipeSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            this.Mode = security.Mode;
            this.Transport.InitializeFrom(security.Transport);
        }

        [ConfigurationProperty("mode", DefaultValue=1), ServiceModelEnumValidator(typeof(NetNamedPipeSecurityModeHelper))]
        public NetNamedPipeSecurityMode Mode
        {
            get
            {
                return (NetNamedPipeSecurityMode) base["mode"];
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
                    propertys.Add(new ConfigurationProperty("mode", typeof(NetNamedPipeSecurityMode), NetNamedPipeSecurityMode.Transport, null, new ServiceModelEnumValidator(typeof(NetNamedPipeSecurityModeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("transport", typeof(NamedPipeTransportSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("transport")]
        public NamedPipeTransportSecurityElement Transport
        {
            get
            {
                return (NamedPipeTransportSecurityElement) base["transport"];
            }
        }
    }
}


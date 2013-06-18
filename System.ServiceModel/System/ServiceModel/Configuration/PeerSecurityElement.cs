namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    public sealed class PeerSecurityElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(PeerSecuritySettings security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.Mode = this.Mode;
            if (security.Mode != SecurityMode.None)
            {
                this.Transport.ApplyConfiguration(security.Transport);
            }
        }

        internal void CopyFrom(PeerSecurityElement source)
        {
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            this.Mode = source.Mode;
            if (source.Mode != SecurityMode.None)
            {
                this.Transport.CopyFrom(source.Transport);
            }
        }

        internal void InitializeFrom(PeerSecuritySettings security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            this.Mode = security.Mode;
            if (security.Mode != SecurityMode.None)
            {
                this.Transport.InitializeFrom(security.Transport);
            }
        }

        [ConfigurationProperty("mode", DefaultValue=1), ServiceModelEnumValidator(typeof(SecurityModeHelper))]
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
                    propertys.Add(new ConfigurationProperty("mode", typeof(SecurityMode), SecurityMode.Transport, null, new ServiceModelEnumValidator(typeof(SecurityModeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("transport", typeof(PeerTransportSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("transport")]
        public PeerTransportSecurityElement Transport
        {
            get
            {
                return (PeerTransportSecurityElement) base["transport"];
            }
        }
    }
}


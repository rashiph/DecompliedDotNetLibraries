namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Security.Authentication.ExtendedProtection.Configuration;
    using System.ServiceModel.Channels;

    public sealed class TcpTransportElement : ConnectionOrientedTransportElement
    {
        private ConfigurationPropertyCollection properties;

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            TcpTransportBindingElement element = (TcpTransportBindingElement) bindingElement;
            element.ListenBacklog = this.ListenBacklog;
            element.PortSharingEnabled = this.PortSharingEnabled;
            element.TeredoEnabled = this.TeredoEnabled;
            this.ConnectionPoolSettings.ApplyConfiguration(element.ConnectionPoolSettings);
            element.ExtendedProtectionPolicy = ChannelBindingUtility.BuildPolicy(this.ExtendedProtectionPolicy);
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            TcpTransportElement element = (TcpTransportElement) from;
            this.ListenBacklog = element.ListenBacklog;
            this.PortSharingEnabled = element.PortSharingEnabled;
            this.TeredoEnabled = element.TeredoEnabled;
            this.ConnectionPoolSettings.CopyFrom(element.ConnectionPoolSettings);
            ChannelBindingUtility.CopyFrom(element.ExtendedProtectionPolicy, this.ExtendedProtectionPolicy);
        }

        protected override TransportBindingElement CreateDefaultBindingElement()
        {
            return new TcpTransportBindingElement();
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            TcpTransportBindingElement element = (TcpTransportBindingElement) bindingElement;
            this.ListenBacklog = element.ListenBacklog;
            this.PortSharingEnabled = element.PortSharingEnabled;
            this.TeredoEnabled = element.TeredoEnabled;
            this.ConnectionPoolSettings.InitializeFrom(element.ConnectionPoolSettings);
            ChannelBindingUtility.InitializeFrom(element.ExtendedProtectionPolicy, this.ExtendedProtectionPolicy);
        }

        public override System.Type BindingElementType
        {
            get
            {
                return typeof(TcpTransportBindingElement);
            }
        }

        [ConfigurationProperty("connectionPoolSettings")]
        public TcpConnectionPoolSettingsElement ConnectionPoolSettings
        {
            get
            {
                return (TcpConnectionPoolSettingsElement) base["connectionPoolSettings"];
            }
            set
            {
                base["connectionPoolSettings"] = value;
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

        [ConfigurationProperty("listenBacklog", DefaultValue=10), IntegerValidator(MinValue=1)]
        public int ListenBacklog
        {
            get
            {
                return (int) base["listenBacklog"];
            }
            set
            {
                base["listenBacklog"] = value;
            }
        }

        [ConfigurationProperty("portSharingEnabled", DefaultValue=false)]
        public bool PortSharingEnabled
        {
            get
            {
                return (bool) base["portSharingEnabled"];
            }
            set
            {
                base["portSharingEnabled"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("listenBacklog", typeof(int), 10, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("portSharingEnabled", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("teredoEnabled", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("connectionPoolSettings", typeof(TcpConnectionPoolSettingsElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("extendedProtectionPolicy", typeof(ExtendedProtectionPolicyElement), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("teredoEnabled", DefaultValue=false)]
        public bool TeredoEnabled
        {
            get
            {
                return (bool) base["teredoEnabled"];
            }
            set
            {
                base["teredoEnabled"] = value;
            }
        }
    }
}


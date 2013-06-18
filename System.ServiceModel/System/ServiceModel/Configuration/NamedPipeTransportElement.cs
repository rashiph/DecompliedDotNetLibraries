namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;

    public sealed class NamedPipeTransportElement : ConnectionOrientedTransportElement
    {
        private ConfigurationPropertyCollection properties;

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            NamedPipeTransportBindingElement element = (NamedPipeTransportBindingElement) bindingElement;
            this.ConnectionPoolSettings.ApplyConfiguration(element.ConnectionPoolSettings);
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            NamedPipeTransportElement element = (NamedPipeTransportElement) from;
            this.ConnectionPoolSettings.CopyFrom(element.ConnectionPoolSettings);
        }

        protected override TransportBindingElement CreateDefaultBindingElement()
        {
            return new NamedPipeTransportBindingElement();
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            NamedPipeTransportBindingElement element = (NamedPipeTransportBindingElement) bindingElement;
            this.ConnectionPoolSettings.InitializeFrom(element.ConnectionPoolSettings);
        }

        public override System.Type BindingElementType
        {
            get
            {
                return typeof(NamedPipeTransportBindingElement);
            }
        }

        [ConfigurationProperty("connectionPoolSettings")]
        public NamedPipeConnectionPoolSettingsElement ConnectionPoolSettings
        {
            get
            {
                return (NamedPipeConnectionPoolSettingsElement) base["connectionPoolSettings"];
            }
            set
            {
                base["connectionPoolSettings"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("connectionPoolSettings", typeof(NamedPipeConnectionPoolSettingsElement), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}


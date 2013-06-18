namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.MsmqIntegration;

    public sealed class MsmqIntegrationElement : MsmqElementBase
    {
        private ConfigurationPropertyCollection properties;

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            System.ServiceModel.MsmqIntegration.MsmqIntegrationBindingElement element = bindingElement as System.ServiceModel.MsmqIntegration.MsmqIntegrationBindingElement;
            element.SerializationFormat = this.SerializationFormat;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            MsmqIntegrationElement element = from as MsmqIntegrationElement;
            if (element != null)
            {
                this.SerializationFormat = element.SerializationFormat;
            }
        }

        protected override TransportBindingElement CreateDefaultBindingElement()
        {
            return new System.ServiceModel.MsmqIntegration.MsmqIntegrationBindingElement();
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            System.ServiceModel.MsmqIntegration.MsmqIntegrationBindingElement element = bindingElement as System.ServiceModel.MsmqIntegration.MsmqIntegrationBindingElement;
            this.SerializationFormat = element.SerializationFormat;
        }

        public override System.Type BindingElementType
        {
            get
            {
                return typeof(System.ServiceModel.MsmqIntegration.MsmqIntegrationBindingElement);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("serializationFormat", typeof(MsmqMessageSerializationFormat), MsmqMessageSerializationFormat.Xml, null, new ServiceModelEnumValidator(typeof(MsmqMessageSerializationFormatHelper)), ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("serializationFormat", DefaultValue=0), ServiceModelEnumValidator(typeof(MsmqMessageSerializationFormatHelper))]
        public MsmqMessageSerializationFormat SerializationFormat
        {
            get
            {
                return (MsmqMessageSerializationFormat) base["serializationFormat"];
            }
            set
            {
                base["serializationFormat"] = value;
            }
        }
    }
}


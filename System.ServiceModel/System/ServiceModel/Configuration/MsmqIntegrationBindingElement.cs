namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.MsmqIntegration;

    public class MsmqIntegrationBindingElement : System.ServiceModel.Configuration.MsmqBindingElementBase
    {
        private ConfigurationPropertyCollection properties;

        public MsmqIntegrationBindingElement() : this(null)
        {
        }

        public MsmqIntegrationBindingElement(string name) : base(name)
        {
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            MsmqIntegrationBinding binding2 = (MsmqIntegrationBinding) binding;
            this.SerializationFormat = binding2.SerializationFormat;
            this.Security.InitializeFrom(binding2.Security);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            base.OnApplyConfiguration(binding);
            MsmqIntegrationBinding binding2 = (MsmqIntegrationBinding) binding;
            binding2.SerializationFormat = this.SerializationFormat;
            this.Security.ApplyConfiguration(binding2.Security);
        }

        protected override System.Type BindingElementType
        {
            get
            {
                return typeof(MsmqIntegrationBinding);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("security", typeof(MsmqIntegrationSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("serializationFormat", typeof(MsmqMessageSerializationFormat), MsmqMessageSerializationFormat.Xml, null, new ServiceModelEnumValidator(typeof(MsmqMessageSerializationFormatHelper)), ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("security")]
        public MsmqIntegrationSecurityElement Security
        {
            get
            {
                return (MsmqIntegrationSecurityElement) base["security"];
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


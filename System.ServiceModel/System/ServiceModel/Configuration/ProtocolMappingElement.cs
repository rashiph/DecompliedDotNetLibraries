namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    public sealed class ProtocolMappingElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        public ProtocolMappingElement()
        {
        }

        public ProtocolMappingElement(string schemeType, string binding, string bindingConfiguration)
        {
            if (string.IsNullOrEmpty(schemeType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("schemeType");
            }
            this.Scheme = schemeType;
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            this.Binding = binding;
            this.BindingConfiguration = bindingConfiguration;
        }

        [ConfigurationProperty("binding", Options=ConfigurationPropertyOptions.IsRequired), StringValidator(MinLength=1)]
        public string Binding
        {
            get
            {
                return (string) base["binding"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["binding"] = value;
            }
        }

        [ConfigurationProperty("bindingConfiguration", Options=ConfigurationPropertyOptions.None), StringValidator(MinLength=0)]
        public string BindingConfiguration
        {
            get
            {
                return (string) base["bindingConfiguration"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["bindingConfiguration"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("scheme", typeof(string), null, null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired));
                    propertys.Add(new ConfigurationProperty("binding", typeof(string), null, null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsRequired));
                    propertys.Add(new ConfigurationProperty("bindingConfiguration", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [StringValidator(MinLength=1), ConfigurationProperty("scheme", Options=ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        public string Scheme
        {
            get
            {
                return (string) base["scheme"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["scheme"] = value;
            }
        }
    }
}


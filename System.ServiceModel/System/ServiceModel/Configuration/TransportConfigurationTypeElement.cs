namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    public sealed class TransportConfigurationTypeElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        public TransportConfigurationTypeElement()
        {
        }

        public TransportConfigurationTypeElement(string name) : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            this.Name = name;
        }

        public TransportConfigurationTypeElement(string name, string transportConfigurationTypeName) : this(name)
        {
            if (string.IsNullOrEmpty(transportConfigurationTypeName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("transportConfigurationTypeName");
            }
            this.TransportConfigurationType = transportConfigurationTypeName;
        }

        [ConfigurationProperty("name", Options=ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired), StringValidator(MinLength=1)]
        public string Name
        {
            get
            {
                return (string) base["name"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["name"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("name", typeof(string), null, null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired));
                    propertys.Add(new ConfigurationProperty("transportConfigurationType", typeof(string), null, null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsRequired));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("transportConfigurationType", Options=ConfigurationPropertyOptions.IsRequired), StringValidator(MinLength=1)]
        public string TransportConfigurationType
        {
            get
            {
                return (string) base["transportConfigurationType"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["transportConfigurationType"] = value;
            }
        }
    }
}


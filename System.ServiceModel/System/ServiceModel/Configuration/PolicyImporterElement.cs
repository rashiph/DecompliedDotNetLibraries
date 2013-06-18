namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Description;

    public sealed class PolicyImporterElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        public PolicyImporterElement()
        {
        }

        public PolicyImporterElement(string type)
        {
            this.Type = type;
        }

        public PolicyImporterElement(System.Type type)
        {
            new SubclassTypeValidator(typeof(IPolicyImportExtension)).Validate(type);
            this.Type = type.AssemblyQualifiedName;
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("type", typeof(string), null, null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("type", Options=ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired), StringValidator(MinLength=1)]
        public string Type
        {
            get
            {
                return (string) base["type"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["type"] = value;
            }
        }
    }
}


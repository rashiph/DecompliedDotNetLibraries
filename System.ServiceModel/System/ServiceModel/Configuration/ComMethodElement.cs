namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    public sealed class ComMethodElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        public ComMethodElement()
        {
        }

        public ComMethodElement(string method) : this()
        {
            this.ExposedMethod = method;
        }

        [StringValidator(MinLength=1), ConfigurationProperty("exposedMethod", Options=ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        public string ExposedMethod
        {
            get
            {
                return (string) base["exposedMethod"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["exposedMethod"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("exposedMethod", typeof(string), null, null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}


namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    public sealed class BaseAddressElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        [ConfigurationProperty("baseAddress", Options=ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired), StringValidator(MinLength=1)]
        public string BaseAddress
        {
            get
            {
                return (string) base["baseAddress"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["baseAddress"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("baseAddress", typeof(string), null, null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}


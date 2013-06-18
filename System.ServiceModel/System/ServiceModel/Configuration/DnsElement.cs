namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    public sealed class DnsElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("value", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("value", DefaultValue=""), StringValidator(MinLength=0)]
        public string Value
        {
            get
            {
                return (string) base["value"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["value"] = value;
            }
        }
    }
}


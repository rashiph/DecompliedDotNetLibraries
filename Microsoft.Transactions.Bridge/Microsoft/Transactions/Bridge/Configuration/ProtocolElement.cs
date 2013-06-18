namespace Microsoft.Transactions.Bridge.Configuration
{
    using System;
    using System.Configuration;

    internal sealed class ProtocolElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("type", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("type", DefaultValue="", Options=ConfigurationPropertyOptions.IsKey)]
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


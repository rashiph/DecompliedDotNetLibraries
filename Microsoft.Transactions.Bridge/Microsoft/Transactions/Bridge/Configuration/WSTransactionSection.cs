namespace Microsoft.Transactions.Bridge.Configuration
{
    using System;
    using System.Configuration;

    internal sealed class WSTransactionSection : ConfigurationSection
    {
        private ConfigurationPropertyCollection properties;

        [ConfigurationProperty("addressPrefix", DefaultValue="WsatService", Options=ConfigurationPropertyOptions.None), StringValidator(MinLength=0)]
        public string AddressPrefix
        {
            get
            {
                return (string) base["addressPrefix"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["addressPrefix"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("addressPrefix", typeof(string), "WsatService", null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}


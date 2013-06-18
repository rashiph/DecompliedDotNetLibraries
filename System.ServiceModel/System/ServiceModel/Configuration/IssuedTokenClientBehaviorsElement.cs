namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    public sealed class IssuedTokenClientBehaviorsElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        [StringValidator(MinLength=0), ConfigurationProperty("behaviorConfiguration", DefaultValue="")]
        public string BehaviorConfiguration
        {
            get
            {
                return (string) base["behaviorConfiguration"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["behaviorConfiguration"] = value;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("issuerAddress", DefaultValue="", Options=ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        public string IssuerAddress
        {
            get
            {
                return (string) base["issuerAddress"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["issuerAddress"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("issuerAddress", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired));
                    propertys.Add(new ConfigurationProperty("behaviorConfiguration", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}


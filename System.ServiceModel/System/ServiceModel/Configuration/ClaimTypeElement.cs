namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    public sealed class ClaimTypeElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        public ClaimTypeElement()
        {
        }

        public ClaimTypeElement(string claimType, bool isOptional)
        {
            this.ClaimType = claimType;
            this.IsOptional = isOptional;
        }

        [ConfigurationProperty("claimType", DefaultValue="", Options=ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired), StringValidator(MinLength=0)]
        public string ClaimType
        {
            get
            {
                return (string) base["claimType"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["claimType"] = value;
            }
        }

        [ConfigurationProperty("isOptional", DefaultValue=false)]
        public bool IsOptional
        {
            get
            {
                return (bool) base["isOptional"];
            }
            set
            {
                base["isOptional"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("claimType", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired));
                    propertys.Add(new ConfigurationProperty("isOptional", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}


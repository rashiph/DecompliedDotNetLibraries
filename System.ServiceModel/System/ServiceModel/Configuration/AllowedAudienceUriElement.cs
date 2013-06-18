namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    public sealed class AllowedAudienceUriElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        [ConfigurationProperty("allowedAudienceUri", Options=ConfigurationPropertyOptions.IsKey), StringValidator(MinLength=1)]
        public string AllowedAudienceUri
        {
            get
            {
                return (string) base["allowedAudienceUri"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["allowedAudienceUri"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("allowedAudienceUri", typeof(string), null, null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}


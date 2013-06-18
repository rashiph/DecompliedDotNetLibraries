namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    public sealed class BaseAddressPrefixFilterElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        public BaseAddressPrefixFilterElement()
        {
        }

        public BaseAddressPrefixFilterElement(Uri prefix) : this()
        {
            if (prefix == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("prefix");
            }
            this.Prefix = prefix;
        }

        [ConfigurationProperty("prefix", Options=ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        public Uri Prefix
        {
            get
            {
                return (Uri) base["prefix"];
            }
            set
            {
                base["prefix"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("prefix", typeof(Uri), null, null, null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}


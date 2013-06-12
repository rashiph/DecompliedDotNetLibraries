namespace System.Security.Authentication.ExtendedProtection.Configuration
{
    using System;
    using System.Configuration;

    public sealed class ServiceNameElement : ConfigurationElement
    {
        private readonly ConfigurationProperty name = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        public ServiceNameElement()
        {
            this.properties.Add(this.name);
        }

        internal string Key
        {
            get
            {
                return this.Name;
            }
        }

        [ConfigurationProperty("name")]
        public string Name
        {
            get
            {
                return (string) base[this.name];
            }
            set
            {
                base[this.name] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }
    }
}


namespace System.Net.Configuration
{
    using System;
    using System.Configuration;

    public sealed class Ipv6Element : ConfigurationElement
    {
        private readonly ConfigurationProperty enabled = new ConfigurationProperty("enabled", typeof(bool), false, ConfigurationPropertyOptions.None);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        public Ipv6Element()
        {
            this.properties.Add(this.enabled);
        }

        [ConfigurationProperty("enabled", DefaultValue=false)]
        public bool Enabled
        {
            get
            {
                return (bool) base[this.enabled];
            }
            set
            {
                base[this.enabled] = value;
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


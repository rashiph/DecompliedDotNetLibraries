namespace System.Configuration
{
    using System;

    public sealed class IriParsingElement : ConfigurationElement
    {
        private readonly ConfigurationProperty enabled = new ConfigurationProperty("enabled", typeof(bool), false, ConfigurationPropertyOptions.None);
        internal const bool EnabledDefaultValue = false;
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        public IriParsingElement()
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


namespace System.Net.Configuration
{
    using System;
    using System.Configuration;

    public sealed class ModuleElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private readonly ConfigurationProperty type = new ConfigurationProperty("type", typeof(string), null, ConfigurationPropertyOptions.None);

        public ModuleElement()
        {
            this.properties.Add(this.type);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty("type")]
        public string Type
        {
            get
            {
                return (string) base[this.type];
            }
            set
            {
                base[this.type] = value;
            }
        }
    }
}


namespace System.Xml.Serialization.Configuration
{
    using System;
    using System.Configuration;

    public sealed class XmlSerializerSection : ConfigurationSection
    {
        private readonly ConfigurationProperty checkDeserializeAdvances = new ConfigurationProperty("checkDeserializeAdvances", typeof(bool), false, ConfigurationPropertyOptions.None);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private readonly ConfigurationProperty tempFilesLocation = new ConfigurationProperty("tempFilesLocation", typeof(string), null, null, new RootedPathValidator(), ConfigurationPropertyOptions.None);

        public XmlSerializerSection()
        {
            this.properties.Add(this.checkDeserializeAdvances);
            this.properties.Add(this.tempFilesLocation);
        }

        [ConfigurationProperty("checkDeserializeAdvances", DefaultValue=false)]
        public bool CheckDeserializeAdvances
        {
            get
            {
                return (bool) base[this.checkDeserializeAdvances];
            }
            set
            {
                base[this.checkDeserializeAdvances] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty("tempFilesLocation", DefaultValue=null)]
        public string TempFilesLocation
        {
            get
            {
                return (string) base[this.tempFilesLocation];
            }
            set
            {
                base[this.tempFilesLocation] = value;
            }
        }
    }
}


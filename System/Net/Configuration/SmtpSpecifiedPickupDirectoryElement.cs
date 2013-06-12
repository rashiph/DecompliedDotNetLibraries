namespace System.Net.Configuration
{
    using System;
    using System.Configuration;

    public sealed class SmtpSpecifiedPickupDirectoryElement : ConfigurationElement
    {
        private readonly ConfigurationProperty pickupDirectoryLocation = new ConfigurationProperty("pickupDirectoryLocation", typeof(string), null, ConfigurationPropertyOptions.None);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        public SmtpSpecifiedPickupDirectoryElement()
        {
            this.properties.Add(this.pickupDirectoryLocation);
        }

        [ConfigurationProperty("pickupDirectoryLocation")]
        public string PickupDirectoryLocation
        {
            get
            {
                return (string) base[this.pickupDirectoryLocation];
            }
            set
            {
                base[this.pickupDirectoryLocation] = value;
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


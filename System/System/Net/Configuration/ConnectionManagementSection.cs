namespace System.Net.Configuration
{
    using System;
    using System.Configuration;

    public sealed class ConnectionManagementSection : ConfigurationSection
    {
        private readonly ConfigurationProperty connectionManagement = new ConfigurationProperty(null, typeof(ConnectionManagementElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        public ConnectionManagementSection()
        {
            this.properties.Add(this.connectionManagement);
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public ConnectionManagementElementCollection ConnectionManagement
        {
            get
            {
                return (ConnectionManagementElementCollection) base[this.connectionManagement];
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


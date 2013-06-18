namespace System.Configuration
{
    using System;
    using System.Runtime;

    public sealed class ConnectionStringSettings : ConfigurationElement
    {
        private static readonly ConfigurationProperty _propConnectionString = new ConfigurationProperty("connectionString", typeof(string), "", ConfigurationPropertyOptions.IsRequired);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), null, null, ConfigurationProperty.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propProviderName = new ConfigurationProperty("providerName", typeof(string), string.Empty, ConfigurationPropertyOptions.None);

        static ConnectionStringSettings()
        {
            _properties.Add(_propName);
            _properties.Add(_propConnectionString);
            _properties.Add(_propProviderName);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConnectionStringSettings()
        {
        }

        public ConnectionStringSettings(string name, string connectionString) : this()
        {
            this.Name = name;
            this.ConnectionString = connectionString;
        }

        public ConnectionStringSettings(string name, string connectionString, string providerName) : this()
        {
            this.Name = name;
            this.ConnectionString = connectionString;
            this.ProviderName = providerName;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public override string ToString()
        {
            return this.ConnectionString;
        }

        [ConfigurationProperty("connectionString", Options=ConfigurationPropertyOptions.IsRequired, DefaultValue="")]
        public string ConnectionString
        {
            get
            {
                return (string) base[_propConnectionString];
            }
            set
            {
                base[_propConnectionString] = value;
            }
        }

        internal string Key
        {
            get
            {
                return this.Name;
            }
        }

        [ConfigurationProperty("name", Options=ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired, DefaultValue="")]
        public string Name
        {
            get
            {
                return (string) base[_propName];
            }
            set
            {
                base[_propName] = value;
            }
        }

        protected internal override ConfigurationPropertyCollection Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("providerName", DefaultValue="System.Data.SqlClient")]
        public string ProviderName
        {
            get
            {
                return (string) base[_propProviderName];
            }
            set
            {
                base[_propProviderName] = value;
            }
        }
    }
}


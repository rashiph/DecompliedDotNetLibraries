namespace System.Configuration
{
    using System;
    using System.Runtime;

    public sealed class ConnectionStringsSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _propConnectionStrings = new ConfigurationProperty(null, typeof(ConnectionStringSettingsCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        static ConnectionStringsSection()
        {
            _properties.Add(_propConnectionStrings);
        }

        protected internal override object GetRuntimeObject()
        {
            this.SetReadOnly();
            return this;
        }

        [ConfigurationProperty("", Options=ConfigurationPropertyOptions.IsDefaultCollection)]
        public ConnectionStringSettingsCollection ConnectionStrings
        {
            get
            {
                return (ConnectionStringSettingsCollection) base[_propConnectionStrings];
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
    }
}


namespace System.Web.Configuration
{
    using System;
    using System.Configuration;

    public sealed class ProtocolsSection : ConfigurationSection
    {
        private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propProtocols = new ConfigurationProperty(null, typeof(ProtocolCollection), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsDefaultCollection);

        static ProtocolsSection()
        {
            _properties.Add(_propProtocols);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("protocols", IsRequired=true, IsDefaultCollection=true)]
        public ProtocolCollection Protocols
        {
            get
            {
                return (ProtocolCollection) base[_propProtocols];
            }
        }
    }
}


namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Security;

    public sealed class ProtocolMappingSection : ConfigurationSection
    {
        private ConfigurationPropertyCollection properties;

        internal static ProtocolMappingSection GetSection()
        {
            return (ProtocolMappingSection) ConfigurationHelpers.GetSection(ConfigurationStrings.ProtocolMappingSectionPath);
        }

        protected override void InitializeDefault()
        {
            this.ProtocolMappingCollection.Add(new ProtocolMappingElement("http", "basicHttpBinding", ""));
            this.ProtocolMappingCollection.Add(new ProtocolMappingElement("net.tcp", "netTcpBinding", ""));
            this.ProtocolMappingCollection.Add(new ProtocolMappingElement("net.pipe", "netNamedPipeBinding", ""));
            this.ProtocolMappingCollection.Add(new ProtocolMappingElement("net.msmq", "netMsmqBinding", ""));
        }

        [SecurityCritical]
        internal static ProtocolMappingSection UnsafeGetSection()
        {
            return (ProtocolMappingSection) ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.ProtocolMappingSectionPath);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("", typeof(ProtocolMappingElementCollection), null, null, null, ConfigurationPropertyOptions.IsDefaultCollection));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("", Options=ConfigurationPropertyOptions.IsDefaultCollection)]
        public ProtocolMappingElementCollection ProtocolMappingCollection
        {
            get
            {
                return (ProtocolMappingElementCollection) base[""];
            }
        }
    }
}


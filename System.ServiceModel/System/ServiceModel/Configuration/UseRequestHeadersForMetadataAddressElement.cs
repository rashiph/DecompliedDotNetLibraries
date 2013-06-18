namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Description;

    public sealed class UseRequestHeadersForMetadataAddressElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            UseRequestHeadersForMetadataAddressElement element = (UseRequestHeadersForMetadataAddressElement) from;
            this.DefaultPorts.Clear();
            foreach (DefaultPortElement element2 in element.DefaultPorts)
            {
                this.DefaultPorts.Add(new DefaultPortElement(element2));
            }
        }

        protected internal override object CreateBehavior()
        {
            UseRequestHeadersForMetadataAddressBehavior behavior = new UseRequestHeadersForMetadataAddressBehavior();
            foreach (DefaultPortElement element in this.DefaultPorts)
            {
                behavior.DefaultPortsByScheme.Add(element.Scheme, element.Port);
            }
            return behavior;
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(UseRequestHeadersForMetadataAddressBehavior);
            }
        }

        [ConfigurationProperty("defaultPorts")]
        public DefaultPortElementCollection DefaultPorts
        {
            get
            {
                return (DefaultPortElementCollection) base["defaultPorts"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("defaultPorts", typeof(DefaultPortElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}


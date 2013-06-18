namespace System.Web.Services.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;

    public sealed class ProtocolElement : ConfigurationElement
    {
        private readonly ConfigurationProperty name;
        private ConfigurationPropertyCollection properties;

        public ProtocolElement()
        {
            this.properties = new ConfigurationPropertyCollection();
            this.name = new ConfigurationProperty("name", typeof(WebServiceProtocols), WebServiceProtocols.Unknown, ConfigurationPropertyOptions.IsKey);
            this.properties.Add(this.name);
        }

        public ProtocolElement(WebServiceProtocols protocol) : this()
        {
            this.Name = protocol;
        }

        private bool IsValidProtocolsValue(WebServiceProtocols value)
        {
            return Enum.IsDefined(typeof(WebServiceProtocols), value);
        }

        [ConfigurationProperty("name", IsKey=true, DefaultValue=0)]
        public WebServiceProtocols Name
        {
            get
            {
                return (WebServiceProtocols) base[this.name];
            }
            set
            {
                if (!this.IsValidProtocolsValue(value))
                {
                    value = WebServiceProtocols.Unknown;
                }
                base[this.name] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.properties;
            }
        }
    }
}


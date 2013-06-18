namespace System.ServiceModel.Activation.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Security.Principal;

    public sealed class SecurityIdentifierElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        public SecurityIdentifierElement()
        {
        }

        public SecurityIdentifierElement(System.Security.Principal.SecurityIdentifier sid) : this()
        {
            this.SecurityIdentifier = sid;
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("securityIdentifier", typeof(System.Security.Principal.SecurityIdentifier), null, new SecurityIdentifierConverter(), null, ConfigurationPropertyOptions.IsKey));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("securityIdentifier", DefaultValue=null, Options=ConfigurationPropertyOptions.IsKey), TypeConverter(typeof(SecurityIdentifierConverter))]
        public System.Security.Principal.SecurityIdentifier SecurityIdentifier
        {
            get
            {
                return (System.Security.Principal.SecurityIdentifier) base["securityIdentifier"];
            }
            set
            {
                base["securityIdentifier"] = value;
            }
        }
    }
}


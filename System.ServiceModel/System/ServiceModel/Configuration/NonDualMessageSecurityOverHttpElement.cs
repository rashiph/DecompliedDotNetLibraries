namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    public sealed class NonDualMessageSecurityOverHttpElement : MessageSecurityOverHttpElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(NonDualMessageSecurityOverHttp security)
        {
            base.ApplyConfiguration(security);
            security.EstablishSecurityContext = this.EstablishSecurityContext;
        }

        internal void InitializeFrom(NonDualMessageSecurityOverHttp security)
        {
            base.InitializeFrom(security);
            if (!security.EstablishSecurityContext)
            {
                this.EstablishSecurityContext = security.EstablishSecurityContext;
            }
        }

        [ConfigurationProperty("establishSecurityContext", DefaultValue=true)]
        public bool EstablishSecurityContext
        {
            get
            {
                return (bool) base["establishSecurityContext"];
            }
            set
            {
                base["establishSecurityContext"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("establishSecurityContext", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}


namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Net;

    public sealed class DefaultProxySection : ConfigurationSection
    {
        private readonly ConfigurationProperty bypasslist = new ConfigurationProperty("bypasslist", typeof(BypassElementCollection), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty enabled = new ConfigurationProperty("enabled", typeof(bool), true, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty module = new ConfigurationProperty("module", typeof(ModuleElement), null, ConfigurationPropertyOptions.None);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private readonly ConfigurationProperty proxy = new ConfigurationProperty("proxy", typeof(ProxyElement), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty useDefaultCredentials = new ConfigurationProperty("useDefaultCredentials", typeof(bool), false, ConfigurationPropertyOptions.None);

        public DefaultProxySection()
        {
            this.properties.Add(this.bypasslist);
            this.properties.Add(this.module);
            this.properties.Add(this.proxy);
            this.properties.Add(this.enabled);
            this.properties.Add(this.useDefaultCredentials);
        }

        protected override void PostDeserialize()
        {
            if (!base.EvaluationContext.IsMachineLevel)
            {
                try
                {
                    ExceptionHelper.WebPermissionUnrestricted.Demand();
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(System.SR.GetString("net_config_section_permission", new object[] { "defaultProxy" }), exception);
                }
            }
        }

        protected override void Reset(ConfigurationElement parentElement)
        {
            DefaultProxySection section = new DefaultProxySection();
            section.InitializeDefault();
            base.Reset(section);
        }

        [ConfigurationProperty("bypasslist")]
        public BypassElementCollection BypassList
        {
            get
            {
                return (BypassElementCollection) base[this.bypasslist];
            }
        }

        [ConfigurationProperty("enabled", DefaultValue=true)]
        public bool Enabled
        {
            get
            {
                return (bool) base[this.enabled];
            }
            set
            {
                base[this.enabled] = value;
            }
        }

        [ConfigurationProperty("module")]
        public ModuleElement Module
        {
            get
            {
                return (ModuleElement) base[this.module];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty("proxy")]
        public ProxyElement Proxy
        {
            get
            {
                return (ProxyElement) base[this.proxy];
            }
        }

        [ConfigurationProperty("useDefaultCredentials", DefaultValue=false)]
        public bool UseDefaultCredentials
        {
            get
            {
                return (bool) base[this.useDefaultCredentials];
            }
            set
            {
                base[this.useDefaultCredentials] = value;
            }
        }
    }
}


namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.Net.Sockets;

    public sealed class SocketElement : ConfigurationElement
    {
        private readonly ConfigurationProperty alwaysUseCompletionPortsForAccept = new ConfigurationProperty("alwaysUseCompletionPortsForAccept", typeof(bool), false, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty alwaysUseCompletionPortsForConnect = new ConfigurationProperty("alwaysUseCompletionPortsForConnect", typeof(bool), false, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty ipProtectionLevel = new ConfigurationProperty("ipProtectionLevel", typeof(System.Net.Sockets.IPProtectionLevel), System.Net.Sockets.IPProtectionLevel.Unspecified, ConfigurationPropertyOptions.None);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        public SocketElement()
        {
            this.properties.Add(this.alwaysUseCompletionPortsForAccept);
            this.properties.Add(this.alwaysUseCompletionPortsForConnect);
            this.properties.Add(this.ipProtectionLevel);
        }

        protected override void PostDeserialize()
        {
            if (!base.EvaluationContext.IsMachineLevel)
            {
                try
                {
                    ExceptionHelper.UnrestrictedSocketPermission.Demand();
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(System.SR.GetString("net_config_element_permission", new object[] { "socket" }), exception);
                }
            }
        }

        [ConfigurationProperty("alwaysUseCompletionPortsForAccept", DefaultValue=false)]
        public bool AlwaysUseCompletionPortsForAccept
        {
            get
            {
                return (bool) base[this.alwaysUseCompletionPortsForAccept];
            }
            set
            {
                base[this.alwaysUseCompletionPortsForAccept] = value;
            }
        }

        [ConfigurationProperty("alwaysUseCompletionPortsForConnect", DefaultValue=false)]
        public bool AlwaysUseCompletionPortsForConnect
        {
            get
            {
                return (bool) base[this.alwaysUseCompletionPortsForConnect];
            }
            set
            {
                base[this.alwaysUseCompletionPortsForConnect] = value;
            }
        }

        [ConfigurationProperty("ipProtectionLevel", DefaultValue=-1)]
        public System.Net.Sockets.IPProtectionLevel IPProtectionLevel
        {
            get
            {
                return (System.Net.Sockets.IPProtectionLevel) base[this.ipProtectionLevel];
            }
            set
            {
                base[this.ipProtectionLevel] = value;
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


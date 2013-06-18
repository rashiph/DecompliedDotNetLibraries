namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    public sealed class WSDualHttpSecurityElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(WSDualHttpSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.Mode = this.Mode;
            this.Message.ApplyConfiguration(security.Message);
        }

        internal void InitializeFrom(WSDualHttpSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            this.Mode = security.Mode;
            this.Message.InitializeFrom(security.Message);
        }

        [ConfigurationProperty("message")]
        public MessageSecurityOverHttpElement Message
        {
            get
            {
                return (MessageSecurityOverHttpElement) base["message"];
            }
        }

        [ServiceModelEnumValidator(typeof(WSDualHttpSecurityModeHelper)), ConfigurationProperty("mode", DefaultValue=1)]
        public WSDualHttpSecurityMode Mode
        {
            get
            {
                return (WSDualHttpSecurityMode) base["mode"];
            }
            set
            {
                base["mode"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("mode", typeof(WSDualHttpSecurityMode), WSDualHttpSecurityMode.Message, null, new ServiceModelEnumValidator(typeof(WSDualHttpSecurityModeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("message", typeof(MessageSecurityOverHttpElement), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}


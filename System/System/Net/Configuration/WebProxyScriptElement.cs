namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Net;

    public sealed class WebProxyScriptElement : ConfigurationElement
    {
        private readonly ConfigurationProperty downloadTimeout = new ConfigurationProperty("downloadTimeout", typeof(TimeSpan), TimeSpan.FromMinutes(1.0), null, new TimeSpanValidator(new TimeSpan(0, 0, 0), TimeSpan.MaxValue, false), ConfigurationPropertyOptions.None);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        public WebProxyScriptElement()
        {
            this.properties.Add(this.downloadTimeout);
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
                    throw new ConfigurationErrorsException(System.SR.GetString("net_config_element_permission", new object[] { "webProxyScript" }), exception);
                }
            }
        }

        [ConfigurationProperty("downloadTimeout", DefaultValue="00:01:00")]
        public TimeSpan DownloadTimeout
        {
            get
            {
                return (TimeSpan) base[this.downloadTimeout];
            }
            set
            {
                base[this.downloadTimeout] = value;
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


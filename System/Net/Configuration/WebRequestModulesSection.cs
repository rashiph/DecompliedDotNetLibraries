namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Net;

    public sealed class WebRequestModulesSection : ConfigurationSection
    {
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private readonly ConfigurationProperty webRequestModules = new ConfigurationProperty(null, typeof(WebRequestModuleElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        public WebRequestModulesSection()
        {
            this.properties.Add(this.webRequestModules);
        }

        protected override void InitializeDefault()
        {
            this.WebRequestModules.Add(new WebRequestModuleElement("https:", typeof(HttpRequestCreator)));
            this.WebRequestModules.Add(new WebRequestModuleElement("http:", typeof(HttpRequestCreator)));
            this.WebRequestModules.Add(new WebRequestModuleElement("file:", typeof(FileWebRequestCreator)));
            this.WebRequestModules.Add(new WebRequestModuleElement("ftp:", typeof(FtpWebRequestCreator)));
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
                    throw new ConfigurationErrorsException(System.SR.GetString("net_config_section_permission", new object[] { "webRequestModules" }), exception);
                }
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public WebRequestModuleElementCollection WebRequestModules
        {
            get
            {
                return (WebRequestModuleElementCollection) base[this.webRequestModules];
            }
        }
    }
}


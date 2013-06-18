namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web;
    using System.Web.Security;

    public sealed class HttpModulesSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propHttpModules = new ConfigurationProperty(null, typeof(HttpModuleActionCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        static HttpModulesSection()
        {
            _properties.Add(_propHttpModules);
        }

        internal HttpModuleCollection CreateModules()
        {
            HttpModuleCollection modules = new HttpModuleCollection();
            foreach (HttpModuleAction action in this.Modules)
            {
                modules.AddModule(action.Entry.ModuleName, action.Entry.Create());
            }
            modules.AddModule("DefaultAuthentication", DefaultAuthenticationModule.CreateDefaultAuthenticationModuleWithAssert());
            return modules;
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public HttpModuleActionCollection Modules
        {
            get
            {
                return (HttpModuleActionCollection) base[_propHttpModules];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }
}


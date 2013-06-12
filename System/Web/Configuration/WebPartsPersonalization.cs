namespace System.Web.Configuration
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Web;

    public sealed class WebPartsPersonalization : ConfigurationElement
    {
        private static readonly ConfigurationProperty _propAuthorization = new ConfigurationProperty("authorization", typeof(WebPartsPersonalizationAuthorization), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDefaultProvider = new ConfigurationProperty("defaultProvider", typeof(string), "AspNetSqlPersonalizationProvider", null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propProviders = new ConfigurationProperty("providers", typeof(ProviderSettingsCollection), null, ConfigurationPropertyOptions.None);

        static WebPartsPersonalization()
        {
            _properties.Add(_propDefaultProvider);
            _properties.Add(_propProviders);
            _properties.Add(_propAuthorization);
        }

        internal void ValidateAuthorization()
        {
            foreach (AuthorizationRule rule in this.Authorization.Rules)
            {
                StringCollection verbs = rule.Verbs;
                if (verbs.Count == 0)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("WebPartsSection_NoVerbs"), rule.ElementInformation.Properties["verbs"].Source, rule.ElementInformation.Properties["verbs"].LineNumber);
                }
                foreach (string str in verbs)
                {
                    if ((str != "enterSharedScope") && (str != "modifyState"))
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("WebPartsSection_InvalidVerb", new object[] { str }), rule.ElementInformation.Properties["verbs"].Source, rule.ElementInformation.Properties["verbs"].LineNumber);
                    }
                }
            }
        }

        [ConfigurationProperty("authorization")]
        public WebPartsPersonalizationAuthorization Authorization
        {
            get
            {
                return (WebPartsPersonalizationAuthorization) base[_propAuthorization];
            }
        }

        [ConfigurationProperty("defaultProvider", DefaultValue="AspNetSqlPersonalizationProvider"), StringValidator(MinLength=1)]
        public string DefaultProvider
        {
            get
            {
                return (string) base[_propDefaultProvider];
            }
            set
            {
                base[_propDefaultProvider] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers
        {
            get
            {
                return (ProviderSettingsCollection) base[_propProviders];
            }
        }
    }
}


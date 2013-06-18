namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web;

    public sealed class SiteMapSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _propDefaultProvider = new ConfigurationProperty("defaultProvider", typeof(string), "AspNetXmlSiteMapProvider", null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnabled = new ConfigurationProperty("enabled", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propProviders = new ConfigurationProperty("providers", typeof(ProviderSettingsCollection), null, ConfigurationPropertyOptions.None);
        private SiteMapProviderCollection _siteMapProviders;

        static SiteMapSection()
        {
            _properties.Add(_propDefaultProvider);
            _properties.Add(_propEnabled);
            _properties.Add(_propProviders);
        }

        internal void ValidateDefaultProvider()
        {
            if (!string.IsNullOrEmpty(this.DefaultProvider) && (this.Providers[this.DefaultProvider] == null))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_provider_must_exist", new object[] { this.DefaultProvider }), base.ElementInformation.Properties[_propDefaultProvider.Name].Source, base.ElementInformation.Properties[_propDefaultProvider.Name].LineNumber);
            }
        }

        [StringValidator(MinLength=1), ConfigurationProperty("defaultProvider", DefaultValue="AspNetXmlSiteMapProvider")]
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

        [ConfigurationProperty("enabled", DefaultValue=true)]
        public bool Enabled
        {
            get
            {
                return (bool) base[_propEnabled];
            }
            set
            {
                base[_propEnabled] = value;
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

        internal SiteMapProviderCollection ProvidersInternal
        {
            get
            {
                if (this._siteMapProviders == null)
                {
                    lock (this)
                    {
                        if (this._siteMapProviders == null)
                        {
                            SiteMapProviderCollection providers = new SiteMapProviderCollection();
                            ProvidersHelper.InstantiateProviders(this.Providers, providers, typeof(SiteMapProvider));
                            this._siteMapProviders = providers;
                        }
                    }
                }
                return this._siteMapProviders;
            }
        }
    }
}


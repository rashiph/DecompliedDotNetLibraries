namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Caching;

    public sealed class OutputCacheSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _propDefaultProviderName = new ConfigurationProperty("defaultProvider", typeof(string), "AspNetInternalProvider", null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableFragmentCache = new ConfigurationProperty("enableFragmentCache", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableKernelCacheForVaryByStar = new ConfigurationProperty("enableKernelCacheForVaryByStar", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableOutputCache = new ConfigurationProperty("enableOutputCache", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propOmitVaryStar = new ConfigurationProperty("omitVaryStar", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propProviders = new ConfigurationProperty("providers", typeof(ProviderSettingsCollection), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propSendCacheControlHeader = new ConfigurationProperty("sendCacheControlHeader", typeof(bool), true, ConfigurationPropertyOptions.None);
        internal const bool DefaultOmitVaryStar = false;
        private bool enableKernelCacheForVaryByStar;
        private bool enableKernelCacheForVaryByStarCached;
        private bool enableOutputCache;
        private bool enableOutputCacheCached;
        private bool omitVaryStar;
        private bool omitVaryStarCached;
        private bool sendCacheControlHeaderCache;
        private bool sendCacheControlHeaderCached;

        static OutputCacheSection()
        {
            _properties.Add(_propEnableOutputCache);
            _properties.Add(_propEnableFragmentCache);
            _properties.Add(_propSendCacheControlHeader);
            _properties.Add(_propOmitVaryStar);
            _properties.Add(_propEnableKernelCacheForVaryByStar);
            _properties.Add(_propDefaultProviderName);
            _properties.Add(_propProviders);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal OutputCacheProviderCollection CreateProviderCollection()
        {
            ProviderSettingsCollection configProviders = this.Providers;
            if ((configProviders == null) || (configProviders.Count == 0))
            {
                return null;
            }
            OutputCacheProviderCollection providers = new OutputCacheProviderCollection();
            ProvidersHelper.InstantiateProviders(configProviders, providers, typeof(OutputCacheProvider));
            providers.SetReadOnly();
            return providers;
        }

        internal OutputCacheProvider GetDefaultProvider(OutputCacheProviderCollection providers)
        {
            string defaultProviderName = this.DefaultProviderName;
            if (defaultProviderName == "AspNetInternalProvider")
            {
                return null;
            }
            OutputCacheProvider provider = (providers == null) ? null : providers[defaultProviderName];
            if (provider == null)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Def_provider_not_found"), base.ElementInformation.Properties["defaultProvider"].Source, base.ElementInformation.Properties["defaultProvider"].LineNumber);
            }
            return provider;
        }

        [ConfigurationProperty("defaultProvider", DefaultValue="AspNetInternalProvider"), StringValidator(MinLength=1)]
        public string DefaultProviderName
        {
            get
            {
                return (string) base[_propDefaultProviderName];
            }
            set
            {
                base[_propDefaultProviderName] = value;
            }
        }

        [ConfigurationProperty("enableFragmentCache", DefaultValue=true)]
        public bool EnableFragmentCache
        {
            get
            {
                return (bool) base[_propEnableFragmentCache];
            }
            set
            {
                base[_propEnableFragmentCache] = value;
            }
        }

        [ConfigurationProperty("enableKernelCacheForVaryByStar", DefaultValue=false)]
        public bool EnableKernelCacheForVaryByStar
        {
            get
            {
                if (!this.enableKernelCacheForVaryByStarCached)
                {
                    this.enableKernelCacheForVaryByStar = (bool) base[_propEnableKernelCacheForVaryByStar];
                    this.enableKernelCacheForVaryByStarCached = true;
                }
                return this.enableKernelCacheForVaryByStar;
            }
            set
            {
                base[_propEnableKernelCacheForVaryByStar] = value;
                this.enableKernelCacheForVaryByStar = value;
            }
        }

        [ConfigurationProperty("enableOutputCache", DefaultValue=true)]
        public bool EnableOutputCache
        {
            get
            {
                if (!this.enableOutputCacheCached)
                {
                    this.enableOutputCache = (bool) base[_propEnableOutputCache];
                    this.enableOutputCacheCached = true;
                }
                return this.enableOutputCache;
            }
            set
            {
                base[_propEnableOutputCache] = value;
                this.enableOutputCache = value;
            }
        }

        [ConfigurationProperty("omitVaryStar", DefaultValue=false)]
        public bool OmitVaryStar
        {
            get
            {
                if (!this.omitVaryStarCached)
                {
                    this.omitVaryStar = (bool) base[_propOmitVaryStar];
                    this.omitVaryStarCached = true;
                }
                return this.omitVaryStar;
            }
            set
            {
                base[_propOmitVaryStar] = value;
                this.omitVaryStar = value;
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

        [ConfigurationProperty("sendCacheControlHeader", DefaultValue=true)]
        public bool SendCacheControlHeader
        {
            get
            {
                if (!this.sendCacheControlHeaderCached)
                {
                    this.sendCacheControlHeaderCache = (bool) base[_propSendCacheControlHeader];
                    this.sendCacheControlHeaderCached = true;
                }
                return this.sendCacheControlHeaderCache;
            }
            set
            {
                base[_propSendCacheControlHeader] = value;
                this.sendCacheControlHeaderCache = value;
            }
        }
    }
}


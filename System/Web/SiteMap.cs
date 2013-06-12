namespace System.Web
{
    using System;
    using System.Web.Configuration;

    public static class SiteMap
    {
        private static bool _configEnabledEvaluated;
        private static bool _enabled;
        private static object _lockObject = new object();
        private static SiteMapProvider _provider;
        private static SiteMapProviderCollection _providers;
        internal const string SectionName = "system.web/siteMap";

        public static  event SiteMapResolveEventHandler SiteMapResolve
        {
            add
            {
                Provider.SiteMapResolve += value;
            }
            remove
            {
                Provider.SiteMapResolve -= value;
            }
        }

        private static void Initialize()
        {
            if (_providers == null)
            {
                HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, "Feature_not_supported_at_this_level");
                lock (_lockObject)
                {
                    if (_providers == null)
                    {
                        SiteMapSection siteMap = RuntimeConfig.GetAppConfig().SiteMap;
                        if (siteMap == null)
                        {
                            _providers = new SiteMapProviderCollection();
                        }
                        else
                        {
                            if (!siteMap.Enabled)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("SiteMap_feature_disabled", new object[] { "system.web/siteMap" }));
                            }
                            siteMap.ValidateDefaultProvider();
                            _providers = siteMap.ProvidersInternal;
                            _provider = _providers[siteMap.DefaultProvider];
                            _providers.SetReadOnly();
                        }
                    }
                }
            }
        }

        public static SiteMapNode CurrentNode
        {
            get
            {
                return Provider.CurrentNode;
            }
        }

        public static bool Enabled
        {
            get
            {
                if (!_configEnabledEvaluated)
                {
                    SiteMapSection siteMap = RuntimeConfig.GetAppConfig().SiteMap;
                    _enabled = (siteMap != null) && siteMap.Enabled;
                    _configEnabledEvaluated = true;
                }
                return _enabled;
            }
        }

        public static SiteMapProvider Provider
        {
            get
            {
                Initialize();
                return _provider;
            }
        }

        public static SiteMapProviderCollection Providers
        {
            get
            {
                Initialize();
                return _providers;
            }
        }

        public static SiteMapNode RootNode
        {
            get
            {
                SiteMapProvider rootProvider = Provider.RootProvider;
                SiteMapNode rootNode = rootProvider.RootNode;
                if (rootNode == null)
                {
                    string name = rootProvider.Name;
                    throw new InvalidOperationException(System.Web.SR.GetString("SiteMapProvider_Invalid_RootNode", new object[] { name }));
                }
                return rootNode;
            }
        }
    }
}


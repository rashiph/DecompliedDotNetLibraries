namespace System.Net.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Net;
    using System.Net.Cache;

    public sealed class SettingsSection : ConfigurationSection
    {
        private readonly ConfigurationProperty httpListener = new ConfigurationProperty("httpListener", typeof(HttpListenerElement), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty httpWebRequest = new ConfigurationProperty("httpWebRequest", typeof(HttpWebRequestElement), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty ipv6 = new ConfigurationProperty("ipv6", typeof(Ipv6Element), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty performanceCounters = new ConfigurationProperty("performanceCounters", typeof(PerformanceCountersElement), null, ConfigurationPropertyOptions.None);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private readonly ConfigurationProperty servicePointManager = new ConfigurationProperty("servicePointManager", typeof(ServicePointManagerElement), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty socket = new ConfigurationProperty("socket", typeof(SocketElement), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty webProxyScript = new ConfigurationProperty("webProxyScript", typeof(WebProxyScriptElement), null, ConfigurationPropertyOptions.None);

        public SettingsSection()
        {
            this.properties.Add(this.httpWebRequest);
            this.properties.Add(this.ipv6);
            this.properties.Add(this.servicePointManager);
            this.properties.Add(this.socket);
            this.properties.Add(this.webProxyScript);
            this.properties.Add(this.performanceCounters);
            this.properties.Add(this.httpListener);
        }

        internal static void EnsureConfigLoaded()
        {
            try
            {
                AuthenticationManager.EnsureConfigLoaded();
                bool isCachingEnabled = RequestCacheManager.IsCachingEnabled;
                int defaultConnectionLimit = System.Net.ServicePointManager.DefaultConnectionLimit;
                bool flag2 = System.Net.ServicePointManager.Expect100Continue;
                ArrayList prefixList = WebRequest.PrefixList;
                IWebProxy internalDefaultWebProxy = WebRequest.InternalDefaultWebProxy;
            }
            catch
            {
            }
        }

        [ConfigurationProperty("httpListener")]
        public HttpListenerElement HttpListener
        {
            get
            {
                return (HttpListenerElement) base[this.httpListener];
            }
        }

        [ConfigurationProperty("httpWebRequest")]
        public HttpWebRequestElement HttpWebRequest
        {
            get
            {
                return (HttpWebRequestElement) base[this.httpWebRequest];
            }
        }

        [ConfigurationProperty("ipv6")]
        public Ipv6Element Ipv6
        {
            get
            {
                return (Ipv6Element) base[this.ipv6];
            }
        }

        [ConfigurationProperty("performanceCounters")]
        public PerformanceCountersElement PerformanceCounters
        {
            get
            {
                return (PerformanceCountersElement) base[this.performanceCounters];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty("servicePointManager")]
        public ServicePointManagerElement ServicePointManager
        {
            get
            {
                return (ServicePointManagerElement) base[this.servicePointManager];
            }
        }

        [ConfigurationProperty("socket")]
        public SocketElement Socket
        {
            get
            {
                return (SocketElement) base[this.socket];
            }
        }

        [ConfigurationProperty("webProxyScript")]
        public WebProxyScriptElement WebProxyScript
        {
            get
            {
                return (WebProxyScriptElement) base[this.webProxyScript];
            }
        }
    }
}


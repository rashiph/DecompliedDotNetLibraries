namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Configuration.Internal;
    using System.Net.Configuration;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Hosting;

    internal class RuntimeConfig
    {
        protected IInternalConfigRecord _configRecord;
        private bool _permitNull;
        private object[] _results;
        private System.Web.Configuration.RuntimeConfigLKG _runtimeConfigLKG;
        private static RuntimeConfig s_clientRuntimeConfig;
        private static RuntimeConfig s_errorRuntimeConfig;
        private static RuntimeConfig s_nullRuntimeConfig;
        private static object s_unevaluatedResult = new object();

        static RuntimeConfig()
        {
            GetErrorRuntimeConfig();
        }

        internal RuntimeConfig(IInternalConfigRecord configRecord) : this(configRecord, false)
        {
        }

        protected RuntimeConfig(IInternalConfigRecord configRecord, bool permitNull)
        {
            this._configRecord = configRecord;
            this._permitNull = permitNull;
            this._results = new object[0x18];
            for (int i = 0; i < this._results.Length; i++)
            {
                this._results[i] = s_unevaluatedResult;
            }
        }

        internal static RuntimeConfig GetAppConfig()
        {
            if (!HttpConfigurationSystem.UseHttpConfigurationSystem)
            {
                return GetClientRuntimeConfig();
            }
            return CachedPathData.GetApplicationPathData().RuntimeConfig;
        }

        internal static RuntimeConfig GetAppLKGConfig()
        {
            RuntimeConfig appConfig = null;
            bool flag = false;
            try
            {
                appConfig = GetAppConfig();
                flag = true;
            }
            catch
            {
            }
            if (!flag)
            {
                appConfig = GetLKGRuntimeConfig(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPathObject);
            }
            return appConfig.RuntimeConfigLKG;
        }

        private static RuntimeConfig GetClientRuntimeConfig()
        {
            if (s_clientRuntimeConfig == null)
            {
                s_clientRuntimeConfig = new ClientRuntimeConfig();
            }
            return s_clientRuntimeConfig;
        }

        internal static RuntimeConfig GetConfig()
        {
            if (!HttpConfigurationSystem.UseHttpConfigurationSystem)
            {
                return GetClientRuntimeConfig();
            }
            HttpContext current = HttpContext.Current;
            if (current != null)
            {
                return GetConfig(current);
            }
            return GetAppConfig();
        }

        internal static RuntimeConfig GetConfig(string path)
        {
            return GetConfig(VirtualPath.CreateNonRelativeAllowNull(path));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static RuntimeConfig GetConfig(HttpContext context)
        {
            if (!HttpConfigurationSystem.UseHttpConfigurationSystem)
            {
                return GetClientRuntimeConfig();
            }
            return context.GetRuntimeConfig();
        }

        internal static RuntimeConfig GetConfig(VirtualPath path)
        {
            if (!HttpConfigurationSystem.UseHttpConfigurationSystem)
            {
                return GetClientRuntimeConfig();
            }
            return CachedPathData.GetVirtualPathData(path, true).RuntimeConfig;
        }

        internal static RuntimeConfig GetConfig(HttpContext context, VirtualPath path)
        {
            if (!HttpConfigurationSystem.UseHttpConfigurationSystem)
            {
                return GetClientRuntimeConfig();
            }
            return context.GetRuntimeConfig(path);
        }

        internal static RuntimeConfig GetErrorRuntimeConfig()
        {
            if (s_errorRuntimeConfig == null)
            {
                s_errorRuntimeConfig = new ErrorRuntimeConfig();
            }
            return s_errorRuntimeConfig;
        }

        private object GetHandlerSection(string sectionName, Type type, ResultsIndex index)
        {
            object sectionObject = this._results[(int) index];
            if (sectionObject == s_unevaluatedResult)
            {
                sectionObject = this.GetSectionObject(sectionName);
                if ((sectionObject != null) && (sectionObject.GetType() != type))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_unable_to_get_section", new object[] { sectionName }));
                }
                if (index != ResultsIndex.UNUSED)
                {
                    this._results[(int) index] = sectionObject;
                }
            }
            return sectionObject;
        }

        internal static RuntimeConfig GetLKGConfig(HttpContext context)
        {
            RuntimeConfig lKGRuntimeConfig = null;
            bool flag = false;
            try
            {
                lKGRuntimeConfig = GetConfig(context);
                flag = true;
            }
            catch
            {
            }
            if (!flag)
            {
                lKGRuntimeConfig = GetLKGRuntimeConfig(context.Request.FilePathObject);
            }
            return lKGRuntimeConfig.RuntimeConfigLKG;
        }

        private static RuntimeConfig GetLKGRuntimeConfig(VirtualPath path)
        {
            try
            {
                path = path.Parent;
            }
            catch
            {
                path = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPathObject;
            }
            while (path != null)
            {
                try
                {
                    return GetConfig(path);
                }
                catch
                {
                    path = path.Parent;
                }
            }
            try
            {
                return GetRootWebConfig();
            }
            catch
            {
            }
            try
            {
                return GetMachineConfig();
            }
            catch
            {
            }
            return GetNullRuntimeConfig();
        }

        internal static RuntimeConfig GetMachineConfig()
        {
            if (!HttpConfigurationSystem.UseHttpConfigurationSystem)
            {
                return GetClientRuntimeConfig();
            }
            return CachedPathData.GetMachinePathData().RuntimeConfig;
        }

        private static RuntimeConfig GetNullRuntimeConfig()
        {
            if (s_nullRuntimeConfig == null)
            {
                s_nullRuntimeConfig = new NullRuntimeConfig();
            }
            return s_nullRuntimeConfig;
        }

        internal static RuntimeConfig GetRootWebConfig()
        {
            if (!HttpConfigurationSystem.UseHttpConfigurationSystem)
            {
                return GetClientRuntimeConfig();
            }
            return CachedPathData.GetRootWebPathData().RuntimeConfig;
        }

        private object GetSection(string sectionName, Type type)
        {
            return this.GetSection(sectionName, type, ResultsIndex.UNUSED);
        }

        private object GetSection(string sectionName, Type type, ResultsIndex index)
        {
            object sectionObject = this._results[(int) index];
            if (sectionObject == s_unevaluatedResult)
            {
                sectionObject = this.GetSectionObject(sectionName);
                if (sectionObject == null)
                {
                    if (!this._permitNull)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_unable_to_get_section", new object[] { sectionName }));
                    }
                }
                else if (sectionObject.GetType() != type)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_unable_to_get_section", new object[] { sectionName }));
                }
                if (index != ResultsIndex.UNUSED)
                {
                    this._results[(int) index] = sectionObject;
                }
            }
            return sectionObject;
        }

        [ConfigurationPermission(SecurityAction.Assert, Unrestricted=true)]
        protected virtual object GetSectionObject(string sectionName)
        {
            return this._configRecord.GetSection(sectionName);
        }

        internal AnonymousIdentificationSection AnonymousIdentification
        {
            get
            {
                return (AnonymousIdentificationSection) this.GetSection("system.web/anonymousIdentification", typeof(AnonymousIdentificationSection));
            }
        }

        internal AuthenticationSection Authentication
        {
            get
            {
                return (AuthenticationSection) this.GetSection("system.web/authentication", typeof(AuthenticationSection), ResultsIndex.Authentication);
            }
        }

        internal AuthorizationSection Authorization
        {
            get
            {
                return (AuthorizationSection) this.GetSection("system.web/authorization", typeof(AuthorizationSection), ResultsIndex.Authorization);
            }
        }

        internal HttpCapabilitiesDefaultProvider BrowserCaps
        {
            get
            {
                return (HttpCapabilitiesDefaultProvider) this.GetHandlerSection("system.web/browserCaps", typeof(HttpCapabilitiesDefaultProvider), ResultsIndex.BrowserCaps);
            }
        }

        internal CacheSection Cache
        {
            get
            {
                return (CacheSection) this.GetSection("system.web/caching/cache", typeof(CacheSection));
            }
        }

        internal ClientTargetSection ClientTarget
        {
            get
            {
                return (ClientTargetSection) this.GetSection("system.web/clientTarget", typeof(ClientTargetSection), ResultsIndex.ClientTarget);
            }
        }

        internal CompilationSection Compilation
        {
            get
            {
                return (CompilationSection) this.GetSection("system.web/compilation", typeof(CompilationSection), ResultsIndex.Compilation);
            }
        }

        internal IInternalConfigRecord ConfigRecord
        {
            get
            {
                return this._configRecord;
            }
        }

        internal ConnectionStringsSection ConnectionStrings
        {
            get
            {
                return (ConnectionStringsSection) this.GetSection("connectionStrings", typeof(ConnectionStringsSection), ResultsIndex.ConnectionStrings);
            }
        }

        internal CustomErrorsSection CustomErrors
        {
            get
            {
                return (CustomErrorsSection) this.GetSection("system.web/customErrors", typeof(CustomErrorsSection));
            }
        }

        internal DeploymentSection Deployment
        {
            get
            {
                return (DeploymentSection) this.GetSection("system.web/deployment", typeof(DeploymentSection));
            }
        }

        internal FullTrustAssembliesSection FullTrustAssemblies
        {
            get
            {
                return (FullTrustAssembliesSection) this.GetSection("system.web/fullTrustAssemblies", typeof(FullTrustAssembliesSection));
            }
        }

        internal GlobalizationSection Globalization
        {
            get
            {
                return (GlobalizationSection) this.GetSection("system.web/globalization", typeof(GlobalizationSection), ResultsIndex.Globalization);
            }
        }

        internal HealthMonitoringSection HealthMonitoring
        {
            get
            {
                return (HealthMonitoringSection) this.GetSection("system.web/healthMonitoring", typeof(HealthMonitoringSection));
            }
        }

        internal HostingEnvironmentSection HostingEnvironment
        {
            get
            {
                return (HostingEnvironmentSection) this.GetSection("system.web/hostingEnvironment", typeof(HostingEnvironmentSection));
            }
        }

        internal HttpCookiesSection HttpCookies
        {
            get
            {
                return (HttpCookiesSection) this.GetSection("system.web/httpCookies", typeof(HttpCookiesSection), ResultsIndex.HttpCookies);
            }
        }

        internal HttpHandlersSection HttpHandlers
        {
            get
            {
                return (HttpHandlersSection) this.GetSection("system.web/httpHandlers", typeof(HttpHandlersSection), ResultsIndex.HttpHandlers);
            }
        }

        internal HttpModulesSection HttpModules
        {
            get
            {
                return (HttpModulesSection) this.GetSection("system.web/httpModules", typeof(HttpModulesSection), ResultsIndex.HttpModules);
            }
        }

        internal HttpRuntimeSection HttpRuntime
        {
            get
            {
                return (HttpRuntimeSection) this.GetSection("system.web/httpRuntime", typeof(HttpRuntimeSection), ResultsIndex.HttpRuntime);
            }
        }

        internal IdentitySection Identity
        {
            get
            {
                return (IdentitySection) this.GetSection("system.web/identity", typeof(IdentitySection), ResultsIndex.Identity);
            }
        }

        internal MachineKeySection MachineKey
        {
            get
            {
                return (MachineKeySection) this.GetSection("system.web/machineKey", typeof(MachineKeySection), ResultsIndex.MachineKey);
            }
        }

        internal MembershipSection Membership
        {
            get
            {
                return (MembershipSection) this.GetSection("system.web/membership", typeof(MembershipSection), ResultsIndex.Membership);
            }
        }

        internal OutputCacheSection OutputCache
        {
            get
            {
                return (OutputCacheSection) this.GetSection("system.web/caching/outputCache", typeof(OutputCacheSection), ResultsIndex.OutputCache);
            }
        }

        internal OutputCacheSettingsSection OutputCacheSettings
        {
            get
            {
                return (OutputCacheSettingsSection) this.GetSection("system.web/caching/outputCacheSettings", typeof(OutputCacheSettingsSection), ResultsIndex.OutputCacheSettings);
            }
        }

        internal PagesSection Pages
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (PagesSection) this.GetSection("system.web/pages", typeof(PagesSection), ResultsIndex.Pages);
            }
        }

        internal PartialTrustVisibleAssembliesSection PartialTrustVisibleAssemblies
        {
            get
            {
                return (PartialTrustVisibleAssembliesSection) this.GetSection("system.web/partialTrustVisibleAssemblies", typeof(PartialTrustVisibleAssembliesSection));
            }
        }

        internal ProcessModelSection ProcessModel
        {
            get
            {
                return (ProcessModelSection) this.GetSection("system.web/processModel", typeof(ProcessModelSection));
            }
        }

        internal ProfileSection Profile
        {
            get
            {
                return (ProfileSection) this.GetSection("system.web/profile", typeof(ProfileSection), ResultsIndex.Profile);
            }
        }

        internal ProtocolsSection Protocols
        {
            get
            {
                return (ProtocolsSection) this.GetSection("system.web/protocols", typeof(ProtocolsSection));
            }
        }

        internal RoleManagerSection RoleManager
        {
            get
            {
                return (RoleManagerSection) this.GetSection("system.web/roleManager", typeof(RoleManagerSection));
            }
        }

        private System.Web.Configuration.RuntimeConfigLKG RuntimeConfigLKG
        {
            get
            {
                if (this._runtimeConfigLKG == null)
                {
                    lock (this)
                    {
                        if (this._runtimeConfigLKG == null)
                        {
                            this._runtimeConfigLKG = new System.Web.Configuration.RuntimeConfigLKG(this._configRecord);
                        }
                    }
                }
                return this._runtimeConfigLKG;
            }
        }

        internal SecurityPolicySection SecurityPolicy
        {
            get
            {
                return (SecurityPolicySection) this.GetSection("system.web/securityPolicy", typeof(SecurityPolicySection));
            }
        }

        internal SessionPageStateSection SessionPageState
        {
            get
            {
                return (SessionPageStateSection) this.GetSection("system.web/sessionPageState", typeof(SessionPageStateSection), ResultsIndex.SessionPageState);
            }
        }

        internal SessionStateSection SessionState
        {
            get
            {
                return (SessionStateSection) this.GetSection("system.web/sessionState", typeof(SessionStateSection));
            }
        }

        internal SiteMapSection SiteMap
        {
            get
            {
                return (SiteMapSection) this.GetSection("system.web/siteMap", typeof(SiteMapSection));
            }
        }

        internal SmtpSection Smtp
        {
            get
            {
                return (SmtpSection) this.GetSection("system.net/mailSettings/smtp", typeof(SmtpSection));
            }
        }

        internal SqlCacheDependencySection SqlCacheDependency
        {
            get
            {
                return (SqlCacheDependencySection) this.GetSection("system.web/caching/sqlCacheDependency", typeof(SqlCacheDependencySection));
            }
        }

        internal TraceSection Trace
        {
            get
            {
                return (TraceSection) this.GetSection("system.web/trace", typeof(TraceSection));
            }
        }

        internal TrustSection Trust
        {
            get
            {
                return (TrustSection) this.GetSection("system.web/trust", typeof(TrustSection));
            }
        }

        internal UrlMappingsSection UrlMappings
        {
            get
            {
                return (UrlMappingsSection) this.GetSection("system.web/urlMappings", typeof(UrlMappingsSection), ResultsIndex.UrlMappings);
            }
        }

        internal Hashtable WebControls
        {
            get
            {
                return (Hashtable) this.GetSection("system.web/webControls", typeof(Hashtable), ResultsIndex.WebControls);
            }
        }

        internal WebPartsSection WebParts
        {
            get
            {
                return (WebPartsSection) this.GetSection("system.web/webParts", typeof(WebPartsSection), ResultsIndex.WebParts);
            }
        }

        internal XhtmlConformanceSection XhtmlConformance
        {
            get
            {
                return (XhtmlConformanceSection) this.GetSection("system.web/xhtmlConformance", typeof(XhtmlConformanceSection), ResultsIndex.XhtmlConformance);
            }
        }

        internal enum ResultsIndex
        {
            UNUSED,
            Authentication,
            Authorization,
            BrowserCaps,
            ClientTarget,
            Compilation,
            ConnectionStrings,
            Globalization,
            HttpCookies,
            HttpHandlers,
            HttpModules,
            HttpRuntime,
            Identity,
            MachineKey,
            Membership,
            OutputCache,
            OutputCacheSettings,
            Pages,
            Profile,
            SessionPageState,
            WebControls,
            WebParts,
            UrlMappings,
            XhtmlConformance,
            SIZE
        }
    }
}


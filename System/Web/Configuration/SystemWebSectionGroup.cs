namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web.Services.Configuration;

    public sealed class SystemWebSectionGroup : ConfigurationSectionGroup
    {
        [ConfigurationProperty("anonymousIdentification")]
        public AnonymousIdentificationSection AnonymousIdentification
        {
            get
            {
                return (AnonymousIdentificationSection) base.Sections["anonymousIdentification"];
            }
        }

        [ConfigurationProperty("authentication")]
        public AuthenticationSection Authentication
        {
            get
            {
                return (AuthenticationSection) base.Sections["authentication"];
            }
        }

        [ConfigurationProperty("authorization")]
        public AuthorizationSection Authorization
        {
            get
            {
                return (AuthorizationSection) base.Sections["authorization"];
            }
        }

        [ConfigurationProperty("browserCaps")]
        public DefaultSection BrowserCaps
        {
            get
            {
                return (DefaultSection) base.Sections["browserCaps"];
            }
        }

        [ConfigurationProperty("clientTarget")]
        public ClientTargetSection ClientTarget
        {
            get
            {
                return (ClientTargetSection) base.Sections["clientTarget"];
            }
        }

        [ConfigurationProperty("compilation")]
        public CompilationSection Compilation
        {
            get
            {
                return (CompilationSection) base.Sections["compilation"];
            }
        }

        [ConfigurationProperty("customErrors")]
        public CustomErrorsSection CustomErrors
        {
            get
            {
                return (CustomErrorsSection) base.Sections["customErrors"];
            }
        }

        [ConfigurationProperty("deployment")]
        public DeploymentSection Deployment
        {
            get
            {
                return (DeploymentSection) base.Sections["deployment"];
            }
        }

        [ConfigurationProperty("deviceFilters")]
        public DefaultSection DeviceFilters
        {
            get
            {
                return (DefaultSection) base.Sections["deviceFilters"];
            }
        }

        [ConfigurationProperty("fullTrustAssemblies")]
        public FullTrustAssembliesSection FullTrustAssemblies
        {
            get
            {
                return (FullTrustAssembliesSection) base.Sections["fullTrustAssemblies"];
            }
        }

        [ConfigurationProperty("globalization")]
        public GlobalizationSection Globalization
        {
            get
            {
                return (GlobalizationSection) base.Sections["globalization"];
            }
        }

        [ConfigurationProperty("healthMonitoring")]
        public HealthMonitoringSection HealthMonitoring
        {
            get
            {
                return (HealthMonitoringSection) base.Sections["healthMonitoring"];
            }
        }

        [ConfigurationProperty("hostingEnvironment")]
        public HostingEnvironmentSection HostingEnvironment
        {
            get
            {
                return (HostingEnvironmentSection) base.Sections["hostingEnvironment"];
            }
        }

        [ConfigurationProperty("httpCookies")]
        public HttpCookiesSection HttpCookies
        {
            get
            {
                return (HttpCookiesSection) base.Sections["httpCookies"];
            }
        }

        [ConfigurationProperty("httpHandlers")]
        public HttpHandlersSection HttpHandlers
        {
            get
            {
                return (HttpHandlersSection) base.Sections["httpHandlers"];
            }
        }

        [ConfigurationProperty("httpModules")]
        public HttpModulesSection HttpModules
        {
            get
            {
                return (HttpModulesSection) base.Sections["httpModules"];
            }
        }

        [ConfigurationProperty("httpRuntime")]
        public HttpRuntimeSection HttpRuntime
        {
            get
            {
                return (HttpRuntimeSection) base.Sections["httpRuntime"];
            }
        }

        [ConfigurationProperty("identity")]
        public IdentitySection Identity
        {
            get
            {
                return (IdentitySection) base.Sections["identity"];
            }
        }

        [ConfigurationProperty("machineKey")]
        public MachineKeySection MachineKey
        {
            get
            {
                return (MachineKeySection) base.Sections["machineKey"];
            }
        }

        [ConfigurationProperty("membership")]
        public MembershipSection Membership
        {
            get
            {
                return (MembershipSection) base.Sections["membership"];
            }
        }

        [ConfigurationProperty("mobileControls"), Obsolete("System.Web.Mobile.dll is obsolete.")]
        public ConfigurationSection MobileControls
        {
            get
            {
                return base.Sections["mobileControls"];
            }
        }

        [ConfigurationProperty("pages")]
        public PagesSection Pages
        {
            get
            {
                return (PagesSection) base.Sections["pages"];
            }
        }

        [ConfigurationProperty("partialTrustVisibleAssemblies")]
        public PartialTrustVisibleAssembliesSection PartialTrustVisibleAssemblies
        {
            get
            {
                return (PartialTrustVisibleAssembliesSection) base.Sections["partialTrustVisibleAssemblies"];
            }
        }

        [ConfigurationProperty("processModel")]
        public ProcessModelSection ProcessModel
        {
            get
            {
                return (ProcessModelSection) base.Sections["processModel"];
            }
        }

        [ConfigurationProperty("profile")]
        public ProfileSection Profile
        {
            get
            {
                return (ProfileSection) base.Sections["profile"];
            }
        }

        [ConfigurationProperty("protocols")]
        public DefaultSection Protocols
        {
            get
            {
                return (DefaultSection) base.Sections["protocols"];
            }
        }

        [ConfigurationProperty("roleManager")]
        public RoleManagerSection RoleManager
        {
            get
            {
                return (RoleManagerSection) base.Sections["roleManager"];
            }
        }

        [ConfigurationProperty("securityPolicy")]
        public SecurityPolicySection SecurityPolicy
        {
            get
            {
                return (SecurityPolicySection) base.Sections["securityPolicy"];
            }
        }

        [ConfigurationProperty("sessionState")]
        public SessionStateSection SessionState
        {
            get
            {
                return (SessionStateSection) base.Sections["sessionState"];
            }
        }

        [ConfigurationProperty("siteMap")]
        public SiteMapSection SiteMap
        {
            get
            {
                return (SiteMapSection) base.Sections["siteMap"];
            }
        }

        [ConfigurationProperty("trace")]
        public TraceSection Trace
        {
            get
            {
                return (TraceSection) base.Sections["trace"];
            }
        }

        [ConfigurationProperty("trust")]
        public TrustSection Trust
        {
            get
            {
                return (TrustSection) base.Sections["trust"];
            }
        }

        [ConfigurationProperty("urlMappings")]
        public UrlMappingsSection UrlMappings
        {
            get
            {
                return (UrlMappingsSection) base.Sections["urlMappings"];
            }
        }

        [ConfigurationProperty("webControls")]
        public WebControlsSection WebControls
        {
            get
            {
                return (WebControlsSection) base.Sections["webControls"];
            }
        }

        [ConfigurationProperty("webParts")]
        public WebPartsSection WebParts
        {
            get
            {
                return (WebPartsSection) base.Sections["WebParts"];
            }
        }

        [ConfigurationProperty("webServices")]
        public WebServicesSection WebServices
        {
            get
            {
                return (WebServicesSection) base.Sections["webServices"];
            }
        }

        [ConfigurationProperty("xhtmlConformance")]
        public XhtmlConformanceSection XhtmlConformance
        {
            get
            {
                return (XhtmlConformanceSection) base.Sections["xhtmlConformance"];
            }
        }
    }
}


namespace System.Web.Security
{
    using System;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Management;

    public sealed class UrlAuthorizationModule : IHttpModule
    {
        private static GenericPrincipal _AnonUser;
        private static bool s_Enabled;
        private static bool s_EnabledDetermined;

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public static bool CheckUrlAccessForPrincipal(string virtualPath, IPrincipal user, string verb)
        {
            if (virtualPath == null)
            {
                throw new ArgumentNullException("virtualPath");
            }
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (verb == null)
            {
                throw new ArgumentNullException("verb");
            }
            verb = verb.Trim();
            VirtualPath path = VirtualPath.Create(virtualPath);
            if (!path.IsWithinAppRoot)
            {
                throw new ArgumentException(System.Web.SR.GetString("Virtual_path_outside_application_not_supported"), "virtualPath");
            }
            if (!s_EnabledDetermined)
            {
                if (!HttpRuntime.UseIntegratedPipeline)
                {
                    HttpModulesSection httpModules = RuntimeConfig.GetConfig().HttpModules;
                    int count = httpModules.Modules.Count;
                    for (int i = 0; i < count; i++)
                    {
                        HttpModuleAction action = httpModules.Modules[i];
                        if (Type.GetType(action.Type, false) == typeof(UrlAuthorizationModule))
                        {
                            s_Enabled = true;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (ModuleConfigurationInfo info in HttpApplication.IntegratedModuleList)
                    {
                        if (Type.GetType(info.Type, false) == typeof(UrlAuthorizationModule))
                        {
                            s_Enabled = true;
                            break;
                        }
                    }
                }
                s_EnabledDetermined = true;
            }
            if (s_Enabled)
            {
                AuthorizationSection authorization = RuntimeConfig.GetConfig(path).Authorization;
                if (!authorization.EveryoneAllowed)
                {
                    return authorization.IsUserAllowed(user, verb);
                }
            }
            return true;
        }

        public void Dispose()
        {
        }

        public void Init(HttpApplication app)
        {
            app.AuthorizeRequest += new EventHandler(this.OnEnter);
        }

        internal static bool IsUserAllowedToPath(HttpContext context, VirtualPath virtualPath)
        {
            AuthorizationSection authorization = RuntimeConfig.GetConfig(context, virtualPath).Authorization;
            if (!authorization.EveryoneAllowed)
            {
                return authorization.IsUserAllowed(context.User, context.Request.RequestType);
            }
            return true;
        }

        private void OnEnter(object source, EventArgs eventArgs)
        {
            HttpApplication application = (HttpApplication) source;
            HttpContext context = application.Context;
            if (context.SkipAuthorization)
            {
                if ((context.User == null) || !context.User.Identity.IsAuthenticated)
                {
                    PerfCounters.IncrementCounter(AppPerfCounter.ANONYMOUS_REQUESTS);
                }
            }
            else
            {
                AuthorizationSection authorization = RuntimeConfig.GetConfig(context).Authorization;
                if (!authorization.EveryoneAllowed && !authorization.IsUserAllowed(context.User, context.Request.RequestType))
                {
                    ReportUrlAuthorizationFailure(context, this);
                }
                else
                {
                    if ((context.User == null) || !context.User.Identity.IsAuthenticated)
                    {
                        PerfCounters.IncrementCounter(AppPerfCounter.ANONYMOUS_REQUESTS);
                    }
                    WebBaseEvent.RaiseSystemEvent(this, 0xfa3);
                }
            }
        }

        internal static void ReportUrlAuthorizationFailure(HttpContext context, object webEventSource)
        {
            context.Response.StatusCode = 0x191;
            WriteErrorMessage(context);
            if ((context.User != null) && context.User.Identity.IsAuthenticated)
            {
                WebBaseEvent.RaiseSystemEvent(webEventSource, 0xfa7);
            }
            context.ApplicationInstance.CompleteRequest();
        }

        internal static bool RequestRequiresAuthorization(HttpContext context)
        {
            if (context.SkipAuthorization)
            {
                return false;
            }
            AuthorizationSection authorization = RuntimeConfig.GetConfig(context).Authorization;
            if (_AnonUser == null)
            {
                _AnonUser = new GenericPrincipal(new GenericIdentity(string.Empty, string.Empty), new string[0]);
            }
            return !authorization.IsUserAllowed(_AnonUser, context.Request.RequestType);
        }

        private static void WriteErrorMessage(HttpContext context)
        {
            context.Response.Write(UrlAuthFailedErrorFormatter.GetErrorText());
            context.Response.GenerateResponseHeadersForHandler();
        }
    }
}


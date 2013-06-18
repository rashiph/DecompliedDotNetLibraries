namespace System.ServiceModel.Activation
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.Web.Routing;

    public class ServiceRoute : Route
    {
        internal const string LeftCurlyBracket = "{";
        internal const char PathSeperator = '/';
        internal const string RightCurlyBracket = "}";
        internal const string UnmatchedPathSegment = "{*pathInfo}";

        public ServiceRoute(string routePrefix, ServiceHostFactoryBase serviceHostFactory, Type serviceType) : base(CheckAndCreateRouteString(routePrefix), new ServiceRouteHandler(routePrefix, serviceHostFactory, serviceType))
        {
            if (TD.AspNetRouteIsEnabled())
            {
                TD.AspNetRoute(routePrefix, serviceType.AssemblyQualifiedName, serviceHostFactory.GetType().AssemblyQualifiedName);
            }
        }

        private static string CheckAndCreateRouteString(string routePrefix)
        {
            ServiceHostingEnvironment.EnsureInitialized();
            if (!ServiceHostingEnvironment.AspNetCompatibilityEnabled)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_RouteServiceRequiresCompatibilityMode));
            }
            if (routePrefix == null)
            {
                throw FxTrace.Exception.ArgumentNull("routePrefix");
            }
            if (routePrefix.Contains("{") || routePrefix.Contains("}"))
            {
                throw FxTrace.Exception.Argument("routePrefix", System.ServiceModel.Activation.SR.Hosting_CurlyBracketFoundInRoutePrefix("{", "}"));
            }
            char ch = '/';
            if (routePrefix.EndsWith(ch.ToString(), StringComparison.CurrentCultureIgnoreCase) || routePrefix.Equals(string.Empty, StringComparison.CurrentCultureIgnoreCase))
            {
                routePrefix = string.Format(CultureInfo.CurrentCulture, "{0}{1}", new object[] { routePrefix, "{*pathInfo}" });
                return routePrefix;
            }
            routePrefix = string.Format(CultureInfo.CurrentCulture, "{0}/{1}", new object[] { routePrefix, "{*pathInfo}" });
            return routePrefix;
        }
    }
}


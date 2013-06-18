namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Web;
    using System.Web.Routing;

    internal class ServiceRouteHandler : IRouteHandler
    {
        private string baseAddress;
        private IHttpHandler handler;
        private object locker = new object();
        private static Hashtable routeServiceTable = new Hashtable(StringComparer.CurrentCultureIgnoreCase);

        public ServiceRouteHandler(string baseAddress, ServiceHostFactoryBase serviceHostFactory, Type webServiceType)
        {
            this.baseAddress = string.Format(CultureInfo.CurrentCulture, "~/{0}", new object[] { baseAddress });
            if (webServiceType == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("webServiceType"));
            }
            if (serviceHostFactory == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("serviceHostFactory"));
            }
            string assemblyQualifiedName = webServiceType.AssemblyQualifiedName;
            AddServiceInfo(this.baseAddress, new ServiceDeploymentInfo(this.baseAddress, serviceHostFactory, assemblyQualifiedName));
        }

        private static void AddServiceInfo(string virtualPath, ServiceDeploymentInfo serviceInfo)
        {
            try
            {
                routeServiceTable.Add(virtualPath, serviceInfo);
            }
            catch (ArgumentException)
            {
                throw FxTrace.Exception.Argument("virtualPath", System.ServiceModel.Activation.SR.Hosting_RouteHasAlreadyBeenAdded(virtualPath));
            }
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            if (this.handler == null)
            {
                lock (this.locker)
                {
                    if (this.handler == null)
                    {
                        this.handler = new AspNetRouteServiceHttpHandler(this.baseAddress);
                        MarkRouteAsActive(this.baseAddress);
                    }
                }
            }
            return this.handler;
        }

        public static ServiceDeploymentInfo GetServiceInfo(string normalizedVirtualPath)
        {
            return (ServiceDeploymentInfo) routeServiceTable[normalizedVirtualPath];
        }

        public static bool IsActiveAspNetRoute(string virtualPath)
        {
            bool messageHandledByRoute = false;
            if (!string.IsNullOrEmpty(virtualPath))
            {
                ServiceDeploymentInfo info = (ServiceDeploymentInfo) routeServiceTable[virtualPath];
                if (info != null)
                {
                    messageHandledByRoute = info.MessageHandledByRoute;
                }
            }
            return messageHandledByRoute;
        }

        public static void MarkARouteAsInactive(string normalizedVirtualPath)
        {
            ServiceDeploymentInfo info = (ServiceDeploymentInfo) routeServiceTable[normalizedVirtualPath];
            if (info != null)
            {
                info.MessageHandledByRoute = false;
            }
        }

        public static void MarkRouteAsActive(string normalizedVirtualPath)
        {
            ServiceDeploymentInfo info = (ServiceDeploymentInfo) routeServiceTable[normalizedVirtualPath];
            if (info != null)
            {
                info.MessageHandledByRoute = true;
            }
        }
    }
}


namespace System.Web.Routing
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Security;
    using System.Web.UI;

    public class PageRouteHandler : IRouteHandler
    {
        private Route _routeVirtualPath;
        private bool _useRouteVirtualPath;

        public PageRouteHandler(string virtualPath) : this(virtualPath, true)
        {
        }

        public PageRouteHandler(string virtualPath, bool checkPhysicalUrlAccess)
        {
            if (string.IsNullOrEmpty(virtualPath) || !virtualPath.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(System.Web.SR.GetString("PageRouteHandler_InvalidVirtualPath"), "virtualPath");
            }
            this.VirtualPath = virtualPath;
            this.CheckPhysicalUrlAccess = checkPhysicalUrlAccess;
            this._useRouteVirtualPath = this.VirtualPath.Contains("{");
        }

        private bool CheckUrlAccess(string virtualPath, RequestContext requestContext)
        {
            IPrincipal user = requestContext.HttpContext.User;
            if (user == null)
            {
                user = new GenericPrincipal(new GenericIdentity(string.Empty, string.Empty), new string[0]);
            }
            return this.CheckUrlAccessWithAssert(virtualPath, requestContext, user);
        }

        [SecurityPermission(SecurityAction.Assert, Unrestricted=true)]
        private bool CheckUrlAccessWithAssert(string virtualPath, RequestContext requestContext, IPrincipal user)
        {
            return UrlAuthorizationModule.CheckUrlAccessForPrincipal(virtualPath, user, requestContext.HttpContext.Request.HttpMethod);
        }

        public virtual IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }
            string substitutedVirtualPath = this.GetSubstitutedVirtualPath(requestContext);
            int index = substitutedVirtualPath.IndexOf('?');
            if (index != -1)
            {
                substitutedVirtualPath = substitutedVirtualPath.Substring(0, index);
            }
            if (this.CheckPhysicalUrlAccess && !this.CheckUrlAccess(substitutedVirtualPath, requestContext))
            {
                return new UrlAuthFailureHandler();
            }
            return (BuildManager.CreateInstanceFromVirtualPath(substitutedVirtualPath, typeof(Page)) as Page);
        }

        public string GetSubstitutedVirtualPath(RequestContext requestContext)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }
            if (!this._useRouteVirtualPath)
            {
                return this.VirtualPath;
            }
            VirtualPathData virtualPath = this.RouteVirtualPath.GetVirtualPath(requestContext, requestContext.RouteData.Values);
            if (virtualPath == null)
            {
                return this.VirtualPath;
            }
            return ("~/" + virtualPath.VirtualPath);
        }

        public bool CheckPhysicalUrlAccess { get; private set; }

        private Route RouteVirtualPath
        {
            get
            {
                if (this._routeVirtualPath == null)
                {
                    this._routeVirtualPath = new Route(this.VirtualPath.Substring(2), this);
                }
                return this._routeVirtualPath;
            }
        }

        public string VirtualPath { get; private set; }
    }
}


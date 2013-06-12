namespace System.Web.Routing
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Web;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class RouteBase
    {
        protected RouteBase()
        {
        }

        public abstract RouteData GetRouteData(HttpContextBase httpContext);
        public abstract VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values);
    }
}


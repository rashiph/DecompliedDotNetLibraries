namespace System.Web.Routing
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Web;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class RequestContext
    {
        public RequestContext()
        {
        }

        public RequestContext(HttpContextBase httpContext, System.Web.Routing.RouteData routeData)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }
            if (routeData == null)
            {
                throw new ArgumentNullException("routeData");
            }
            this.HttpContext = httpContext;
            this.RouteData = routeData;
        }

        public virtual HttpContextBase HttpContext { get; set; }

        public virtual System.Web.Routing.RouteData RouteData { get; set; }
    }
}


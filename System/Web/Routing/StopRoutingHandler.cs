namespace System.Web.Routing
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Web;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class StopRoutingHandler : IRouteHandler
    {
        protected virtual IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            throw new NotSupportedException();
        }

        IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext)
        {
            return this.GetHttpHandler(requestContext);
        }
    }
}


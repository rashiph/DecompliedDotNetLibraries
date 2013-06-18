namespace System.Web.Routing
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Web;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class UrlRoutingHandler : IHttpHandler
    {
        private System.Web.Routing.RouteCollection _routeCollection;

        protected UrlRoutingHandler()
        {
        }

        protected virtual void ProcessRequest(HttpContext httpContext)
        {
            this.ProcessRequest(new HttpContextWrapper(httpContext));
        }

        protected virtual void ProcessRequest(HttpContextBase httpContext)
        {
            RouteData routeData = this.RouteCollection.GetRouteData(httpContext);
            if (routeData == null)
            {
                throw new HttpException(0x194, System.Web.SR.GetString("UrlRoutingHandler_NoRouteMatches"));
            }
            IRouteHandler routeHandler = routeData.RouteHandler;
            if (routeHandler == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("UrlRoutingModule_NoRouteHandler"));
            }
            RequestContext requestContext = new RequestContext(httpContext, routeData);
            IHttpHandler httpHandler = routeHandler.GetHttpHandler(requestContext);
            if (httpHandler == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, System.Web.SR.GetString("UrlRoutingModule_NoHttpHandler"), new object[] { routeHandler.GetType() }));
            }
            this.VerifyAndProcessRequest(httpHandler, httpContext);
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            this.ProcessRequest(context);
        }

        protected abstract void VerifyAndProcessRequest(IHttpHandler httpHandler, HttpContextBase httpContext);

        protected virtual bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public System.Web.Routing.RouteCollection RouteCollection
        {
            get
            {
                if (this._routeCollection == null)
                {
                    this._routeCollection = RouteTable.Routes;
                }
                return this._routeCollection;
            }
            set
            {
                this._routeCollection = value;
            }
        }

        bool IHttpHandler.IsReusable
        {
            get
            {
                return this.IsReusable;
            }
        }
    }
}


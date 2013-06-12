namespace System.Web.Routing
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Web;
    using System.Web.Security;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class UrlRoutingModule : IHttpModule
    {
        private static readonly object _contextKey = new object();
        private static readonly object _requestDataKey = new object();
        private System.Web.Routing.RouteCollection _routeCollection;

        protected virtual void Dispose()
        {
        }

        protected virtual void Init(HttpApplication application)
        {
            if (application.Context.Items[_contextKey] == null)
            {
                application.Context.Items[_contextKey] = _contextKey;
                application.PostResolveRequestCache += new EventHandler(this.OnApplicationPostResolveRequestCache);
            }
        }

        private void OnApplicationPostResolveRequestCache(object sender, EventArgs e)
        {
            HttpContextBase context = new HttpContextWrapper(((HttpApplication) sender).Context);
            this.PostResolveRequestCache(context);
        }

        [Obsolete("This method is obsolete. Override the Init method to use the PostMapRequestHandler event.")]
        public virtual void PostMapRequestHandler(HttpContextBase context)
        {
        }

        public virtual void PostResolveRequestCache(HttpContextBase context)
        {
            RouteData routeData = this.RouteCollection.GetRouteData(context);
            if (routeData != null)
            {
                IRouteHandler routeHandler = routeData.RouteHandler;
                if (routeHandler == null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, System.Web.SR.GetString("UrlRoutingModule_NoRouteHandler"), new object[0]));
                }
                if (!(routeHandler is StopRoutingHandler))
                {
                    RequestContext requestContext = new RequestContext(context, routeData);
                    context.Request.RequestContext = requestContext;
                    IHttpHandler httpHandler = routeHandler.GetHttpHandler(requestContext);
                    if (httpHandler == null)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, System.Web.SR.GetString("UrlRoutingModule_NoHttpHandler"), new object[] { routeHandler.GetType() }));
                    }
                    if (httpHandler is UrlAuthFailureHandler)
                    {
                        if (!FormsAuthenticationModule.FormsAuthRequired)
                        {
                            throw new HttpException(0x191, System.Web.SR.GetString("Assess_Denied_Description3"));
                        }
                        UrlAuthorizationModule.ReportUrlAuthorizationFailure(HttpContext.Current, this);
                    }
                    else
                    {
                        context.RemapHandler(httpHandler);
                    }
                }
            }
        }

        void IHttpModule.Dispose()
        {
            this.Dispose();
        }

        void IHttpModule.Init(HttpApplication application)
        {
            this.Init(application);
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
    }
}


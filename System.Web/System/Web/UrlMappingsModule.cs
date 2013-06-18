namespace System.Web
{
    using System;
    using System.Web.Configuration;

    internal sealed class UrlMappingsModule : IHttpModule
    {
        internal UrlMappingsModule()
        {
        }

        public void Dispose()
        {
        }

        public void Init(HttpApplication application)
        {
            UrlMappingsSection urlMappings = RuntimeConfig.GetConfig().UrlMappings;
            if (urlMappings.IsEnabled && (urlMappings.UrlMappings.Count > 0))
            {
                application.BeginRequest += new EventHandler(this.OnEnter);
            }
        }

        internal void OnEnter(object source, EventArgs eventArgs)
        {
            HttpApplication application = (HttpApplication) source;
            UrlMappingRewritePath(application.Context);
        }

        internal static void UrlMappingRewritePath(HttpContext context)
        {
            HttpRequest request = context.Request;
            UrlMappingsSection urlMappings = RuntimeConfig.GetAppConfig().UrlMappings;
            string path = request.Path;
            string str2 = null;
            string queryStringText = request.QueryStringText;
            if (!string.IsNullOrEmpty(queryStringText))
            {
                str2 = urlMappings.HttpResolveMapping(path + "?" + queryStringText);
            }
            if (str2 == null)
            {
                str2 = urlMappings.HttpResolveMapping(path);
            }
            if (!string.IsNullOrEmpty(str2))
            {
                context.RewritePath(str2, false);
            }
        }
    }
}


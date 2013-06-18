namespace System.Web.Configuration
{
    using System;
    using System.Web;

    public abstract class HttpCapabilitiesProvider
    {
        protected HttpCapabilitiesProvider()
        {
        }

        public abstract HttpBrowserCapabilities GetBrowserCapabilities(HttpRequest request);
    }
}


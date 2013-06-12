namespace System.Web.Routing
{
    using System;
    using System.Web;

    internal sealed class UrlAuthFailureHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}


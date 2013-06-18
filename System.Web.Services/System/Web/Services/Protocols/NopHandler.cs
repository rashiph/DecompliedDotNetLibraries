namespace System.Web.Services.Protocols
{
    using System;
    using System.Web;

    internal class NopHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}


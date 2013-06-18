namespace System.Web
{
    using System;

    internal class HttpMethodNotAllowedHandler : IHttpHandler
    {
        internal HttpMethodNotAllowedHandler()
        {
        }

        public void ProcessRequest(HttpContext context)
        {
            throw new HttpException(0x195, System.Web.SR.GetString("Path_forbidden", new object[] { context.Request.HttpMethod }));
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


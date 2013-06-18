namespace System.Web
{
    using System;

    internal class HttpNotImplementedHandler : IHttpHandler
    {
        internal HttpNotImplementedHandler()
        {
        }

        public void ProcessRequest(HttpContext context)
        {
            throw new HttpException(0x1f5, System.Web.SR.GetString("Method_for_path_not_implemented", new object[] { context.Request.HttpMethod, context.Request.Path }));
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


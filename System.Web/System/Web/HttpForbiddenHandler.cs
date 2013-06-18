namespace System.Web
{
    using System;

    internal class HttpForbiddenHandler : IHttpHandler
    {
        internal HttpForbiddenHandler()
        {
        }

        public void ProcessRequest(HttpContext context)
        {
            PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_NOT_FOUND);
            throw new HttpException(0x193, System.Web.SR.GetString("Path_forbidden", new object[] { context.Request.Path }));
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


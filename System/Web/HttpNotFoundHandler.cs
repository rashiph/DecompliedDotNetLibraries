namespace System.Web
{
    using System;

    internal class HttpNotFoundHandler : IHttpHandler
    {
        internal HttpNotFoundHandler()
        {
        }

        public void ProcessRequest(HttpContext context)
        {
            PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_NOT_FOUND);
            throw new HttpException(0x194, System.Web.SR.GetString("Path_not_found", new object[] { context.Request.Path }));
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


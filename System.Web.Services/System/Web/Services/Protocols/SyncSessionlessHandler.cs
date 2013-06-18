namespace System.Web.Services.Protocols
{
    using System;
    using System.Web;
    using System.Web.Services.Diagnostics;

    internal class SyncSessionlessHandler : WebServiceHandler, IHttpHandler
    {
        internal SyncSessionlessHandler(ServerProtocol protocol) : base(protocol)
        {
        }

        public void ProcessRequest(HttpContext context)
        {
            TraceMethod caller = Tracing.On ? new TraceMethod(this, "ProcessRequest", new object[0]) : null;
            if (Tracing.On)
            {
                Tracing.Enter("IHttpHandler.ProcessRequest", caller, Tracing.Details(context.Request));
            }
            base.CoreProcessRequest();
            if (Tracing.On)
            {
                Tracing.Exit("IHttpHandler.ProcessRequest", caller);
            }
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


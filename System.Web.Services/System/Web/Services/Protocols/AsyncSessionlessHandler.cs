namespace System.Web.Services.Protocols
{
    using System;
    using System.Web;
    using System.Web.Services.Diagnostics;

    internal class AsyncSessionlessHandler : SyncSessionlessHandler, IHttpAsyncHandler, IHttpHandler
    {
        internal AsyncSessionlessHandler(ServerProtocol protocol) : base(protocol)
        {
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback callback, object asyncState)
        {
            TraceMethod caller = Tracing.On ? new TraceMethod(this, "BeginProcessRequest", new object[0]) : null;
            if (Tracing.On)
            {
                Tracing.Enter("IHttpAsyncHandler.BeginProcessRequest", caller, Tracing.Details(context.Request));
            }
            IAsyncResult result = base.BeginCoreProcessRequest(callback, asyncState);
            if (Tracing.On)
            {
                Tracing.Exit("IHttpAsyncHandler.BeginProcessRequest", caller);
            }
            return result;
        }

        public void EndProcessRequest(IAsyncResult asyncResult)
        {
            TraceMethod caller = Tracing.On ? new TraceMethod(this, "EndProcessRequest", new object[0]) : null;
            if (Tracing.On)
            {
                Tracing.Enter("IHttpAsyncHandler.EndProcessRequest", caller);
            }
            base.EndCoreProcessRequest(asyncResult);
            if (Tracing.On)
            {
                Tracing.Exit("IHttpAsyncHandler.EndProcessRequest", caller);
            }
        }
    }
}


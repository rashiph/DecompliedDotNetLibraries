namespace System.ServiceModel.Activation
{
    using System;
    using System.Security;
    using System.ServiceModel;
    using System.Web;
    using System.Web.SessionState;

    internal class ServiceHttpHandlerFactory : IHttpHandlerFactory
    {
        private IHttpHandler handler;

        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            if (this.handler == null)
            {
                this.handler = new ServiceHttpHandler();
            }
            return this.handler;
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
        }

        private class ServiceHttpHandler : IHttpAsyncHandler, IHttpHandler, IRequiresSessionState
        {
            [SecurityCritical]
            public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback callback, object extraData)
            {
                ServiceHostingEnvironment.SafeEnsureInitialized();
                return new HostedHttpRequestAsyncResult(context.ApplicationInstance, true, false, callback, extraData);
            }

            public void EndProcessRequest(IAsyncResult result)
            {
                HostedHttpRequestAsyncResult.End(result);
            }

            [SecurityCritical]
            public void ProcessRequest(HttpContext context)
            {
                ServiceHostingEnvironment.SafeEnsureInitialized();
                HostedHttpRequestAsyncResult.ExecuteSynchronous(context.ApplicationInstance, true, false);
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
}


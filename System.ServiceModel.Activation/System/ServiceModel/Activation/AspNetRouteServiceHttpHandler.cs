namespace System.ServiceModel.Activation
{
    using System;
    using System.Security;
    using System.ServiceModel;
    using System.Web;
    using System.Web.SessionState;

    internal class AspNetRouteServiceHttpHandler : IHttpAsyncHandler, IHttpHandler, IRequiresSessionState
    {
        private string serviceVirtualPath;

        public AspNetRouteServiceHttpHandler(string virtualPath)
        {
            this.serviceVirtualPath = virtualPath;
        }

        [SecurityCritical]
        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback callback, object extraData)
        {
            ServiceHostingEnvironment.SafeEnsureInitialized();
            return new HostedHttpRequestAsyncResult(context.ApplicationInstance, this.serviceVirtualPath, true, false, callback, extraData);
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            HostedHttpRequestAsyncResult.End(result);
        }

        [SecurityCritical]
        public void ProcessRequest(HttpContext context)
        {
            ServiceHostingEnvironment.SafeEnsureInitialized();
            HostedHttpRequestAsyncResult.ExecuteSynchronous(context.ApplicationInstance, this.serviceVirtualPath, true, false);
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


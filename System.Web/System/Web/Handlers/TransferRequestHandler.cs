namespace System.Web.Handlers
{
    using System;
    using System.Web;
    using System.Web.Hosting;

    internal class TransferRequestHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            IIS7WorkerRequest workerRequest = context.WorkerRequest as IIS7WorkerRequest;
            if (workerRequest == null)
            {
                throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
            }
            bool preserveUser = false;
            workerRequest.ScheduleExecuteUrl(null, null, null, true, context.Request.EntityBody, null, preserveUser);
            context.ApplicationInstance.EnsureReleaseState();
            context.ApplicationInstance.CompleteRequest();
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


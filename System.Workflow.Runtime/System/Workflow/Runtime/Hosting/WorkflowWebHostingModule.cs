namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Diagnostics;
    using System.Web;
    using System.Workflow.Runtime;

    public sealed class WorkflowWebHostingModule : IHttpModule
    {
        private HttpApplication currentApplication;

        public WorkflowWebHostingModule()
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Workflow Web Hosting Module Created");
        }

        private void OnAcquireRequestState(object sender, EventArgs e)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WebHost Module Routing Begin");
            HttpCookie cookie = HttpContext.Current.Request.Cookies.Get("WF_WorkflowInstanceId");
            if (cookie != null)
            {
                HttpContext.Current.Items.Add("__WorkflowInstanceId__", new Guid(cookie.Value));
            }
        }

        private void OnReleaseRequestState(object sender, EventArgs e)
        {
            if (HttpContext.Current.Request.Cookies.Get("WF_WorkflowInstanceId") == null)
            {
                HttpCookie cookie = new HttpCookie("WF_WorkflowInstanceId");
                object obj2 = HttpContext.Current.Items["__WorkflowInstanceId__"];
                if (obj2 != null)
                {
                    cookie.Value = obj2.ToString();
                    HttpContext.Current.Response.Cookies.Add(cookie);
                }
            }
        }

        void IHttpModule.Dispose()
        {
        }

        void IHttpModule.Init(HttpApplication application)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Workflow Web Hosting Module Initialized");
            this.currentApplication = application;
            application.ReleaseRequestState += new EventHandler(this.OnReleaseRequestState);
            application.AcquireRequestState += new EventHandler(this.OnAcquireRequestState);
        }
    }
}


namespace System.ServiceModel.Activation
{
    using System;
    using System.Diagnostics;
    using System.Security;
    using System.ServiceModel;
    using System.Web;

    internal class HttpModule : IHttpModule
    {
        private static bool disabled;

        public void Dispose()
        {
        }

        [SecurityCritical]
        public void Init(HttpApplication context)
        {
            context.PostAuthenticateRequest += new EventHandler(HttpModule.ProcessRequest);
        }

        [SecurityCritical]
        private static void ProcessRequest(object sender, EventArgs e)
        {
            if (!disabled)
            {
                try
                {
                    ServiceHostingEnvironment.SafeEnsureInitialized();
                }
                catch (SecurityException exception)
                {
                    disabled = true;
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                    }
                    return;
                }
                HttpApplication application = (HttpApplication) sender;
                string currentExecutionFilePathExtension = application.Request.CurrentExecutionFilePathExtension;
                if (!string.IsNullOrEmpty(currentExecutionFilePathExtension))
                {
                    ServiceHostingEnvironment.ServiceType serviceType = ServiceHostingEnvironment.GetServiceType(currentExecutionFilePathExtension);
                    if (serviceType != ServiceHostingEnvironment.ServiceType.Unknown)
                    {
                        if (ServiceHostingEnvironment.AspNetCompatibilityEnabled)
                        {
                            if ((serviceType == ServiceHostingEnvironment.ServiceType.Workflow) && ServiceHostingEnvironment.IsConfigurationBasedService(application))
                            {
                                application.Context.RemapHandler(new HttpHandler());
                            }
                        }
                        else
                        {
                            switch (serviceType)
                            {
                                case ServiceHostingEnvironment.ServiceType.WCF:
                                    HostedHttpRequestAsyncResult.ExecuteSynchronous(application, false, false);
                                    return;

                                case ServiceHostingEnvironment.ServiceType.Workflow:
                                    HostedHttpRequestAsyncResult.ExecuteSynchronous(application, false, true);
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}


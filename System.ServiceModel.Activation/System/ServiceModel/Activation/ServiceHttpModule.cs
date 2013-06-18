namespace System.ServiceModel.Activation
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.Web;

    internal class ServiceHttpModule : IHttpModule
    {
        [SecurityCritical]
        private static BeginEventHandler beginEventHandler;
        private static CompletedAsyncResult cachedAsyncResult = new CompletedAsyncResult(null, null);
        private static bool disabled;
        [SecurityCritical]
        private static EndEventHandler endEventHandler;

        [SecurityCritical]
        public static IAsyncResult BeginProcessRequest(object sender, EventArgs e, AsyncCallback cb, object extraData)
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
                    return cachedAsyncResult;
                }
                HttpApplication application = (HttpApplication) sender;
                string currentExecutionFilePathExtension = application.Request.CurrentExecutionFilePathExtension;
                if (string.IsNullOrEmpty(currentExecutionFilePathExtension))
                {
                    return cachedAsyncResult;
                }
                ServiceHostingEnvironment.ServiceType serviceType = ServiceHostingEnvironment.GetServiceType(currentExecutionFilePathExtension);
                if (serviceType == ServiceHostingEnvironment.ServiceType.Unknown)
                {
                    return cachedAsyncResult;
                }
                if (ServiceHostingEnvironment.AspNetCompatibilityEnabled)
                {
                    if ((serviceType == ServiceHostingEnvironment.ServiceType.Workflow) && ServiceHostingEnvironment.IsConfigurationBasedService(application))
                    {
                        IHttpHandler handler = new ServiceHttpHandlerFactory().GetHandler(application.Context, application.Request.RequestType, application.Request.RawUrl.ToString(), application.Request.PhysicalApplicationPath);
                        application.Context.RemapHandler(handler);
                    }
                    return cachedAsyncResult;
                }
                switch (serviceType)
                {
                    case ServiceHostingEnvironment.ServiceType.WCF:
                        return new HostedHttpRequestAsyncResult(application, false, false, cb, extraData);

                    case ServiceHostingEnvironment.ServiceType.Workflow:
                        return new HostedHttpRequestAsyncResult(application, false, true, cb, extraData);
                }
            }
            return cachedAsyncResult;
        }

        public void Dispose()
        {
        }

        public static void EndProcessRequest(IAsyncResult ar)
        {
            if (ar is HostedHttpRequestAsyncResult)
            {
                HostedHttpRequestAsyncResult.End(ar);
            }
        }

        [SecurityCritical]
        public void Init(HttpApplication context)
        {
            if (beginEventHandler == null)
            {
                beginEventHandler = new BeginEventHandler(ServiceHttpModule.BeginProcessRequest);
            }
            if (endEventHandler == null)
            {
                endEventHandler = new EndEventHandler(ServiceHttpModule.EndProcessRequest);
            }
            context.AddOnPostAuthenticateRequestAsync(beginEventHandler, endEventHandler);
        }
    }
}


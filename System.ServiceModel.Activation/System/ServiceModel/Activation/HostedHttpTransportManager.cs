namespace System.ServiceModel.Activation
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Web;

    internal class HostedHttpTransportManager : HttpTransportManager
    {
        private static bool canTraceConnectionInformation = true;
        private string host;
        private int port;
        private string scheme;

        internal HostedHttpTransportManager(BaseUriWithWildcard baseAddress) : base(baseAddress.BaseAddress, baseAddress.HostNameComparisonMode)
        {
            base.IsHosted = true;
        }

        [SecuritySafeCritical]
        public ServiceModelActivity CreateReceiveBytesActivity(HostedHttpRequestAsyncResult result)
        {
            ServiceModelActivity activity = null;
            if (result != null)
            {
                base.TraceMessageReceived(result.RequestUri);
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    activity = ServiceModelActivity.CreateBoundedActivity(GetRequestTraceIdentifier(result.Application.Context));
                    base.StartReceiveBytesActivity(activity, result.RequestUri);
                }
            }
            return activity;
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        private static Guid GetRequestTraceIdentifier(IServiceProvider provider)
        {
            return ((HttpWorkerRequest) provider.GetService(typeof(HttpWorkerRequest))).RequestTraceIdentifier;
        }

        internal void HttpContextReceived(HostedHttpRequestAsyncResult result)
        {
            using (DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.BoundOperation(base.Activity) : null)
            {
                using (this.CreateReceiveBytesActivity(result))
                {
                    HttpChannelListener listener;
                    this.TraceConnectionInformation(result);
                    if (base.TryLookupUri(result.RequestUri, result.GetHttpMethod(), base.HostNameComparisonMode, out listener))
                    {
                        HostedHttpContext context = new HostedHttpContext(listener, result);
                        listener.HttpContextReceived(context, null);
                    }
                    else
                    {
                        if (DiagnosticUtility.ShouldTraceError)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Error, 0x4000b, System.ServiceModel.Activation.SR.TraceCodeHttpChannelMessageReceiveFailed, new StringTraceRecord("IsRecycling", ServiceHostingEnvironment.IsRecycling.ToString(CultureInfo.CurrentCulture)), this, null);
                        }
                        if (ServiceHostingEnvironment.IsRecycling)
                        {
                            throw FxTrace.Exception.AsError(new EndpointNotFoundException(System.ServiceModel.Activation.SR.Hosting_ListenerNotFoundForActivationInRecycling(result.RequestUri.ToString())));
                        }
                        throw FxTrace.Exception.AsError(new EndpointNotFoundException(System.ServiceModel.Activation.SR.Hosting_ListenerNotFoundForActivation(result.RequestUri.ToString())));
                    }
                }
            }
        }

        internal override bool IsCompatible(HttpChannelListener factory)
        {
            return true;
        }

        internal override void OnAbort()
        {
        }

        internal override void OnClose(TimeSpan timeout)
        {
        }

        internal override void OnOpen()
        {
        }

        public void TraceConnectionInformation(HostedHttpRequestAsyncResult result)
        {
            if (((result != null) && DiagnosticUtility.ShouldTraceInformation) && canTraceConnectionInformation)
            {
                try
                {
                    HttpWorkerRequest service = (HttpWorkerRequest) ((IServiceProvider) result.Application.Context).GetService(typeof(HttpWorkerRequest));
                    string localEndpoint = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", new object[] { service.GetLocalAddress(), service.GetLocalPort() });
                    string remoteEndpoint = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", new object[] { service.GetRemoteAddress(), service.GetRemotePort() });
                    TraceUtility.TraceHttpConnectionInformation(localEndpoint, remoteEndpoint, this);
                }
                catch (SecurityException exception)
                {
                    canTraceConnectionInformation = false;
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                    }
                }
            }
        }

        internal string Host
        {
            get
            {
                return (this.host ?? (this.host = base.ListenUri.Host));
            }
        }

        internal int Port
        {
            get
            {
                if (this.port != 0)
                {
                    return this.port;
                }
                return (this.port = base.ListenUri.Port);
            }
        }

        internal override string Scheme
        {
            get
            {
                return (this.scheme ?? (this.scheme = base.ListenUri.Scheme));
            }
        }
    }
}


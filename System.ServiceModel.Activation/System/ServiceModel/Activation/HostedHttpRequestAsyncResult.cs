namespace System.ServiceModel.Activation
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation.Diagnostics;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Threading;
    using System.Web;
    using System.Web.Routing;

    internal class HostedHttpRequestAsyncResult : AsyncResult, HttpChannelListener.IHttpAuthenticationContext
    {
        [SecurityCritical]
        private static WindowsIdentity anonymousIdentity;
        private string configurationBasedServiceVirtualPath;
        [SecurityCritical]
        private HttpApplication context;
        [SecurityCritical]
        private static ContextCallback contextOnBeginRequest;
        private bool ensureWFService;
        [SecurityCritical]
        private bool flowContext;
        [SecurityCritical]
        private System.ServiceModel.Activation.HostedThreadData hostedThreadData;
        private static bool? iisSupportsExtendedProtection;
        [SecurityCritical]
        private HostedImpersonationContext impersonationContext;
        [SecurityCritical]
        private static AsyncCallback processRequestCompleteCallback;
        private int state;
        [ThreadStatic]
        private static AutoResetEvent waitObject;
        [SecurityCritical]
        private static Action<object> waitOnBeginRequest;
        [SecurityCritical]
        private static Action<object> waitOnBeginRequestWithFlow;

        [SecurityCritical]
        public HostedHttpRequestAsyncResult(HttpApplication context, bool flowContext, bool ensureWFService, AsyncCallback callback, object state) : this(context, null, flowContext, ensureWFService, callback, state)
        {
        }

        [SecurityCritical]
        public HostedHttpRequestAsyncResult(HttpApplication context, string aspNetRouteServiceVirtualPath, bool flowContext, bool ensureWFService, AsyncCallback callback, object state) : base(callback, state)
        {
            if (context == null)
            {
                throw System.ServiceModel.Activation.FxTrace.Exception.ArgumentNull("context");
            }
            this.context = context;
            this.flowContext = flowContext;
            if (ensureWFService)
            {
                if (ServiceHostingEnvironment.IsConfigurationBasedService(context, out this.configurationBasedServiceVirtualPath))
                {
                    this.ensureWFService = false;
                }
                else
                {
                    this.ensureWFService = true;
                }
            }
            if (!string.IsNullOrEmpty(aspNetRouteServiceVirtualPath))
            {
                if (!RouteTable.Routes.RouteExistingFiles && ServiceHostingEnvironment.IsConfigurationBasedService(context, out this.configurationBasedServiceVirtualPath))
                {
                    this.AspNetRouteServiceVirtualPath = null;
                }
                else
                {
                    this.AspNetRouteServiceVirtualPath = aspNetRouteServiceVirtualPath;
                }
            }
            string strA = context.Request.HttpMethod ?? "";
            char ch = (strA.Length == 5) ? strA[0] : '\0';
            if (((ch == 'd') || (ch == 'D')) && (string.Compare(strA, "DEBUG", StringComparison.OrdinalIgnoreCase) == 0))
            {
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    System.ServiceModel.Activation.Diagnostics.TraceUtility.TraceEvent(TraceEventType.Verbose, 0x90005, System.ServiceModel.Activation.SR.TraceCodeWebHostDebugRequest, this);
                }
                this.state = 1;
                base.Complete(true, null);
            }
            else
            {
                this.impersonationContext = new HostedImpersonationContext();
                if (flowContext && ServiceHostingEnvironment.AspNetCompatibilityEnabled)
                {
                    this.hostedThreadData = new System.ServiceModel.Activation.HostedThreadData();
                }
                Action<object> action = (AspNetPartialTrustHelpers.NeedPartialTrustInvoke || flowContext) ? WaitOnBeginRequestWithFlow : WaitOnBeginRequest;
                if (!ServiceHostingEnvironment.AspNetCompatibilityEnabled && !this.ensureWFService)
                {
                    context.CompleteRequest();
                }
                context.Server.ScriptTimeout = 0x7fffffff;
                ServiceHostingEnvironment.IncrementRequestCount();
                IOThreadScheduler.ScheduleCallbackLowPriNoFlow(action, this);
            }
        }

        public void Abort()
        {
            if ((this.state == 0) && (Interlocked.CompareExchange(ref this.state, 2, 0) == 0))
            {
                this.Application.Response.Close();
                base.Complete(false, null);
                ServiceHostingEnvironment.DecrementRequestCount();
            }
        }

        [SecuritySafeCritical]
        internal void AppendHeader(string name, string value)
        {
            this.context.Response.AppendHeader(name, value);
        }

        private void BeginRequest()
        {
            try
            {
                this.HandleRequest();
            }
            catch (EndpointNotFoundException exception)
            {
                if (string.Compare(this.GetHttpMethod(), "GET", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new HttpException(0x194, exception.Message, exception));
                }
                this.SetStatusCode(0x194);
                this.CompleteOperation(null);
            }
            catch (ServiceActivationException exception2)
            {
                if (string.Compare(this.GetHttpMethod(), "GET", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (exception2.InnerException is HttpException)
                    {
                        throw exception2.InnerException;
                    }
                    throw;
                }
                this.SetStatusCode(500);
                this.SetStatusDescription("System.ServiceModel.ServiceActivationException");
                this.CompleteOperation(null);
            }
            finally
            {
                this.ReleaseImpersonation();
            }
        }

        private void CompleteOperation(Exception exception)
        {
            if ((this.state == 0) && (Interlocked.CompareExchange(ref this.state, 1, 0) == 0))
            {
                base.Complete(false, exception);
                ServiceHostingEnvironment.DecrementRequestCount();
            }
        }

        [SecuritySafeCritical]
        internal void CompleteRequest()
        {
            this.context.CompleteRequest();
        }

        public static void End(IAsyncResult result)
        {
            try
            {
                AsyncResult.End<HostedHttpRequestAsyncResult>(result);
            }
            catch (Exception exception)
            {
                if (!Fx.IsFatal(exception))
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.WebHost, (System.ServiceModel.Diagnostics.EventLogEventId) (-1073610749), new string[] { System.ServiceModel.Activation.Diagnostics.TraceUtility.CreateSourceString(result), (exception == null) ? string.Empty : exception.ToString() });
                }
                throw;
            }
        }

        [SecurityCritical]
        public static void ExecuteSynchronous(HttpApplication context, bool flowContext, bool ensureWFService)
        {
            ExecuteSynchronous(context, null, flowContext, ensureWFService);
        }

        [SecurityCritical]
        public static void ExecuteSynchronous(HttpApplication context, string routeServiceVirtualPath, bool flowContext, bool ensureWFService)
        {
            HostedHttpRequestAsyncResult result;
            AutoResetEvent waitObject = HostedHttpRequestAsyncResult.waitObject;
            if (waitObject == null)
            {
                waitObject = new AutoResetEvent(false);
                HostedHttpRequestAsyncResult.waitObject = waitObject;
            }
            try
            {
                result = new HostedHttpRequestAsyncResult(context, routeServiceVirtualPath, flowContext, ensureWFService, ProcessRequestCompleteCallback, waitObject);
                if (!result.CompletedSynchronously)
                {
                    waitObject.WaitOne();
                }
                waitObject = null;
            }
            finally
            {
                if (waitObject != null)
                {
                    HostedHttpRequestAsyncResult.waitObject = null;
                    waitObject.Close();
                }
            }
            End(result);
        }

        [SecuritySafeCritical]
        private string GetAppRelativeCurrentExecutionFilePath()
        {
            return this.context.Request.AppRelativeCurrentExecutionFilePath;
        }

        [SecuritySafeCritical]
        internal ChannelBinding GetChannelBinding()
        {
            if (!this.IISSupportsExtendedProtection)
            {
                return null;
            }
            return this.context.Request.HttpChannelBinding;
        }

        [SecuritySafeCritical]
        internal int GetContentLength()
        {
            return this.context.Request.ContentLength;
        }

        [SecuritySafeCritical]
        internal string GetContentType()
        {
            return this.context.Request.Headers["Content-Type"];
        }

        [SecuritySafeCritical]
        internal string GetContentTypeFast()
        {
            return this.context.Request.ContentType;
        }

        [SecuritySafeCritical]
        internal string GetHttpMethod()
        {
            return this.context.Request.HttpMethod;
        }

        [SecuritySafeCritical]
        public Stream GetInputStream()
        {
            Stream inputStream;
            try
            {
                inputStream = this.context.Request.InputStream;
            }
            catch (HttpException exception)
            {
                throw System.ServiceModel.Activation.FxTrace.Exception.AsError(new CommunicationException(exception.Message, exception));
            }
            return inputStream;
        }

        [SecuritySafeCritical]
        internal Stream GetOutputStream()
        {
            return this.context.Response.OutputStream;
        }

        [SecuritySafeCritical]
        internal byte[] GetPrereadBuffer(ref int contentLength)
        {
            byte[] buffer = new byte[1];
            if (this.GetInputStream().Read(buffer, 0, 1) > 0)
            {
                contentLength = -1;
                return buffer;
            }
            return null;
        }

        [SecuritySafeCritical]
        internal string GetSoapAction()
        {
            return this.context.Request.Headers["SOAPAction"];
        }

        [SecuritySafeCritical]
        private Uri GetUrl()
        {
            return this.context.Request.Url;
        }

        private void HandleRequest()
        {
            string aspNetRouteServiceVirtualPath;
            this.OriginalRequestUri = this.GetUrl();
            if (!string.IsNullOrEmpty(this.AspNetRouteServiceVirtualPath))
            {
                aspNetRouteServiceVirtualPath = this.AspNetRouteServiceVirtualPath;
            }
            else if (!string.IsNullOrEmpty(this.configurationBasedServiceVirtualPath))
            {
                aspNetRouteServiceVirtualPath = this.configurationBasedServiceVirtualPath;
            }
            else
            {
                aspNetRouteServiceVirtualPath = this.GetAppRelativeCurrentExecutionFilePath();
            }
            if (this.ensureWFService)
            {
                bool flag = false;
                try
                {
                    if (!ServiceHostingEnvironment.EnsureWorkflowService(aspNetRouteServiceVirtualPath))
                    {
                        this.CompleteOperation(null);
                        flag = true;
                        return;
                    }
                }
                finally
                {
                    if (!flag)
                    {
                        this.CompleteRequest();
                    }
                }
            }
            if (ServiceHostingEnvironment.IsSimpleApplicationHost)
            {
                HostedTransportConfigurationManager.EnsureInitializedForSimpleApplicationHost(this);
            }
            HttpHostedTransportConfiguration configuration = HostedTransportConfigurationManager.GetConfiguration(this.OriginalRequestUri.Scheme) as HttpHostedTransportConfiguration;
            HostedHttpTransportManager httpTransportManager = null;
            if (configuration != null)
            {
                httpTransportManager = configuration.GetHttpTransportManager(this.OriginalRequestUri);
            }
            if (httpTransportManager == null)
            {
                InvalidOperationException innerException = new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_TransportBindingNotFound(this.OriginalRequestUri.ToString()));
                ServiceActivationException activationException = new ServiceActivationException(innerException.Message, innerException);
                this.LogServiceActivationException(activationException);
                throw System.ServiceModel.Activation.FxTrace.Exception.AsError(activationException);
            }
            this.RequestUri = new Uri(httpTransportManager.ListenUri, this.OriginalRequestUri.PathAndQuery);
            ServiceHostingEnvironment.EnsureServiceAvailableFast(aspNetRouteServiceVirtualPath);
            httpTransportManager.HttpContextReceived(this);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private bool IISSupportsExtendedProtectionInternal()
        {
            try
            {
                ChannelBinding httpChannelBinding = this.context.Request.HttpChannelBinding;
                return true;
            }
            catch (PlatformNotSupportedException)
            {
                return false;
            }
            catch (COMException)
            {
                return true;
            }
        }

        [SecuritySafeCritical]
        private void LogServiceActivationException(ServiceActivationException activationException)
        {
            if (System.ServiceModel.Diagnostics.Application.TD.ServiceExceptionIsEnabled())
            {
                System.ServiceModel.Diagnostics.Application.TD.ServiceException(activationException.ToString(), typeof(ServiceActivationException).FullName);
            }
            DiagnosticUtility.UnsafeEventLog.UnsafeLogEvent(TraceEventType.Error, EventLogCategory.WebHost, (System.ServiceModel.Diagnostics.EventLogEventId) (-1073610749), true, new string[] { System.ServiceModel.Activation.Diagnostics.TraceUtility.CreateSourceString(this), activationException.ToString() });
        }

        private static void OnBeginRequest(object state)
        {
            HostedHttpRequestAsyncResult result = (HostedHttpRequestAsyncResult) state;
            Exception exception = null;
            try
            {
                result.BeginRequest();
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                exception = exception2;
            }
            if (exception != null)
            {
                result.CompleteOperation(exception);
            }
        }

        [SecuritySafeCritical]
        private static void OnBeginRequestWithFlow(object state)
        {
            HostedHttpRequestAsyncResult result = (HostedHttpRequestAsyncResult) state;
            using (IDisposable disposable = null)
            {
                if (result.flowContext && (result.hostedThreadData != null))
                {
                    disposable = result.hostedThreadData.CreateContext();
                }
                AspNetPartialTrustHelpers.PartialTrustInvoke(ContextOnBeginRequest, result);
            }
        }

        public void OnReplySent()
        {
            this.CompleteOperation(null);
        }

        private static void ProcessRequestComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                try
                {
                    ((AutoResetEvent) result.AsyncState).Set();
                }
                catch (ObjectDisposedException exception)
                {
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                    }
                }
            }
        }

        [SecuritySafeCritical]
        private void ReleaseImpersonation()
        {
            if (this.impersonationContext != null)
            {
                this.impersonationContext.Release();
            }
        }

        [SecuritySafeCritical]
        internal void SetConnectionClose()
        {
            this.context.Response.AppendHeader("Connection", "close");
        }

        [SecuritySafeCritical]
        internal void SetContentType(string contentType)
        {
            this.context.Response.ContentType = contentType;
        }

        [SecuritySafeCritical]
        internal void SetStatusCode(int statusCode)
        {
            this.context.Response.TrySkipIisCustomErrors = true;
            this.context.Response.StatusCode = statusCode;
        }

        [SecuritySafeCritical]
        internal void SetStatusDescription(string statusDescription)
        {
            this.context.Response.StatusDescription = statusDescription;
        }

        [SecuritySafeCritical]
        internal void SetTransferModeToStreaming()
        {
            this.context.Response.BufferOutput = false;
        }

        TraceRecord HttpChannelListener.IHttpAuthenticationContext.CreateTraceRecord()
        {
            return new HttpRequestTraceRecord(this.Application.Request);
        }

        X509Certificate2 HttpChannelListener.IHttpAuthenticationContext.GetClientCertificate(out bool isValidCertificate)
        {
            HttpClientCertificate clientCertificate = this.Application.Request.ClientCertificate;
            isValidCertificate = clientCertificate.IsValid;
            if (clientCertificate.IsPresent)
            {
                return new X509Certificate2(clientCertificate.Certificate);
            }
            return null;
        }

        public static WindowsIdentity AnonymousIdentity
        {
            [SecuritySafeCritical]
            get
            {
                if (anonymousIdentity == null)
                {
                    anonymousIdentity = WindowsIdentity.GetAnonymous();
                }
                return anonymousIdentity;
            }
        }

        public HttpApplication Application
        {
            [SecuritySafeCritical]
            get
            {
                return this.context;
            }
        }

        public string AspNetRouteServiceVirtualPath { get; private set; }

        public static ContextCallback ContextOnBeginRequest
        {
            [SecuritySafeCritical]
            get
            {
                if (contextOnBeginRequest == null)
                {
                    contextOnBeginRequest = new ContextCallback(HostedHttpRequestAsyncResult.OnBeginRequest);
                }
                return contextOnBeginRequest;
            }
        }

        public System.ServiceModel.Activation.HostedThreadData HostedThreadData
        {
            [SecuritySafeCritical]
            get
            {
                return this.hostedThreadData;
            }
        }

        public bool IISSupportsExtendedProtection
        {
            get
            {
                if (!iisSupportsExtendedProtection.HasValue)
                {
                    iisSupportsExtendedProtection = new bool?(this.IISSupportsExtendedProtectionInternal());
                }
                return iisSupportsExtendedProtection.Value;
            }
        }

        public HostedImpersonationContext ImpersonationContext
        {
            [SecuritySafeCritical]
            get
            {
                return this.impersonationContext;
            }
        }

        public WindowsIdentity LogonUserIdentity
        {
            get
            {
                if (this.Application.User.Identity is WindowsIdentity)
                {
                    return (WindowsIdentity) this.Application.User.Identity;
                }
                return AnonymousIdentity;
            }
        }

        public Uri OriginalRequestUri { get; private set; }

        public static AsyncCallback ProcessRequestCompleteCallback
        {
            [SecuritySafeCritical]
            get
            {
                if (processRequestCompleteCallback == null)
                {
                    processRequestCompleteCallback = Fx.ThunkCallback(new AsyncCallback(HostedHttpRequestAsyncResult.ProcessRequestComplete));
                }
                return processRequestCompleteCallback;
            }
        }

        public Uri RequestUri { get; private set; }

        WindowsIdentity HttpChannelListener.IHttpAuthenticationContext.LogonUserIdentity
        {
            get
            {
                return this.LogonUserIdentity;
            }
        }

        public static Action<object> WaitOnBeginRequest
        {
            [SecuritySafeCritical]
            get
            {
                if (waitOnBeginRequest == null)
                {
                    waitOnBeginRequest = new Action<object>(HostedHttpRequestAsyncResult.OnBeginRequest);
                }
                return waitOnBeginRequest;
            }
        }

        public static Action<object> WaitOnBeginRequestWithFlow
        {
            [SecuritySafeCritical]
            get
            {
                if (waitOnBeginRequestWithFlow == null)
                {
                    waitOnBeginRequestWithFlow = new Action<object>(HostedHttpRequestAsyncResult.OnBeginRequestWithFlow);
                }
                return waitOnBeginRequestWithFlow;
            }
        }

        private static class State
        {
            internal const int Aborted = 2;
            internal const int Completed = 1;
            internal const int Running = 0;
        }
    }
}


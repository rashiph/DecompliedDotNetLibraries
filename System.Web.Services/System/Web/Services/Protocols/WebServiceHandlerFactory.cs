namespace System.Web.Services.Protocols
{
    using System;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Web.Services.Diagnostics;
    using System.Web.UI;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class WebServiceHandlerFactory : IHttpHandlerFactory
    {
        internal IHttpHandler CoreGetHandler(Type type, HttpContext context, HttpRequest request, HttpResponse response)
        {
            TraceMethod method = Tracing.On ? new TraceMethod(this, "CoreGetHandler", new object[0]) : null;
            ServerProtocolFactory[] serverProtocolFactories = this.GetServerProtocolFactories();
            ServerProtocol protocol = null;
            bool abortProcessing = false;
            for (int i = 0; i < serverProtocolFactories.Length; i++)
            {
                try
                {
                    protocol = serverProtocolFactories[i].Create(type, context, request, response, out abortProcessing);
                    if (((protocol != null) && (protocol.GetType() != typeof(UnsupportedRequestProtocol))) || abortProcessing)
                    {
                        break;
                    }
                }
                catch (Exception exception)
                {
                    if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                    {
                        throw;
                    }
                    throw Tracing.ExceptionThrow(method, new InvalidOperationException(Res.GetString("FailedToHandleRequest0"), exception));
                }
            }
            if (abortProcessing)
            {
                return new NopHandler();
            }
            if (protocol == null)
            {
                if ((request.PathInfo != null) && (request.PathInfo.Length != 0))
                {
                    throw Tracing.ExceptionThrow(method, new InvalidOperationException(Res.GetString("WebUnrecognizedRequestFormatUrl", new object[] { request.PathInfo })));
                }
                throw Tracing.ExceptionThrow(method, new InvalidOperationException(Res.GetString("WebUnrecognizedRequestFormat")));
            }
            if (protocol is UnsupportedRequestProtocol)
            {
                throw Tracing.ExceptionThrow(method, new HttpException(((UnsupportedRequestProtocol) protocol).HttpCode, Res.GetString("WebUnrecognizedRequestFormat")));
            }
            bool isAsync = protocol.MethodInfo.IsAsync;
            bool enableSession = protocol.MethodAttribute.EnableSession;
            if (isAsync)
            {
                if (enableSession)
                {
                    return new AsyncSessionHandler(protocol);
                }
                return new AsyncSessionlessHandler(protocol);
            }
            if (enableSession)
            {
                return new SyncSessionHandler(protocol);
            }
            return new SyncSessionlessHandler(protocol);
        }

        [SecurityPermission(SecurityAction.Assert, Unrestricted=true)]
        private Type GetCompiledType(string url, HttpContext context)
        {
            return WebServiceParser.GetCompiledType(url, context);
        }

        public IHttpHandler GetHandler(HttpContext context, string verb, string url, string filePath)
        {
            TraceMethod caller = Tracing.On ? new TraceMethod(this, "GetHandler", new object[0]) : null;
            if (Tracing.On)
            {
                Tracing.Enter("IHttpHandlerFactory.GetHandler", caller, Tracing.Details(context.Request));
            }
            new AspNetHostingPermission(AspNetHostingPermissionLevel.Minimal).Demand();
            Type compiledType = this.GetCompiledType(url, context);
            IHttpHandler handler = this.CoreGetHandler(compiledType, context, context.Request, context.Response);
            if (Tracing.On)
            {
                Tracing.Exit("IHttpHandlerFactory.GetHandler", caller);
            }
            return handler;
        }

        [PermissionSet(SecurityAction.Assert, Name="FullTrust")]
        private ServerProtocolFactory[] GetServerProtocolFactories()
        {
            return WebServicesSection.Current.ServerProtocolFactories;
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
        }
    }
}


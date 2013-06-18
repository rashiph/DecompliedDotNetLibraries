namespace System.Web.Services.Protocols
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Services;
    using System.Web.Services.Diagnostics;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class ServerProtocolFactory
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ServerProtocolFactory()
        {
        }

        internal ServerProtocol Create(Type type, HttpContext context, HttpRequest request, HttpResponse response, out bool abortProcessing)
        {
            ServerProtocol protocol = null;
            abortProcessing = false;
            protocol = this.CreateIfRequestCompatible(request);
            try
            {
                if (protocol != null)
                {
                    protocol.SetContext(type, context, request, response);
                }
                return protocol;
            }
            catch (Exception exception)
            {
                abortProcessing = true;
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Warning, this, "Create", exception);
                }
                if ((protocol != null) && !protocol.WriteException(exception, protocol.Response.OutputStream))
                {
                    throw new InvalidOperationException(Res.GetString("UnableToHandleRequest0"), exception);
                }
                return null;
            }
        }

        protected abstract ServerProtocol CreateIfRequestCompatible(HttpRequest request);
    }
}


namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;

    internal class RequestContextMessageProperty : IDisposable
    {
        private RequestContext context;
        private object thisLock = new object();

        public RequestContextMessageProperty(RequestContext context)
        {
            this.context = context;
        }

        void IDisposable.Dispose()
        {
            RequestContext context;
            bool flag = false;
            lock (this.thisLock)
            {
                if (this.context == null)
                {
                    return;
                }
                context = this.context;
                this.context = null;
            }
            try
            {
                context.Close();
                flag = true;
            }
            catch (CommunicationException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
            }
            catch (TimeoutException exception2)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
            }
            finally
            {
                if (!flag)
                {
                    context.Abort();
                }
            }
        }

        public static string Name
        {
            get
            {
                return "requestContext";
            }
        }
    }
}


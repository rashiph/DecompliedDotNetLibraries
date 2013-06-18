namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;

    internal abstract class FaultHelper
    {
        private object thisLock = new object();

        protected FaultHelper()
        {
        }

        public abstract void Abort();
        public static bool AddressReply(Message message, Message faultMessage)
        {
            try
            {
                RequestReplyCorrelator.PrepareReply(faultMessage, message);
            }
            catch (MessageHeaderException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
            }
            bool flag = true;
            try
            {
                flag = RequestReplyCorrelator.AddressReply(faultMessage, message);
            }
            catch (MessageHeaderException exception2)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
            }
            return flag;
        }

        public abstract IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state);
        public abstract void Close(TimeSpan timeout);
        public abstract void EndClose(IAsyncResult result);
        public abstract void SendFaultAsync(IReliableChannelBinder binder, RequestContext requestContext, Message faultMessage);

        protected object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }
    }
}


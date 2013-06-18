namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal abstract class RequestMessageDispatcher
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected RequestMessageDispatcher()
        {
        }

        protected void OnMessageException(Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result, Message message, CommunicationException exception, Fault fault)
        {
            DebugTrace.Trace(TraceLevel.Warning, "{0} - {1} reading request with action {2}: {3}", base.GetType().Name, exception.GetType().Name, message.Headers.Action, exception.Message);
            this.SendFaultReply(result, fault);
            DebugTrace.Trace(TraceLevel.Warning, "Replied with {0} fault", fault.Code.Name);
        }

        protected abstract void SendFaultReply(Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result, Fault fault);
    }
}


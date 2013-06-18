namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class CompletionParticipantProxy : DatagramProxy
    {
        public CompletionParticipantProxy(CoordinationService coordination, EndpointAddress to) : base(coordination, to, null)
        {
        }

        public IAsyncResult BeginSendAborted(AsyncCallback callback, object state)
        {
            Message message = new AbortedMessage(base.messageVersion, base.protocolVersion);
            return base.BeginSendMessage(message, callback, state);
        }

        public IAsyncResult BeginSendCommitted(AsyncCallback callback, object state)
        {
            Message message = new CommittedMessage(base.messageVersion, base.protocolVersion);
            return base.BeginSendMessage(message, callback, state);
        }
    }
}


namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class TwoPhaseCommitCoordinatorProxy : DatagramProxy
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TwoPhaseCommitCoordinatorProxy(CoordinationService coordination, EndpointAddress to, EndpointAddress from) : base(coordination, to, from)
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

        public IAsyncResult BeginSendPrepared(AsyncCallback callback, object state)
        {
            Message message = new PreparedMessage(base.messageVersion, base.protocolVersion);
            return base.BeginSendMessage(message, callback, state);
        }

        public IAsyncResult BeginSendReadOnly(AsyncCallback callback, object state)
        {
            Message message = new ReadOnlyMessage(base.messageVersion, base.protocolVersion);
            return base.BeginSendMessage(message, callback, state);
        }

        public IAsyncResult BeginSendRecoverMessage(AsyncCallback callback, object state)
        {
            Message message = NotificationMessage.CreateRecoverMessage(base.messageVersion, base.protocolVersion);
            return base.BeginSendMessage(message, callback, state);
        }
    }
}


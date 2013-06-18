namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class CompletionCoordinatorProxy : DatagramProxy
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CompletionCoordinatorProxy(CoordinationService coordination, EndpointAddress to, EndpointAddress from) : base(coordination, to, from)
        {
        }

        public IAsyncResult BeginSendCommit(AsyncCallback callback, object state)
        {
            Message message = new CommitMessage(base.messageVersion, base.protocolVersion);
            return base.BeginSendMessage(message, callback, state);
        }

        public IAsyncResult BeginSendRollback(AsyncCallback callback, object state)
        {
            Message message = new RollbackMessage(base.messageVersion, base.protocolVersion);
            return base.BeginSendMessage(message, callback, state);
        }
    }
}


namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel.Channels;

    internal interface ICompletionCoordinator
    {
        void Commit(Message message);
        void Fault(Message message, MessageFault messageFault);
        void Rollback(Message message);
    }
}


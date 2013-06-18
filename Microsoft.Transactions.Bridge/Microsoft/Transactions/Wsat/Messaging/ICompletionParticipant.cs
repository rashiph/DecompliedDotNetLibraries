namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel.Channels;

    internal interface ICompletionParticipant
    {
        void Aborted(Message message);
        void Committed(Message message);
        void Fault(Message message, MessageFault messageFault);
    }
}


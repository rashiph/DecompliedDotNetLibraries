namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel.Channels;

    internal interface ITwoPhaseCommitParticipant
    {
        void Commit(Message message);
        void Fault(Message message, MessageFault messageFault);
        void Prepare(Message message);
        void Rollback(Message message);
    }
}


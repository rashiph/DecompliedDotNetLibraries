namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel.Channels;

    internal interface ITwoPhaseCommitCoordinator
    {
        void Aborted(Message message);
        void Committed(Message message);
        void Fault(Message message, MessageFault messageFault);
        void Prepared(Message message);
        void ReadOnly(Message message);
        void Replay(Message message);
    }
}


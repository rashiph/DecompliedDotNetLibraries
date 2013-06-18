namespace Microsoft.Transactions.Bridge
{
    using System;

    internal interface IProtocolProviderPropagationService
    {
        void Begin(Enlistment enlistment, ProtocolProviderCallback callback, object state);
        void EnlistPrePrepare(Enlistment enlistment, ProtocolProviderCallback callback, object state);
        void MarshalTransaction(Enlistment enlistment, ProtocolProviderCallback callback, object state);
        void Pull(Enlistment enlistment, byte[] protocolInformation, ProtocolProviderCallback callback, object state);
        void Push(Enlistment enlistment, byte[] protocolInformation, ProtocolProviderCallback callback, object state);
        void RecoveryBeginning();
        void RecoveryComplete();
        void Rejoin(Enlistment enlistment, ProtocolProviderCallback callback, object state);
        void Replay(Enlistment enlistment, ProtocolProviderCallback callback, object state);
    }
}


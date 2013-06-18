namespace Microsoft.Transactions.Bridge
{
    using System;

    internal interface IProtocolProviderCoordinatorService
    {
        void Commit(Enlistment enlistment, ProtocolProviderCallback callback, object state);
        void Forget(Enlistment enlistment, ProtocolProviderCallback callback, object state);
        void Prepare(Enlistment enlistment, ProtocolProviderCallback callback, object state);
        void PrePrepare(Enlistment enlistment, ProtocolProviderCallback callback, object state);
        void Rollback(Enlistment enlistment, ProtocolProviderCallback callback, object state);
        void SinglePhaseCommit(Enlistment enlistment, ProtocolProviderCallback callback, object state);
    }
}


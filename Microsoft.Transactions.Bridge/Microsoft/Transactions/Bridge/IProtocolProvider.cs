namespace Microsoft.Transactions.Bridge
{
    using System;

    internal interface IProtocolProvider
    {
        byte[] GetProtocolInformation();
        void Initialize(TransactionManager transactionManager);
        void Start();
        void Stop();

        IProtocolProviderCoordinatorService CoordinatorService { get; }

        uint MarshalCapabilities { get; }

        IProtocolProviderPropagationService PropagationService { get; }

        Guid ProtocolId { get; }

        ProtocolProviderState State { get; }
    }
}


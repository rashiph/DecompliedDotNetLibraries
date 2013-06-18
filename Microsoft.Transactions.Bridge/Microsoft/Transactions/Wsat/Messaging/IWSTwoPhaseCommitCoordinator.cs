namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;

    internal interface IWSTwoPhaseCommitCoordinator
    {
        Type ContractType { get; }
    }
}


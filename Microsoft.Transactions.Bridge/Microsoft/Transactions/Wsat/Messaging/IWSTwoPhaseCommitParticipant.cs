namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;

    internal interface IWSTwoPhaseCommitParticipant
    {
        Type ContractType { get; }
    }
}


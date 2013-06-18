namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;

    internal interface IWSCompletionParticipant
    {
        Type ContractType { get; }
    }
}


namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;

    internal interface ITimerRecipient
    {
        void OnTimerNotification(object token);

        TimeSpan NextNotification { get; }

        Guid UniqueId { get; }
    }
}


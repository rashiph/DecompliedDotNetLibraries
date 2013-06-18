namespace Microsoft.Transactions.Wsat.StateMachines
{
    using System;

    internal enum TimerProfile
    {
        None,
        Preparing,
        Prepared,
        Replaying,
        Committing,
        VolatileOutcomeAssurance
    }
}


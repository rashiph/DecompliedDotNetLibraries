namespace System.Transactions.Diagnostics
{
    using System;

    internal enum EnlistmentType
    {
        Volatile,
        Durable,
        PromotableSinglePhase
    }
}


namespace System.Transactions.Oletx
{
    using System;

    internal enum OletxPrepareVoteType
    {
        ReadOnly,
        SinglePhase,
        Prepared,
        Failed,
        InDoubt
    }
}


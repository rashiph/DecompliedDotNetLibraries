namespace System.Transactions.Oletx
{
    using System;

    internal enum ShimNotificationType
    {
        None,
        Phase0RequestNotify,
        VoteRequestNotify,
        PrepareRequestNotify,
        CommitRequestNotify,
        AbortRequestNotify,
        CommittedNotify,
        AbortedNotify,
        InDoubtNotify,
        EnlistmentTmDownNotify,
        ResourceManagerTmDownNotify
    }
}


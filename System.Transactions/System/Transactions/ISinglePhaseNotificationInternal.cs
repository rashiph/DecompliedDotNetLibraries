namespace System.Transactions
{
    using System;

    internal interface ISinglePhaseNotificationInternal : IEnlistmentNotificationInternal
    {
        void SinglePhaseCommit(IPromotedEnlistment singlePhaseEnlistment);
    }
}


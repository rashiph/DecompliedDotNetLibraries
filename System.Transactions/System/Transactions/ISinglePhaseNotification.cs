namespace System.Transactions
{
    using System;

    public interface ISinglePhaseNotification : IEnlistmentNotification
    {
        void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment);
    }
}


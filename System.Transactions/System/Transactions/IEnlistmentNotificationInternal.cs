namespace System.Transactions
{
    using System;

    internal interface IEnlistmentNotificationInternal
    {
        void Commit(IPromotedEnlistment enlistment);
        void InDoubt(IPromotedEnlistment enlistment);
        void Prepare(IPromotedEnlistment preparingEnlistment);
        void Rollback(IPromotedEnlistment enlistment);
    }
}


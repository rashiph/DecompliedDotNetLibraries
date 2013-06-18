namespace System.Transactions
{
    using System;

    internal interface IPromotedEnlistment
    {
        void Aborted();
        void Aborted(Exception e);
        void Committed();
        void EnlistmentDone();
        void ForceRollback();
        void ForceRollback(Exception e);
        byte[] GetRecoveryInformation();
        void InDoubt();
        void InDoubt(Exception e);
        void Prepared();

        System.Transactions.InternalEnlistment InternalEnlistment { get; set; }
    }
}


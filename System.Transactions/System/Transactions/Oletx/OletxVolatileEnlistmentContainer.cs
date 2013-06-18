namespace System.Transactions.Oletx
{
    using System;
    using System.Collections;
    using System.Transactions;

    internal abstract class OletxVolatileEnlistmentContainer
    {
        protected bool alreadyVoted;
        protected bool collectedVoteYes;
        protected ArrayList enlistmentList;
        protected int incompleteDependentClones;
        protected int outstandingNotifications;
        protected int phase;
        protected RealOletxTransaction realOletxTransaction;

        protected OletxVolatileEnlistmentContainer()
        {
        }

        internal abstract void Aborted();
        internal abstract void AddDependentClone();
        internal abstract void Committed();
        internal abstract void DecrementOutstandingNotifications(bool voteYes);
        internal abstract void DependentCloneCompleted();
        internal abstract void InDoubt();
        internal abstract void OutcomeFromTransaction(TransactionStatus outcome);
        internal abstract void RollbackFromTransaction();

        internal Guid TransactionIdentifier
        {
            get
            {
                return this.realOletxTransaction.Identifier;
            }
        }
    }
}


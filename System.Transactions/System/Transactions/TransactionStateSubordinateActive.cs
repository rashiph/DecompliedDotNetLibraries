namespace System.Transactions
{
    using System;

    internal class TransactionStateSubordinateActive : TransactionStateActive
    {
        internal override void AddOutcomeRegistrant(InternalTransaction tx, TransactionCompletedEventHandler transactionCompletedDelegate)
        {
            tx.promoteState.EnterState(tx);
            tx.State.AddOutcomeRegistrant(tx, transactionCompletedDelegate);
        }

        internal override void CreateAbortingClone(InternalTransaction tx)
        {
            tx.promoteState.EnterState(tx);
            tx.State.CreateAbortingClone(tx);
        }

        internal override void CreateBlockingClone(InternalTransaction tx)
        {
            tx.promoteState.EnterState(tx);
            tx.State.CreateBlockingClone(tx);
        }

        internal override bool EnlistPromotableSinglePhase(InternalTransaction tx, IPromotableSinglePhaseNotification promotableSinglePhaseNotification, Transaction atomicTransaction)
        {
            return false;
        }

        internal override Enlistment EnlistVolatile(InternalTransaction tx, IEnlistmentNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            tx.promoteState.EnterState(tx);
            return tx.State.EnlistVolatile(tx, enlistmentNotification, enlistmentOptions, atomicTransaction);
        }

        internal override Enlistment EnlistVolatile(InternalTransaction tx, ISinglePhaseNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            tx.promoteState.EnterState(tx);
            return tx.State.EnlistVolatile(tx, enlistmentNotification, enlistmentOptions, atomicTransaction);
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.CommonEnterState(tx);
        }

        internal override TransactionStatus get_Status(InternalTransaction tx)
        {
            tx.promoteState.EnterState(tx);
            return tx.State.get_Status(tx);
        }

        internal override void Rollback(InternalTransaction tx, Exception e)
        {
            if (tx.innerException == null)
            {
                tx.innerException = e;
            }
            ((ISimpleTransactionSuperior) tx.promoter).Rollback();
            TransactionState._TransactionStateAborted.EnterState(tx);
        }
    }
}


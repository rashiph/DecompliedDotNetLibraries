namespace System.Transactions
{
    using System;
    using System.Threading;
    using System.Transactions.Diagnostics;

    internal class TransactionStateDelegatedAborting : TransactionStatePromotedAborted
    {
        internal override void BeginCommit(InternalTransaction tx, bool asyncCommit, AsyncCallback asyncCallback, object asyncState)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal override void ChangeStatePromotedAborted(InternalTransaction tx)
        {
            TransactionState._TransactionStatePromotedAborted.EnterState(tx);
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.CommonEnterState(tx);
            Monitor.Exit(tx);
            try
            {
                if (DiagnosticTrace.Verbose)
                {
                    EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), tx.durableEnlistment.EnlistmentTraceId, NotificationCall.Rollback);
                }
                tx.durableEnlistment.PromotableSinglePhaseNotification.Rollback(tx.durableEnlistment.SinglePhaseEnlistment);
            }
            finally
            {
                Monitor.Enter(tx);
            }
        }
    }
}


namespace System.Transactions
{
    using System;
    using System.Collections;
    using System.Transactions.Diagnostics;
    using System.Transactions.Oletx;

    internal abstract class TransactionStateDelegatedBase : TransactionStatePromoted
    {
        protected TransactionStateDelegatedBase()
        {
        }

        internal override void EnterState(InternalTransaction tx)
        {
            if (tx.outcomeSource.isoLevel == IsolationLevel.Snapshot)
            {
                throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("CannotPromoteSnapshot"), null);
            }
            base.CommonEnterState(tx);
            OletxTransaction transaction = null;
            try
            {
                if (DiagnosticTrace.Verbose && (tx.durableEnlistment != null))
                {
                    EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), tx.durableEnlistment.EnlistmentTraceId, NotificationCall.Promote);
                }
                transaction = TransactionState._TransactionStatePSPEOperation.PSPEPromote(tx);
            }
            catch (TransactionPromotionException exception)
            {
                tx.innerException = exception;
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), exception);
                }
            }
            finally
            {
                if (transaction == null)
                {
                    tx.State.ChangeStateAbortedDuringPromotion(tx);
                }
            }
            if (transaction != null)
            {
                tx.PromotedTransaction = transaction;
                Hashtable promotedTransactionTable = TransactionManager.PromotedTransactionTable;
                lock (promotedTransactionTable)
                {
                    tx.finalizedObject = new FinalizedObject(tx, tx.PromotedTransaction.Identifier);
                    WeakReference reference = new WeakReference(tx.outcomeSource, false);
                    promotedTransactionTable[tx.PromotedTransaction.Identifier] = reference;
                }
                TransactionManager.FireDistributedTransactionStarted(tx.outcomeSource);
                if (DiagnosticTrace.Information)
                {
                    TransactionPromotedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), tx.TransactionTraceId, transaction.TransactionTraceId);
                }
                this.PromoteEnlistmentsAndOutcome(tx);
            }
        }
    }
}


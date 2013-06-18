namespace System.Transactions
{
    using System;
    using System.Collections;
    using System.Transactions.Diagnostics;
    using System.Transactions.Oletx;

    internal class TransactionStatePromoted : TransactionStatePromotedBase
    {
        internal override void DisposeRoot(InternalTransaction tx)
        {
            tx.State.Rollback(tx, null);
        }

        internal override void EnterState(InternalTransaction tx)
        {
            if (tx.outcomeSource.isoLevel == IsolationLevel.Snapshot)
            {
                throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("CannotPromoteSnapshot"), null);
            }
            base.CommonEnterState(tx);
            OletxCommittableTransaction transaction = null;
            try
            {
                TimeSpan zero;
                if (tx.AbsoluteTimeout == 0x7fffffffffffffffL)
                {
                    zero = TimeSpan.Zero;
                }
                else
                {
                    zero = TransactionManager.TransactionTable.RecalcTimeout(tx);
                    if (zero <= TimeSpan.Zero)
                    {
                        return;
                    }
                }
                TransactionOptions properties = new TransactionOptions {
                    IsolationLevel = tx.outcomeSource.isoLevel,
                    Timeout = zero
                };
                transaction = TransactionManager.DistributedTransactionManager.CreateTransaction(properties);
                transaction.savedLtmPromotedTransaction = tx.outcomeSource;
                if (DiagnosticTrace.Information)
                {
                    TransactionPromotedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), tx.TransactionTraceId, transaction.TransactionTraceId);
                }
            }
            catch (TransactionException exception)
            {
                tx.innerException = exception;
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), exception);
                }
                return;
            }
            finally
            {
                if (transaction == null)
                {
                    tx.State.ChangeStateAbortedDuringPromotion(tx);
                }
            }
            tx.PromotedTransaction = transaction;
            Hashtable promotedTransactionTable = TransactionManager.PromotedTransactionTable;
            lock (promotedTransactionTable)
            {
                tx.finalizedObject = new FinalizedObject(tx, transaction.Identifier);
                WeakReference reference = new WeakReference(tx.outcomeSource, false);
                promotedTransactionTable[transaction.Identifier] = reference;
            }
            TransactionManager.FireDistributedTransactionStarted(tx.outcomeSource);
            this.PromoteEnlistmentsAndOutcome(tx);
        }

        internal virtual bool PromoteDurable(InternalTransaction tx)
        {
            if (tx.durableEnlistment != null)
            {
                InternalEnlistment durableEnlistment = tx.durableEnlistment;
                IPromotedEnlistment promotedEnlistment = tx.PromotedTransaction.EnlistDurable(durableEnlistment.ResourceManagerIdentifier, (DurableInternalEnlistment) durableEnlistment, durableEnlistment.SinglePhaseNotification != null, EnlistmentOptions.None);
                tx.durableEnlistment.State.ChangeStatePromoted(tx.durableEnlistment, promotedEnlistment);
            }
            return true;
        }

        internal virtual void PromoteEnlistmentsAndOutcome(InternalTransaction tx)
        {
            bool flag = false;
            tx.PromotedTransaction.RealTransaction.InternalTransaction = tx;
            try
            {
                flag = this.PromotePhaseVolatiles(tx, ref tx.phase0Volatiles, true);
            }
            catch (TransactionException exception3)
            {
                tx.innerException = exception3;
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), exception3);
                }
                return;
            }
            finally
            {
                if (!flag)
                {
                    tx.PromotedTransaction.Rollback();
                    tx.State.ChangeStateAbortedDuringPromotion(tx);
                }
            }
            flag = false;
            try
            {
                flag = this.PromotePhaseVolatiles(tx, ref tx.phase1Volatiles, false);
            }
            catch (TransactionException exception2)
            {
                tx.innerException = exception2;
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), exception2);
                }
                return;
            }
            finally
            {
                if (!flag)
                {
                    tx.PromotedTransaction.Rollback();
                    tx.State.ChangeStateAbortedDuringPromotion(tx);
                }
            }
            flag = false;
            try
            {
                flag = this.PromoteDurable(tx);
            }
            catch (TransactionException exception)
            {
                tx.innerException = exception;
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), exception);
                }
            }
            finally
            {
                if (!flag)
                {
                    tx.PromotedTransaction.Rollback();
                    tx.State.ChangeStateAbortedDuringPromotion(tx);
                }
            }
        }

        protected bool PromotePhaseVolatiles(InternalTransaction tx, ref VolatileEnlistmentSet volatiles, bool phase0)
        {
            if ((volatiles.volatileEnlistmentCount + volatiles.dependentClones) > 0)
            {
                if (phase0)
                {
                    volatiles.VolatileDemux = new Phase0VolatileDemultiplexer(tx);
                }
                else
                {
                    volatiles.VolatileDemux = new Phase1VolatileDemultiplexer(tx);
                }
                volatiles.VolatileDemux.oletxEnlistment = tx.PromotedTransaction.EnlistVolatile(volatiles.VolatileDemux, phase0 ? EnlistmentOptions.EnlistDuringPrepareRequired : EnlistmentOptions.None);
            }
            return true;
        }
    }
}


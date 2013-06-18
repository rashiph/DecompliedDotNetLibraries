namespace System.Transactions
{
    using System;
    using System.Transactions.Diagnostics;

    internal class TransactionStatePromotedP0Wave : TransactionStatePromotedBase
    {
        internal override void BeginCommit(InternalTransaction tx, bool asyncCommit, AsyncCallback asyncCallback, object asyncState)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal override void ChangeStateTransactionAborted(InternalTransaction tx, Exception e)
        {
            if (tx.innerException == null)
            {
                tx.innerException = e;
            }
            TransactionState._TransactionStatePromotedP0Aborting.EnterState(tx);
        }

        internal override bool ContinuePhase0Prepares()
        {
            return true;
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.CommonEnterState(tx);
        }

        internal override void Phase0VolatilePrepareDone(InternalTransaction tx)
        {
            try
            {
                TransactionState._TransactionStatePromotedCommitting.EnterState(tx);
            }
            catch (TransactionException exception)
            {
                if (tx.innerException == null)
                {
                    tx.innerException = exception;
                }
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), exception);
                }
            }
        }
    }
}


namespace System.Transactions
{
    using System;
    using System.Transactions.Oletx;

    internal class TransactionStatePSPEOperation : TransactionState
    {
        internal override void EnterState(InternalTransaction tx)
        {
            throw new InvalidOperationException();
        }

        internal override TransactionStatus get_Status(InternalTransaction tx)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal void Phase0PSPEInitialize(InternalTransaction tx, IPromotableSinglePhaseNotification promotableSinglePhaseNotification)
        {
            base.CommonEnterState(tx);
            try
            {
                promotableSinglePhaseNotification.Initialize();
            }
            finally
            {
                TransactionState._TransactionStatePhase0.CommonEnterState(tx);
            }
        }

        internal void PSPEInitialize(InternalTransaction tx, IPromotableSinglePhaseNotification promotableSinglePhaseNotification)
        {
            base.CommonEnterState(tx);
            try
            {
                promotableSinglePhaseNotification.Initialize();
            }
            finally
            {
                TransactionState._TransactionStateActive.CommonEnterState(tx);
            }
        }

        internal OletxTransaction PSPEPromote(InternalTransaction tx)
        {
            TransactionState state = tx.State;
            base.CommonEnterState(tx);
            OletxTransaction oletxTransactionFromTransmitterPropigationToken = null;
            try
            {
                byte[] propagationToken = tx.promoter.Promote();
                if (propagationToken == null)
                {
                    throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("PromotedReturnedInvalidValue"), null);
                }
                try
                {
                    oletxTransactionFromTransmitterPropigationToken = TransactionInterop.GetOletxTransactionFromTransmitterPropigationToken(propagationToken);
                }
                catch (ArgumentException exception)
                {
                    throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("PromotedReturnedInvalidValue"), exception);
                }
                if (TransactionManager.FindPromotedTransaction(oletxTransactionFromTransmitterPropigationToken.Identifier) != null)
                {
                    oletxTransactionFromTransmitterPropigationToken.Dispose();
                    throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("PromotedTransactionExists"), null);
                }
            }
            finally
            {
                state.CommonEnterState(tx);
            }
            return oletxTransactionFromTransmitterPropigationToken;
        }
    }
}


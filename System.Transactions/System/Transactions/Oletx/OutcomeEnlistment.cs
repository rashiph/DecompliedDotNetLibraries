namespace System.Transactions.Oletx
{
    using System;
    using System.Transactions;

    internal sealed class OutcomeEnlistment
    {
        private bool haveIssuedOutcome = false;
        private TransactionStatus savedStatus = TransactionStatus.InDoubt;
        private Guid txGuid;
        private WeakReference weakRealTransaction;

        internal OutcomeEnlistment()
        {
        }

        public void Aborted()
        {
            this.InvokeOutcomeFunction(TransactionStatus.Aborted);
        }

        public void Committed()
        {
            this.InvokeOutcomeFunction(TransactionStatus.Committed);
        }

        public void InDoubt()
        {
            this.InvokeOutcomeFunction(TransactionStatus.InDoubt);
        }

        private void InvokeOutcomeFunction(TransactionStatus status)
        {
            WeakReference weakRealTransaction = null;
            lock (this)
            {
                if (this.haveIssuedOutcome)
                {
                    return;
                }
                this.haveIssuedOutcome = true;
                this.savedStatus = status;
                weakRealTransaction = this.weakRealTransaction;
            }
            if (weakRealTransaction != null)
            {
                RealOletxTransaction target = weakRealTransaction.Target as RealOletxTransaction;
                if (target != null)
                {
                    target.FireOutcome(status);
                    if (target.phase0EnlistVolatilementContainerList != null)
                    {
                        foreach (OletxPhase0VolatileEnlistmentContainer container in target.phase0EnlistVolatilementContainerList)
                        {
                            container.OutcomeFromTransaction(status);
                        }
                    }
                    if (((TransactionStatus.Aborted == status) || (TransactionStatus.InDoubt == status)) && (target.phase1EnlistVolatilementContainer != null))
                    {
                        target.phase1EnlistVolatilementContainer.OutcomeFromTransaction(status);
                    }
                }
                weakRealTransaction.Target = null;
            }
        }

        internal void SetRealTransaction(RealOletxTransaction realTx)
        {
            bool haveIssuedOutcome = false;
            TransactionStatus inDoubt = TransactionStatus.InDoubt;
            lock (this)
            {
                haveIssuedOutcome = this.haveIssuedOutcome;
                inDoubt = this.savedStatus;
                if (!haveIssuedOutcome)
                {
                    this.weakRealTransaction = new WeakReference(realTx);
                    this.txGuid = realTx.TxGuid;
                }
            }
            if (haveIssuedOutcome)
            {
                realTx.FireOutcome(inDoubt);
                if (((TransactionStatus.Aborted == inDoubt) || (TransactionStatus.InDoubt == inDoubt)) && (realTx.phase1EnlistVolatilementContainer != null))
                {
                    realTx.phase1EnlistVolatilementContainer.OutcomeFromTransaction(inDoubt);
                }
            }
        }

        internal void TMDown()
        {
            bool flag = true;
            RealOletxTransaction realTx = null;
            lock (this)
            {
                if (this.weakRealTransaction != null)
                {
                    realTx = this.weakRealTransaction.Target as RealOletxTransaction;
                }
            }
            if (realTx != null)
            {
                lock (realTx)
                {
                    flag = this.TransactionIsInDoubt(realTx);
                }
            }
            if (flag)
            {
                this.InDoubt();
            }
            else
            {
                this.Aborted();
            }
        }

        internal bool TransactionIsInDoubt(RealOletxTransaction realTx)
        {
            if ((realTx.committableTransaction != null) && !realTx.committableTransaction.CommitCalled)
            {
                return false;
            }
            return (realTx.UndecidedEnlistments == 0);
        }

        internal void UnregisterOutcomeCallback()
        {
            this.weakRealTransaction = null;
        }

        internal Guid TransactionIdentifier
        {
            get
            {
                return this.txGuid;
            }
        }
    }
}


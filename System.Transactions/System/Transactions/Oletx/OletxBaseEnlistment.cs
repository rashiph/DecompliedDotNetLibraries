namespace System.Transactions.Oletx
{
    using System;
    using System.Threading;
    using System.Transactions;

    internal abstract class OletxBaseEnlistment
    {
        protected Guid enlistmentGuid = Guid.NewGuid();
        protected int enlistmentId;
        protected InternalEnlistment internalEnlistment;
        protected OletxResourceManager oletxResourceManager;
        protected OletxTransaction oletxTransaction;
        internal EnlistmentTraceIdentifier traceIdentifier;
        protected string transactionGuidString;

        public OletxBaseEnlistment(OletxResourceManager oletxResourceManager, OletxTransaction oletxTransaction)
        {
            this.oletxResourceManager = oletxResourceManager;
            this.oletxTransaction = oletxTransaction;
            if (oletxTransaction != null)
            {
                this.enlistmentId = oletxTransaction.realOletxTransaction.enlistmentCount++;
                this.transactionGuidString = oletxTransaction.realOletxTransaction.TxGuid.ToString();
            }
            else
            {
                this.transactionGuidString = Guid.Empty.ToString();
            }
            this.traceIdentifier = EnlistmentTraceIdentifier.Empty;
        }

        protected void AddToEnlistmentTable()
        {
            lock (this.oletxResourceManager.enlistmentHashtable.SyncRoot)
            {
                this.oletxResourceManager.enlistmentHashtable.Add(this.enlistmentGuid, this);
            }
        }

        protected void RemoveFromEnlistmentTable()
        {
            lock (this.oletxResourceManager.enlistmentHashtable.SyncRoot)
            {
                this.oletxResourceManager.enlistmentHashtable.Remove(this.enlistmentGuid);
            }
        }

        protected EnlistmentTraceIdentifier InternalTraceIdentifier
        {
            get
            {
                if (EnlistmentTraceIdentifier.Empty == this.traceIdentifier)
                {
                    lock (this)
                    {
                        if (EnlistmentTraceIdentifier.Empty == this.traceIdentifier)
                        {
                            EnlistmentTraceIdentifier identifier2;
                            Guid empty = Guid.Empty;
                            if (this.oletxResourceManager != null)
                            {
                                empty = this.oletxResourceManager.resourceManagerIdentifier;
                            }
                            if (this.oletxTransaction != null)
                            {
                                identifier2 = new EnlistmentTraceIdentifier(empty, this.oletxTransaction.TransactionTraceId, this.enlistmentId);
                            }
                            else
                            {
                                TransactionTraceIdentifier transactionTraceId = new TransactionTraceIdentifier(this.transactionGuidString, 0);
                                identifier2 = new EnlistmentTraceIdentifier(empty, transactionTraceId, this.enlistmentId);
                            }
                            Thread.MemoryBarrier();
                            this.traceIdentifier = identifier2;
                        }
                    }
                }
                return this.traceIdentifier;
            }
        }
    }
}


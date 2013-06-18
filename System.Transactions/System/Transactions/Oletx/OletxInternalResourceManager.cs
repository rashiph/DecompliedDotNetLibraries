namespace System.Transactions.Oletx
{
    using System;
    using System.Collections;
    using System.Transactions;
    using System.Transactions.Diagnostics;

    internal class OletxInternalResourceManager
    {
        private Guid myGuid;
        private OletxTransactionManager oletxTm;
        internal IResourceManagerShim resourceManagerShim;

        internal OletxInternalResourceManager(OletxTransactionManager oletxTm)
        {
            this.oletxTm = oletxTm;
            this.myGuid = Guid.NewGuid();
        }

        internal void CallReenlistComplete()
        {
            this.resourceManagerShim.ReenlistComplete();
        }

        public void TMDown()
        {
            this.resourceManagerShim = null;
            Transaction target = null;
            RealOletxTransaction realOletxTransaction = null;
            IDictionaryEnumerator enumerator = null;
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxInternalResourceManager.TMDown");
            }
            Hashtable hashtable2 = null;
            lock (TransactionManager.PromotedTransactionTable.SyncRoot)
            {
                hashtable2 = (Hashtable) TransactionManager.PromotedTransactionTable.Clone();
            }
            enumerator = hashtable2.GetEnumerator();
            while (enumerator.MoveNext())
            {
                WeakReference reference = (WeakReference) enumerator.Value;
                if (reference != null)
                {
                    target = (Transaction) reference.Target;
                    if (null != target)
                    {
                        realOletxTransaction = target.internalTransaction.PromotedTransaction.realOletxTransaction;
                        if (realOletxTransaction.OletxTransactionManagerInstance == this.oletxTm)
                        {
                            realOletxTransaction.TMDown();
                        }
                    }
                }
            }
            Hashtable hashtable = null;
            if (OletxTransactionManager.resourceManagerHashTable != null)
            {
                OletxTransactionManager.resourceManagerHashTableLock.AcquireReaderLock(-1);
                try
                {
                    hashtable = (Hashtable) OletxTransactionManager.resourceManagerHashTable.Clone();
                }
                finally
                {
                    OletxTransactionManager.resourceManagerHashTableLock.ReleaseReaderLock();
                }
            }
            if (hashtable != null)
            {
                enumerator = hashtable.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    OletxResourceManager manager = (OletxResourceManager) enumerator.Value;
                    if (manager != null)
                    {
                        manager.TMDownFromInternalRM(this.oletxTm);
                    }
                }
            }
            this.oletxTm.dtcTransactionManagerLock.AcquireWriterLock(-1);
            try
            {
                this.oletxTm.ReinitializeProxy();
            }
            finally
            {
                this.oletxTm.dtcTransactionManagerLock.ReleaseWriterLock();
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxInternalResourceManager.TMDown");
            }
        }

        internal Guid Identifier
        {
            get
            {
                return this.myGuid;
            }
        }
    }
}


namespace System.Transactions
{
    using System;
    using System.Collections;

    internal sealed class FinalizedObject : IDisposable
    {
        private Guid identifier;
        private InternalTransaction internalTransaction;

        internal FinalizedObject(InternalTransaction internalTransaction, Guid identifier)
        {
            this.internalTransaction = internalTransaction;
            this.identifier = identifier;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
            Hashtable promotedTransactionTable = TransactionManager.PromotedTransactionTable;
            lock (promotedTransactionTable)
            {
                WeakReference reference = (WeakReference) promotedTransactionTable[this.identifier];
                if ((reference != null) && (reference.Target != null))
                {
                    reference.Target = null;
                }
                promotedTransactionTable.Remove(this.identifier);
            }
        }

        ~FinalizedObject()
        {
            this.Dispose(false);
        }
    }
}


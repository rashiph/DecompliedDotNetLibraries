namespace System.ServiceModel.Transactions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Threading;
    using System.Transactions;

    internal abstract class TransactionCache<T, S>
    {
        private static Dictionary<T, S> cache;
        private static ReaderWriterLock cacheLock;
        private T key;

        static TransactionCache()
        {
            TransactionCache<T, S>.cache = new Dictionary<T, S>();
            TransactionCache<T, S>.cacheLock = new ReaderWriterLock();
        }

        protected TransactionCache()
        {
        }

        private static bool Add(T key, S value)
        {
            bool flag = false;
            try
            {
                try
                {
                }
                finally
                {
                    TransactionCache<T, S>.cacheLock.AcquireWriterLock(-1);
                    flag = true;
                }
                if (!TransactionCache<T, S>.cache.ContainsKey(key))
                {
                    TransactionCache<T, S>.cache.Add(key, value);
                    return true;
                }
            }
            finally
            {
                if (flag)
                {
                    TransactionCache<T, S>.cacheLock.ReleaseWriterLock();
                }
            }
            return false;
        }

        protected void AddEntry(Transaction transaction, T key, S value)
        {
            this.key = key;
            if (TransactionCache<T, S>.Add(key, value))
            {
                transaction.TransactionCompleted += new TransactionCompletedEventHandler(this.OnTransactionCompleted);
            }
        }

        public static bool Find(T key, out S value)
        {
            bool flag = false;
            try
            {
                try
                {
                }
                finally
                {
                    TransactionCache<T, S>.cacheLock.AcquireReaderLock(-1);
                    flag = true;
                }
                if (TransactionCache<T, S>.cache.TryGetValue(key, out value))
                {
                    return true;
                }
            }
            finally
            {
                if (flag)
                {
                    TransactionCache<T, S>.cacheLock.ReleaseReaderLock();
                }
            }
            return false;
        }

        private void OnTransactionCompleted(object sender, TransactionEventArgs e)
        {
            TransactionCache<T, S>.Remove(this.key);
        }

        private static void Remove(T key)
        {
            bool flag = false;
            try
            {
                try
                {
                }
                finally
                {
                    TransactionCache<T, S>.cacheLock.AcquireWriterLock(-1);
                    flag = true;
                }
                if (!TransactionCache<T, S>.cache.Remove(key))
                {
                    DiagnosticUtility.FailFast("TransactionCache: key must be present in transaction cache");
                }
            }
            finally
            {
                if (flag)
                {
                    TransactionCache<T, S>.cacheLock.ReleaseWriterLock();
                }
            }
        }
    }
}


namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class LookupTables
    {
        private ReaderWriterLock enlistmentLock = new ReaderWriterLock();
        private Dictionary<Guid, TransactionEnlistment> enlistments = new Dictionary<Guid, TransactionEnlistment>();
        private ProtocolState state;
        private ReaderWriterLock transactionLock = new ReaderWriterLock();
        private Dictionary<string, TransactionContextManager> transactions = new Dictionary<string, TransactionContextManager>();

        public LookupTables(ProtocolState state)
        {
            this.state = state;
        }

        private void Add<T, S>(Dictionary<T, S> dictionary, ReaderWriterLock rwLock, T key, S value)
        {
            bool flag = false;
            try
            {
                try
                {
                }
                finally
                {
                    rwLock.AcquireWriterLock(-1);
                    flag = true;
                }
                dictionary.Add(key, value);
            }
            finally
            {
                if (flag)
                {
                    rwLock.ReleaseWriterLock();
                }
            }
            if (DebugTrace.Verbose)
            {
                int count = dictionary.Count;
                DebugTrace.Trace(TraceLevel.Verbose, "Added {0} {1} to lookup table. Table contains {2} object{3}", value.GetType().Name, value, count, (count == 1) ? string.Empty : "s");
            }
        }

        public void AddEnlistment(TransactionEnlistment enlistment)
        {
            this.Add<Guid, TransactionEnlistment>(this.enlistments, this.enlistmentLock, enlistment.EnlistmentId, enlistment);
        }

        public void AddTransactionContextManager(TransactionContextManager contextManager)
        {
            this.Add<string, TransactionContextManager>(this.transactions, this.transactionLock, contextManager.Identifier, contextManager);
        }

        private S Find<T, S>(Dictionary<T, S> dictionary, ReaderWriterLock rwLock, T key) where S: class
        {
            S local;
            bool flag = false;
            try
            {
                try
                {
                }
                finally
                {
                    rwLock.AcquireReaderLock(-1);
                    flag = true;
                }
                if (!dictionary.TryGetValue(key, out local))
                {
                    local = default(S);
                }
            }
            finally
            {
                if (flag)
                {
                    rwLock.ReleaseReaderLock();
                }
            }
            return local;
        }

        public TransactionEnlistment FindEnlistment(Guid enlistmentId)
        {
            return this.Find<Guid, TransactionEnlistment>(this.enlistments, this.enlistmentLock, enlistmentId);
        }

        private S FindOrAdd<T, S>(Dictionary<T, S> dictionary, ReaderWriterLock rwLock, T key, S value, out bool found)
        {
            S local;
            bool flag = false;
            try
            {
                try
                {
                }
                finally
                {
                    rwLock.AcquireWriterLock(-1);
                    flag = true;
                }
                found = dictionary.TryGetValue(key, out local);
                if (!found)
                {
                    dictionary.Add(key, value);
                    local = value;
                }
            }
            finally
            {
                if (flag)
                {
                    rwLock.ReleaseWriterLock();
                }
            }
            if (DebugTrace.Verbose && !found)
            {
                int count = dictionary.Count;
                DebugTrace.Trace(TraceLevel.Verbose, "Added {0} {1} to lookup table. Table contains {2} object{3}", value.GetType().Name, value, count, (count == 1) ? string.Empty : "s");
            }
            return local;
        }

        public TransactionContextManager FindOrAddTransactionContextManager(TransactionContextManager contextManager, out bool found)
        {
            return this.FindOrAdd<string, TransactionContextManager>(this.transactions, this.transactionLock, contextManager.Identifier, contextManager, out found);
        }

        public TransactionContextManager FindTransactionContextManager(string contextId)
        {
            return this.Find<string, TransactionContextManager>(this.transactions, this.transactionLock, contextId);
        }

        private void Remove<T, S>(Dictionary<T, S> dictionary, ReaderWriterLock rwLock, T key, S value)
        {
            bool flag = false;
            try
            {
                try
                {
                }
                finally
                {
                    rwLock.AcquireWriterLock(-1);
                    flag = true;
                }
                if (!dictionary.Remove(key))
                {
                    DiagnosticUtility.FailFast("The lookup table does not contain the object");
                }
            }
            finally
            {
                if (flag)
                {
                    rwLock.ReleaseWriterLock();
                }
            }
            if (DebugTrace.Verbose)
            {
                int count = dictionary.Count;
                DebugTrace.Trace(TraceLevel.Verbose, "Removed {0} {1} from lookup table. Table contains {2} object{3}", value.GetType().Name, value, count, (count == 1) ? string.Empty : "s");
            }
        }

        public void RemoveEnlistment(TransactionEnlistment enlistment)
        {
            this.Remove<Guid, TransactionEnlistment>(this.enlistments, this.enlistmentLock, enlistment.EnlistmentId, enlistment);
        }

        public void RemoveTransactionContextManager(TransactionContextManager contextManager)
        {
            this.Remove<string, TransactionContextManager>(this.transactions, this.transactionLock, contextManager.Identifier, contextManager);
        }
    }
}


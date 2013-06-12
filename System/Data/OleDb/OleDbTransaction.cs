namespace System.Data.OleDb
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public sealed class OleDbTransaction : DbTransaction
    {
        private readonly System.Data.IsolationLevel _isolationLevel;
        private WeakReference _nestedTransaction;
        internal readonly int _objectID = Interlocked.Increment(ref _objectTypeCount);
        private static int _objectTypeCount;
        internal OleDbConnection _parentConnection;
        private readonly OleDbTransaction _parentTransaction;
        private WrappedTransaction _transaction;

        internal OleDbTransaction(OleDbConnection connection, OleDbTransaction transaction, System.Data.IsolationLevel isolevel)
        {
            this._parentConnection = connection;
            this._parentTransaction = transaction;
            switch (isolevel)
            {
                case System.Data.IsolationLevel.Unspecified:
                    isolevel = System.Data.IsolationLevel.ReadCommitted;
                    break;

                case System.Data.IsolationLevel.Chaos:
                case System.Data.IsolationLevel.ReadUncommitted:
                case System.Data.IsolationLevel.Serializable:
                case System.Data.IsolationLevel.Snapshot:
                case System.Data.IsolationLevel.ReadCommitted:
                case System.Data.IsolationLevel.RepeatableRead:
                    break;

                default:
                    throw ADP.InvalidIsolationLevel(isolevel);
            }
            this._isolationLevel = isolevel;
        }

        public OleDbTransaction Begin()
        {
            return this.Begin(System.Data.IsolationLevel.ReadCommitted);
        }

        public OleDbTransaction Begin(System.Data.IsolationLevel isolevel)
        {
            OleDbTransaction transaction2;
            IntPtr ptr;
            OleDbConnection.ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<oledb.OleDbTransaction.Begin|API> %d#, isolevel=%d{IsolationLevel}", this.ObjectID, (int) isolevel);
            try
            {
                if (this._transaction == null)
                {
                    throw ADP.TransactionZombied(this);
                }
                if ((this._nestedTransaction != null) && this._nestedTransaction.IsAlive)
                {
                    throw ADP.ParallelTransactionsNotSupported(this.Connection);
                }
                OleDbTransaction target = new OleDbTransaction(this._parentConnection, this, isolevel);
                this._nestedTransaction = new WeakReference(target, false);
                UnsafeNativeMethods.ITransactionLocal transaction = null;
                try
                {
                    transaction = (UnsafeNativeMethods.ITransactionLocal) this._transaction.ComWrapper();
                    target.BeginInternal(transaction);
                }
                finally
                {
                    if (transaction != null)
                    {
                        Marshal.ReleaseComObject(transaction);
                    }
                }
                transaction2 = target;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return transaction2;
        }

        internal void BeginInternal(UnsafeNativeMethods.ITransactionLocal transaction)
        {
            OleDbHResult result;
            this._transaction = new WrappedTransaction(transaction, (int) this._isolationLevel, out result);
            if (result < OleDbHResult.S_OK)
            {
                this._transaction.Dispose();
                this._transaction = null;
                this.ProcessResults(result);
            }
        }

        public override void Commit()
        {
            IntPtr ptr;
            OleDbConnection.ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<oledb.OleDbTransaction.Commit|API> %d#", this.ObjectID);
            try
            {
                if (this._transaction == null)
                {
                    throw ADP.TransactionZombied(this);
                }
                this.CommitInternal();
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void CommitInternal()
        {
            if (this._transaction != null)
            {
                if (this._nestedTransaction != null)
                {
                    OleDbTransaction target = (OleDbTransaction) this._nestedTransaction.Target;
                    if ((target != null) && this._nestedTransaction.IsAlive)
                    {
                        target.CommitInternal();
                    }
                    this._nestedTransaction = null;
                }
                OleDbHResult hr = this._transaction.Commit();
                if (!this._transaction.MustComplete)
                {
                    this._transaction.Dispose();
                    this._transaction = null;
                    this.DisposeManaged();
                }
                if (hr < OleDbHResult.S_OK)
                {
                    this.ProcessResults(hr);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DisposeManaged();
                this.RollbackInternal(false);
            }
            base.Dispose(disposing);
        }

        private void DisposeManaged()
        {
            if (this._parentTransaction != null)
            {
                this._parentTransaction._nestedTransaction = null;
            }
            else if (this._parentConnection != null)
            {
                this._parentConnection.LocalTransaction = null;
            }
            this._parentConnection = null;
        }

        private void ProcessResults(OleDbHResult hr)
        {
            Exception exception = OleDbConnection.ProcessResults(hr, this._parentConnection, this);
            if (exception != null)
            {
                throw exception;
            }
        }

        public override void Rollback()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<oledb.OleDbTransaction.Rollback|API> %d#", this.ObjectID);
            try
            {
                if (this._transaction == null)
                {
                    throw ADP.TransactionZombied(this);
                }
                this.DisposeManaged();
                this.RollbackInternal(true);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal OleDbHResult RollbackInternal(bool exceptionHandling)
        {
            OleDbHResult hr = OleDbHResult.S_OK;
            if (this._transaction != null)
            {
                if (this._nestedTransaction != null)
                {
                    OleDbTransaction target = (OleDbTransaction) this._nestedTransaction.Target;
                    if ((target != null) && this._nestedTransaction.IsAlive)
                    {
                        hr = target.RollbackInternal(exceptionHandling);
                        if (exceptionHandling && (hr < OleDbHResult.S_OK))
                        {
                            SafeNativeMethods.Wrapper.ClearErrorInfo();
                            return hr;
                        }
                    }
                    this._nestedTransaction = null;
                }
                hr = this._transaction.Abort();
                this._transaction.Dispose();
                this._transaction = null;
                if (hr >= OleDbHResult.S_OK)
                {
                    return hr;
                }
                if (exceptionHandling)
                {
                    this.ProcessResults(hr);
                    return hr;
                }
                SafeNativeMethods.Wrapper.ClearErrorInfo();
            }
            return hr;
        }

        internal static OleDbTransaction TransactionLast(OleDbTransaction head)
        {
            if (head._nestedTransaction != null)
            {
                OleDbTransaction target = (OleDbTransaction) head._nestedTransaction.Target;
                if ((target != null) && head._nestedTransaction.IsAlive)
                {
                    return TransactionLast(target);
                }
            }
            return head;
        }

        internal static OleDbTransaction TransactionUpdate(OleDbTransaction transaction)
        {
            if ((transaction != null) && (transaction._transaction == null))
            {
                return null;
            }
            return transaction;
        }

        public OleDbConnection Connection
        {
            get
            {
                return this._parentConnection;
            }
        }

        protected override System.Data.Common.DbConnection DbConnection
        {
            get
            {
                return this.Connection;
            }
        }

        public override System.Data.IsolationLevel IsolationLevel
        {
            get
            {
                if (this._transaction == null)
                {
                    throw ADP.TransactionZombied(this);
                }
                return this._isolationLevel;
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        internal OleDbTransaction Parent
        {
            get
            {
                return this._parentTransaction;
            }
        }

        private sealed class WrappedTransaction : WrappedIUnknown
        {
            private bool _mustComplete;

            internal WrappedTransaction(UnsafeNativeMethods.ITransactionLocal transaction, int isolevel, out OleDbHResult hr) : base(transaction)
            {
                int pulTransactionLevel = 0;
                Bid.Trace("<oledb.ITransactionLocal.StartTransaction|API|OLEDB>\n");
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    hr = transaction.StartTransaction(isolevel, 0, IntPtr.Zero, out pulTransactionLevel);
                    if (OleDbHResult.S_OK <= hr)
                    {
                        this._mustComplete = true;
                    }
                }
                Bid.Trace("<oledb.ITransactionLocal.StartTransaction|API|OLEDB|RET> %08X{HRESULT}\n", hr);
            }

            internal OleDbHResult Abort()
            {
                OleDbHResult result;
                bool success = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    base.DangerousAddRef(ref success);
                    Bid.Trace("<oledb.ITransactionLocal.Abort|API|OLEDB> handle=%p\n", base.handle);
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                    }
                    finally
                    {
                        result = NativeOledbWrapper.ITransactionAbort(base.DangerousGetHandle());
                        this._mustComplete = false;
                    }
                    Bid.Trace("<oledb.ITransactionLocal.Abort|API|OLEDB|RET> %08X{HRESULT}\n", result);
                }
                finally
                {
                    if (success)
                    {
                        base.DangerousRelease();
                    }
                }
                return result;
            }

            internal OleDbHResult Commit()
            {
                OleDbHResult result;
                bool success = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    base.DangerousAddRef(ref success);
                    Bid.Trace("<oledb.ITransactionLocal.Commit|API|OLEDB> handle=%p\n", base.handle);
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                    }
                    finally
                    {
                        result = NativeOledbWrapper.ITransactionCommit(base.DangerousGetHandle());
                        if ((OleDbHResult.S_OK <= result) || (OleDbHResult.XACT_E_NOTRANSACTION == result))
                        {
                            this._mustComplete = false;
                        }
                    }
                    Bid.Trace("<oledb.ITransactionLocal.Commit|API|OLEDB|RET> %08X{HRESULT}\n", result);
                }
                finally
                {
                    if (success)
                    {
                        base.DangerousRelease();
                    }
                }
                return result;
            }

            protected override bool ReleaseHandle()
            {
                if (this._mustComplete && (IntPtr.Zero != base.handle))
                {
                    Bid.Trace("<oledb.ITransactionLocal.Abort|API|OLEDB|INFO> handle=%p\n", base.handle);
                    OleDbHResult result = NativeOledbWrapper.ITransactionAbort(base.handle);
                    this._mustComplete = false;
                    Bid.Trace("<oledb.ITransactionLocal.Abort|API|OLEDB|INFO|RET> %08X{HRESULT}\n", result);
                }
                return base.ReleaseHandle();
            }

            internal bool MustComplete
            {
                get
                {
                    return this._mustComplete;
                }
            }
        }
    }
}


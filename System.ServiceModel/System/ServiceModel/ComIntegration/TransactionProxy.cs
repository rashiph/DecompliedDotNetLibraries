namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Transactions;

    internal class TransactionProxy : ITransactionProxy, IExtension<InstanceContext>
    {
        private Guid appid;
        private Guid clsid;
        private Transaction currentTransaction;
        private VoterBallot currentVoter;
        private int instanceID;
        private object syncRoot = new object();

        public TransactionProxy(Guid appid, Guid clsid)
        {
            this.appid = appid;
            this.clsid = clsid;
        }

        public void Abort()
        {
            if (this.currentTransaction != null)
            {
                this.currentTransaction.Rollback();
            }
        }

        public void Attach(InstanceContext owner)
        {
        }

        private void ClearTransaction(ProxyEnlistment enlistment)
        {
            lock (this.syncRoot)
            {
                if (this.currentTransaction == null)
                {
                    DiagnosticUtility.FailFast("Clearing inactive TransactionProxy");
                }
                if (enlistment.Transaction != this.currentTransaction)
                {
                    DiagnosticUtility.FailFast("Incorrectly working on multiple transactions");
                }
                this.currentTransaction = null;
                this.currentVoter = null;
            }
        }

        public void Commit(Guid guid)
        {
            DiagnosticUtility.FailFast("Commit not supported: BYOT only!");
        }

        public void CreateVoter(ITransactionVoterNotifyAsync2 voterNotification, IntPtr voterBallot)
        {
            if (IntPtr.Zero == voterBallot)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("voterBallot");
            }
            lock (this.syncRoot)
            {
                if (this.currentVoter != null)
                {
                    DiagnosticUtility.FailFast("Assumption: proxy only needs one voter");
                }
                VoterBallot ballot = new VoterBallot(voterNotification, this);
                if (this.currentTransaction != null)
                {
                    ballot.SetTransaction(this.currentTransaction);
                }
                this.currentVoter = ballot;
                IntPtr interfacePtrForObject = InterfaceHelper.GetInterfacePtrForObject(typeof(ITransactionVoterBallotAsync2).GUID, this.currentVoter);
                Marshal.WriteIntPtr(voterBallot, interfacePtrForObject);
            }
        }

        public void Detach(InstanceContext owner)
        {
        }

        private void EnsureTransaction()
        {
            lock (this.syncRoot)
            {
                if (this.currentTransaction == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(null, HR.CONTEXT_E_NOTRANSACTION));
                }
            }
        }

        public Guid GetIdentifier()
        {
            return this.currentTransaction.TransactionInformation.DistributedIdentifier;
        }

        public DtcIsolationLevel GetIsolationLevel()
        {
            switch (this.currentTransaction.IsolationLevel)
            {
                case IsolationLevel.Serializable:
                    return DtcIsolationLevel.ISOLATIONLEVEL_SERIALIZABLE;

                case IsolationLevel.RepeatableRead:
                    return DtcIsolationLevel.ISOLATIONLEVEL_REPEATABLEREAD;

                case IsolationLevel.ReadCommitted:
                    return DtcIsolationLevel.ISOLATIONLEVEL_CURSORSTABILITY;

                case IsolationLevel.ReadUncommitted:
                    return DtcIsolationLevel.ISOLATIONLEVEL_READUNCOMMITTED;
            }
            return DtcIsolationLevel.ISOLATIONLEVEL_SERIALIZABLE;
        }

        public bool IsReusable()
        {
            return true;
        }

        public IDtcTransaction Promote()
        {
            this.EnsureTransaction();
            return TransactionInterop.GetDtcTransaction(this.currentTransaction);
        }

        public void SetTransaction(Transaction transaction)
        {
            lock (this.syncRoot)
            {
                if (transaction == null)
                {
                    DiagnosticUtility.FailFast("Attempting to set transaction to NULL");
                }
                if (this.currentTransaction == null)
                {
                    ProxyEnlistment enlistmentNotification = new ProxyEnlistment(this, transaction);
                    transaction.EnlistVolatile(enlistmentNotification, EnlistmentOptions.None);
                    this.currentTransaction = transaction;
                    if (this.currentVoter != null)
                    {
                        this.currentVoter.SetTransaction(this.currentTransaction);
                    }
                }
                else if (this.currentTransaction != transaction)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.TransactionMismatch());
                }
            }
        }

        public Guid AppId
        {
            get
            {
                return this.appid;
            }
        }

        public Guid Clsid
        {
            get
            {
                return this.clsid;
            }
        }

        public Transaction CurrentTransaction
        {
            get
            {
                return this.currentTransaction;
            }
        }

        public int InstanceID
        {
            get
            {
                return this.instanceID;
            }
            set
            {
                this.instanceID = value;
            }
        }

        private class ProxyEnlistment : IEnlistmentNotification
        {
            private TransactionProxy proxy;
            private System.Transactions.Transaction transaction;

            public ProxyEnlistment(TransactionProxy proxy, System.Transactions.Transaction transaction)
            {
                this.proxy = proxy;
                this.transaction = transaction;
            }

            public void Commit(Enlistment enlistment)
            {
                DiagnosticUtility.FailFast("Should have voted read only");
            }

            public void InDoubt(Enlistment enlistment)
            {
                DiagnosticUtility.FailFast("Should have voted read only");
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                this.proxy.ClearTransaction(this);
                this.proxy = null;
                preparingEnlistment.Done();
            }

            public void Rollback(Enlistment enlistment)
            {
                this.proxy.ClearTransaction(this);
                this.proxy = null;
                enlistment.Done();
            }

            public System.Transactions.Transaction Transaction
            {
                get
                {
                    return this.transaction;
                }
            }
        }

        private class VoterBallot : ITransactionVoterBallotAsync2, IEnlistmentNotification
        {
            private Enlistment enlistment;
            private ITransactionVoterNotifyAsync2 notification;
            private PreparingEnlistment preparingEnlistment;
            private TransactionProxy proxy;
            private const int S_OK = 0;
            private Transaction transaction;

            public VoterBallot(ITransactionVoterNotifyAsync2 notification, TransactionProxy proxy)
            {
                this.notification = notification;
                this.proxy = proxy;
            }

            public void Commit(Enlistment enlistment)
            {
                enlistment.Done();
                this.notification.Committed(false, 0, 0);
                ComPlusTxProxyTrace.Trace(TraceEventType.Verbose, 0x50021, "TraceCodeComIntegrationTxProxyTxCommitted", this.proxy.AppId, this.proxy.Clsid, this.transaction.TransactionInformation.DistributedIdentifier, this.proxy.InstanceID);
                Marshal.ReleaseComObject(this.notification);
                this.notification = null;
            }

            public void InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
                this.notification.InDoubt();
                Marshal.ReleaseComObject(this.notification);
                this.notification = null;
            }

            public void Prepare(PreparingEnlistment enlistment)
            {
                this.preparingEnlistment = enlistment;
                this.notification.VoteRequest();
            }

            public void Rollback(Enlistment enlistment)
            {
                enlistment.Done();
                this.notification.Aborted(0, false, 0, 0);
                ComPlusTxProxyTrace.Trace(TraceEventType.Verbose, 0x50023, "TraceCodeComIntegrationTxProxyTxAbortedByTM", this.proxy.AppId, this.proxy.Clsid, this.transaction.TransactionInformation.DistributedIdentifier, this.proxy.InstanceID);
                Marshal.ReleaseComObject(this.notification);
                this.notification = null;
            }

            public void SetTransaction(Transaction transaction)
            {
                if (this.transaction != null)
                {
                    DiagnosticUtility.FailFast("Already have a transaction in the ballot!");
                }
                this.transaction = transaction;
                this.enlistment = transaction.EnlistVolatile(this, EnlistmentOptions.None);
            }

            public void VoteRequestDone(int hr, int reason)
            {
                if (this.preparingEnlistment == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NoVoteIssued")));
                }
                if (hr == 0)
                {
                    this.preparingEnlistment.Prepared();
                }
                else
                {
                    this.preparingEnlistment.ForceRollback();
                    ComPlusTxProxyTrace.Trace(TraceEventType.Verbose, 0x50022, "TraceCodeComIntegrationTxProxyTxAbortedByContext", this.proxy.AppId, this.proxy.Clsid, this.transaction.TransactionInformation.DistributedIdentifier, this.proxy.InstanceID);
                }
            }
        }
    }
}


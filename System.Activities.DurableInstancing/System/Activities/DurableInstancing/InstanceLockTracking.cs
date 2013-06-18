namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Transactions;

    internal sealed class InstanceLockTracking
    {
        private SqlWorkflowInstanceStore store;
        private object synchLock;

        public InstanceLockTracking(SqlWorkflowInstanceStore store)
        {
            this.InstanceId = Guid.Empty;
            this.store = store;
            this.synchLock = new object();
        }

        public void HandleFreed()
        {
            lock (this.synchLock)
            {
                if (this.BoundToLock && this.IsSafeToUnlock)
                {
                    this.store.GenerateUnlockCommand(this);
                }
                this.IsHandleFreed = true;
            }
        }

        public void TrackStoreLock(Guid instanceId, long instanceVersion, DependentTransaction dependentTransaction)
        {
            this.BoundToLock = true;
            this.InstanceId = instanceId;
            this.InstanceVersion = instanceVersion;
            if (dependentTransaction != null)
            {
                dependentTransaction.TransactionCompleted += new TransactionCompletedEventHandler(this.TransactionCompleted);
            }
            else
            {
                this.IsSafeToUnlock = true;
            }
        }

        public void TrackStoreUnlock(DependentTransaction dependentTransaction)
        {
            this.BoundToLock = false;
            this.IsHandleFreed = true;
            if (dependentTransaction != null)
            {
                dependentTransaction.TransactionCompleted += new TransactionCompletedEventHandler(this.TransactedUnlockCompleted);
            }
        }

        private void TransactedUnlockCompleted(object sender, TransactionEventArgs e)
        {
            lock (this.synchLock)
            {
                if ((e.Transaction.TransactionInformation.Status != TransactionStatus.Committed) && this.IsSafeToUnlock)
                {
                    this.store.GenerateUnlockCommand(this);
                }
            }
        }

        private void TransactionCompleted(object sender, TransactionEventArgs e)
        {
            lock (this.synchLock)
            {
                if (e.Transaction.TransactionInformation.Status == TransactionStatus.Committed)
                {
                    if (this.IsHandleFreed)
                    {
                        this.store.GenerateUnlockCommand(this);
                    }
                    else
                    {
                        this.IsSafeToUnlock = true;
                    }
                }
                else
                {
                    this.BoundToLock = false;
                }
            }
        }

        public bool BoundToLock { get; set; }

        public Guid InstanceId { get; set; }

        public long InstanceVersion { get; set; }

        public bool IsHandleFreed { get; set; }

        public bool IsSafeToUnlock { get; set; }
    }
}


namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.Threading;

    internal class SqlWorkflowInstanceStoreLock
    {
        private bool isBeingModified;
        private Guid lockOwnerId;
        private WeakReference lockOwnerInstanceHandle;
        private SqlWorkflowInstanceStore sqlWorkflowInstanceStore;
        private object thisLock;

        public SqlWorkflowInstanceStoreLock(SqlWorkflowInstanceStore sqlWorkflowInstanceStore)
        {
            this.sqlWorkflowInstanceStore = sqlWorkflowInstanceStore;
            this.thisLock = new object();
            this.SurrogateLockOwnerId = -1L;
        }

        public bool IsLockOwnerValid(long surrogateLockOwnerId)
        {
            return (((this.SurrogateLockOwnerId != -1L) && (surrogateLockOwnerId == this.SurrogateLockOwnerId)) && this.sqlWorkflowInstanceStore.InstanceOwnersExist);
        }

        public void MarkInstanceOwnerCreated(Guid lockOwnerId, long surrogateLockOwnerId, InstanceHandle lockOwnerInstanceHandle, bool detectRunnableInstances, bool detectActivatableInstances)
        {
            this.lockOwnerId = lockOwnerId;
            this.SurrogateLockOwnerId = surrogateLockOwnerId;
            this.lockOwnerInstanceHandle = new WeakReference(lockOwnerInstanceHandle);
            TimeSpan hostLockRenewalPeriod = this.sqlWorkflowInstanceStore.HostLockRenewalPeriod;
            TimeSpan runnableInstancesDetectionPeriod = this.sqlWorkflowInstanceStore.RunnableInstancesDetectionPeriod;
            if (detectActivatableInstances)
            {
                this.InstanceDetectionTask = new DetectActivatableWorkflowsTask(this.sqlWorkflowInstanceStore, this, runnableInstancesDetectionPeriod);
            }
            else if (detectRunnableInstances)
            {
                this.InstanceDetectionTask = new DetectRunnableInstancesTask(this.sqlWorkflowInstanceStore, this, runnableInstancesDetectionPeriod);
            }
            this.LockRenewalTask = new System.Activities.DurableInstancing.LockRenewalTask(this.sqlWorkflowInstanceStore, this, hostLockRenewalPeriod);
            this.LockRecoveryTask = new System.Activities.DurableInstancing.LockRecoveryTask(this.sqlWorkflowInstanceStore, this, hostLockRenewalPeriod);
            if (this.InstanceDetectionTask != null)
            {
                this.InstanceDetectionTask.ResetTimer(true);
            }
            this.LockRenewalTask.ResetTimer(true);
            this.LockRecoveryTask.ResetTimer(true);
        }

        private void MarkInstanceOwnerLost(long surrogateLockOwnerId)
        {
            if (this.SurrogateLockOwnerId == surrogateLockOwnerId)
            {
                this.SurrogateLockOwnerId = -1L;
                InstanceHandle target = this.lockOwnerInstanceHandle.Target as InstanceHandle;
                if (target != null)
                {
                    target.Free();
                }
                if (this.sqlWorkflowInstanceStore.IsLockRetryEnabled())
                {
                    this.sqlWorkflowInstanceStore.LoadRetryHandler.AbortPendingRetries();
                }
                if (this.LockRenewalTask != null)
                {
                    this.LockRenewalTask.CancelTimer();
                }
                if (this.LockRecoveryTask != null)
                {
                    this.LockRecoveryTask.CancelTimer();
                }
                if (this.InstanceDetectionTask != null)
                {
                    this.InstanceDetectionTask.CancelTimer();
                }
            }
        }

        public void MarkInstanceOwnerLost(long surrogateLockOwnerId, bool hasModificationLock)
        {
            if (hasModificationLock)
            {
                this.MarkInstanceOwnerLost(surrogateLockOwnerId);
            }
            else
            {
                this.TakeModificationLock();
                this.MarkInstanceOwnerLost(surrogateLockOwnerId);
                this.ReturnModificationLock();
            }
        }

        public void ReturnModificationLock()
        {
            bool lockTaken = false;
            do
            {
                Monitor.Enter(this.ThisLock, ref lockTaken);
            }
            while (!lockTaken);
            this.isBeingModified = false;
            Monitor.Pulse(this.ThisLock);
            Monitor.Exit(this.ThisLock);
        }

        public void TakeModificationLock()
        {
            bool lockTaken = false;
            do
            {
                Monitor.Enter(this.ThisLock, ref lockTaken);
            }
            while (!lockTaken);
            while (this.isBeingModified)
            {
                Monitor.Wait(this.ThisLock);
            }
            this.isBeingModified = true;
            Monitor.Exit(this.ThisLock);
        }

        public PersistenceTask InstanceDetectionTask { get; set; }

        public bool IsValid
        {
            get
            {
                return this.IsLockOwnerValid(this.SurrogateLockOwnerId);
            }
        }

        public Guid LockOwnerId
        {
            get
            {
                return this.lockOwnerId;
            }
        }

        public PersistenceTask LockRecoveryTask { get; set; }

        public PersistenceTask LockRenewalTask { get; set; }

        public long SurrogateLockOwnerId { get; private set; }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }
    }
}


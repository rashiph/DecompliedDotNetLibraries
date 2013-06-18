namespace System.Activities.DurableInstancing
{
    using System;

    internal class LockRenewalTask : PersistenceTask
    {
        public LockRenewalTask(SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, TimeSpan taskInterval) : base(store, storeLock, new ExtendLockCommand(), taskInterval, true)
        {
        }

        protected override void HandleError(Exception exception)
        {
            if (TD.RenewLockSystemErrorIsEnabled())
            {
                TD.RenewLockSystemError();
            }
            base.StoreLock.MarkInstanceOwnerLost(base.SurrogateLockOwnerId, false);
        }
    }
}


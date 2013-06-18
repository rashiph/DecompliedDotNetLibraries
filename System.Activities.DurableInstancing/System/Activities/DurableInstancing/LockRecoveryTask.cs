namespace System.Activities.DurableInstancing
{
    using System;

    internal class LockRecoveryTask : PersistenceTask
    {
        public LockRecoveryTask(SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, TimeSpan taskInterval) : base(store, storeLock, new RecoverInstanceLocksCommand(), taskInterval, true)
        {
        }

        protected override void HandleError(Exception exception)
        {
            if (TD.InstanceLocksRecoveryErrorIsEnabled())
            {
                TD.InstanceLocksRecoveryError(exception);
            }
            base.ResetTimer(false);
        }
    }
}


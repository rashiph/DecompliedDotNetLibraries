namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime.DurableInstancing;

    internal sealed class ExtendLockCommand : InstancePersistenceCommand
    {
        public ExtendLockCommand() : base(SqlWorkflowInstanceStoreConstants.DurableInstancingNamespace.GetName("ExtendLock"))
        {
        }
    }
}


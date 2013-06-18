namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime.DurableInstancing;

    internal sealed class RecoverInstanceLocksCommand : InstancePersistenceCommand
    {
        public RecoverInstanceLocksCommand() : base(SqlWorkflowInstanceStoreConstants.DurableInstancingNamespace.GetName("RecoverInstanceLocks"))
        {
        }
    }
}


namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;

    internal sealed class UnlockInstanceCommand : InstancePersistenceCommand
    {
        public UnlockInstanceCommand() : base(SqlWorkflowInstanceStoreConstants.DurableInstancingNamespace.GetName("UnlockInstance"))
        {
        }

        public Guid InstanceId { get; set; }

        public long InstanceVersion { get; set; }

        public long SurrogateOwnerId { get; set; }
    }
}


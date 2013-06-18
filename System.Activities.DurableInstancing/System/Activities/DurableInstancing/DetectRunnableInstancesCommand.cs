namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime.DurableInstancing;

    internal sealed class DetectRunnableInstancesCommand : InstancePersistenceCommand
    {
        public DetectRunnableInstancesCommand() : base(SqlWorkflowInstanceStoreConstants.DurableInstancingNamespace.GetName("DetectRunnableInstances"))
        {
        }
    }
}


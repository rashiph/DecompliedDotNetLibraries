namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime.DurableInstancing;

    internal sealed class DetectActivatableWorkflowsCommand : InstancePersistenceCommand
    {
        public DetectActivatableWorkflowsCommand() : base(SqlWorkflowInstanceStoreConstants.DurableInstancingNamespace.GetName("DetectActivatableWorkflows"))
        {
        }
    }
}


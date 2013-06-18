namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime.DurableInstancing;

    public sealed class HasRunnableWorkflowEvent : InstancePersistenceEvent<HasRunnableWorkflowEvent>
    {
        public HasRunnableWorkflowEvent() : base(InstancePersistence.ActivitiesEventNamespace.GetName("HasRunnableWorkflow"))
        {
        }
    }
}


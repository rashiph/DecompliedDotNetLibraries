namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime.DurableInstancing;

    public sealed class HasActivatableWorkflowEvent : InstancePersistenceEvent<HasActivatableWorkflowEvent>
    {
        public HasActivatableWorkflowEvent() : base(InstancePersistence.ActivitiesEventNamespace.GetName("HasActivatableWorkflow"))
        {
        }
    }
}


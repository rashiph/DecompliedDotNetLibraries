namespace System.Activities.DurableInstancing
{
    using System;
    using System.Activities;
    using System.Runtime;
    using System.Runtime.DurableInstancing;

    public sealed class QueryActivatableWorkflowsCommand : InstancePersistenceCommand
    {
        public QueryActivatableWorkflowsCommand() : base(InstancePersistence.ActivitiesCommandNamespace.GetName("QueryActivatableWorkflows"))
        {
        }

        protected internal override void Validate(InstanceView view)
        {
            if (!view.IsBoundToInstanceOwner)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.OwnerRequired));
            }
            if (view.IsBoundToInstance)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.AlreadyBoundToInstance));
            }
        }

        protected internal override bool IsTransactionEnlistmentOptional
        {
            get
            {
                return true;
            }
        }
    }
}


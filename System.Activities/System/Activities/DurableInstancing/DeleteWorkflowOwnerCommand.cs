namespace System.Activities.DurableInstancing
{
    using System;
    using System.Activities;
    using System.Runtime;
    using System.Runtime.DurableInstancing;

    public sealed class DeleteWorkflowOwnerCommand : InstancePersistenceCommand
    {
        public DeleteWorkflowOwnerCommand() : base(InstancePersistence.ActivitiesCommandNamespace.GetName("DeleteWorkflowOwner"))
        {
        }

        protected internal override void Validate(InstanceView view)
        {
            if (!view.IsBoundToInstanceOwner)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.OwnerRequired));
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


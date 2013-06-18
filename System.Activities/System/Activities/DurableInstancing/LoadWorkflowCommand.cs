namespace System.Activities.DurableInstancing
{
    using System;
    using System.Activities;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;

    public sealed class LoadWorkflowCommand : InstancePersistenceCommand
    {
        public LoadWorkflowCommand() : base(InstancePersistence.ActivitiesCommandNamespace.GetName("LoadWorkflow"))
        {
        }

        protected internal override void Validate(InstanceView view)
        {
            if (!view.IsBoundToInstance)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.InstanceRequired));
            }
            if (!view.IsBoundToInstanceOwner)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.OwnerRequired));
            }
        }

        public bool AcceptUninitializedInstance { get; set; }

        protected internal override bool AutomaticallyAcquiringLock
        {
            get
            {
                return true;
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


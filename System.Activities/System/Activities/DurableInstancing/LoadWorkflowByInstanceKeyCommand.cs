namespace System.Activities.DurableInstancing
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;

    public sealed class LoadWorkflowByInstanceKeyCommand : InstancePersistenceCommand
    {
        private Dictionary<Guid, IDictionary<XName, InstanceValue>> keysToAssociate;

        public LoadWorkflowByInstanceKeyCommand() : base(InstancePersistence.ActivitiesCommandNamespace.GetName("LoadWorkflowByInstanceKey"))
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
            if (this.LookupInstanceKey == Guid.Empty)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.LoadOpKeyMustBeValid));
            }
            if (this.AssociateInstanceKeyToInstanceId == Guid.Empty)
            {
                if (this.InstanceKeysToAssociate.ContainsKey(this.LookupInstanceKey))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.LoadOpAssociateKeysCannotContainLookupKey));
                }
            }
            else if (!this.AcceptUninitializedInstance)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.LoadOpFreeKeyRequiresAcceptUninitialized));
            }
            if (this.keysToAssociate != null)
            {
                foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> pair in this.keysToAssociate)
                {
                    pair.Value.ValidatePropertyBag();
                }
            }
        }

        public bool AcceptUninitializedInstance { get; set; }

        public Guid AssociateInstanceKeyToInstanceId { get; set; }

        protected internal override bool AutomaticallyAcquiringLock
        {
            get
            {
                return true;
            }
        }

        public IDictionary<Guid, IDictionary<XName, InstanceValue>> InstanceKeysToAssociate
        {
            get
            {
                if (this.keysToAssociate == null)
                {
                    this.keysToAssociate = new Dictionary<Guid, IDictionary<XName, InstanceValue>>();
                }
                return this.keysToAssociate;
            }
        }

        protected internal override bool IsTransactionEnlistmentOptional
        {
            get
            {
                if ((this.keysToAssociate != null) && (this.keysToAssociate.Count != 0))
                {
                    return false;
                }
                return (this.AssociateInstanceKeyToInstanceId == Guid.Empty);
            }
        }

        public Guid LookupInstanceKey { get; set; }
    }
}


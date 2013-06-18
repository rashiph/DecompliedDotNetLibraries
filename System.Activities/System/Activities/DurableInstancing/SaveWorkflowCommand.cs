namespace System.Activities.DurableInstancing
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;

    public sealed class SaveWorkflowCommand : InstancePersistenceCommand
    {
        private Dictionary<XName, InstanceValue> instanceData;
        private Dictionary<XName, InstanceValue> instanceMetadataChanges;
        private Dictionary<Guid, IDictionary<XName, InstanceValue>> keyMetadataChanges;
        private Dictionary<Guid, IDictionary<XName, InstanceValue>> keysToAssociate;
        private Collection<Guid> keysToComplete;
        private Collection<Guid> keysToFree;

        public SaveWorkflowCommand() : base(InstancePersistence.ActivitiesCommandNamespace.GetName("SaveWorkflow"))
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
            if (this.keysToAssociate != null)
            {
                foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> pair in this.keysToAssociate)
                {
                    pair.Value.ValidatePropertyBag();
                }
            }
            if (this.keyMetadataChanges != null)
            {
                foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> pair2 in this.keyMetadataChanges)
                {
                    pair2.Value.ValidatePropertyBag(true);
                }
            }
            if (this.CompleteInstance && !this.UnlockInstance)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.ValidateUnlockInstance));
            }
            this.instanceMetadataChanges.ValidatePropertyBag(true);
            this.instanceData.ValidatePropertyBag();
        }

        protected internal override bool AutomaticallyAcquiringLock
        {
            get
            {
                return true;
            }
        }

        public bool CompleteInstance { get; set; }

        public IDictionary<XName, InstanceValue> InstanceData
        {
            get
            {
                if (this.instanceData == null)
                {
                    this.instanceData = new Dictionary<XName, InstanceValue>();
                }
                return this.instanceData;
            }
        }

        public IDictionary<Guid, IDictionary<XName, InstanceValue>> InstanceKeyMetadataChanges
        {
            get
            {
                if (this.keyMetadataChanges == null)
                {
                    this.keyMetadataChanges = new Dictionary<Guid, IDictionary<XName, InstanceValue>>();
                }
                return this.keyMetadataChanges;
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

        public ICollection<Guid> InstanceKeysToComplete
        {
            get
            {
                if (this.keysToComplete == null)
                {
                    this.keysToComplete = new Collection<Guid>();
                }
                return this.keysToComplete;
            }
        }

        public ICollection<Guid> InstanceKeysToFree
        {
            get
            {
                if (this.keysToFree == null)
                {
                    this.keysToFree = new Collection<Guid>();
                }
                return this.keysToFree;
            }
        }

        public IDictionary<XName, InstanceValue> InstanceMetadataChanges
        {
            get
            {
                if (this.instanceMetadataChanges == null)
                {
                    this.instanceMetadataChanges = new Dictionary<XName, InstanceValue>();
                }
                return this.instanceMetadataChanges;
            }
        }

        protected internal override bool IsTransactionEnlistmentOptional
        {
            get
            {
                if ((((!this.CompleteInstance && ((this.instanceData == null) || (this.instanceData.Count == 0))) && ((this.keyMetadataChanges == null) || (this.keyMetadataChanges.Count == 0))) && (((this.instanceMetadataChanges == null) || (this.instanceMetadataChanges.Count == 0)) && ((this.keysToFree == null) || (this.keysToFree.Count == 0)))) && ((this.keysToComplete == null) || (this.keysToComplete.Count == 0)))
                {
                    if (this.keysToAssociate != null)
                    {
                        return (this.keysToAssociate.Count == 0);
                    }
                    return true;
                }
                return false;
            }
        }

        public bool UnlockInstance { get; set; }
    }
}


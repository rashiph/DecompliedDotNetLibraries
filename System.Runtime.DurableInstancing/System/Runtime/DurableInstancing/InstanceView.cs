namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public sealed class InstanceView
    {
        private Dictionary<XName, InstanceValue> accumulatedMetadataWrites;
        private Dictionary<XName, InstanceValue> accumulatedOwnerMetadataWrites;
        private IDictionary<XName, InstanceValue> data;
        private static readonly ReadOnlyDictionary<Guid, InstanceKeyView> emptyKeys = new ReadOnlyDictionary<Guid, InstanceKeyView>(new Dictionary<Guid, InstanceKeyView>(0), false);
        private static readonly ReadOnlyDictionary<XName, InstanceValue> emptyProperties = new ReadOnlyDictionary<XName, InstanceValue>(new Dictionary<XName, InstanceValue>(0), false);
        private long instanceVersion;
        private IDictionary<Guid, InstanceKeyView> keys;
        private IDictionary<XName, InstanceValue> metadata;
        private IDictionary<XName, InstanceValue> ownerMetadata;
        private ReadOnlyCollection<InstanceStoreQueryResult> queryResults;
        private Collection<InstanceStoreQueryResult> queryResultsBackingCollection;

        private InstanceView()
        {
            this.instanceVersion = -1L;
            this.InstanceDataConsistency = InstanceValueConsistency.Partial | InstanceValueConsistency.InDoubt;
            this.InstanceMetadataConsistency = InstanceValueConsistency.Partial | InstanceValueConsistency.InDoubt;
            this.InstanceOwnerMetadataConsistency = InstanceValueConsistency.Partial | InstanceValueConsistency.InDoubt;
            this.InstanceKeysConsistency = InstanceValueConsistency.Partial | InstanceValueConsistency.InDoubt;
        }

        internal InstanceView(System.Runtime.DurableInstancing.InstanceOwner owner) : this()
        {
            this.InstanceOwner = owner;
        }

        private InstanceView(InstanceView source)
        {
            this.instanceVersion = source.instanceVersion;
            this.InstanceOwner = source.InstanceOwner;
            this.InstanceId = source.InstanceId;
            this.IsBoundToInstance = source.IsBoundToInstance;
            this.InstanceState = source.InstanceState;
            this.InstanceDataConsistency = source.InstanceDataConsistency;
            this.InstanceMetadataConsistency = source.InstanceMetadataConsistency;
            this.InstanceOwnerMetadataConsistency = source.InstanceOwnerMetadataConsistency;
            this.InstanceKeysConsistency = source.InstanceKeysConsistency;
            this.InstanceData = source.InstanceData;
            this.InstanceMetadata = source.InstanceMetadata;
            this.InstanceOwnerMetadata = source.InstanceOwnerMetadata;
            this.InstanceStoreQueryResults = source.InstanceStoreQueryResults;
            Dictionary<Guid, InstanceKeyView> dictionary = null;
            if (source.InstanceKeys.Count > 0)
            {
                dictionary = new Dictionary<Guid, InstanceKeyView>(source.InstanceKeys.Count);
                foreach (KeyValuePair<Guid, InstanceKeyView> pair in source.InstanceKeys)
                {
                    dictionary.Add(pair.Key, pair.Value.Clone());
                }
            }
            this.InstanceKeys = (dictionary == null) ? null : new ReadOnlyDictionary<Guid, InstanceKeyView>(dictionary, false);
        }

        internal InstanceView(System.Runtime.DurableInstancing.InstanceOwner owner, Guid instanceId) : this()
        {
            this.InstanceOwner = owner;
            this.InstanceId = instanceId;
            this.IsBoundToInstance = true;
        }

        internal void BindInstance(Guid instanceId)
        {
            Fx.AssertAndThrow(!this.IsViewFrozen, "BindInstance called on read-only InstanceView.");
            if (this.IsBoundToInstance)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextAlreadyBoundToInstance));
            }
            this.InstanceId = instanceId;
            this.IsBoundToInstance = true;
        }

        internal void BindLock(long instanceVersion)
        {
            Fx.AssertAndThrow(!this.IsViewFrozen, "BindLock called on read-only InstanceView.");
            if (!this.IsBoundToInstanceOwner)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextMustBeBoundToOwner));
            }
            if (!this.IsBoundToInstance)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextMustBeBoundToInstance));
            }
            if (Interlocked.CompareExchange(ref this.instanceVersion, instanceVersion, -1L) != -1L)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextAlreadyBoundToLock));
            }
        }

        internal void BindOwner(System.Runtime.DurableInstancing.InstanceOwner owner)
        {
            Fx.AssertAndThrow(!this.IsViewFrozen, "BindOwner called on read-only InstanceView.");
            if (this.IsBoundToInstanceOwner)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextAlreadyBoundToOwner));
            }
            this.InstanceOwner = owner;
        }

        internal InstanceView Clone()
        {
            return new InstanceView(this);
        }

        internal void FinishBindLock(long instanceVersion)
        {
            Fx.AssertAndThrow(!this.IsViewFrozen, "FinishBindLock called on read-only InstanceView.");
            Fx.AssertAndThrow(Interlocked.CompareExchange(ref this.instanceVersion, instanceVersion, -instanceVersion - 2L) == (-instanceVersion - 2L), "FinishBindLock called with mismatched instance version.");
        }

        internal void MakeReadOnly()
        {
            this.IsViewFrozen = true;
        }

        internal void StartBindLock(long instanceVersion)
        {
            Fx.AssertAndThrow(!this.IsViewFrozen, "StartBindLock called on read-only InstanceView.");
            if (!this.IsBoundToInstanceOwner)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextMustBeBoundToOwner));
            }
            if (!this.IsBoundToInstance)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextMustBeBoundToInstance));
            }
            if (Interlocked.CompareExchange(ref this.instanceVersion, (0L - instanceVersion) - 2L, -1L) != -1L)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextAlreadyBoundToLock));
            }
        }

        internal Dictionary<XName, InstanceValue> AccumulatedMetadataWrites
        {
            get
            {
                if (this.accumulatedMetadataWrites == null)
                {
                    this.accumulatedMetadataWrites = new Dictionary<XName, InstanceValue>();
                }
                return this.accumulatedMetadataWrites;
            }
        }

        internal Dictionary<XName, InstanceValue> AccumulatedOwnerMetadataWrites
        {
            get
            {
                if (this.accumulatedOwnerMetadataWrites == null)
                {
                    this.accumulatedOwnerMetadataWrites = new Dictionary<XName, InstanceValue>();
                }
                return this.accumulatedOwnerMetadataWrites;
            }
        }

        public IDictionary<XName, InstanceValue> InstanceData
        {
            get
            {
                return (this.data ?? emptyProperties);
            }
            internal set
            {
                Fx.AssertAndThrow(!this.IsViewFrozen, "Setting Data on frozen View.");
                this.data = value;
            }
        }

        public InstanceValueConsistency InstanceDataConsistency
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceDataConsistency>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal set
            {
                this.<InstanceDataConsistency>k__BackingField = value;
            }
        }

        public Guid InstanceId
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceId>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<InstanceId>k__BackingField = value;
            }
        }

        public IDictionary<Guid, InstanceKeyView> InstanceKeys
        {
            get
            {
                return (this.keys ?? emptyKeys);
            }
            internal set
            {
                Fx.AssertAndThrow(!this.IsViewFrozen, "Setting Keys on frozen View.");
                this.keys = value;
            }
        }

        public InstanceValueConsistency InstanceKeysConsistency
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceKeysConsistency>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal set
            {
                this.<InstanceKeysConsistency>k__BackingField = value;
            }
        }

        public IDictionary<XName, InstanceValue> InstanceMetadata
        {
            get
            {
                IDictionary<XName, InstanceValue> accumulatedMetadataWrites = this.accumulatedMetadataWrites;
                this.accumulatedMetadataWrites = null;
                this.metadata = accumulatedMetadataWrites.ReadOnlyMergeInto(this.metadata ?? emptyProperties, true);
                return this.metadata;
            }
            internal set
            {
                Fx.AssertAndThrow(!this.IsViewFrozen, "Setting Metadata on frozen View.");
                this.accumulatedMetadataWrites = null;
                this.metadata = value;
            }
        }

        public InstanceValueConsistency InstanceMetadataConsistency
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceMetadataConsistency>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal set
            {
                this.<InstanceMetadataConsistency>k__BackingField = value;
            }
        }

        public System.Runtime.DurableInstancing.InstanceOwner InstanceOwner
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceOwner>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<InstanceOwner>k__BackingField = value;
            }
        }

        public IDictionary<XName, InstanceValue> InstanceOwnerMetadata
        {
            get
            {
                IDictionary<XName, InstanceValue> accumulatedOwnerMetadataWrites = this.accumulatedOwnerMetadataWrites;
                this.accumulatedOwnerMetadataWrites = null;
                this.ownerMetadata = accumulatedOwnerMetadataWrites.ReadOnlyMergeInto(this.ownerMetadata ?? emptyProperties, true);
                return this.ownerMetadata;
            }
            internal set
            {
                Fx.AssertAndThrow(!this.IsViewFrozen, "Setting OwnerMetadata on frozen View.");
                this.accumulatedOwnerMetadataWrites = null;
                this.ownerMetadata = value;
            }
        }

        public InstanceValueConsistency InstanceOwnerMetadataConsistency
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceOwnerMetadataConsistency>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal set
            {
                this.<InstanceOwnerMetadataConsistency>k__BackingField = value;
            }
        }

        public System.Runtime.DurableInstancing.InstanceState InstanceState
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceState>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal set
            {
                this.<InstanceState>k__BackingField = value;
            }
        }

        public ReadOnlyCollection<InstanceStoreQueryResult> InstanceStoreQueryResults
        {
            get
            {
                if (this.queryResults == null)
                {
                    this.queryResults = new ReadOnlyCollection<InstanceStoreQueryResult>(this.QueryResultsBacking);
                }
                return this.queryResults;
            }
            internal set
            {
                Fx.AssertAndThrow(!this.IsViewFrozen, "Setting InstanceStoreQueryResults on frozen View.");
                this.queryResults = null;
                if (value == null)
                {
                    this.queryResultsBackingCollection = null;
                }
                else
                {
                    this.queryResultsBackingCollection = new Collection<InstanceStoreQueryResult>();
                    foreach (InstanceStoreQueryResult result in value)
                    {
                        this.queryResultsBackingCollection.Add(result);
                    }
                }
            }
        }

        public bool IsBoundToInstance
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<IsBoundToInstance>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<IsBoundToInstance>k__BackingField = value;
            }
        }

        public bool IsBoundToInstanceOwner
        {
            get
            {
                return (this.InstanceOwner != null);
            }
        }

        public bool IsBoundToLock
        {
            get
            {
                return (this.instanceVersion >= 0L);
            }
        }

        private bool IsViewFrozen { get; set; }

        internal Collection<InstanceStoreQueryResult> QueryResultsBacking
        {
            get
            {
                if (this.queryResultsBackingCollection == null)
                {
                    this.queryResultsBackingCollection = new Collection<InstanceStoreQueryResult>();
                }
                return this.queryResultsBackingCollection;
            }
        }
    }
}


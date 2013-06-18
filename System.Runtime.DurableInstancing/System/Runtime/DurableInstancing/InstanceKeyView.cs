namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public sealed class InstanceKeyView
    {
        private Dictionary<XName, InstanceValue> accumulatedMetadataWrites;
        private static readonly ReadOnlyDictionary<XName, InstanceValue> emptyProperties = new ReadOnlyDictionary<XName, InstanceValue>(new Dictionary<XName, InstanceValue>(0), false);
        private IDictionary<XName, InstanceValue> metadata;

        internal InstanceKeyView(Guid key)
        {
            this.InstanceKey = key;
            this.InstanceKeyMetadataConsistency = InstanceValueConsistency.Partial | InstanceValueConsistency.InDoubt;
        }

        private InstanceKeyView(InstanceKeyView source)
        {
            this.InstanceKey = source.InstanceKey;
            this.InstanceKeyState = source.InstanceKeyState;
            this.InstanceKeyMetadata = source.InstanceKeyMetadata;
            this.InstanceKeyMetadataConsistency = source.InstanceKeyMetadataConsistency;
        }

        internal InstanceKeyView Clone()
        {
            return new InstanceKeyView(this);
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

        public Guid InstanceKey
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceKey>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<InstanceKey>k__BackingField = value;
            }
        }

        public IDictionary<XName, InstanceValue> InstanceKeyMetadata
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
                this.accumulatedMetadataWrites = null;
                this.metadata = value;
            }
        }

        public InstanceValueConsistency InstanceKeyMetadataConsistency
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceKeyMetadataConsistency>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal set
            {
                this.<InstanceKeyMetadataConsistency>k__BackingField = value;
            }
        }

        public System.Runtime.DurableInstancing.InstanceKeyState InstanceKeyState
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceKeyState>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal set
            {
                this.<InstanceKeyState>k__BackingField = value;
            }
        }
    }
}


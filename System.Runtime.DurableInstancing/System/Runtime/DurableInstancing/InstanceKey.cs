namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public class InstanceKey
    {
        private static IDictionary<XName, InstanceValue> emptyMetadata = new ReadOnlyDictionary<XName, InstanceValue>(new Dictionary<XName, InstanceValue>(0));
        private readonly bool invalid;
        private static InstanceKey invalidKey = new InstanceKey();

        private InstanceKey()
        {
            this.Value = Guid.Empty;
            this.invalid = true;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceKey(Guid value) : this(value, null)
        {
        }

        public InstanceKey(Guid value, IDictionary<XName, InstanceValue> metadata)
        {
            if (value == Guid.Empty)
            {
                throw Fx.Exception.Argument("value", SRCore.InstanceKeyRequiresValidGuid);
            }
            this.Value = value;
            if (metadata != null)
            {
                this.Metadata = ReadOnlyDictionary<XName, InstanceValue>.Create(metadata);
            }
            else
            {
                this.Metadata = emptyMetadata;
            }
        }

        public override bool Equals(object obj)
        {
            return this.Value.Equals(((InstanceKey) obj).Value);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public static InstanceKey InvalidKey
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return invalidKey;
            }
        }

        public bool IsValid
        {
            get
            {
                return !this.invalid;
            }
        }

        public IDictionary<XName, InstanceValue> Metadata
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Metadata>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<Metadata>k__BackingField = value;
            }
        }

        public Guid Value
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Value>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<Value>k__BackingField = value;
            }
        }
    }
}


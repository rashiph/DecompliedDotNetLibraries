namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public sealed class InstanceLockQueryResult : InstanceStoreQueryResult
    {
        private static readonly ReadOnlyDictionary<Guid, Guid> EmptyQueryResult = new ReadOnlyDictionary<Guid, Guid>(new Dictionary<Guid, Guid>(0), false);

        public InstanceLockQueryResult()
        {
            this.InstanceOwnerIds = EmptyQueryResult;
        }

        public InstanceLockQueryResult(IDictionary<Guid, Guid> instanceOwnerIds)
        {
            this.InstanceOwnerIds = new ReadOnlyDictionary<Guid, Guid>(instanceOwnerIds);
        }

        public InstanceLockQueryResult(Guid instanceId, Guid instanceOwnerId)
        {
            Dictionary<Guid, Guid> dictionary = new Dictionary<Guid, Guid>(1);
            dictionary.Add(instanceId, instanceOwnerId);
            this.InstanceOwnerIds = new ReadOnlyDictionary<Guid, Guid>(dictionary, false);
        }

        public IDictionary<Guid, Guid> InstanceOwnerIds
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceOwnerIds>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<InstanceOwnerIds>k__BackingField = value;
            }
        }
    }
}


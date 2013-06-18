namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public sealed class InstanceOwnerQueryResult : InstanceStoreQueryResult
    {
        private static readonly ReadOnlyDictionary<XName, InstanceValue> EmptyMetadata = new ReadOnlyDictionary<XName, InstanceValue>(new Dictionary<XName, InstanceValue>(0), false);
        private static readonly ReadOnlyDictionary<Guid, IDictionary<XName, InstanceValue>> EmptyQueryResult = new ReadOnlyDictionary<Guid, IDictionary<XName, InstanceValue>>(new Dictionary<Guid, IDictionary<XName, InstanceValue>>(0), false);

        public InstanceOwnerQueryResult()
        {
            this.InstanceOwners = EmptyQueryResult;
        }

        public InstanceOwnerQueryResult(IDictionary<Guid, IDictionary<XName, InstanceValue>> instanceOwners)
        {
            Dictionary<Guid, IDictionary<XName, InstanceValue>> dictionary = new Dictionary<Guid, IDictionary<XName, InstanceValue>>(instanceOwners.Count);
            foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> pair in instanceOwners)
            {
                dictionary.Add(pair.Key, (pair.Value == null) ? EmptyMetadata : (pair.Value.IsReadOnly ? pair.Value : new ReadOnlyDictionary<XName, InstanceValue>(pair.Value)));
            }
            this.InstanceOwners = new ReadOnlyDictionary<Guid, IDictionary<XName, InstanceValue>>(dictionary, false);
        }

        public InstanceOwnerQueryResult(Guid instanceOwnerId, IDictionary<XName, InstanceValue> metadata)
        {
            Dictionary<Guid, IDictionary<XName, InstanceValue>> dictionary = new Dictionary<Guid, IDictionary<XName, InstanceValue>>(1);
            dictionary.Add(instanceOwnerId, (metadata == null) ? EmptyMetadata : (metadata.IsReadOnly ? metadata : new ReadOnlyDictionary<XName, InstanceValue>(metadata)));
            this.InstanceOwners = new ReadOnlyDictionary<Guid, IDictionary<XName, InstanceValue>>(dictionary, false);
        }

        public IDictionary<Guid, IDictionary<XName, InstanceValue>> InstanceOwners
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceOwners>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<InstanceOwners>k__BackingField = value;
            }
        }
    }
}


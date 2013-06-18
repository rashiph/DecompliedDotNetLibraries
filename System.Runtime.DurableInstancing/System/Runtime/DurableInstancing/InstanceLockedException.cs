namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Xml.Linq;

    [Serializable]
    public class InstanceLockedException : InstancePersistenceCommandException
    {
        private const string InstanceOwnerIdName = "instancePersistenceInstanceOwnerId";
        private const string SerializableInstanceOwnerMetadataName = "instancePersistenceSerializableInstanceOwnerMetadata";

        public InstanceLockedException() : this(SRCore.CannotAcquireLockDefault, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceLockedException(string message) : this(message, null)
        {
        }

        [SecurityCritical]
        protected InstanceLockedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.InstanceOwnerId = (Guid) info.GetValue("instancePersistenceInstanceOwnerId", typeof(Guid));
            this.SerializableInstanceOwnerMetadata = (ReadOnlyDictionary<XName, object>) info.GetValue("instancePersistenceSerializableInstanceOwnerMetadata", typeof(ReadOnlyDictionary<XName, object>));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceLockedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceLockedException(XName commandName, Guid instanceId) : this(commandName, instanceId, null)
        {
        }

        public InstanceLockedException(XName commandName, Guid instanceId, Exception innerException) : this(commandName, instanceId, ToMessage(instanceId), innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceLockedException(XName commandName, Guid instanceId, Guid instanceOwnerId, IDictionary<XName, object> serializableInstanceOwnerMetadata) : this(commandName, instanceId, instanceOwnerId, serializableInstanceOwnerMetadata, null)
        {
        }

        public InstanceLockedException(XName commandName, Guid instanceId, string message, Exception innerException) : this(commandName, instanceId, Guid.Empty, null, message, innerException)
        {
        }

        public InstanceLockedException(XName commandName, Guid instanceId, Guid instanceOwnerId, IDictionary<XName, object> serializableInstanceOwnerMetadata, Exception innerException) : this(commandName, instanceId, instanceOwnerId, serializableInstanceOwnerMetadata, ToMessage(instanceId, instanceOwnerId), innerException)
        {
        }

        public InstanceLockedException(XName commandName, Guid instanceId, Guid instanceOwnerId, IDictionary<XName, object> serializableInstanceOwnerMetadata, string message, Exception innerException) : base(commandName, instanceId, message, innerException)
        {
            this.InstanceOwnerId = instanceOwnerId;
            if (serializableInstanceOwnerMetadata != null)
            {
                this.SerializableInstanceOwnerMetadata = new ReadOnlyDictionary<XName, object>(serializableInstanceOwnerMetadata);
            }
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("instancePersistenceInstanceOwnerId", this.InstanceOwnerId, typeof(Guid));
            info.AddValue("instancePersistenceSerializableInstanceOwnerMetadata", this.SerializableInstanceOwnerMetadata, typeof(ReadOnlyDictionary<XName, object>));
        }

        private static string ToMessage(Guid instanceId)
        {
            if (instanceId == Guid.Empty)
            {
                return SRCore.CannotAcquireLockDefault;
            }
            return SRCore.CannotAcquireLockSpecific(instanceId);
        }

        private static string ToMessage(Guid instanceId, Guid instanceOwnerId)
        {
            if (instanceId == Guid.Empty)
            {
                return SRCore.CannotAcquireLockDefault;
            }
            if (instanceOwnerId == Guid.Empty)
            {
                return SRCore.CannotAcquireLockSpecific(instanceId);
            }
            return SRCore.CannotAcquireLockSpecificWithOwner(instanceId, instanceOwnerId);
        }

        public Guid InstanceOwnerId
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceOwnerId>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<InstanceOwnerId>k__BackingField = value;
            }
        }

        public IDictionary<XName, object> SerializableInstanceOwnerMetadata
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<SerializableInstanceOwnerMetadata>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<SerializableInstanceOwnerMetadata>k__BackingField = value;
            }
        }
    }
}


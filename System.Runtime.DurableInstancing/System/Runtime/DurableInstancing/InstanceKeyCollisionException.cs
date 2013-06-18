namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Xml.Linq;

    [Serializable]
    public class InstanceKeyCollisionException : InstancePersistenceCommandException
    {
        private const string ConflictingInstanceIdName = "instancePersistenceConflictingInstanceId";
        private const string InstanceKeyName = "instancePersistenceInstanceKey";

        public InstanceKeyCollisionException() : this(SRCore.KeyCollisionDefault, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceKeyCollisionException(string message) : this(message, null)
        {
        }

        [SecurityCritical]
        protected InstanceKeyCollisionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.ConflictingInstanceId = (Guid) info.GetValue("instancePersistenceConflictingInstanceId", typeof(Guid));
            Guid guid = (Guid) info.GetValue("instancePersistenceInstanceKey", typeof(Guid));
            this.InstanceKey = (guid == Guid.Empty) ? null : new System.Runtime.DurableInstancing.InstanceKey(guid);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceKeyCollisionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceKeyCollisionException(XName commandName, Guid instanceId, System.Runtime.DurableInstancing.InstanceKey instanceKey, Guid conflictingInstanceId) : this(commandName, instanceId, instanceKey, conflictingInstanceId, null)
        {
        }

        public InstanceKeyCollisionException(XName commandName, Guid instanceId, System.Runtime.DurableInstancing.InstanceKey instanceKey, Guid conflictingInstanceId, Exception innerException) : this(commandName, instanceId, instanceKey, conflictingInstanceId, ToMessage(instanceId, instanceKey, conflictingInstanceId), innerException)
        {
        }

        public InstanceKeyCollisionException(XName commandName, Guid instanceId, System.Runtime.DurableInstancing.InstanceKey instanceKey, Guid conflictingInstanceId, string message, Exception innerException) : base(commandName, instanceId, message, innerException)
        {
            this.ConflictingInstanceId = conflictingInstanceId;
            this.InstanceKey = instanceKey;
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("instancePersistenceConflictingInstanceId", this.ConflictingInstanceId, typeof(Guid));
            info.AddValue("instancePersistenceInstanceKey", ((this.InstanceKey != null) && this.InstanceKey.IsValid) ? this.InstanceKey.Value : Guid.Empty, typeof(Guid));
        }

        private static string ToMessage(Guid instanceId, System.Runtime.DurableInstancing.InstanceKey instanceKey, Guid conflictingInstanceId)
        {
            if ((instanceKey == null) || !instanceKey.IsValid)
            {
                return SRCore.KeyCollisionDefault;
            }
            if ((instanceId != Guid.Empty) && (conflictingInstanceId != Guid.Empty))
            {
                return SRCore.KeyCollisionSpecific(instanceId, instanceKey.Value, conflictingInstanceId);
            }
            return SRCore.KeyCollisionSpecificKeyOnly(instanceKey.Value);
        }

        public Guid ConflictingInstanceId
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<ConflictingInstanceId>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<ConflictingInstanceId>k__BackingField = value;
            }
        }

        public System.Runtime.DurableInstancing.InstanceKey InstanceKey
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
    }
}


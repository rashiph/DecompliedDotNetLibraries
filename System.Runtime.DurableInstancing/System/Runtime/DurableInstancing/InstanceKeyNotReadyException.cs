namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Xml.Linq;

    [Serializable]
    public class InstanceKeyNotReadyException : InstancePersistenceCommandException
    {
        private const string InstanceKeyName = "instancePersistenceInstanceKey";

        public InstanceKeyNotReadyException() : this(SRCore.KeyNotReadyDefault, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceKeyNotReadyException(string message) : this(message, null)
        {
        }

        [SecurityCritical]
        protected InstanceKeyNotReadyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Guid guid = (Guid) info.GetValue("instancePersistenceInstanceKey", typeof(Guid));
            this.InstanceKey = (guid == Guid.Empty) ? null : new System.Runtime.DurableInstancing.InstanceKey(guid);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceKeyNotReadyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceKeyNotReadyException(XName commandName, System.Runtime.DurableInstancing.InstanceKey instanceKey) : this(commandName, instanceKey, null)
        {
        }

        public InstanceKeyNotReadyException(XName commandName, System.Runtime.DurableInstancing.InstanceKey instanceKey, Exception innerException) : this(commandName, Guid.Empty, instanceKey, ToMessage(instanceKey), innerException)
        {
        }

        public InstanceKeyNotReadyException(XName commandName, Guid instanceId, System.Runtime.DurableInstancing.InstanceKey instanceKey, string message, Exception innerException) : base(commandName, instanceId, message, innerException)
        {
            this.InstanceKey = instanceKey;
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("instancePersistenceInstanceKey", ((this.InstanceKey != null) && this.InstanceKey.IsValid) ? this.InstanceKey.Value : Guid.Empty, typeof(Guid));
        }

        private static string ToMessage(System.Runtime.DurableInstancing.InstanceKey instanceKey)
        {
            if ((instanceKey != null) && instanceKey.IsValid)
            {
                return SRCore.KeyNotReadySpecific(instanceKey.Value);
            }
            return SRCore.KeyNotReadyDefault;
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


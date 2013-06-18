namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Xml.Linq;

    [Serializable]
    public class InstanceKeyCompleteException : InstancePersistenceCommandException
    {
        private const string InstanceKeyName = "instancePersistenceInstanceKey";

        public InstanceKeyCompleteException() : this(SRCore.KeyNotReadyDefault, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceKeyCompleteException(string message) : this(message, null)
        {
        }

        [SecurityCritical]
        protected InstanceKeyCompleteException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Guid guid = (Guid) info.GetValue("instancePersistenceInstanceKey", typeof(Guid));
            this.InstanceKey = (guid == Guid.Empty) ? null : new System.Runtime.DurableInstancing.InstanceKey(guid);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceKeyCompleteException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceKeyCompleteException(XName commandName, System.Runtime.DurableInstancing.InstanceKey instanceKey) : this(commandName, instanceKey, null)
        {
        }

        public InstanceKeyCompleteException(XName commandName, System.Runtime.DurableInstancing.InstanceKey instanceKey, Exception innerException) : this(commandName, Guid.Empty, instanceKey, ToMessage(instanceKey), innerException)
        {
        }

        public InstanceKeyCompleteException(XName commandName, Guid instanceId, System.Runtime.DurableInstancing.InstanceKey instanceKey, string message, Exception innerException) : base(commandName, instanceId, message, innerException)
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
                return SRCore.KeyCompleteSpecific(instanceKey.Value);
            }
            return SRCore.KeyCompleteDefault;
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


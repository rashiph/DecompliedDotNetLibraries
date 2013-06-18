namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Xml.Linq;

    [Serializable]
    public class InstanceOwnerException : InstancePersistenceException
    {
        private const string InstanceOwnerIdName = "instancePersistenceInstanceOwnerId";

        public InstanceOwnerException() : base(SRCore.InstanceOwnerDefault)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceOwnerException(string message) : base(message)
        {
        }

        [SecurityCritical]
        protected InstanceOwnerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.InstanceOwnerId = (Guid) info.GetValue("instancePersistenceInstanceOwnerId", typeof(Guid));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceOwnerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceOwnerException(XName commandName, Guid instanceOwnerId) : this(commandName, instanceOwnerId, null)
        {
        }

        public InstanceOwnerException(XName commandName, Guid instanceOwnerId, Exception innerException) : this(commandName, instanceOwnerId, ToMessage(instanceOwnerId), innerException)
        {
        }

        public InstanceOwnerException(XName commandName, Guid instanceOwnerId, string message, Exception innerException) : base(commandName, message, innerException)
        {
            this.InstanceOwnerId = instanceOwnerId;
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("instancePersistenceInstanceOwnerId", this.InstanceOwnerId, typeof(Guid));
        }

        private static string ToMessage(Guid instanceOwnerId)
        {
            if (instanceOwnerId == Guid.Empty)
            {
                return SRCore.InstanceOwnerDefault;
            }
            return SRCore.InstanceOwnerSpecific(instanceOwnerId);
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
    }
}


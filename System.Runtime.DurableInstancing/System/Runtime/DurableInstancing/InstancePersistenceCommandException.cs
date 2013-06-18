namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Xml.Linq;

    [Serializable]
    public class InstancePersistenceCommandException : InstancePersistenceException
    {
        private const string InstanceIdName = "instancePersistenceInstanceId";

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstancePersistenceCommandException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstancePersistenceCommandException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstancePersistenceCommandException(XName commandName) : base(commandName)
        {
        }

        [SecurityCritical]
        protected InstancePersistenceCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.InstanceId = (Guid) info.GetValue("instancePersistenceInstanceId", typeof(Guid));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstancePersistenceCommandException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstancePersistenceCommandException(XName commandName, Exception innerException) : base(commandName, innerException)
        {
        }

        public InstancePersistenceCommandException(XName commandName, Guid instanceId) : base(commandName)
        {
            this.InstanceId = instanceId;
        }

        public InstancePersistenceCommandException(XName commandName, Guid instanceId, Exception innerException) : base(commandName, innerException)
        {
            this.InstanceId = instanceId;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstancePersistenceCommandException(XName commandName, string message, Exception innerException) : base(commandName, message, innerException)
        {
        }

        public InstancePersistenceCommandException(XName commandName, Guid instanceId, string message, Exception innerException) : base(commandName, message, innerException)
        {
            this.InstanceId = instanceId;
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("instancePersistenceInstanceId", this.InstanceId, typeof(Guid));
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
    }
}


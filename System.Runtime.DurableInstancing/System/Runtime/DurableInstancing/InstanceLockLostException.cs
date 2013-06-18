namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Xml.Linq;

    [Serializable]
    public class InstanceLockLostException : InstancePersistenceCommandException
    {
        public InstanceLockLostException() : this(SRCore.InstanceLockLostDefault, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceLockLostException(string message) : this(message, null)
        {
        }

        [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected InstanceLockLostException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceLockLostException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceLockLostException(XName commandName, Guid instanceId) : this(commandName, instanceId, null)
        {
        }

        public InstanceLockLostException(XName commandName, Guid instanceId, Exception innerException) : this(commandName, instanceId, ToMessage(instanceId), innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceLockLostException(XName commandName, Guid instanceId, string message, Exception innerException) : base(commandName, instanceId, message, innerException)
        {
        }

        private static string ToMessage(Guid instanceId)
        {
            if (instanceId != Guid.Empty)
            {
                return SRCore.InstanceLockLostSpecific(instanceId);
            }
            return SRCore.InstanceLockLostDefault;
        }
    }
}


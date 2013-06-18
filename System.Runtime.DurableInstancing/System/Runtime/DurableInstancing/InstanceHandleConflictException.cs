namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Xml.Linq;

    [Serializable]
    public class InstanceHandleConflictException : InstancePersistenceCommandException
    {
        public InstanceHandleConflictException() : this(SRCore.InstanceHandleConflictDefault, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceHandleConflictException(string message) : this(message, null)
        {
        }

        [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected InstanceHandleConflictException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceHandleConflictException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceHandleConflictException(XName commandName, Guid instanceId) : this(commandName, instanceId, null)
        {
        }

        public InstanceHandleConflictException(XName commandName, Guid instanceId, Exception innerException) : this(commandName, instanceId, ToMessage(instanceId), innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceHandleConflictException(XName commandName, Guid instanceId, string message, Exception innerException) : base(commandName, instanceId, message, innerException)
        {
        }

        private static string ToMessage(Guid instanceId)
        {
            if (instanceId != Guid.Empty)
            {
                return SRCore.InstanceHandleConflictSpecific(instanceId);
            }
            return SRCore.InstanceHandleConflictDefault;
        }
    }
}


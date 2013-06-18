namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Xml.Linq;

    [Serializable]
    public class InstanceNotReadyException : InstancePersistenceCommandException
    {
        public InstanceNotReadyException() : this(SRCore.InstanceNotReadyDefault, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceNotReadyException(string message) : this(message, null)
        {
        }

        [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected InstanceNotReadyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceNotReadyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceNotReadyException(XName commandName, Guid instanceId) : this(commandName, instanceId, null)
        {
        }

        public InstanceNotReadyException(XName commandName, Guid instanceId, Exception innerException) : this(commandName, instanceId, ToMessage(instanceId), innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceNotReadyException(XName commandName, Guid instanceId, string message, Exception innerException) : base(commandName, instanceId, message, innerException)
        {
        }

        private static string ToMessage(Guid instanceId)
        {
            if (instanceId != Guid.Empty)
            {
                return SRCore.InstanceNotReadySpecific(instanceId);
            }
            return SRCore.InstanceNotReadyDefault;
        }
    }
}


namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Xml.Linq;

    [Serializable]
    public class InstanceCompleteException : InstancePersistenceCommandException
    {
        public InstanceCompleteException() : this(SRCore.InstanceCompleteDefault, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceCompleteException(string message) : this(message, null)
        {
        }

        [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected InstanceCompleteException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceCompleteException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceCompleteException(XName commandName, Guid instanceId) : this(commandName, instanceId, null)
        {
        }

        public InstanceCompleteException(XName commandName, Guid instanceId, Exception innerException) : this(commandName, instanceId, ToMessage(instanceId), innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceCompleteException(XName commandName, Guid instanceId, string message, Exception innerException) : base(commandName, instanceId, message, innerException)
        {
        }

        private static string ToMessage(Guid instanceId)
        {
            if (instanceId != Guid.Empty)
            {
                return SRCore.InstanceCompleteSpecific(instanceId);
            }
            return SRCore.InstanceCompleteDefault;
        }
    }
}


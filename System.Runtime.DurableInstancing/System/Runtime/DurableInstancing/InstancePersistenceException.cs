namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Xml.Linq;

    [Serializable]
    public class InstancePersistenceException : Exception
    {
        private const string CommandNameName = "instancePersistenceCommandName";

        public InstancePersistenceException() : base(ToMessage(null))
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstancePersistenceException(string message) : base(message)
        {
        }

        public InstancePersistenceException(XName commandName) : this(commandName, ToMessage(commandName))
        {
        }

        [SecurityCritical]
        protected InstancePersistenceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.CommandName = info.GetValue("instancePersistenceCommandName", typeof(XName)) as XName;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstancePersistenceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InstancePersistenceException(XName commandName, Exception innerException) : this(commandName, ToMessage(commandName), innerException)
        {
        }

        public InstancePersistenceException(XName commandName, string message) : base(message)
        {
            this.CommandName = commandName;
        }

        public InstancePersistenceException(XName commandName, string message, Exception innerException) : base(message, innerException)
        {
            this.CommandName = commandName;
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("instancePersistenceCommandName", this.CommandName, typeof(XName));
        }

        private static string ToMessage(XName commandName)
        {
            if (commandName != null)
            {
                return SRCore.GenericInstanceCommand(commandName);
            }
            return SRCore.GenericInstanceCommandNull;
        }

        public XName CommandName
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<CommandName>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<CommandName>k__BackingField = value;
            }
        }
    }
}


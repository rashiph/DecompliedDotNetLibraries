namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EventLogNotFoundException : EventLogException
    {
        public EventLogNotFoundException()
        {
        }

        internal EventLogNotFoundException(int errorCode) : base(errorCode)
        {
        }

        public EventLogNotFoundException(string message) : base(message)
        {
        }

        protected EventLogNotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        public EventLogNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}


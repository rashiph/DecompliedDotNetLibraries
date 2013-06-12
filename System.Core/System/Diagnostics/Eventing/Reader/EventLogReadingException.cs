namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EventLogReadingException : EventLogException
    {
        public EventLogReadingException()
        {
        }

        internal EventLogReadingException(int errorCode) : base(errorCode)
        {
        }

        public EventLogReadingException(string message) : base(message)
        {
        }

        protected EventLogReadingException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        public EventLogReadingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}


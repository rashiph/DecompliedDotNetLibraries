namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EventLogInvalidDataException : EventLogException
    {
        public EventLogInvalidDataException()
        {
        }

        internal EventLogInvalidDataException(int errorCode) : base(errorCode)
        {
        }

        public EventLogInvalidDataException(string message) : base(message)
        {
        }

        protected EventLogInvalidDataException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        public EventLogInvalidDataException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}


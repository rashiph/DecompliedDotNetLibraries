namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EventLogProviderDisabledException : EventLogException
    {
        public EventLogProviderDisabledException()
        {
        }

        internal EventLogProviderDisabledException(int errorCode) : base(errorCode)
        {
        }

        public EventLogProviderDisabledException(string message) : base(message)
        {
        }

        protected EventLogProviderDisabledException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        public EventLogProviderDisabledException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}


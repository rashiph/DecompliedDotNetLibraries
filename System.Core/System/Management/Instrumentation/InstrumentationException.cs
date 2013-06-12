namespace System.Management.Instrumentation
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class InstrumentationException : InstrumentationBaseException
    {
        public InstrumentationException()
        {
        }

        public InstrumentationException(Exception innerException) : base(null, innerException)
        {
        }

        public InstrumentationException(string message) : base(message)
        {
        }

        protected InstrumentationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InstrumentationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}


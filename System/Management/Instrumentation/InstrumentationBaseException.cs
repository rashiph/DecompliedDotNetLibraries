namespace System.Management.Instrumentation
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class InstrumentationBaseException : Exception
    {
        public InstrumentationBaseException()
        {
        }

        public InstrumentationBaseException(string message) : base(message)
        {
        }

        protected InstrumentationBaseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InstrumentationBaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}


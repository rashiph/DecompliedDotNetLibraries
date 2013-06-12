namespace System.Management.Instrumentation
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class InstanceNotFoundException : InstrumentationException
    {
        public InstanceNotFoundException()
        {
        }

        public InstanceNotFoundException(string message) : base(message)
        {
        }

        protected InstanceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InstanceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}


namespace System.ComponentModel
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class InvalidAsynchronousStateException : ArgumentException
    {
        public InvalidAsynchronousStateException() : this(null)
        {
        }

        public InvalidAsynchronousStateException(string message) : base(message)
        {
        }

        protected InvalidAsynchronousStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidAsynchronousStateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}


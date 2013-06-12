namespace System
{
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089"), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class InvalidTimeZoneException : Exception
    {
        public InvalidTimeZoneException()
        {
        }

        public InvalidTimeZoneException(string message) : base(message)
        {
        }

        [SecuritySafeCritical]
        protected InvalidTimeZoneException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidTimeZoneException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}


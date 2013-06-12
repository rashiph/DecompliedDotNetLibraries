namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089"), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class LockRecursionException : Exception
    {
        public LockRecursionException()
        {
        }

        public LockRecursionException(string message) : base(message)
        {
        }

        protected LockRecursionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public LockRecursionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}


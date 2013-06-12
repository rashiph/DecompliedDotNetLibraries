namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ComVisible(false), TypeForwardedFrom("System, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089")]
    public class SemaphoreFullException : SystemException
    {
        public SemaphoreFullException() : base(Environment.GetResourceString("Threading_SemaphoreFullException"))
        {
        }

        public SemaphoreFullException(string message) : base(message)
        {
        }

        protected SemaphoreFullException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SemaphoreFullException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}


namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class TargetException : ApplicationException
    {
        public TargetException()
        {
            base.SetErrorCode(-2146232829);
        }

        public TargetException(string message) : base(message)
        {
            base.SetErrorCode(-2146232829);
        }

        [SecuritySafeCritical]
        protected TargetException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TargetException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146232829);
        }
    }
}


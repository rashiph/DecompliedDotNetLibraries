namespace System.Runtime.InteropServices
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class InvalidComObjectException : SystemException
    {
        public InvalidComObjectException() : base(Environment.GetResourceString("Arg_InvalidComObjectException"))
        {
            base.SetErrorCode(-2146233049);
        }

        public InvalidComObjectException(string message) : base(message)
        {
            base.SetErrorCode(-2146233049);
        }

        [SecuritySafeCritical]
        protected InvalidComObjectException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidComObjectException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233049);
        }
    }
}


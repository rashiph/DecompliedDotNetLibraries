namespace System.Runtime.InteropServices
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class SafeArrayTypeMismatchException : SystemException
    {
        public SafeArrayTypeMismatchException() : base(Environment.GetResourceString("Arg_SafeArrayTypeMismatchException"))
        {
            base.SetErrorCode(-2146233037);
        }

        public SafeArrayTypeMismatchException(string message) : base(message)
        {
            base.SetErrorCode(-2146233037);
        }

        [SecuritySafeCritical]
        protected SafeArrayTypeMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SafeArrayTypeMismatchException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233037);
        }
    }
}


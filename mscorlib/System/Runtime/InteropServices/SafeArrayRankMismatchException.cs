namespace System.Runtime.InteropServices
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class SafeArrayRankMismatchException : SystemException
    {
        public SafeArrayRankMismatchException() : base(Environment.GetResourceString("Arg_SafeArrayRankMismatchException"))
        {
            base.SetErrorCode(-2146233032);
        }

        public SafeArrayRankMismatchException(string message) : base(message)
        {
            base.SetErrorCode(-2146233032);
        }

        [SecuritySafeCritical]
        protected SafeArrayRankMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SafeArrayRankMismatchException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233032);
        }
    }
}


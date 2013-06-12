namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class ThreadStateException : SystemException
    {
        public ThreadStateException() : base(Environment.GetResourceString("Arg_ThreadStateException"))
        {
            base.SetErrorCode(-2146233056);
        }

        public ThreadStateException(string message) : base(message)
        {
            base.SetErrorCode(-2146233056);
        }

        [SecuritySafeCritical]
        protected ThreadStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ThreadStateException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146233056);
        }
    }
}


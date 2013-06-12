namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class ThreadInterruptedException : SystemException
    {
        public ThreadInterruptedException() : base(Exception.GetMessageFromNativeResources(Exception.ExceptionMessageKind.ThreadInterrupted))
        {
            base.SetErrorCode(-2146233063);
        }

        public ThreadInterruptedException(string message) : base(message)
        {
            base.SetErrorCode(-2146233063);
        }

        [SecuritySafeCritical]
        protected ThreadInterruptedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ThreadInterruptedException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146233063);
        }
    }
}


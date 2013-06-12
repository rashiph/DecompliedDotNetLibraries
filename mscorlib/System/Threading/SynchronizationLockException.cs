namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class SynchronizationLockException : SystemException
    {
        public SynchronizationLockException() : base(Environment.GetResourceString("Arg_SynchronizationLockException"))
        {
            base.SetErrorCode(-2146233064);
        }

        public SynchronizationLockException(string message) : base(message)
        {
            base.SetErrorCode(-2146233064);
        }

        [SecuritySafeCritical]
        protected SynchronizationLockException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SynchronizationLockException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146233064);
        }
    }
}


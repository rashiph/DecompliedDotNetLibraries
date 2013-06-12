namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(false)]
    public class WaitHandleCannotBeOpenedException : ApplicationException
    {
        public WaitHandleCannotBeOpenedException() : base(Environment.GetResourceString("Threading.WaitHandleCannotBeOpenedException"))
        {
            base.SetErrorCode(-2146233044);
        }

        public WaitHandleCannotBeOpenedException(string message) : base(message)
        {
            base.SetErrorCode(-2146233044);
        }

        [SecuritySafeCritical]
        protected WaitHandleCannotBeOpenedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public WaitHandleCannotBeOpenedException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146233044);
        }
    }
}


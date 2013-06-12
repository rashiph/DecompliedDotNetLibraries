namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class PlatformNotSupportedException : NotSupportedException
    {
        public PlatformNotSupportedException() : base(Environment.GetResourceString("Arg_PlatformNotSupported"))
        {
            base.SetErrorCode(-2146233031);
        }

        public PlatformNotSupportedException(string message) : base(message)
        {
            base.SetErrorCode(-2146233031);
        }

        [SecuritySafeCritical]
        protected PlatformNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public PlatformNotSupportedException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233031);
        }
    }
}


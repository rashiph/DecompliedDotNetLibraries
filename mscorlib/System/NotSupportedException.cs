namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class NotSupportedException : SystemException
    {
        public NotSupportedException() : base(Environment.GetResourceString("Arg_NotSupportedException"))
        {
            base.SetErrorCode(-2146233067);
        }

        public NotSupportedException(string message) : base(message)
        {
            base.SetErrorCode(-2146233067);
        }

        [SecuritySafeCritical]
        protected NotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public NotSupportedException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146233067);
        }
    }
}


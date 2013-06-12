namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class UnauthorizedAccessException : SystemException
    {
        public UnauthorizedAccessException() : base(Environment.GetResourceString("Arg_UnauthorizedAccessException"))
        {
            base.SetErrorCode(-2147024891);
        }

        public UnauthorizedAccessException(string message) : base(message)
        {
            base.SetErrorCode(-2147024891);
        }

        [SecuritySafeCritical]
        protected UnauthorizedAccessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public UnauthorizedAccessException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2147024891);
        }
    }
}


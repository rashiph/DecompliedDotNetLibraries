namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class InvalidCastException : SystemException
    {
        public InvalidCastException() : base(Environment.GetResourceString("Arg_InvalidCastException"))
        {
            base.SetErrorCode(-2147467262);
        }

        public InvalidCastException(string message) : base(message)
        {
            base.SetErrorCode(-2147467262);
        }

        [SecuritySafeCritical]
        protected InvalidCastException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidCastException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2147467262);
        }

        public InvalidCastException(string message, int errorCode) : base(message)
        {
            base.SetErrorCode(errorCode);
        }
    }
}


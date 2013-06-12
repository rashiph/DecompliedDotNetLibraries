namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class InvalidOperationException : SystemException
    {
        public InvalidOperationException() : base(Environment.GetResourceString("Arg_InvalidOperationException"))
        {
            base.SetErrorCode(-2146233079);
        }

        public InvalidOperationException(string message) : base(message)
        {
            base.SetErrorCode(-2146233079);
        }

        [SecuritySafeCritical]
        protected InvalidOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidOperationException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146233079);
        }
    }
}


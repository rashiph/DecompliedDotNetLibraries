namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class OverflowException : ArithmeticException
    {
        public OverflowException() : base(Environment.GetResourceString("Arg_OverflowException"))
        {
            base.SetErrorCode(-2146233066);
        }

        public OverflowException(string message) : base(message)
        {
            base.SetErrorCode(-2146233066);
        }

        [SecuritySafeCritical]
        protected OverflowException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public OverflowException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146233066);
        }
    }
}


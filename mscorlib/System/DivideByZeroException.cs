namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class DivideByZeroException : ArithmeticException
    {
        public DivideByZeroException() : base(Environment.GetResourceString("Arg_DivideByZero"))
        {
            base.SetErrorCode(-2147352558);
        }

        public DivideByZeroException(string message) : base(message)
        {
            base.SetErrorCode(-2147352558);
        }

        [SecuritySafeCritical]
        protected DivideByZeroException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DivideByZeroException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2147352558);
        }
    }
}


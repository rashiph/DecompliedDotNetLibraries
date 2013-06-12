namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class ArithmeticException : SystemException
    {
        public ArithmeticException() : base(Environment.GetResourceString("Arg_ArithmeticException"))
        {
            base.SetErrorCode(-2147024362);
        }

        public ArithmeticException(string message) : base(message)
        {
            base.SetErrorCode(-2147024362);
        }

        [SecuritySafeCritical]
        protected ArithmeticException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ArithmeticException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2147024362);
        }
    }
}


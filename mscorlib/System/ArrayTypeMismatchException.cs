namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class ArrayTypeMismatchException : SystemException
    {
        public ArrayTypeMismatchException() : base(Environment.GetResourceString("Arg_ArrayTypeMismatchException"))
        {
            base.SetErrorCode(-2146233085);
        }

        public ArrayTypeMismatchException(string message) : base(message)
        {
            base.SetErrorCode(-2146233085);
        }

        [SecuritySafeCritical]
        protected ArrayTypeMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ArrayTypeMismatchException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146233085);
        }
    }
}


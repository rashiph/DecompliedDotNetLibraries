namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class FormatException : SystemException
    {
        public FormatException() : base(Environment.GetResourceString("Arg_FormatException"))
        {
            base.SetErrorCode(-2146233033);
        }

        public FormatException(string message) : base(message)
        {
            base.SetErrorCode(-2146233033);
        }

        [SecuritySafeCritical]
        protected FormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public FormatException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146233033);
        }
    }
}


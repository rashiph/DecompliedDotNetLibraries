namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class NullReferenceException : SystemException
    {
        public NullReferenceException() : base(Environment.GetResourceString("Arg_NullReferenceException"))
        {
            base.SetErrorCode(-2147467261);
        }

        public NullReferenceException(string message) : base(message)
        {
            base.SetErrorCode(-2147467261);
        }

        [SecuritySafeCritical]
        protected NullReferenceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public NullReferenceException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2147467261);
        }
    }
}


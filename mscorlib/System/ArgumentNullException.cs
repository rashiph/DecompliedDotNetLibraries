namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class ArgumentNullException : ArgumentException
    {
        public ArgumentNullException() : base(Environment.GetResourceString("ArgumentNull_Generic"))
        {
            base.SetErrorCode(-2147467261);
        }

        public ArgumentNullException(string paramName) : base(Environment.GetResourceString("ArgumentNull_Generic"), paramName)
        {
            base.SetErrorCode(-2147467261);
        }

        [SecurityCritical]
        protected ArgumentNullException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ArgumentNullException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2147467261);
        }

        public ArgumentNullException(string paramName, string message) : base(message, paramName)
        {
            base.SetErrorCode(-2147467261);
        }
    }
}


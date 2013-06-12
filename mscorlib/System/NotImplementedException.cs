namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class NotImplementedException : SystemException
    {
        public NotImplementedException() : base(Environment.GetResourceString("Arg_NotImplementedException"))
        {
            base.SetErrorCode(-2147467263);
        }

        public NotImplementedException(string message) : base(message)
        {
            base.SetErrorCode(-2147467263);
        }

        [SecuritySafeCritical]
        protected NotImplementedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public NotImplementedException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2147467263);
        }
    }
}


namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class ContextMarshalException : SystemException
    {
        public ContextMarshalException() : base(Environment.GetResourceString("Arg_ContextMarshalException"))
        {
            base.SetErrorCode(-2146233084);
        }

        public ContextMarshalException(string message) : base(message)
        {
            base.SetErrorCode(-2146233084);
        }

        [SecuritySafeCritical]
        protected ContextMarshalException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ContextMarshalException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233084);
        }
    }
}


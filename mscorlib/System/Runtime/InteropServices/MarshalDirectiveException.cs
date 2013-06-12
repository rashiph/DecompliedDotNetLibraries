namespace System.Runtime.InteropServices
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class MarshalDirectiveException : SystemException
    {
        public MarshalDirectiveException() : base(Environment.GetResourceString("Arg_MarshalDirectiveException"))
        {
            base.SetErrorCode(-2146233035);
        }

        public MarshalDirectiveException(string message) : base(message)
        {
            base.SetErrorCode(-2146233035);
        }

        [SecuritySafeCritical]
        protected MarshalDirectiveException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MarshalDirectiveException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233035);
        }
    }
}


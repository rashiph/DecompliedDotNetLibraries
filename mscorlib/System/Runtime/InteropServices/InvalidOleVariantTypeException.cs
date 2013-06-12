namespace System.Runtime.InteropServices
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class InvalidOleVariantTypeException : SystemException
    {
        public InvalidOleVariantTypeException() : base(Environment.GetResourceString("Arg_InvalidOleVariantTypeException"))
        {
            base.SetErrorCode(-2146233039);
        }

        public InvalidOleVariantTypeException(string message) : base(message)
        {
            base.SetErrorCode(-2146233039);
        }

        [SecuritySafeCritical]
        protected InvalidOleVariantTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidOleVariantTypeException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233039);
        }
    }
}


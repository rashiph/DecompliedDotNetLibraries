namespace System.Collections.Generic
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class KeyNotFoundException : SystemException, ISerializable
    {
        public KeyNotFoundException() : base(Environment.GetResourceString("Arg_KeyNotFound"))
        {
            base.SetErrorCode(-2146232969);
        }

        public KeyNotFoundException(string message) : base(message)
        {
            base.SetErrorCode(-2146232969);
        }

        [SecuritySafeCritical]
        protected KeyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public KeyNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146232969);
        }
    }
}


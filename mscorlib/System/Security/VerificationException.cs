namespace System.Security
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ComVisible(true)]
    public class VerificationException : SystemException
    {
        public VerificationException() : base(Environment.GetResourceString("Verification_Exception"))
        {
            base.SetErrorCode(-2146233075);
        }

        public VerificationException(string message) : base(message)
        {
            base.SetErrorCode(-2146233075);
        }

        [SecuritySafeCritical]
        protected VerificationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public VerificationException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146233075);
        }
    }
}


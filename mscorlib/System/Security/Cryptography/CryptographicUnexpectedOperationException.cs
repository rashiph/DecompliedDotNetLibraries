namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class CryptographicUnexpectedOperationException : CryptographicException
    {
        public CryptographicUnexpectedOperationException()
        {
            base.SetErrorCode(-2146233295);
        }

        public CryptographicUnexpectedOperationException(string message) : base(message)
        {
            base.SetErrorCode(-2146233295);
        }

        [SecuritySafeCritical]
        protected CryptographicUnexpectedOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CryptographicUnexpectedOperationException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233295);
        }

        public CryptographicUnexpectedOperationException(string format, string insert) : base(string.Format(CultureInfo.CurrentCulture, format, new object[] { insert }))
        {
            base.SetErrorCode(-2146233295);
        }
    }
}


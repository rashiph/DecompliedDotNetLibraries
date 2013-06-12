namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class CryptographicException : SystemException
    {
        private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;

        public CryptographicException() : base(Environment.GetResourceString("Arg_CryptographyException"))
        {
            base.SetErrorCode(-2146233296);
        }

        [SecuritySafeCritical]
        public CryptographicException(int hr) : this(Win32Native.GetMessage(hr))
        {
            if ((hr & 0x80000000L) != 0x80000000L)
            {
                hr = (hr & 0xffff) | -2147024896;
            }
            base.SetErrorCode(hr);
        }

        public CryptographicException(string message) : base(message)
        {
            base.SetErrorCode(-2146233296);
        }

        [SecuritySafeCritical]
        protected CryptographicException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CryptographicException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233296);
        }

        public CryptographicException(string format, string insert) : base(string.Format(CultureInfo.CurrentCulture, format, new object[] { insert }))
        {
            base.SetErrorCode(-2146233296);
        }

        private static void ThrowCryptographicException(int hr)
        {
            throw new CryptographicException(hr);
        }
    }
}


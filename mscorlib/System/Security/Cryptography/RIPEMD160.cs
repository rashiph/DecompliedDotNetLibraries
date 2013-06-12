namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public abstract class RIPEMD160 : HashAlgorithm
    {
        protected RIPEMD160()
        {
            base.HashSizeValue = 160;
        }

        [SecuritySafeCritical]
        public static RIPEMD160 Create()
        {
            return Create("System.Security.Cryptography.RIPEMD160");
        }

        [SecuritySafeCritical]
        public static RIPEMD160 Create(string hashName)
        {
            return (RIPEMD160) CryptoConfig.CreateFromName(hashName);
        }
    }
}


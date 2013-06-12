namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public abstract class SHA512 : HashAlgorithm
    {
        protected SHA512()
        {
            base.HashSizeValue = 0x200;
        }

        [SecuritySafeCritical]
        public static SHA512 Create()
        {
            return Create("System.Security.Cryptography.SHA512");
        }

        [SecuritySafeCritical]
        public static SHA512 Create(string hashName)
        {
            return (SHA512) CryptoConfig.CreateFromName(hashName);
        }
    }
}


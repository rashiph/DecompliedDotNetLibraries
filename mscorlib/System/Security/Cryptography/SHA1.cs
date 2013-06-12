namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public abstract class SHA1 : HashAlgorithm
    {
        protected SHA1()
        {
            base.HashSizeValue = 160;
        }

        [SecuritySafeCritical]
        public static SHA1 Create()
        {
            return Create("System.Security.Cryptography.SHA1");
        }

        [SecuritySafeCritical]
        public static SHA1 Create(string hashName)
        {
            return (SHA1) CryptoConfig.CreateFromName(hashName);
        }
    }
}


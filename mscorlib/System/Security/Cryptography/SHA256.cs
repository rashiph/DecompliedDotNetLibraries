namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public abstract class SHA256 : HashAlgorithm
    {
        protected SHA256()
        {
            base.HashSizeValue = 0x100;
        }

        [SecuritySafeCritical]
        public static SHA256 Create()
        {
            return Create("System.Security.Cryptography.SHA256");
        }

        [SecuritySafeCritical]
        public static SHA256 Create(string hashName)
        {
            return (SHA256) CryptoConfig.CreateFromName(hashName);
        }
    }
}


namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public abstract class SHA384 : HashAlgorithm
    {
        protected SHA384()
        {
            base.HashSizeValue = 0x180;
        }

        [SecuritySafeCritical]
        public static SHA384 Create()
        {
            return Create("System.Security.Cryptography.SHA384");
        }

        [SecuritySafeCritical]
        public static SHA384 Create(string hashName)
        {
            return (SHA384) CryptoConfig.CreateFromName(hashName);
        }
    }
}


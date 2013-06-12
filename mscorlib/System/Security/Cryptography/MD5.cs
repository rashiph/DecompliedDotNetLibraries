namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public abstract class MD5 : HashAlgorithm
    {
        protected MD5()
        {
            base.HashSizeValue = 0x80;
        }

        [SecuritySafeCritical]
        public static MD5 Create()
        {
            return Create("System.Security.Cryptography.MD5");
        }

        [SecuritySafeCritical]
        public static MD5 Create(string algName)
        {
            return (MD5) CryptoConfig.CreateFromName(algName);
        }
    }
}


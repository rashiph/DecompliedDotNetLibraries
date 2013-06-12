namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class HMACSHA256 : HMAC
    {
        public HMACSHA256() : this(Utils.GenerateRandom(0x40))
        {
        }

        public HMACSHA256(byte[] key)
        {
            base.m_hashName = "SHA256";
            base.m_hash1 = new SHA256Managed();
            base.m_hash2 = new SHA256Managed();
            base.HashSizeValue = 0x100;
            base.InitializeKey(key);
        }
    }
}


namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class HMACMD5 : HMAC
    {
        public HMACMD5() : this(Utils.GenerateRandom(0x40))
        {
        }

        public HMACMD5(byte[] key)
        {
            base.m_hashName = "MD5";
            base.m_hash1 = new MD5CryptoServiceProvider();
            base.m_hash2 = new MD5CryptoServiceProvider();
            base.HashSizeValue = 0x80;
            base.InitializeKey(key);
        }
    }
}


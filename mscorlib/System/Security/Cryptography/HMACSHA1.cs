namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class HMACSHA1 : HMAC
    {
        public HMACSHA1() : this(Utils.GenerateRandom(0x40))
        {
        }

        public HMACSHA1(byte[] key) : this(key, false)
        {
        }

        public HMACSHA1(byte[] key, bool useManagedSha1)
        {
            base.m_hashName = "SHA1";
            if (useManagedSha1)
            {
                base.m_hash1 = new SHA1Managed();
                base.m_hash2 = new SHA1Managed();
            }
            else
            {
                base.m_hash1 = new SHA1CryptoServiceProvider();
                base.m_hash2 = new SHA1CryptoServiceProvider();
            }
            base.HashSizeValue = 160;
            base.InitializeKey(key);
        }
    }
}


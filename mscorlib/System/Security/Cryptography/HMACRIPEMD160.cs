namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class HMACRIPEMD160 : HMAC
    {
        public HMACRIPEMD160() : this(Utils.GenerateRandom(0x40))
        {
        }

        public HMACRIPEMD160(byte[] key)
        {
            base.m_hashName = "RIPEMD160";
            base.m_hash1 = new RIPEMD160Managed();
            base.m_hash2 = new RIPEMD160Managed();
            base.HashSizeValue = 160;
            base.InitializeKey(key);
        }
    }
}


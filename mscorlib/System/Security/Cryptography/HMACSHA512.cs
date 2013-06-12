namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class HMACSHA512 : HMAC
    {
        private bool m_useLegacyBlockSize;

        public HMACSHA512() : this(Utils.GenerateRandom(0x80))
        {
        }

        [SecuritySafeCritical]
        public HMACSHA512(byte[] key)
        {
            this.m_useLegacyBlockSize = Utils._ProduceLegacyHmacValues();
            base.m_hashName = "SHA512";
            base.m_hash1 = new SHA512Managed();
            base.m_hash2 = new SHA512Managed();
            base.HashSizeValue = 0x200;
            base.BlockSizeValue = this.BlockSize;
            base.InitializeKey(key);
        }

        private int BlockSize
        {
            get
            {
                if (!this.m_useLegacyBlockSize)
                {
                    return 0x80;
                }
                return 0x40;
            }
        }

        public bool ProduceLegacyHmacValues
        {
            get
            {
                return this.m_useLegacyBlockSize;
            }
            set
            {
                this.m_useLegacyBlockSize = value;
                base.BlockSizeValue = this.BlockSize;
                base.InitializeKey(base.KeyValue);
            }
        }
    }
}


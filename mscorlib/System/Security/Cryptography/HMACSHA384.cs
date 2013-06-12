namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class HMACSHA384 : HMAC
    {
        private bool m_useLegacyBlockSize;

        public HMACSHA384() : this(Utils.GenerateRandom(0x80))
        {
        }

        [SecuritySafeCritical]
        public HMACSHA384(byte[] key)
        {
            this.m_useLegacyBlockSize = Utils._ProduceLegacyHmacValues();
            base.m_hashName = "SHA384";
            base.m_hash1 = new SHA384Managed();
            base.m_hash2 = new SHA384Managed();
            base.HashSizeValue = 0x180;
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


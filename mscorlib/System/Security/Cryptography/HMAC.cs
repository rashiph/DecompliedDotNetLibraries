namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public abstract class HMAC : KeyedHashAlgorithm
    {
        private int blockSizeValue = 0x40;
        internal HashAlgorithm m_hash1;
        internal HashAlgorithm m_hash2;
        private bool m_hashing;
        internal string m_hashName;
        private byte[] m_inner;
        private byte[] m_outer;

        protected HMAC()
        {
        }

        [SecuritySafeCritical]
        public static HMAC Create()
        {
            return Create("System.Security.Cryptography.HMAC");
        }

        [SecuritySafeCritical]
        public static HMAC Create(string algorithmName)
        {
            return (HMAC) CryptoConfig.CreateFromName(algorithmName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.m_hash1 != null)
                {
                    this.m_hash1.Dispose();
                }
                if (this.m_hash2 != null)
                {
                    this.m_hash2.Dispose();
                }
                if (this.m_inner != null)
                {
                    Array.Clear(this.m_inner, 0, this.m_inner.Length);
                }
                if (this.m_outer != null)
                {
                    Array.Clear(this.m_outer, 0, this.m_outer.Length);
                }
            }
            base.Dispose(disposing);
        }

        protected override void HashCore(byte[] rgb, int ib, int cb)
        {
            if (!this.m_hashing)
            {
                this.m_hash1.TransformBlock(this.m_inner, 0, this.m_inner.Length, this.m_inner, 0);
                this.m_hashing = true;
            }
            this.m_hash1.TransformBlock(rgb, ib, cb, rgb, ib);
        }

        protected override byte[] HashFinal()
        {
            if (!this.m_hashing)
            {
                this.m_hash1.TransformBlock(this.m_inner, 0, this.m_inner.Length, this.m_inner, 0);
                this.m_hashing = true;
            }
            this.m_hash1.TransformFinalBlock(new byte[0], 0, 0);
            byte[] hashValue = this.m_hash1.HashValue;
            this.m_hash2.TransformBlock(this.m_outer, 0, this.m_outer.Length, this.m_outer, 0);
            this.m_hash2.TransformBlock(hashValue, 0, hashValue.Length, hashValue, 0);
            this.m_hashing = false;
            this.m_hash2.TransformFinalBlock(new byte[0], 0, 0);
            return this.m_hash2.HashValue;
        }

        public override void Initialize()
        {
            this.m_hash1.Initialize();
            this.m_hash2.Initialize();
            this.m_hashing = false;
        }

        internal void InitializeKey(byte[] key)
        {
            this.m_inner = null;
            this.m_outer = null;
            if (key.Length > this.BlockSizeValue)
            {
                base.KeyValue = this.m_hash1.ComputeHash(key);
            }
            else
            {
                base.KeyValue = (byte[]) key.Clone();
            }
            this.UpdateIOPadBuffers();
        }

        private void UpdateIOPadBuffers()
        {
            int num;
            if (this.m_inner == null)
            {
                this.m_inner = new byte[this.BlockSizeValue];
            }
            if (this.m_outer == null)
            {
                this.m_outer = new byte[this.BlockSizeValue];
            }
            for (num = 0; num < this.BlockSizeValue; num++)
            {
                this.m_inner[num] = 0x36;
                this.m_outer[num] = 0x5c;
            }
            for (num = 0; num < base.KeyValue.Length; num++)
            {
                this.m_inner[num] = (byte) (this.m_inner[num] ^ base.KeyValue[num]);
                this.m_outer[num] = (byte) (this.m_outer[num] ^ base.KeyValue[num]);
            }
        }

        protected int BlockSizeValue
        {
            get
            {
                return this.blockSizeValue;
            }
            set
            {
                this.blockSizeValue = value;
            }
        }

        public string HashName
        {
            get
            {
                return this.m_hashName;
            }
            [SecuritySafeCritical]
            set
            {
                if (this.m_hashing)
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_HashNameSet"));
                }
                this.m_hashName = value;
                this.m_hash1 = HashAlgorithm.Create(this.m_hashName);
                this.m_hash2 = HashAlgorithm.Create(this.m_hashName);
            }
        }

        public override byte[] Key
        {
            get
            {
                return (byte[]) base.KeyValue.Clone();
            }
            set
            {
                if (this.m_hashing)
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_HashKeySet"));
                }
                this.InitializeKey(value);
            }
        }
    }
}


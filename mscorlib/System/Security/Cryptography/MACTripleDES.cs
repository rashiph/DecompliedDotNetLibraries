namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class MACTripleDES : KeyedHashAlgorithm
    {
        private CryptoStream _cs;
        private TailStream _ts;
        private TripleDES des;
        private const int m_bitsPerByte = 8;
        private int m_bytesPerBlock;
        private ICryptoTransform m_encryptor;

        [SecuritySafeCritical]
        public MACTripleDES()
        {
            base.KeyValue = new byte[0x18];
            Utils.StaticRandomNumberGenerator.GetBytes(base.KeyValue);
            this.des = TripleDES.Create();
            base.HashSizeValue = this.des.BlockSize;
            this.m_bytesPerBlock = this.des.BlockSize / 8;
            this.des.IV = new byte[this.m_bytesPerBlock];
            this.des.Padding = PaddingMode.Zeros;
            this.m_encryptor = null;
        }

        [SecuritySafeCritical]
        public MACTripleDES(byte[] rgbKey) : this("System.Security.Cryptography.TripleDES", rgbKey)
        {
        }

        [SecuritySafeCritical]
        public MACTripleDES(string strTripleDES, byte[] rgbKey)
        {
            if (rgbKey == null)
            {
                throw new ArgumentNullException("rgbKey");
            }
            if (strTripleDES == null)
            {
                this.des = TripleDES.Create();
            }
            else
            {
                this.des = TripleDES.Create(strTripleDES);
            }
            base.HashSizeValue = this.des.BlockSize;
            base.KeyValue = (byte[]) rgbKey.Clone();
            this.m_bytesPerBlock = this.des.BlockSize / 8;
            this.des.IV = new byte[this.m_bytesPerBlock];
            this.des.Padding = PaddingMode.Zeros;
            this.m_encryptor = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.des != null)
                {
                    this.des.Clear();
                }
                if (this.m_encryptor != null)
                {
                    this.m_encryptor.Dispose();
                }
                if (this._cs != null)
                {
                    this._cs.Clear();
                }
                if (this._ts != null)
                {
                    this._ts.Clear();
                }
            }
            base.Dispose(disposing);
        }

        protected override void HashCore(byte[] rgbData, int ibStart, int cbSize)
        {
            if (this.m_encryptor == null)
            {
                this.des.Key = this.Key;
                this.m_encryptor = this.des.CreateEncryptor();
                this._ts = new TailStream(this.des.BlockSize / 8);
                this._cs = new CryptoStream(this._ts, this.m_encryptor, CryptoStreamMode.Write);
            }
            this._cs.Write(rgbData, ibStart, cbSize);
        }

        protected override byte[] HashFinal()
        {
            if (this.m_encryptor == null)
            {
                this.des.Key = this.Key;
                this.m_encryptor = this.des.CreateEncryptor();
                this._ts = new TailStream(this.des.BlockSize / 8);
                this._cs = new CryptoStream(this._ts, this.m_encryptor, CryptoStreamMode.Write);
            }
            this._cs.FlushFinalBlock();
            return this._ts.Buffer;
        }

        public override void Initialize()
        {
            this.m_encryptor = null;
        }

        [ComVisible(false)]
        public PaddingMode Padding
        {
            get
            {
                return this.des.Padding;
            }
            set
            {
                if ((value < PaddingMode.None) || (PaddingMode.ISO10126 < value))
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidPaddingMode"));
                }
                this.des.Padding = value;
            }
        }
    }
}


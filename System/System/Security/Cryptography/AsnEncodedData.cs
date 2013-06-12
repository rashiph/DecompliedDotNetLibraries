namespace System.Security.Cryptography
{
    using System;

    public class AsnEncodedData
    {
        internal System.Security.Cryptography.Oid m_oid;
        internal byte[] m_rawData;

        protected AsnEncodedData()
        {
        }

        public AsnEncodedData(AsnEncodedData asnEncodedData)
        {
            if (asnEncodedData == null)
            {
                throw new ArgumentNullException("asnEncodedData");
            }
            this.Reset(asnEncodedData.m_oid, asnEncodedData.m_rawData);
        }

        internal AsnEncodedData(System.Security.Cryptography.Oid oid)
        {
            this.m_oid = oid;
        }

        public AsnEncodedData(byte[] rawData)
        {
            this.Reset(null, rawData);
        }

        internal AsnEncodedData(System.Security.Cryptography.Oid oid, CAPIBase.CRYPTOAPI_BLOB encodedBlob) : this(oid, CAPI.BlobToByteArray(encodedBlob))
        {
        }

        public AsnEncodedData(System.Security.Cryptography.Oid oid, byte[] rawData)
        {
            this.Reset(oid, rawData);
        }

        internal AsnEncodedData(string oid, CAPIBase.CRYPTOAPI_BLOB encodedBlob) : this(oid, CAPI.BlobToByteArray(encodedBlob))
        {
        }

        public AsnEncodedData(string oid, byte[] rawData)
        {
            this.Reset(new System.Security.Cryptography.Oid(oid), rawData);
        }

        public virtual void CopyFrom(AsnEncodedData asnEncodedData)
        {
            if (asnEncodedData == null)
            {
                throw new ArgumentNullException("asnEncodedData");
            }
            this.Reset(asnEncodedData.m_oid, asnEncodedData.m_rawData);
        }

        public virtual string Format(bool multiLine)
        {
            if ((this.m_rawData == null) || (this.m_rawData.Length == 0))
            {
                return string.Empty;
            }
            string lpszStructType = string.Empty;
            if ((this.m_oid != null) && (this.m_oid.Value != null))
            {
                lpszStructType = this.m_oid.Value;
            }
            return CAPI.CryptFormatObject(1, multiLine ? 1 : 0, lpszStructType, this.m_rawData);
        }

        private void Reset(System.Security.Cryptography.Oid oid, byte[] rawData)
        {
            this.Oid = oid;
            this.RawData = rawData;
        }

        public System.Security.Cryptography.Oid Oid
        {
            get
            {
                return this.m_oid;
            }
            set
            {
                if (value == null)
                {
                    this.m_oid = null;
                }
                else
                {
                    this.m_oid = new System.Security.Cryptography.Oid(value);
                }
            }
        }

        public byte[] RawData
        {
            get
            {
                return this.m_rawData;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_rawData = (byte[]) value.Clone();
            }
        }
    }
}


namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    public sealed class X509KeyUsageExtension : X509Extension
    {
        private bool m_decoded;
        private uint m_keyUsages;

        public X509KeyUsageExtension() : base("2.5.29.15")
        {
            this.m_decoded = true;
        }

        public X509KeyUsageExtension(AsnEncodedData encodedKeyUsage, bool critical) : base("2.5.29.15", encodedKeyUsage.RawData, critical)
        {
        }

        public X509KeyUsageExtension(X509KeyUsageFlags keyUsages, bool critical) : base("2.5.29.15", EncodeExtension(keyUsages), critical)
        {
        }

        public override void CopyFrom(AsnEncodedData asnEncodedData)
        {
            base.CopyFrom(asnEncodedData);
            this.m_decoded = false;
        }

        private void DecodeExtension()
        {
            uint cbDecodedValue = 0;
            SafeLocalAllocHandle decodedValue = null;
            if (!CAPI.DecodeObject(new IntPtr(14L), base.m_rawData, out decodedValue, out cbDecodedValue))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            CAPIBase.CRYPTOAPI_BLOB cryptoapi_blob = (CAPIBase.CRYPTOAPI_BLOB) Marshal.PtrToStructure(decodedValue.DangerousGetHandle(), typeof(CAPIBase.CRYPTOAPI_BLOB));
            if (cryptoapi_blob.cbData > 4)
            {
                cryptoapi_blob.cbData = 4;
            }
            byte[] destination = new byte[4];
            if (cryptoapi_blob.pbData != IntPtr.Zero)
            {
                Marshal.Copy(cryptoapi_blob.pbData, destination, 0, (int) cryptoapi_blob.cbData);
            }
            this.m_keyUsages = BitConverter.ToUInt32(destination, 0);
            this.m_decoded = true;
            decodedValue.Dispose();
        }

        private static unsafe byte[] EncodeExtension(X509KeyUsageFlags keyUsages)
        {
            CAPIBase.CRYPT_BIT_BLOB crypt_bit_blob = new CAPIBase.CRYPT_BIT_BLOB {
                cbData = 2,
                pbData = new IntPtr((void*) &keyUsages),
                cUnusedBits = 0
            };
            byte[] encodedData = null;
            if (!CAPI.EncodeObject("2.5.29.15", new IntPtr((void*) &crypt_bit_blob), out encodedData))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            return encodedData;
        }

        public X509KeyUsageFlags KeyUsages
        {
            get
            {
                if (!this.m_decoded)
                {
                    this.DecodeExtension();
                }
                return (X509KeyUsageFlags) this.m_keyUsages;
            }
        }
    }
}


namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    public sealed class X509EnhancedKeyUsageExtension : X509Extension
    {
        private bool m_decoded;
        private OidCollection m_enhancedKeyUsages;

        public X509EnhancedKeyUsageExtension() : base("2.5.29.37")
        {
            this.m_enhancedKeyUsages = new OidCollection();
            this.m_decoded = true;
        }

        public X509EnhancedKeyUsageExtension(AsnEncodedData encodedEnhancedKeyUsages, bool critical) : base("2.5.29.37", encodedEnhancedKeyUsages.RawData, critical)
        {
        }

        public X509EnhancedKeyUsageExtension(OidCollection enhancedKeyUsages, bool critical) : base("2.5.29.37", EncodeExtension(enhancedKeyUsages), critical)
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
            if (!CAPI.DecodeObject(new IntPtr(0x24L), base.m_rawData, out decodedValue, out cbDecodedValue))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            CAPIBase.CERT_ENHKEY_USAGE cert_enhkey_usage = (CAPIBase.CERT_ENHKEY_USAGE) Marshal.PtrToStructure(decodedValue.DangerousGetHandle(), typeof(CAPIBase.CERT_ENHKEY_USAGE));
            this.m_enhancedKeyUsages = new OidCollection();
            for (int i = 0; i < cert_enhkey_usage.cUsageIdentifier; i++)
            {
                Oid oid = new Oid(Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(new IntPtr(((long) cert_enhkey_usage.rgpszUsageIdentifier) + (i * Marshal.SizeOf(typeof(IntPtr)))))), System.Security.Cryptography.OidGroup.ExtensionOrAttribute, false);
                this.m_enhancedKeyUsages.Add(oid);
            }
            this.m_decoded = true;
            decodedValue.Dispose();
        }

        private static unsafe byte[] EncodeExtension(OidCollection enhancedKeyUsages)
        {
            if (enhancedKeyUsages == null)
            {
                throw new ArgumentNullException("enhancedKeyUsages");
            }
            SafeLocalAllocHandle handle = System.Security.Cryptography.X509Certificates.X509Utils.CopyOidsToUnmanagedMemory(enhancedKeyUsages);
            byte[] encodedData = null;
            using (handle)
            {
                CAPIBase.CERT_ENHKEY_USAGE cert_enhkey_usage = new CAPIBase.CERT_ENHKEY_USAGE {
                    cUsageIdentifier = (uint) enhancedKeyUsages.Count,
                    rgpszUsageIdentifier = handle.DangerousGetHandle()
                };
                if (!CAPI.EncodeObject("2.5.29.37", new IntPtr((void*) &cert_enhkey_usage), out encodedData))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
            return encodedData;
        }

        public OidCollection EnhancedKeyUsages
        {
            get
            {
                if (!this.m_decoded)
                {
                    this.DecodeExtension();
                }
                OidCollection oids = new OidCollection();
                OidEnumerator enumerator = this.m_enhancedKeyUsages.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Oid current = enumerator.Current;
                    oids.Add(current);
                }
                return oids;
            }
        }
    }
}


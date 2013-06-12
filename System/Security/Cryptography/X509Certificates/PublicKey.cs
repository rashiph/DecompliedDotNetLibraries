namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    public sealed class PublicKey
    {
        private uint m_aiPubKey;
        private byte[] m_cspBlobData;
        private AsnEncodedData m_encodedKeyValue;
        private AsnEncodedData m_encodedParameters;
        private AsymmetricAlgorithm m_key;
        private System.Security.Cryptography.Oid m_oid;

        private PublicKey()
        {
        }

        internal PublicKey(PublicKey publicKey)
        {
            this.m_oid = new System.Security.Cryptography.Oid(publicKey.m_oid);
            this.m_encodedParameters = new AsnEncodedData(publicKey.m_encodedParameters);
            this.m_encodedKeyValue = new AsnEncodedData(publicKey.m_encodedKeyValue);
        }

        public PublicKey(System.Security.Cryptography.Oid oid, AsnEncodedData parameters, AsnEncodedData keyValue)
        {
            this.m_oid = new System.Security.Cryptography.Oid(oid);
            this.m_encodedParameters = new AsnEncodedData(parameters);
            this.m_encodedKeyValue = new AsnEncodedData(keyValue);
        }

        private static byte[] ConstructDSSPubKeyCspBlob(SafeLocalAllocHandle decodedKeyValue, SafeLocalAllocHandle decodedParameters)
        {
            CAPIBase.CRYPTOAPI_BLOB cryptoapi_blob = (CAPIBase.CRYPTOAPI_BLOB) Marshal.PtrToStructure(decodedKeyValue.DangerousGetHandle(), typeof(CAPIBase.CRYPTOAPI_BLOB));
            CAPIBase.CERT_DSS_PARAMETERS cert_dss_parameters = (CAPIBase.CERT_DSS_PARAMETERS) Marshal.PtrToStructure(decodedParameters.DangerousGetHandle(), typeof(CAPIBase.CERT_DSS_PARAMETERS));
            uint cbData = cert_dss_parameters.p.cbData;
            if (cbData == 0)
            {
                throw new CryptographicException(-2146893803);
            }
            uint num2 = ((((0x10 + cbData) + 20) + cbData) + cbData) + 0x18;
            MemoryStream output = new MemoryStream((int) num2);
            BinaryWriter writer = new BinaryWriter(output);
            writer.Write((byte) 6);
            writer.Write((byte) 2);
            writer.Write((short) 0);
            writer.Write((uint) 0x2200);
            writer.Write((uint) 0x31535344);
            writer.Write((uint) (cbData * 8));
            byte[] destination = new byte[cert_dss_parameters.p.cbData];
            Marshal.Copy(cert_dss_parameters.p.pbData, destination, 0, destination.Length);
            writer.Write(destination);
            uint num3 = cert_dss_parameters.q.cbData;
            if ((num3 == 0) || (num3 > 20))
            {
                throw new CryptographicException(-2146893803);
            }
            byte[] buffer2 = new byte[cert_dss_parameters.q.cbData];
            Marshal.Copy(cert_dss_parameters.q.pbData, buffer2, 0, buffer2.Length);
            writer.Write(buffer2);
            if (20 > num3)
            {
                writer.Write(new byte[20 - num3]);
            }
            num3 = cert_dss_parameters.g.cbData;
            if ((num3 == 0) || (num3 > cbData))
            {
                throw new CryptographicException(-2146893803);
            }
            byte[] buffer3 = new byte[cert_dss_parameters.g.cbData];
            Marshal.Copy(cert_dss_parameters.g.pbData, buffer3, 0, buffer3.Length);
            writer.Write(buffer3);
            if (cbData > num3)
            {
                writer.Write(new byte[cbData - num3]);
            }
            num3 = cryptoapi_blob.cbData;
            if ((num3 == 0) || (num3 > cbData))
            {
                throw new CryptographicException(-2146893803);
            }
            byte[] buffer4 = new byte[cryptoapi_blob.cbData];
            Marshal.Copy(cryptoapi_blob.pbData, buffer4, 0, buffer4.Length);
            writer.Write(buffer4);
            if (cbData > num3)
            {
                writer.Write(new byte[cbData - num3]);
            }
            writer.Write(uint.MaxValue);
            writer.Write(new byte[20]);
            return output.ToArray();
        }

        private static void DecodePublicKeyObject(uint aiPubKey, byte[] encodedKeyValue, byte[] encodedParameters, out byte[] decodedData)
        {
            decodedData = null;
            IntPtr zero = IntPtr.Zero;
            switch (aiPubKey)
            {
                case 0xaa01:
                case 0xaa02:
                    throw new NotSupportedException(SR.GetString("NotSupported_KeyAlgorithm"));

                case 0xa400:
                case 0x2400:
                    zero = new IntPtr(0x13L);
                    break;

                case 0x2200:
                    zero = new IntPtr(0x26L);
                    break;

                default:
                    throw new NotSupportedException(SR.GetString("NotSupported_KeyAlgorithm"));
            }
            SafeLocalAllocHandle decodedValue = null;
            uint cbDecodedValue = 0;
            if (!CAPI.DecodeObject(zero, encodedKeyValue, out decodedValue, out cbDecodedValue))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            if (((int) zero) == 0x13)
            {
                decodedData = new byte[cbDecodedValue];
                Marshal.Copy(decodedValue.DangerousGetHandle(), decodedData, 0, decodedData.Length);
            }
            else if (((int) zero) == 0x26)
            {
                SafeLocalAllocHandle handle2 = null;
                uint num2 = 0;
                if (!CAPI.DecodeObject(new IntPtr(0x27L), encodedParameters, out handle2, out num2))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                decodedData = ConstructDSSPubKeyCspBlob(decodedValue, handle2);
                handle2.Dispose();
            }
            decodedValue.Dispose();
        }

        internal uint AlgorithmId
        {
            get
            {
                if (this.m_aiPubKey == 0)
                {
                    this.m_aiPubKey = System.Security.Cryptography.X509Certificates.X509Utils.OidToAlgId(this.m_oid.Value);
                }
                return this.m_aiPubKey;
            }
        }

        private byte[] CspBlobData
        {
            get
            {
                if (this.m_cspBlobData == null)
                {
                    DecodePublicKeyObject(this.AlgorithmId, this.m_encodedKeyValue.RawData, this.m_encodedParameters.RawData, out this.m_cspBlobData);
                }
                return this.m_cspBlobData;
            }
        }

        public AsnEncodedData EncodedKeyValue
        {
            get
            {
                return this.m_encodedKeyValue;
            }
        }

        public AsnEncodedData EncodedParameters
        {
            get
            {
                return this.m_encodedParameters;
            }
        }

        public AsymmetricAlgorithm Key
        {
            get
            {
                if (this.m_key == null)
                {
                    uint algorithmId = this.AlgorithmId;
                    if (algorithmId != 0x2200)
                    {
                        if ((algorithmId != 0x2400) && (algorithmId != 0xa400))
                        {
                            throw new NotSupportedException(SR.GetString("NotSupported_KeyAlgorithm"));
                        }
                        RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                        provider.ImportCspBlob(this.CspBlobData);
                        this.m_key = provider;
                    }
                    else
                    {
                        DSACryptoServiceProvider provider2 = new DSACryptoServiceProvider();
                        provider2.ImportCspBlob(this.CspBlobData);
                        this.m_key = provider2;
                    }
                }
                return this.m_key;
            }
        }

        public System.Security.Cryptography.Oid Oid
        {
            get
            {
                return new System.Security.Cryptography.Oid(this.m_oid);
            }
        }
    }
}


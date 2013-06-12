namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;

    public sealed class X509SubjectKeyIdentifierExtension : X509Extension
    {
        private bool m_decoded;
        private string m_subjectKeyIdentifier;

        public X509SubjectKeyIdentifierExtension() : base("2.5.29.14")
        {
            this.m_subjectKeyIdentifier = null;
            this.m_decoded = true;
        }

        public X509SubjectKeyIdentifierExtension(string subjectKeyIdentifier, bool critical) : base("2.5.29.14", EncodeExtension(subjectKeyIdentifier), critical)
        {
        }

        public X509SubjectKeyIdentifierExtension(byte[] subjectKeyIdentifier, bool critical) : base("2.5.29.14", EncodeExtension(subjectKeyIdentifier), critical)
        {
        }

        public X509SubjectKeyIdentifierExtension(AsnEncodedData encodedSubjectKeyIdentifier, bool critical) : base("2.5.29.14", encodedSubjectKeyIdentifier.RawData, critical)
        {
        }

        public X509SubjectKeyIdentifierExtension(PublicKey key, bool critical) : base("2.5.29.14", EncodePublicKey(key, X509SubjectKeyIdentifierHashAlgorithm.Sha1), critical)
        {
        }

        public X509SubjectKeyIdentifierExtension(PublicKey key, X509SubjectKeyIdentifierHashAlgorithm algorithm, bool critical) : base("2.5.29.14", EncodePublicKey(key, algorithm), critical)
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
            SafeLocalAllocHandle handle2 = System.Security.Cryptography.X509Certificates.X509Utils.StringToAnsiPtr("2.5.29.14");
            if (!CAPI.DecodeObject(handle2.DangerousGetHandle(), base.m_rawData, out decodedValue, out cbDecodedValue))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            CAPIBase.CRYPTOAPI_BLOB blob = (CAPIBase.CRYPTOAPI_BLOB) Marshal.PtrToStructure(decodedValue.DangerousGetHandle(), typeof(CAPIBase.CRYPTOAPI_BLOB));
            byte[] sArray = CAPI.BlobToByteArray(blob);
            this.m_subjectKeyIdentifier = System.Security.Cryptography.X509Certificates.X509Utils.EncodeHexString(sArray);
            this.m_decoded = true;
            decodedValue.Dispose();
            handle2.Dispose();
        }

        private static byte[] EncodeExtension(string subjectKeyIdentifier)
        {
            if (subjectKeyIdentifier == null)
            {
                throw new ArgumentNullException("subjectKeyIdentifier");
            }
            return EncodeExtension(System.Security.Cryptography.X509Certificates.X509Utils.DecodeHexString(subjectKeyIdentifier));
        }

        private static unsafe byte[] EncodeExtension(byte[] subjectKeyIdentifier)
        {
            if (subjectKeyIdentifier == null)
            {
                throw new ArgumentNullException("subjectKeyIdentifier");
            }
            if (subjectKeyIdentifier.Length == 0)
            {
                throw new ArgumentException("subjectKeyIdentifier");
            }
            byte[] encodedData = null;
            fixed (byte* numRef = subjectKeyIdentifier)
            {
                CAPIBase.CRYPTOAPI_BLOB cryptoapi_blob = new CAPIBase.CRYPTOAPI_BLOB {
                    pbData = new IntPtr((void*) numRef),
                    cbData = (uint) subjectKeyIdentifier.Length
                };
                if (!CAPI.EncodeObject("2.5.29.14", new IntPtr((void*) &cryptoapi_blob), out encodedData))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
            return encodedData;
        }

        private static unsafe SafeLocalAllocHandle EncodePublicKey(PublicKey key)
        {
            SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
            CAPIBase.CERT_PUBLIC_KEY_INFO2* handle = null;
            string s = key.Oid.Value;
            byte[] rawData = key.EncodedParameters.RawData;
            byte[] source = key.EncodedKeyValue.RawData;
            uint num = (uint) (((Marshal.SizeOf(typeof(CAPIBase.CERT_PUBLIC_KEY_INFO2)) + System.Security.Cryptography.X509Certificates.X509Utils.AlignedLength((uint) (s.Length + 1))) + System.Security.Cryptography.X509Certificates.X509Utils.AlignedLength((uint) rawData.Length)) + source.Length);
            invalidHandle = CAPI.LocalAlloc(0x40, new IntPtr((long) num));
            handle = (CAPIBase.CERT_PUBLIC_KEY_INFO2*) invalidHandle.DangerousGetHandle();
            IntPtr destination = new IntPtr(((long) ((ulong) handle)) + Marshal.SizeOf(typeof(CAPIBase.CERT_PUBLIC_KEY_INFO2)));
            IntPtr ptr2 = new IntPtr(((long) destination) + System.Security.Cryptography.X509Certificates.X509Utils.AlignedLength((uint) (s.Length + 1)));
            IntPtr ptr3 = new IntPtr(((long) ptr2) + System.Security.Cryptography.X509Certificates.X509Utils.AlignedLength((uint) rawData.Length));
            handle->Algorithm.pszObjId = destination;
            byte[] bytes = new byte[s.Length + 1];
            Encoding.ASCII.GetBytes(s, 0, s.Length, bytes, 0);
            Marshal.Copy(bytes, 0, destination, bytes.Length);
            if (rawData.Length > 0)
            {
                handle->Algorithm.Parameters.cbData = (uint) rawData.Length;
                handle->Algorithm.Parameters.pbData = ptr2;
                Marshal.Copy(rawData, 0, ptr2, rawData.Length);
            }
            handle->PublicKey.cbData = (uint) source.Length;
            handle->PublicKey.pbData = ptr3;
            Marshal.Copy(source, 0, ptr3, source.Length);
            return invalidHandle;
        }

        private static unsafe byte[] EncodePublicKey(PublicKey key, X509SubjectKeyIdentifierHashAlgorithm algorithm)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            SafeLocalAllocHandle handle = EncodePublicKey(key);
            CAPIBase.CERT_PUBLIC_KEY_INFO2* cert_public_key_infoPtr = (CAPIBase.CERT_PUBLIC_KEY_INFO2*) handle.DangerousGetHandle();
            byte[] sourceArray = new byte[20];
            byte[] destinationArray = null;
            fixed (byte* numRef = sourceArray)
            {
                uint length = (uint) sourceArray.Length;
                IntPtr pbComputedHash = new IntPtr((void*) numRef);
                try
                {
                    if ((algorithm == X509SubjectKeyIdentifierHashAlgorithm.Sha1) || (X509SubjectKeyIdentifierHashAlgorithm.ShortSha1 == algorithm))
                    {
                        if (!CAPISafe.CryptHashCertificate(IntPtr.Zero, 0x8004, 0, cert_public_key_infoPtr->PublicKey.pbData, cert_public_key_infoPtr->PublicKey.cbData, pbComputedHash, new IntPtr((void*) &length)))
                        {
                            throw new CryptographicException(Marshal.GetHRForLastWin32Error());
                        }
                    }
                    else
                    {
                        if (X509SubjectKeyIdentifierHashAlgorithm.CapiSha1 != algorithm)
                        {
                            throw new ArgumentException("algorithm");
                        }
                        if (!CAPISafe.CryptHashPublicKeyInfo(IntPtr.Zero, 0x8004, 0, 1, new IntPtr((void*) cert_public_key_infoPtr), pbComputedHash, new IntPtr((void*) &length)))
                        {
                            throw new CryptographicException(Marshal.GetHRForLastWin32Error());
                        }
                    }
                    if (X509SubjectKeyIdentifierHashAlgorithm.ShortSha1 == algorithm)
                    {
                        destinationArray = new byte[8];
                        Array.Copy(sourceArray, sourceArray.Length - 8, destinationArray, 0, destinationArray.Length);
                        destinationArray[0] = (byte) (destinationArray[0] & 15);
                        destinationArray[0] = (byte) (destinationArray[0] | 0x40);
                    }
                    else
                    {
                        destinationArray = sourceArray;
                        if (sourceArray.Length > length)
                        {
                            destinationArray = new byte[length];
                            Array.Copy(sourceArray, 0, destinationArray, 0, destinationArray.Length);
                        }
                    }
                }
                finally
                {
                    handle.Dispose();
                }
            }
            return EncodeExtension(destinationArray);
        }

        public string SubjectKeyIdentifier
        {
            get
            {
                if (!this.m_decoded)
                {
                    this.DecodeExtension();
                }
                return this.m_subjectKeyIdentifier;
            }
        }
    }
}


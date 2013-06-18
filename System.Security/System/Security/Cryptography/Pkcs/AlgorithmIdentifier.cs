namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class AlgorithmIdentifier
    {
        private int m_keyLength;
        private System.Security.Cryptography.Oid m_oid;
        private byte[] m_parameters;

        public AlgorithmIdentifier()
        {
            this.Reset(new System.Security.Cryptography.Oid("1.2.840.113549.3.7"), 0, new byte[0]);
        }

        [SecurityCritical]
        internal AlgorithmIdentifier(System.Security.Cryptography.CAPI.CERT_PUBLIC_KEY_INFO keyInfo)
        {
            System.Security.Cryptography.SafeLocalAllocHandle handle = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CERT_PUBLIC_KEY_INFO))));
            Marshal.StructureToPtr(keyInfo, handle.DangerousGetHandle(), false);
            int keyLength = (int) System.Security.Cryptography.CAPI.CAPISafe.CertGetPublicKeyLength(0x10001, handle.DangerousGetHandle());
            byte[] destination = new byte[keyInfo.Algorithm.Parameters.cbData];
            if (destination.Length > 0)
            {
                Marshal.Copy(keyInfo.Algorithm.Parameters.pbData, destination, 0, destination.Length);
            }
            Marshal.DestroyStructure(handle.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CERT_PUBLIC_KEY_INFO));
            handle.Dispose();
            this.Reset(new System.Security.Cryptography.Oid(keyInfo.Algorithm.pszObjId), keyLength, destination);
        }

        [SecurityCritical]
        internal AlgorithmIdentifier(System.Security.Cryptography.CAPI.CRYPT_ALGORITHM_IDENTIFIER algorithmIdentifier)
        {
            int keyLength = 0;
            uint cbDecodedValue = 0;
            System.Security.Cryptography.SafeLocalAllocHandle invalidHandle = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            byte[] destination = new byte[0];
            uint num3 = System.Security.Cryptography.X509Certificates.X509Utils.OidToAlgId(algorithmIdentifier.pszObjId);
            if (num3 == 0x6602)
            {
                if (algorithmIdentifier.Parameters.cbData > 0)
                {
                    if (!System.Security.Cryptography.CAPI.DecodeObject(new IntPtr(0x29L), algorithmIdentifier.Parameters.pbData, algorithmIdentifier.Parameters.cbData, out invalidHandle, out cbDecodedValue))
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                    System.Security.Cryptography.CAPI.CRYPT_RC2_CBC_PARAMETERS crypt_rc_cbc_parameters = (System.Security.Cryptography.CAPI.CRYPT_RC2_CBC_PARAMETERS) Marshal.PtrToStructure(invalidHandle.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CRYPT_RC2_CBC_PARAMETERS));
                    switch (crypt_rc_cbc_parameters.dwVersion)
                    {
                        case 0x34:
                            keyLength = 0x38;
                            break;

                        case 0x3a:
                            keyLength = 0x80;
                            break;

                        case 160:
                            keyLength = 40;
                            break;
                    }
                    if (crypt_rc_cbc_parameters.fIV)
                    {
                        destination = (byte[]) crypt_rc_cbc_parameters.rgbIV.Clone();
                    }
                }
            }
            else if (((num3 == 0x6801) || (num3 == 0x6601)) || (num3 == 0x6603))
            {
                if (algorithmIdentifier.Parameters.cbData > 0)
                {
                    if (!System.Security.Cryptography.CAPI.DecodeObject(new IntPtr(0x19L), algorithmIdentifier.Parameters.pbData, algorithmIdentifier.Parameters.cbData, out invalidHandle, out cbDecodedValue))
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                    if (cbDecodedValue > 0)
                    {
                        if (num3 == 0x6801)
                        {
                            System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB cryptoapi_blob = (System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB) Marshal.PtrToStructure(invalidHandle.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB));
                            if (cryptoapi_blob.cbData > 0)
                            {
                                destination = new byte[cryptoapi_blob.cbData];
                                Marshal.Copy(cryptoapi_blob.pbData, destination, 0, destination.Length);
                            }
                        }
                        else
                        {
                            destination = new byte[cbDecodedValue];
                            Marshal.Copy(invalidHandle.DangerousGetHandle(), destination, 0, destination.Length);
                        }
                    }
                }
                if (num3 == 0x6801)
                {
                    keyLength = 0x80 - (destination.Length * 8);
                }
                else if (num3 == 0x6601)
                {
                    keyLength = 0x40;
                }
                else
                {
                    keyLength = 0xc0;
                }
            }
            else if (algorithmIdentifier.Parameters.cbData > 0)
            {
                destination = new byte[algorithmIdentifier.Parameters.cbData];
                Marshal.Copy(algorithmIdentifier.Parameters.pbData, destination, 0, destination.Length);
            }
            this.Reset(new System.Security.Cryptography.Oid(algorithmIdentifier.pszObjId), keyLength, destination);
            invalidHandle.Dispose();
        }

        public AlgorithmIdentifier(System.Security.Cryptography.Oid oid)
        {
            this.Reset(oid, 0, new byte[0]);
        }

        internal AlgorithmIdentifier(string oidValue)
        {
            this.Reset(new System.Security.Cryptography.Oid(oidValue), 0, new byte[0]);
        }

        public AlgorithmIdentifier(System.Security.Cryptography.Oid oid, int keyLength)
        {
            this.Reset(oid, keyLength, new byte[0]);
        }

        private void Reset(System.Security.Cryptography.Oid oid, int keyLength, byte[] parameters)
        {
            this.m_oid = oid;
            this.m_keyLength = keyLength;
            this.m_parameters = parameters;
        }

        public int KeyLength
        {
            get
            {
                return this.m_keyLength;
            }
            set
            {
                this.m_keyLength = value;
            }
        }

        public System.Security.Cryptography.Oid Oid
        {
            get
            {
                return this.m_oid;
            }
            set
            {
                this.m_oid = value;
            }
        }

        public byte[] Parameters
        {
            get
            {
                return this.m_parameters;
            }
            set
            {
                this.m_parameters = value;
            }
        }
    }
}


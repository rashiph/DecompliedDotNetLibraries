namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Cryptography.Xml;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class SubjectIdentifier
    {
        private SubjectIdentifierType m_type;
        private object m_value;

        private SubjectIdentifier()
        {
        }

        [SecurityCritical]
        internal SubjectIdentifier(System.Security.Cryptography.CAPI.CERT_ID certId)
        {
            switch (certId.dwIdChoice)
            {
                case 1:
                {
                    X509IssuerSerial serial = PkcsUtils.DecodeIssuerSerial(certId.Value.IssuerSerialNumber);
                    this.Reset(SubjectIdentifierType.IssuerAndSerialNumber, serial);
                    return;
                }
                case 2:
                {
                    byte[] destination = new byte[certId.Value.KeyId.cbData];
                    Marshal.Copy(certId.Value.KeyId.pbData, destination, 0, destination.Length);
                    this.Reset(SubjectIdentifierType.SubjectKeyIdentifier, System.Security.Cryptography.X509Certificates.X509Utils.EncodeHexString(destination));
                    return;
                }
            }
            throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Invalid_Subject_Identifier_Type"), certId.dwIdChoice.ToString(CultureInfo.InvariantCulture));
        }

        [SecurityCritical]
        internal SubjectIdentifier(System.Security.Cryptography.CAPI.CERT_INFO certInfo) : this(certInfo.Issuer, certInfo.SerialNumber)
        {
        }

        [SecurityCritical]
        internal SubjectIdentifier(System.Security.Cryptography.CAPI.CMSG_SIGNER_INFO signerInfo) : this(signerInfo.Issuer, signerInfo.SerialNumber)
        {
        }

        [SecurityCritical]
        internal unsafe SubjectIdentifier(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB issuer, System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB serialNumber)
        {
            System.Security.Cryptography.CAPI.CERT_ISSUER_SERIAL_NUMBER cert_issuer_serial_number;
            bool flag = true;
            byte* pbData = (byte*) serialNumber.pbData;
            for (uint i = 0; i < serialNumber.cbData; i++)
            {
                pbData++;
                if (pbData[0] != 0)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                byte[] destination = new byte[issuer.cbData];
                Marshal.Copy(issuer.pbData, destination, 0, destination.Length);
                X500DistinguishedName name = new X500DistinguishedName(destination);
                if (string.Compare("CN=Dummy Signer", name.Name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.Reset(SubjectIdentifierType.NoSignature, null);
                    return;
                }
            }
            if (flag)
            {
                this.m_type = SubjectIdentifierType.SubjectKeyIdentifier;
                this.m_value = string.Empty;
                uint cbDecodedValue = 0;
                System.Security.Cryptography.SafeLocalAllocHandle invalidHandle = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
                if (!System.Security.Cryptography.CAPI.DecodeObject(new IntPtr(7L), issuer.pbData, issuer.cbData, out invalidHandle, out cbDecodedValue))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                using (invalidHandle)
                {
                    System.Security.Cryptography.CAPI.CERT_NAME_INFO cert_name_info = (System.Security.Cryptography.CAPI.CERT_NAME_INFO) Marshal.PtrToStructure(invalidHandle.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CERT_NAME_INFO));
                    for (uint j = 0; j < cert_name_info.cRDN; j++)
                    {
                        System.Security.Cryptography.CAPI.CERT_RDN cert_rdn = (System.Security.Cryptography.CAPI.CERT_RDN) Marshal.PtrToStructure(new IntPtr(((long) cert_name_info.rgRDN) + (j * Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CERT_RDN)))), typeof(System.Security.Cryptography.CAPI.CERT_RDN));
                        for (uint k = 0; k < cert_rdn.cRDNAttr; k++)
                        {
                            System.Security.Cryptography.CAPI.CERT_RDN_ATTR cert_rdn_attr = (System.Security.Cryptography.CAPI.CERT_RDN_ATTR) Marshal.PtrToStructure(new IntPtr(((long) cert_rdn.rgRDNAttr) + (k * Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CERT_RDN_ATTR)))), typeof(System.Security.Cryptography.CAPI.CERT_RDN_ATTR));
                            if ((string.Compare("1.3.6.1.4.1.311.10.7.1", cert_rdn_attr.pszObjId, StringComparison.OrdinalIgnoreCase) == 0) && (cert_rdn_attr.dwValueType == 2))
                            {
                                byte[] buffer2 = new byte[cert_rdn_attr.Value.cbData];
                                Marshal.Copy(cert_rdn_attr.Value.pbData, buffer2, 0, buffer2.Length);
                                this.Reset(SubjectIdentifierType.SubjectKeyIdentifier, System.Security.Cryptography.X509Certificates.X509Utils.EncodeHexString(buffer2));
                                return;
                            }
                        }
                    }
                }
                throw new CryptographicException(-2146889715);
            }
            cert_issuer_serial_number.Issuer = issuer;
            cert_issuer_serial_number.SerialNumber = serialNumber;
            X509IssuerSerial serial = PkcsUtils.DecodeIssuerSerial(cert_issuer_serial_number);
            this.Reset(SubjectIdentifierType.IssuerAndSerialNumber, serial);
        }

        internal SubjectIdentifier(SubjectIdentifierType type, object value)
        {
            this.Reset(type, value);
        }

        internal void Reset(SubjectIdentifierType type, object value)
        {
            switch (type)
            {
                case SubjectIdentifierType.Unknown:
                case SubjectIdentifierType.NoSignature:
                    break;

                case SubjectIdentifierType.IssuerAndSerialNumber:
                    if (value.GetType() != typeof(X509IssuerSerial))
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Invalid_Subject_Identifier_Type_Value_Mismatch"), value.GetType().ToString());
                    }
                    break;

                case SubjectIdentifierType.SubjectKeyIdentifier:
                    if (!PkcsUtils.CmsSupported())
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Not_Supported"));
                    }
                    if (value.GetType() != typeof(string))
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Invalid_Subject_Identifier_Type_Value_Mismatch"), value.GetType().ToString());
                    }
                    break;

                default:
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Invalid_Subject_Identifier_Type"), type.ToString());
            }
            this.m_type = type;
            this.m_value = value;
        }

        public SubjectIdentifierType Type
        {
            get
            {
                return this.m_type;
            }
        }

        public object Value
        {
            get
            {
                return this.m_value;
            }
        }
    }
}


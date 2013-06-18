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
    public sealed class SubjectIdentifierOrKey
    {
        private SubjectIdentifierOrKeyType m_type;
        private object m_value;

        private SubjectIdentifierOrKey()
        {
        }

        [SecurityCritical]
        internal SubjectIdentifierOrKey(System.Security.Cryptography.CAPI.CERT_ID certId)
        {
            switch (certId.dwIdChoice)
            {
                case 1:
                {
                    X509IssuerSerial serial = PkcsUtils.DecodeIssuerSerial(certId.Value.IssuerSerialNumber);
                    this.Reset(SubjectIdentifierOrKeyType.IssuerAndSerialNumber, serial);
                    return;
                }
                case 2:
                {
                    byte[] destination = new byte[certId.Value.KeyId.cbData];
                    Marshal.Copy(certId.Value.KeyId.pbData, destination, 0, destination.Length);
                    this.Reset(SubjectIdentifierOrKeyType.SubjectKeyIdentifier, System.Security.Cryptography.X509Certificates.X509Utils.EncodeHexString(destination));
                    return;
                }
            }
            throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Invalid_Subject_Identifier_Type"), certId.dwIdChoice.ToString(CultureInfo.InvariantCulture));
        }

        [SecurityCritical]
        internal SubjectIdentifierOrKey(System.Security.Cryptography.CAPI.CERT_PUBLIC_KEY_INFO publicKeyInfo)
        {
            this.Reset(SubjectIdentifierOrKeyType.PublicKeyInfo, new PublicKeyInfo(publicKeyInfo));
        }

        internal SubjectIdentifierOrKey(SubjectIdentifierOrKeyType type, object value)
        {
            this.Reset(type, value);
        }

        internal void Reset(SubjectIdentifierOrKeyType type, object value)
        {
            switch (type)
            {
                case SubjectIdentifierOrKeyType.Unknown:
                    break;

                case SubjectIdentifierOrKeyType.IssuerAndSerialNumber:
                    if (value.GetType() != typeof(X509IssuerSerial))
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Invalid_Subject_Identifier_Type_Value_Mismatch"), value.GetType().ToString());
                    }
                    break;

                case SubjectIdentifierOrKeyType.SubjectKeyIdentifier:
                    if (!PkcsUtils.CmsSupported())
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Not_Supported"));
                    }
                    if (!(value.GetType() != typeof(string)))
                    {
                        break;
                    }
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Invalid_Subject_Identifier_Type_Value_Mismatch"), value.GetType().ToString());

                case SubjectIdentifierOrKeyType.PublicKeyInfo:
                    if (!PkcsUtils.CmsSupported())
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Not_Supported"));
                    }
                    if (value.GetType() != typeof(PublicKeyInfo))
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

        public SubjectIdentifierOrKeyType Type
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


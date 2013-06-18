namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CmsRecipient
    {
        private X509Certificate2 m_certificate;
        private SubjectIdentifierType m_recipientIdentifierType;

        private CmsRecipient()
        {
        }

        public CmsRecipient(X509Certificate2 certificate) : this(SubjectIdentifierType.IssuerAndSerialNumber, certificate)
        {
        }

        public CmsRecipient(SubjectIdentifierType recipientIdentifierType, X509Certificate2 certificate)
        {
            this.Reset(recipientIdentifierType, certificate);
        }

        private void Reset(SubjectIdentifierType recipientIdentifierType, X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }
            switch (recipientIdentifierType)
            {
                case SubjectIdentifierType.Unknown:
                    recipientIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
                    break;

                case SubjectIdentifierType.IssuerAndSerialNumber:
                    break;

                case SubjectIdentifierType.SubjectKeyIdentifier:
                    if (!PkcsUtils.CmsSupported())
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Not_Supported"));
                    }
                    break;

                default:
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Invalid_Subject_Identifier_Type"), recipientIdentifierType.ToString());
            }
            this.m_recipientIdentifierType = recipientIdentifierType;
            this.m_certificate = certificate;
        }

        public X509Certificate2 Certificate
        {
            get
            {
                return this.m_certificate;
            }
        }

        public SubjectIdentifierType RecipientIdentifierType
        {
            get
            {
                return this.m_recipientIdentifierType;
            }
        }
    }
}


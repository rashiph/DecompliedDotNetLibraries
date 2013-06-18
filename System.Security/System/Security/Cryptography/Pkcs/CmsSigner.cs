namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Globalization;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CmsSigner
    {
        private X509Certificate2 m_certificate;
        private X509Certificate2Collection m_certificates;
        private Oid m_digestAlgorithm;
        private bool m_dummyCert;
        private X509IncludeOption m_includeOption;
        private CryptographicAttributeObjectCollection m_signedAttributes;
        private SubjectIdentifierType m_signerIdentifierType;
        private CryptographicAttributeObjectCollection m_unsignedAttributes;

        public CmsSigner() : this(SubjectIdentifierType.IssuerAndSerialNumber, null)
        {
        }

        [SecuritySafeCritical]
        public CmsSigner(CspParameters parameters) : this(SubjectIdentifierType.SubjectKeyIdentifier, PkcsUtils.CreateDummyCertificate(parameters))
        {
            this.m_dummyCert = true;
            this.IncludeOption = X509IncludeOption.None;
        }

        public CmsSigner(SubjectIdentifierType signerIdentifierType) : this(signerIdentifierType, null)
        {
        }

        public CmsSigner(X509Certificate2 certificate) : this(SubjectIdentifierType.IssuerAndSerialNumber, certificate)
        {
        }

        public CmsSigner(SubjectIdentifierType signerIdentifierType, X509Certificate2 certificate)
        {
            switch (signerIdentifierType)
            {
                case SubjectIdentifierType.Unknown:
                    this.SignerIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
                    this.IncludeOption = X509IncludeOption.ExcludeRoot;
                    break;

                case SubjectIdentifierType.IssuerAndSerialNumber:
                    this.SignerIdentifierType = signerIdentifierType;
                    this.IncludeOption = X509IncludeOption.ExcludeRoot;
                    break;

                case SubjectIdentifierType.SubjectKeyIdentifier:
                    this.SignerIdentifierType = signerIdentifierType;
                    this.IncludeOption = X509IncludeOption.ExcludeRoot;
                    break;

                case SubjectIdentifierType.NoSignature:
                    this.SignerIdentifierType = signerIdentifierType;
                    this.IncludeOption = X509IncludeOption.None;
                    break;

                default:
                    this.SignerIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
                    this.IncludeOption = X509IncludeOption.ExcludeRoot;
                    break;
            }
            this.Certificate = certificate;
            this.DigestAlgorithm = new Oid("1.3.14.3.2.26");
            this.m_signedAttributes = new CryptographicAttributeObjectCollection();
            this.m_unsignedAttributes = new CryptographicAttributeObjectCollection();
            this.m_certificates = new X509Certificate2Collection();
        }

        public X509Certificate2 Certificate
        {
            get
            {
                return this.m_certificate;
            }
            set
            {
                this.m_certificate = value;
            }
        }

        public X509Certificate2Collection Certificates
        {
            get
            {
                return this.m_certificates;
            }
        }

        public Oid DigestAlgorithm
        {
            get
            {
                return this.m_digestAlgorithm;
            }
            set
            {
                this.m_digestAlgorithm = value;
            }
        }

        public X509IncludeOption IncludeOption
        {
            get
            {
                return this.m_includeOption;
            }
            set
            {
                if ((value < X509IncludeOption.None) || (value > X509IncludeOption.WholeChain))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SecurityResources.GetResourceString("Arg_EnumIllegalVal"), new object[] { "value" }));
                }
                this.m_includeOption = value;
            }
        }

        public CryptographicAttributeObjectCollection SignedAttributes
        {
            get
            {
                return this.m_signedAttributes;
            }
        }

        public SubjectIdentifierType SignerIdentifierType
        {
            get
            {
                return this.m_signerIdentifierType;
            }
            set
            {
                if (((value != SubjectIdentifierType.IssuerAndSerialNumber) && (value != SubjectIdentifierType.SubjectKeyIdentifier)) && (value != SubjectIdentifierType.NoSignature))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SecurityResources.GetResourceString("Arg_EnumIllegalVal"), new object[] { "value" }));
                }
                if (this.m_dummyCert && (value != SubjectIdentifierType.SubjectKeyIdentifier))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SecurityResources.GetResourceString("Arg_EnumIllegalVal"), new object[] { "value" }));
                }
                this.m_signerIdentifierType = value;
            }
        }

        public CryptographicAttributeObjectCollection UnsignedAttributes
        {
            get
            {
                return this.m_unsignedAttributes;
            }
        }
    }
}


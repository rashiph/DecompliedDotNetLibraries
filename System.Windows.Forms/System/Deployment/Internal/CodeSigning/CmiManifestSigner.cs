namespace System.Deployment.Internal.CodeSigning
{
    using System;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;

    internal class CmiManifestSigner
    {
        internal const uint CimManifestSignerFlagMask = 1;
        private X509Certificate2 m_certificate;
        private X509Certificate2Collection m_certificates;
        private string m_description;
        private X509IncludeOption m_includeOption;
        private System.Deployment.Internal.CodeSigning.CmiManifestSignerFlag m_signerFlag;
        private AsymmetricAlgorithm m_strongNameKey;
        private string m_url;

        private CmiManifestSigner()
        {
        }

        internal CmiManifestSigner(AsymmetricAlgorithm strongNameKey) : this(strongNameKey, null)
        {
        }

        internal CmiManifestSigner(AsymmetricAlgorithm strongNameKey, X509Certificate2 certificate)
        {
            if (strongNameKey == null)
            {
                throw new ArgumentNullException("strongNameKey");
            }
            if (!(strongNameKey is RSA))
            {
                throw new ArgumentNullException("strongNameKey");
            }
            this.m_strongNameKey = strongNameKey;
            this.m_certificate = certificate;
            this.m_certificates = new X509Certificate2Collection();
            this.m_includeOption = X509IncludeOption.ExcludeRoot;
            this.m_signerFlag = System.Deployment.Internal.CodeSigning.CmiManifestSignerFlag.None;
        }

        internal X509Certificate2 Certificate
        {
            get
            {
                return this.m_certificate;
            }
        }

        internal string Description
        {
            get
            {
                return this.m_description;
            }
            set
            {
                this.m_description = value;
            }
        }

        internal string DescriptionUrl
        {
            get
            {
                return this.m_url;
            }
            set
            {
                this.m_url = value;
            }
        }

        internal X509Certificate2Collection ExtraStore
        {
            get
            {
                return this.m_certificates;
            }
        }

        internal System.Deployment.Internal.CodeSigning.CmiManifestSignerFlag Flag
        {
            get
            {
                return this.m_signerFlag;
            }
            set
            {
                if ((value & ~System.Deployment.Internal.CodeSigning.CmiManifestSignerFlag.DontReplacePublicKeyToken) != System.Deployment.Internal.CodeSigning.CmiManifestSignerFlag.None)
                {
                    throw new ArgumentException("value");
                }
                this.m_signerFlag = value;
            }
        }

        internal X509IncludeOption IncludeOption
        {
            get
            {
                return this.m_includeOption;
            }
            set
            {
                if ((value < X509IncludeOption.None) || (value > X509IncludeOption.WholeChain))
                {
                    throw new ArgumentException("value");
                }
                if (this.m_includeOption == X509IncludeOption.None)
                {
                    throw new NotSupportedException();
                }
                this.m_includeOption = value;
            }
        }

        internal AsymmetricAlgorithm StrongNameKey
        {
            get
            {
                return this.m_strongNameKey;
            }
        }
    }
}


namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;

    public class IssuedTokenServiceCredential
    {
        private List<string> allowedAudienceUris;
        private bool allowUntrustedRsaIssuers;
        private System.IdentityModel.Selectors.AudienceUriMode audienceUriMode;
        private X509CertificateValidationMode certificateValidationMode;
        private X509CertificateValidator customCertificateValidator;
        internal const bool DefaultAllowUntrustedRsaIssuers = false;
        internal const System.IdentityModel.Selectors.AudienceUriMode DefaultAudienceUriMode = System.IdentityModel.Selectors.AudienceUriMode.Always;
        internal const X509CertificateValidationMode DefaultCertificateValidationMode = X509CertificateValidationMode.ChainTrust;
        internal const X509RevocationMode DefaultRevocationMode = X509RevocationMode.Online;
        internal const StoreLocation DefaultTrustedStoreLocation = StoreLocation.LocalMachine;
        private bool isReadOnly;
        private List<X509Certificate2> knownCertificates;
        private X509RevocationMode revocationMode;
        private System.IdentityModel.Tokens.SamlSerializer samlSerializer;
        private StoreLocation trustedStoreLocation;

        internal IssuedTokenServiceCredential()
        {
            this.audienceUriMode = System.IdentityModel.Selectors.AudienceUriMode.Always;
            this.certificateValidationMode = X509CertificateValidationMode.ChainTrust;
            this.revocationMode = X509RevocationMode.Online;
            this.trustedStoreLocation = StoreLocation.LocalMachine;
            this.allowedAudienceUris = new List<string>();
            this.knownCertificates = new List<X509Certificate2>();
        }

        internal IssuedTokenServiceCredential(IssuedTokenServiceCredential other)
        {
            this.audienceUriMode = System.IdentityModel.Selectors.AudienceUriMode.Always;
            this.certificateValidationMode = X509CertificateValidationMode.ChainTrust;
            this.revocationMode = X509RevocationMode.Online;
            this.trustedStoreLocation = StoreLocation.LocalMachine;
            this.audienceUriMode = other.audienceUriMode;
            this.allowedAudienceUris = new List<string>(other.allowedAudienceUris);
            this.samlSerializer = other.samlSerializer;
            this.knownCertificates = new List<X509Certificate2>(other.knownCertificates);
            this.certificateValidationMode = other.certificateValidationMode;
            this.customCertificateValidator = other.customCertificateValidator;
            this.trustedStoreLocation = other.trustedStoreLocation;
            this.revocationMode = other.revocationMode;
            this.allowUntrustedRsaIssuers = other.allowUntrustedRsaIssuers;
            this.isReadOnly = other.isReadOnly;
        }

        internal X509CertificateValidator GetCertificateValidator()
        {
            if (this.certificateValidationMode == X509CertificateValidationMode.None)
            {
                return X509CertificateValidator.None;
            }
            if (this.certificateValidationMode == X509CertificateValidationMode.PeerTrust)
            {
                return X509CertificateValidator.PeerTrust;
            }
            if (this.certificateValidationMode == X509CertificateValidationMode.Custom)
            {
                if (this.customCertificateValidator == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MissingCustomCertificateValidator")));
                }
                return this.customCertificateValidator;
            }
            bool useMachineContext = this.trustedStoreLocation == StoreLocation.LocalMachine;
            X509ChainPolicy chainPolicy = new X509ChainPolicy {
                RevocationMode = this.revocationMode
            };
            if (this.certificateValidationMode == X509CertificateValidationMode.ChainTrust)
            {
                return X509CertificateValidator.CreateChainTrustValidator(useMachineContext, chainPolicy);
            }
            return X509CertificateValidator.CreatePeerOrChainTrustValidator(useMachineContext, chainPolicy);
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        private void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
        }

        public IList<string> AllowedAudienceUris
        {
            get
            {
                if (this.isReadOnly)
                {
                    return this.allowedAudienceUris.AsReadOnly();
                }
                return this.allowedAudienceUris;
            }
        }

        public bool AllowUntrustedRsaIssuers
        {
            get
            {
                return this.allowUntrustedRsaIssuers;
            }
            set
            {
                this.ThrowIfImmutable();
                this.allowUntrustedRsaIssuers = value;
            }
        }

        public System.IdentityModel.Selectors.AudienceUriMode AudienceUriMode
        {
            get
            {
                return this.audienceUriMode;
            }
            set
            {
                this.ThrowIfImmutable();
                AudienceUriModeValidationHelper.Validate(this.audienceUriMode);
                this.audienceUriMode = value;
            }
        }

        public X509CertificateValidationMode CertificateValidationMode
        {
            get
            {
                return this.certificateValidationMode;
            }
            set
            {
                X509CertificateValidationModeHelper.Validate(value);
                this.ThrowIfImmutable();
                this.certificateValidationMode = value;
            }
        }

        public X509CertificateValidator CustomCertificateValidator
        {
            get
            {
                return this.customCertificateValidator;
            }
            set
            {
                this.ThrowIfImmutable();
                this.customCertificateValidator = value;
            }
        }

        public IList<X509Certificate2> KnownCertificates
        {
            get
            {
                if (this.isReadOnly)
                {
                    return this.knownCertificates.AsReadOnly();
                }
                return this.knownCertificates;
            }
        }

        public X509RevocationMode RevocationMode
        {
            get
            {
                return this.revocationMode;
            }
            set
            {
                this.ThrowIfImmutable();
                this.revocationMode = value;
            }
        }

        public System.IdentityModel.Tokens.SamlSerializer SamlSerializer
        {
            get
            {
                return this.samlSerializer;
            }
            set
            {
                this.ThrowIfImmutable();
                this.samlSerializer = value;
            }
        }

        public StoreLocation TrustedStoreLocation
        {
            get
            {
                return this.trustedStoreLocation;
            }
            set
            {
                this.ThrowIfImmutable();
                this.trustedStoreLocation = value;
            }
        }
    }
}


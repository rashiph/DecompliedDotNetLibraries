namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;

    public class X509ClientCertificateAuthentication
    {
        private X509CertificateValidationMode certificateValidationMode;
        private X509CertificateValidator customCertificateValidator;
        internal const X509CertificateValidationMode DefaultCertificateValidationMode = X509CertificateValidationMode.ChainTrust;
        private static X509CertificateValidator defaultCertificateValidator;
        internal const bool DefaultMapCertificateToWindowsAccount = false;
        internal const X509RevocationMode DefaultRevocationMode = X509RevocationMode.Online;
        internal const StoreLocation DefaultTrustedStoreLocation = StoreLocation.LocalMachine;
        private bool includeWindowsGroups;
        private bool isReadOnly;
        private bool mapClientCertificateToWindowsAccount;
        private X509RevocationMode revocationMode;
        private StoreLocation trustedStoreLocation;

        internal X509ClientCertificateAuthentication()
        {
            this.certificateValidationMode = X509CertificateValidationMode.ChainTrust;
            this.revocationMode = X509RevocationMode.Online;
            this.trustedStoreLocation = StoreLocation.LocalMachine;
            this.includeWindowsGroups = true;
        }

        internal X509ClientCertificateAuthentication(X509ClientCertificateAuthentication other)
        {
            this.certificateValidationMode = X509CertificateValidationMode.ChainTrust;
            this.revocationMode = X509RevocationMode.Online;
            this.trustedStoreLocation = StoreLocation.LocalMachine;
            this.includeWindowsGroups = true;
            this.certificateValidationMode = other.certificateValidationMode;
            this.customCertificateValidator = other.customCertificateValidator;
            this.includeWindowsGroups = other.includeWindowsGroups;
            this.mapClientCertificateToWindowsAccount = other.mapClientCertificateToWindowsAccount;
            this.trustedStoreLocation = other.trustedStoreLocation;
            this.revocationMode = other.revocationMode;
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

        internal static X509CertificateValidator DefaultCertificateValidator
        {
            get
            {
                if (defaultCertificateValidator == null)
                {
                    bool useMachineContext = true;
                    X509ChainPolicy chainPolicy = new X509ChainPolicy {
                        RevocationMode = X509RevocationMode.Online
                    };
                    defaultCertificateValidator = X509CertificateValidator.CreateChainTrustValidator(useMachineContext, chainPolicy);
                }
                return defaultCertificateValidator;
            }
        }

        public bool IncludeWindowsGroups
        {
            get
            {
                return this.includeWindowsGroups;
            }
            set
            {
                this.ThrowIfImmutable();
                this.includeWindowsGroups = value;
            }
        }

        public bool MapClientCertificateToWindowsAccount
        {
            get
            {
                return this.mapClientCertificateToWindowsAccount;
            }
            set
            {
                this.ThrowIfImmutable();
                this.mapClientCertificateToWindowsAccount = value;
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


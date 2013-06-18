namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class PeerCredential
    {
        private X509Certificate2 certificate;
        internal const X509FindType DefaultFindType = X509FindType.FindBySubjectDistinguishedName;
        internal const StoreLocation DefaultStoreLocation = StoreLocation.CurrentUser;
        internal const StoreName DefaultStoreName = StoreName.My;
        private bool isReadOnly;
        private string meshPassword;
        private X509PeerCertificateAuthentication messageSenderAuthentication;
        private X509PeerCertificateAuthentication peerAuthentication;

        internal PeerCredential()
        {
            this.peerAuthentication = new X509PeerCertificateAuthentication();
            this.messageSenderAuthentication = new X509PeerCertificateAuthentication();
        }

        internal PeerCredential(PeerCredential other)
        {
            this.certificate = other.certificate;
            this.meshPassword = other.meshPassword;
            this.peerAuthentication = new X509PeerCertificateAuthentication(other.peerAuthentication);
            this.messageSenderAuthentication = new X509PeerCertificateAuthentication(other.messageSenderAuthentication);
            this.isReadOnly = other.isReadOnly;
        }

        internal bool Equals(PeerCredential that, PeerAuthenticationMode mode, bool messageAuthentication)
        {
            if (messageAuthentication)
            {
                if (!this.SameAuthenticators(this.MessageSenderAuthentication, that.messageSenderAuthentication))
                {
                    return false;
                }
                if (((this.Certificate != null) && (that.Certificate != null)) && !this.Certificate.Equals((X509Certificate) that.Certificate))
                {
                    return false;
                }
            }
            switch (mode)
            {
                case PeerAuthenticationMode.None:
                    return true;

                case PeerAuthenticationMode.Password:
                    if (this.MeshPassword.Equals(that.MeshPassword))
                    {
                        if ((this.Certificate == null) && (that.Certificate == null))
                        {
                            return true;
                        }
                        if ((this.Certificate != null) && this.Certificate.Equals((X509Certificate) that.Certificate))
                        {
                            break;
                        }
                        return false;
                    }
                    return false;

                case PeerAuthenticationMode.MutualCertificate:
                    if (this.Certificate.Equals((X509Certificate) that.Certificate))
                    {
                        if (!this.SameAuthenticators(this.PeerAuthentication, that.PeerAuthentication))
                        {
                            return false;
                        }
                        break;
                    }
                    return false;
            }
            return true;
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
            this.peerAuthentication.MakeReadOnly();
            this.messageSenderAuthentication.MakeReadOnly();
        }

        private bool SameAuthenticators(X509PeerCertificateAuthentication one, X509PeerCertificateAuthentication two)
        {
            if (one.CertificateValidationMode != two.CertificateValidationMode)
            {
                return false;
            }
            if (one.CertificateValidationMode != X509CertificateValidationMode.Custom)
            {
                return one.GetType().Equals(two.GetType());
            }
            X509CertificateValidator validator = null;
            X509CertificateValidator validator2 = null;
            one.TryGetCertificateValidator(out validator);
            two.TryGetCertificateValidator(out validator2);
            return (((validator != null) && (validator2 != null)) && validator.Equals(validator2));
        }

        public void SetCertificate(string subjectName, StoreLocation storeLocation, StoreName storeName)
        {
            if (subjectName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subjectName");
            }
            this.SetCertificate(storeLocation, storeName, X509FindType.FindBySubjectDistinguishedName, subjectName);
        }

        public void SetCertificate(StoreLocation storeLocation, StoreName storeName, X509FindType findType, object findValue)
        {
            if (findValue == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("findValue");
            }
            this.ThrowIfImmutable();
            this.certificate = System.ServiceModel.Security.SecurityUtils.GetCertificateFromStore(storeName, storeLocation, findType, findValue, null);
        }

        private void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
        }

        public X509Certificate2 Certificate
        {
            get
            {
                return this.certificate;
            }
            set
            {
                this.ThrowIfImmutable();
                this.certificate = value;
            }
        }

        public string MeshPassword
        {
            get
            {
                return this.meshPassword;
            }
            set
            {
                this.ThrowIfImmutable();
                this.meshPassword = value;
            }
        }

        public X509PeerCertificateAuthentication MessageSenderAuthentication
        {
            get
            {
                return this.messageSenderAuthentication;
            }
            set
            {
                this.ThrowIfImmutable();
                this.messageSenderAuthentication = value;
            }
        }

        public X509PeerCertificateAuthentication PeerAuthentication
        {
            get
            {
                return this.peerAuthentication;
            }
            set
            {
                this.ThrowIfImmutable();
                this.peerAuthentication = value;
            }
        }
    }
}


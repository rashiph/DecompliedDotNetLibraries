namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;

    public class ServiceCredentials : SecurityCredentialsManager, IServiceBehavior
    {
        private X509CertificateInitiatorServiceCredential clientCertificate;
        private IssuedTokenServiceCredential issuedToken;
        private PeerCredential peer;
        private SecureConversationServiceCredential secureConversation;
        private X509CertificateRecipientServiceCredential serviceCertificate;
        private UserNamePasswordServiceCredential userName;
        private WindowsServiceCredential windows;

        public ServiceCredentials()
        {
            this.userName = new UserNamePasswordServiceCredential();
            this.clientCertificate = new X509CertificateInitiatorServiceCredential();
            this.serviceCertificate = new X509CertificateRecipientServiceCredential();
            this.windows = new WindowsServiceCredential();
            this.issuedToken = new IssuedTokenServiceCredential();
            this.peer = new PeerCredential();
            this.secureConversation = new SecureConversationServiceCredential();
        }

        protected ServiceCredentials(ServiceCredentials other)
        {
            if (other == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("other");
            }
            this.userName = new UserNamePasswordServiceCredential(other.userName);
            this.clientCertificate = new X509CertificateInitiatorServiceCredential(other.clientCertificate);
            this.serviceCertificate = new X509CertificateRecipientServiceCredential(other.serviceCertificate);
            this.windows = new WindowsServiceCredential(other.windows);
            this.issuedToken = new IssuedTokenServiceCredential(other.issuedToken);
            this.peer = new PeerCredential(other.peer);
            this.secureConversation = new SecureConversationServiceCredential(other.secureConversation);
        }

        public ServiceCredentials Clone()
        {
            ServiceCredentials credentials = this.CloneCore();
            if ((credentials == null) || (credentials.GetType() != base.GetType()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(System.ServiceModel.SR.GetString("CloneNotImplementedCorrectly", new object[] { base.GetType(), (credentials != null) ? credentials.ToString() : "null" })));
            }
            return credentials;
        }

        protected virtual ServiceCredentials CloneCore()
        {
            return new ServiceCredentials(this);
        }

        internal static ServiceCredentials CreateDefaultCredentials()
        {
            return new ServiceCredentials();
        }

        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            return new ServiceCredentialsSecurityTokenManager(this.Clone());
        }

        internal void MakeReadOnly()
        {
            this.ClientCertificate.MakeReadOnly();
            this.IssuedTokenAuthentication.MakeReadOnly();
            this.Peer.MakeReadOnly();
            this.SecureConversationAuthentication.MakeReadOnly();
            this.ServiceCertificate.MakeReadOnly();
            this.UserNameAuthentication.MakeReadOnly();
            this.WindowsAuthentication.MakeReadOnly();
        }

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            SecurityCredentialsManager manager = parameters.Find<SecurityCredentialsManager>();
            if (manager != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MultipleSecurityCredentialsManagersInServiceBindingParameters", new object[] { manager })));
            }
            parameters.Add(this);
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        public X509CertificateInitiatorServiceCredential ClientCertificate
        {
            get
            {
                return this.clientCertificate;
            }
        }

        public IssuedTokenServiceCredential IssuedTokenAuthentication
        {
            get
            {
                return this.issuedToken;
            }
        }

        public PeerCredential Peer
        {
            get
            {
                return this.peer;
            }
        }

        public SecureConversationServiceCredential SecureConversationAuthentication
        {
            get
            {
                return this.secureConversation;
            }
        }

        public X509CertificateRecipientServiceCredential ServiceCertificate
        {
            get
            {
                return this.serviceCertificate;
            }
        }

        public UserNamePasswordServiceCredential UserNameAuthentication
        {
            get
            {
                return this.userName;
            }
        }

        public WindowsServiceCredential WindowsAuthentication
        {
            get
            {
                return this.windows;
            }
        }
    }
}


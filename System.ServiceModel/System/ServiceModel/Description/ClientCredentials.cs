namespace System.ServiceModel.Description
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;

    public class ClientCredentials : SecurityCredentialsManager, IEndpointBehavior
    {
        private X509CertificateInitiatorClientCredential clientCertificate;
        private System.ServiceModel.Security.GetInfoCardTokenCallback getInfoCardTokenCallback;
        private HttpDigestClientCredential httpDigest;
        private bool isReadOnly;
        private IssuedTokenClientCredential issuedToken;
        private PeerCredential peer;
        private X509CertificateRecipientClientCredential serviceCertificate;
        private bool supportInteractive;
        internal const bool SupportInteractiveDefault = true;
        private UserNamePasswordClientCredential userName;
        private WindowsClientCredential windows;

        public ClientCredentials()
        {
            this.supportInteractive = true;
        }

        protected ClientCredentials(ClientCredentials other)
        {
            if (other == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("other");
            }
            if (other.userName != null)
            {
                this.userName = new UserNamePasswordClientCredential(other.userName);
            }
            if (other.clientCertificate != null)
            {
                this.clientCertificate = new X509CertificateInitiatorClientCredential(other.clientCertificate);
            }
            if (other.serviceCertificate != null)
            {
                this.serviceCertificate = new X509CertificateRecipientClientCredential(other.serviceCertificate);
            }
            if (other.windows != null)
            {
                this.windows = new WindowsClientCredential(other.windows);
            }
            if (other.httpDigest != null)
            {
                this.httpDigest = new HttpDigestClientCredential(other.httpDigest);
            }
            if (other.issuedToken != null)
            {
                this.issuedToken = new IssuedTokenClientCredential(other.issuedToken);
            }
            if (other.peer != null)
            {
                this.peer = new PeerCredential(other.peer);
            }
            this.getInfoCardTokenCallback = other.getInfoCardTokenCallback;
            this.supportInteractive = other.supportInteractive;
            this.isReadOnly = other.isReadOnly;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AddInteractiveInitializers(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
            CardSpacePolicyElement[] elementArray;
            Uri uri;
            if (InfoCardHelper.IsInfocardRequired(serviceEndpoint.Binding, this, this.CreateSecurityTokenManager(), EndpointAddress.AnonymousAddress, out elementArray, out uri))
            {
                behavior.InteractiveChannelInitializers.Add(new InfocardInteractiveChannelInitializer(this, serviceEndpoint.Binding));
            }
        }

        public virtual void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
            if (serviceEndpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpoint");
            }
            if (serviceEndpoint.Binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpoint.Binding");
            }
            if (serviceEndpoint.Binding.CreateBindingElements().Find<SecurityBindingElement>() != null)
            {
                try
                {
                    this.AddInteractiveInitializers(serviceEndpoint, behavior);
                }
                catch (FileNotFoundException)
                {
                }
            }
        }

        public ClientCredentials Clone()
        {
            ClientCredentials credentials = this.CloneCore();
            if ((credentials == null) || (credentials.GetType() != base.GetType()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(System.ServiceModel.SR.GetString("CloneNotImplementedCorrectly", new object[] { base.GetType(), (credentials != null) ? credentials.ToString() : "null" })));
            }
            return credentials;
        }

        protected virtual ClientCredentials CloneCore()
        {
            return new ClientCredentials(this);
        }

        internal static ClientCredentials CreateDefaultCredentials()
        {
            return new ClientCredentials();
        }

        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            return new ClientCredentialsSecurityTokenManager(this.Clone());
        }

        protected internal virtual SecurityToken GetInfoCardSecurityToken(bool requiresInfoCard, CardSpacePolicyElement[] chain, SecurityTokenSerializer tokenSerializer)
        {
            if (!requiresInfoCard)
            {
                return null;
            }
            return CardSpaceSelector.GetToken(chain, tokenSerializer);
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
            if (this.clientCertificate != null)
            {
                this.clientCertificate.MakeReadOnly();
            }
            if (this.serviceCertificate != null)
            {
                this.serviceCertificate.MakeReadOnly();
            }
            if (this.userName != null)
            {
                this.userName.MakeReadOnly();
            }
            if (this.windows != null)
            {
                this.windows.MakeReadOnly();
            }
            if (this.httpDigest != null)
            {
                this.httpDigest.MakeReadOnly();
            }
            if (this.issuedToken != null)
            {
                this.issuedToken.MakeReadOnly();
            }
            if (this.peer != null)
            {
                this.peer.MakeReadOnly();
            }
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
        {
            if (bindingParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingParameters");
            }
            SecurityCredentialsManager manager = bindingParameters.Find<SecurityCredentialsManager>();
            if (manager != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MultipleSecurityCredentialsManagersInChannelBindingParameters", new object[] { manager })));
            }
            bindingParameters.Add(this);
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFXEndpointBehaviorUsedOnWrongSide", new object[] { typeof(ClientCredentials).Name })));
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
        }

        public X509CertificateInitiatorClientCredential ClientCertificate
        {
            get
            {
                if (this.clientCertificate == null)
                {
                    this.clientCertificate = new X509CertificateInitiatorClientCredential();
                    if (this.isReadOnly)
                    {
                        this.clientCertificate.MakeReadOnly();
                    }
                }
                return this.clientCertificate;
            }
        }

        internal System.ServiceModel.Security.GetInfoCardTokenCallback GetInfoCardTokenCallback
        {
            get
            {
                if (this.getInfoCardTokenCallback == null)
                {
                    System.ServiceModel.Security.GetInfoCardTokenCallback callback = new System.ServiceModel.Security.GetInfoCardTokenCallback(this.GetInfoCardSecurityToken);
                    this.getInfoCardTokenCallback = callback;
                }
                return this.getInfoCardTokenCallback;
            }
        }

        public HttpDigestClientCredential HttpDigest
        {
            get
            {
                if (this.httpDigest == null)
                {
                    this.httpDigest = new HttpDigestClientCredential();
                    if (this.isReadOnly)
                    {
                        this.httpDigest.MakeReadOnly();
                    }
                }
                return this.httpDigest;
            }
        }

        public IssuedTokenClientCredential IssuedToken
        {
            get
            {
                if (this.issuedToken == null)
                {
                    this.issuedToken = new IssuedTokenClientCredential();
                    if (this.isReadOnly)
                    {
                        this.issuedToken.MakeReadOnly();
                    }
                }
                return this.issuedToken;
            }
        }

        public PeerCredential Peer
        {
            get
            {
                if (this.peer == null)
                {
                    this.peer = new PeerCredential();
                    if (this.isReadOnly)
                    {
                        this.peer.MakeReadOnly();
                    }
                }
                return this.peer;
            }
        }

        public X509CertificateRecipientClientCredential ServiceCertificate
        {
            get
            {
                if (this.serviceCertificate == null)
                {
                    this.serviceCertificate = new X509CertificateRecipientClientCredential();
                    if (this.isReadOnly)
                    {
                        this.serviceCertificate.MakeReadOnly();
                    }
                }
                return this.serviceCertificate;
            }
        }

        public bool SupportInteractive
        {
            get
            {
                return this.supportInteractive;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.supportInteractive = value;
            }
        }

        public UserNamePasswordClientCredential UserName
        {
            get
            {
                if (this.userName == null)
                {
                    this.userName = new UserNamePasswordClientCredential();
                    if (this.isReadOnly)
                    {
                        this.userName.MakeReadOnly();
                    }
                }
                return this.userName;
            }
        }

        public WindowsClientCredential Windows
        {
            get
            {
                if (this.windows == null)
                {
                    this.windows = new WindowsClientCredential();
                    if (this.isReadOnly)
                    {
                        this.windows.MakeReadOnly();
                    }
                }
                return this.windows;
            }
        }
    }
}


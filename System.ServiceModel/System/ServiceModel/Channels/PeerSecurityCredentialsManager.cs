namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    internal class PeerSecurityCredentialsManager : SecurityCredentialsManager, IEndpointBehavior, IServiceBehavior
    {
        private PeerCredential credential;
        private SecurityTokenManager manager;
        private bool messageAuth;
        private PeerAuthenticationMode mode;
        private PeerSecurityManager parent;
        private SelfSignedCertificate ssl;

        public PeerSecurityCredentialsManager()
        {
            this.mode = PeerAuthenticationMode.Password;
        }

        public PeerSecurityCredentialsManager(SecurityTokenManager manager, PeerAuthenticationMode mode, bool messageAuth)
        {
            this.mode = PeerAuthenticationMode.Password;
            this.manager = manager;
            this.mode = mode;
            this.messageAuth = messageAuth;
        }

        public PeerSecurityCredentialsManager(PeerCredential credential, PeerAuthenticationMode mode, bool messageAuth)
        {
            this.mode = PeerAuthenticationMode.Password;
            this.credential = credential;
            this.mode = mode;
            this.messageAuth = messageAuth;
        }

        public void CheckIfCompatible(PeerSecurityCredentialsManager that)
        {
            if (that == null)
            {
                PeerExceptionHelper.ThrowInvalidOperation_PeerConflictingPeerNodeSettings(PeerBindingPropertyNames.Credentials);
            }
            if (this.mode != PeerAuthenticationMode.None)
            {
                if ((this.mode == PeerAuthenticationMode.Password) && (this.Password != that.Password))
                {
                    PeerExceptionHelper.ThrowInvalidOperation_PeerConflictingPeerNodeSettings(PeerBindingPropertyNames.Password);
                }
                if (!this.Certificate.Equals((X509Certificate) that.Certificate))
                {
                    PeerExceptionHelper.ThrowInvalidOperation_PeerConflictingPeerNodeSettings(PeerBindingPropertyNames.Certificate);
                }
            }
        }

        public PeerSecurityCredentialsManager CloneForTransport()
        {
            PeerSecurityCredentialsManager manager = new PeerSecurityCredentialsManager();
            if (this.credential != null)
            {
                manager.credential = new PeerCredential(this.credential);
            }
            manager.mode = this.mode;
            manager.messageAuth = this.messageAuth;
            manager.manager = this.manager;
            manager.parent = this.parent;
            return manager;
        }

        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            if (this.manager != null)
            {
                return new PeerClientSecurityTokenManager(this.parent, this.manager, this.mode, this.messageAuth);
            }
            return new PeerClientSecurityTokenManager(this.parent, this.credential, this.mode, this.messageAuth);
        }

        public override bool Equals(object other)
        {
            PeerSecurityCredentialsManager manager = other as PeerSecurityCredentialsManager;
            if (manager == null)
            {
                return false;
            }
            if (this.credential != null)
            {
                return this.credential.Equals(manager.credential, this.mode, this.messageAuth);
            }
            return this.manager.Equals(manager.manager);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
        {
            if (bindingParameters != null)
            {
                bindingParameters.Add(this);
            }
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
        }

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            parameters.Add(this);
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        internal X509Certificate2 Certificate
        {
            get
            {
                X509Certificate2 certificate = null;
                if ((this.mode == PeerAuthenticationMode.Password) && (this.ssl != null))
                {
                    certificate = this.ssl.GetX509Certificate();
                }
                if (this.credential != null)
                {
                    certificate = this.credential.Certificate;
                }
                else
                {
                    ServiceModelSecurityTokenRequirement tokenRequirement = PeerClientSecurityTokenManager.CreateRequirement(SecurityTokenTypes.X509Certificate);
                    X509SecurityTokenProvider provider = this.manager.CreateSecurityTokenProvider(tokenRequirement) as X509SecurityTokenProvider;
                    if (provider == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TokenProvider");
                    }
                    X509SecurityToken token = provider.GetToken(ServiceDefaults.SendTimeout) as X509SecurityToken;
                    if (token == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token");
                    }
                    certificate = token.Certificate;
                }
                if ((certificate == null) && (this.mode == PeerAuthenticationMode.Password))
                {
                    this.ssl = this.parent.GetCertificate();
                    certificate = this.ssl.GetX509Certificate();
                }
                return certificate;
            }
        }

        internal PeerCredential Credential
        {
            get
            {
                return this.credential;
            }
        }

        public PeerSecurityManager Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;
            }
        }

        internal string Password
        {
            get
            {
                if (this.credential != null)
                {
                    return this.credential.MeshPassword;
                }
                ServiceModelSecurityTokenRequirement tokenRequirement = PeerClientSecurityTokenManager.CreateRequirement(SecurityTokenTypes.UserName);
                UserNameSecurityTokenProvider provider = this.manager.CreateSecurityTokenProvider(tokenRequirement) as UserNameSecurityTokenProvider;
                if (provider == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TokenProvider");
                }
                UserNameSecurityToken token = provider.GetToken(ServiceDefaults.SendTimeout) as UserNameSecurityToken;
                if ((token == null) || string.IsNullOrEmpty(token.Password))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("password");
                }
                return token.Password;
            }
        }

        public class PeerClientSecurityTokenManager : SecurityTokenManager
        {
            private PeerCredential credential;
            private SecurityTokenManager delegateManager;
            private bool messageAuth;
            private PeerAuthenticationMode mode;
            private PeerSecurityManager parent;
            private SelfSignedCertificate ssc;

            public PeerClientSecurityTokenManager(PeerSecurityManager parent, SecurityTokenManager manager, PeerAuthenticationMode mode, bool messageAuth)
            {
                this.delegateManager = manager;
                this.mode = mode;
                this.messageAuth = messageAuth;
                this.parent = parent;
            }

            public PeerClientSecurityTokenManager(PeerSecurityManager parent, PeerCredential credential, PeerAuthenticationMode mode, bool messageAuth)
            {
                this.credential = credential;
                this.mode = mode;
                this.messageAuth = messageAuth;
                this.parent = parent;
            }

            internal static ServiceModelSecurityTokenRequirement CreateRequirement(string tokenType)
            {
                return CreateRequirement(tokenType, false);
            }

            internal static ServiceModelSecurityTokenRequirement CreateRequirement(string tokenType, bool forMessageValidation)
            {
                InitiatorServiceModelSecurityTokenRequirement requirement = new InitiatorServiceModelSecurityTokenRequirement {
                    TokenType = tokenType,
                    TransportScheme = "net.p2p"
                };
                if (forMessageValidation)
                {
                    requirement.Properties[SecurityTokenRequirement.PeerAuthenticationMode] = SecurityMode.Message;
                    return requirement;
                }
                requirement.Properties[SecurityTokenRequirement.PeerAuthenticationMode] = SecurityMode.Transport;
                return requirement;
            }

            public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver)
            {
                ServiceModelSecurityTokenRequirement requirement = tokenRequirement as ServiceModelSecurityTokenRequirement;
                outOfBandTokenResolver = null;
                if (requirement == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenRequirement");
                }
                if (!this.IsX509TokenRequirement(requirement))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("tokenRequirement");
                }
                if ((this.mode == PeerAuthenticationMode.Password) && this.IsForConnectionValidator(requirement))
                {
                    return new X509SecurityTokenAuthenticator(X509CertificateValidator.None);
                }
                if (this.delegateManager != null)
                {
                    if (this.IsForConnectionValidator(requirement))
                    {
                        requirement.TransportScheme = "net.p2p";
                        requirement.Properties[SecurityTokenRequirement.PeerAuthenticationMode] = SecurityMode.Transport;
                    }
                    else
                    {
                        requirement.TransportScheme = "net.p2p";
                        requirement.Properties[SecurityTokenRequirement.PeerAuthenticationMode] = SecurityMode.Message;
                    }
                    return this.delegateManager.CreateSecurityTokenAuthenticator(tokenRequirement, out outOfBandTokenResolver);
                }
                X509CertificateValidator none = null;
                if (this.IsForConnectionValidator(requirement))
                {
                    if (this.mode == PeerAuthenticationMode.MutualCertificate)
                    {
                        if (!this.credential.PeerAuthentication.TryGetCertificateValidator(out none))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SecurityTokenManagerCannotCreateProviderForRequirement", new object[] { requirement })));
                        }
                    }
                    else
                    {
                        none = X509CertificateValidator.None;
                    }
                }
                else if (!this.credential.MessageSenderAuthentication.TryGetCertificateValidator(out none))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SecurityTokenManagerCannotCreateProviderForRequirement", new object[] { requirement })));
                }
                return new X509SecurityTokenAuthenticator(none);
            }

            public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement)
            {
                ServiceModelSecurityTokenRequirement requirement = tokenRequirement as ServiceModelSecurityTokenRequirement;
                if (requirement == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenRequirement");
                }
                if (this.IsX509TokenRequirement(requirement))
                {
                    X509CertificateValidator validator;
                    if (this.IsForConnectionValidator(requirement))
                    {
                        SecurityTokenProvider provider = null;
                        if (this.ssc != null)
                        {
                            provider = new X509SecurityTokenProvider(this.ssc.GetX509Certificate());
                        }
                        else if (this.delegateManager != null)
                        {
                            requirement.Properties[SecurityTokenRequirement.PeerAuthenticationMode] = SecurityMode.Transport;
                            requirement.TransportScheme = "net.p2p";
                            provider = this.delegateManager.CreateSecurityTokenProvider(tokenRequirement);
                        }
                        else if (this.credential.Certificate != null)
                        {
                            provider = new X509SecurityTokenProvider(this.credential.Certificate);
                        }
                        if ((provider == null) && (this.mode == PeerAuthenticationMode.Password))
                        {
                            this.ssc = this.parent.GetCertificate();
                            provider = new X509SecurityTokenProvider(this.ssc.GetX509Certificate());
                        }
                        return provider;
                    }
                    if (this.delegateManager != null)
                    {
                        requirement.TransportScheme = "net.p2p";
                        requirement.Properties[SecurityTokenRequirement.PeerAuthenticationMode] = SecurityMode.Message;
                        return this.delegateManager.CreateSecurityTokenProvider(tokenRequirement);
                    }
                    if (!this.credential.MessageSenderAuthentication.TryGetCertificateValidator(out validator))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("TokenType");
                    }
                    return new PeerX509TokenProvider(validator, this.credential.Certificate);
                }
                if (!this.IsPasswordTokenRequirement(requirement))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("TokenType");
                }
                return this.GetPasswordTokenProvider();
            }

            public override SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityTokenVersion version)
            {
                if (this.delegateManager != null)
                {
                    return this.delegateManager.CreateSecurityTokenSerializer(version);
                }
                MessageSecurityTokenVersion version2 = version as MessageSecurityTokenVersion;
                if (version2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SecurityTokenManagerCannotCreateSerializerForVersion", new object[] { version })));
                }
                return new WSSecurityTokenSerializer(version2.SecurityVersion, version2.TrustVersion, version2.SecureConversationVersion, version2.EmitBspRequiredAttributes, null, null, null);
            }

            public override bool Equals(object other)
            {
                PeerSecurityCredentialsManager.PeerClientSecurityTokenManager manager = other as PeerSecurityCredentialsManager.PeerClientSecurityTokenManager;
                if (manager == null)
                {
                    return false;
                }
                if (this.credential == null)
                {
                    return this.delegateManager.Equals(manager.delegateManager);
                }
                return ((manager.credential != null) && this.credential.Equals(manager.credential, this.mode, this.messageAuth));
            }

            public override int GetHashCode()
            {
                if (this.credential != null)
                {
                    return this.credential.GetHashCode();
                }
                if (this.delegateManager != null)
                {
                    return this.delegateManager.GetHashCode();
                }
                return 0;
            }

            private UserNameSecurityTokenProvider GetPasswordTokenProvider()
            {
                if (this.delegateManager == null)
                {
                    return new UserNameSecurityTokenProvider(string.Empty, this.credential.MeshPassword);
                }
                ServiceModelSecurityTokenRequirement tokenRequirement = CreateRequirement(SecurityTokenTypes.UserName);
                UserNameSecurityTokenProvider provider = this.delegateManager.CreateSecurityTokenProvider(tokenRequirement) as UserNameSecurityTokenProvider;
                if (provider == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SecurityTokenManagerCannotCreateProviderForRequirement", new object[] { tokenRequirement })));
                }
                return provider;
            }

            internal bool HasCompatibleMessageSecuritySettings(PeerSecurityCredentialsManager.PeerClientSecurityTokenManager that)
            {
                if (this.credential == null)
                {
                    return this.delegateManager.Equals(that.delegateManager);
                }
                return ((that.credential != null) && this.credential.Equals(that.credential));
            }

            private bool IsForConnectionValidator(ServiceModelSecurityTokenRequirement requirement)
            {
                return (((requirement.TransportScheme == "net.tcp") && (requirement.SecurityBindingElement == null)) && (requirement.MessageSecurityVersion == null));
            }

            private bool IsPasswordTokenRequirement(ServiceModelSecurityTokenRequirement requirement)
            {
                return ((requirement != null) && (requirement.TokenType == SecurityTokenTypes.UserName));
            }

            private bool IsX509TokenRequirement(ServiceModelSecurityTokenRequirement requirement)
            {
                return ((requirement != null) && (requirement.TokenType == SecurityTokenTypes.X509Certificate));
            }
        }
    }
}


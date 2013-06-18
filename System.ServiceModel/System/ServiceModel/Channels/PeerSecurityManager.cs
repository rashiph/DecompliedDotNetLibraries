namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Xml;

    internal class PeerSecurityManager
    {
        private ServiceSecurityAuditBehavior auditBehavior;
        private PeerAuthenticationMode authenticationMode;
        private byte[] authenticatorHash;
        private PeerSecurityCredentialsManager credManager;
        private bool enableSigning;
        private string meshId = string.Empty;
        public EventHandler OnNeighborAuthenticated;
        internal string password;
        private ChannelProtectionRequirements protection;
        private XmlDictionaryReaderQuotas readerQuotas;
        private DuplexSecurityProtocolFactory securityProtocolFactory;
        private SelfSignedCertificate ssc;
        private object thisLock;
        private SecurityTokenManager tokenManager;

        private PeerSecurityManager(PeerAuthenticationMode authMode, bool signing)
        {
            this.authenticationMode = authMode;
            this.enableSigning = signing;
            this.thisLock = new object();
        }

        private void Abort(IPeerNeighbor neighbor)
        {
            neighbor.Abort(PeerCloseReason.AuthenticationFailure, PeerCloseInitiator.LocalNode);
        }

        private void ApplyAuditBehaviorSettings(BindingContext context)
        {
            ServiceSecurityAuditBehavior behavior = context.BindingParameters.Find<ServiceSecurityAuditBehavior>();
            if (behavior != null)
            {
                this.auditBehavior = behavior.Clone();
            }
            else
            {
                this.auditBehavior = new ServiceSecurityAuditBehavior();
            }
        }

        public void ApplyClientSecurity(ChannelFactory<IPeerProxy> factory)
        {
            factory.Endpoint.Behaviors.Remove<ClientCredentials>();
            if (this.authenticationMode != PeerAuthenticationMode.None)
            {
                factory.Endpoint.Behaviors.Add(this.credManager.CloneForTransport());
            }
        }

        public void ApplyServiceSecurity(System.ServiceModel.Description.ServiceDescription description)
        {
            if (this.AuthenticationMode != PeerAuthenticationMode.None)
            {
                description.Behaviors.Add(this.credManager.CloneForTransport());
            }
        }

        private void ApplySigningRequirements(ScopedMessagePartSpecification spec)
        {
            MessagePartSpecification parts = new MessagePartSpecification(new XmlQualifiedName[] { new XmlQualifiedName("PeerVia", "http://schemas.microsoft.com/net/2006/05/peer"), new XmlQualifiedName("FloodMessage", "http://schemas.microsoft.com/net/2006/05/peer"), new XmlQualifiedName("PeerTo", "http://schemas.microsoft.com/net/2006/05/peer"), new XmlQualifiedName("MessageID", "http://schemas.microsoft.com/net/2006/05/peer") });
            foreach (string str in spec.Actions)
            {
                spec.AddParts(parts, str);
            }
            spec.AddParts(parts, "*");
        }

        public bool Authenticate(ServiceSecurityContext context, byte[] message)
        {
            if (context == null)
            {
                return (this.authenticationMode == PeerAuthenticationMode.None);
            }
            if (this.authenticationMode == PeerAuthenticationMode.Password)
            {
                if (context == null)
                {
                    throw Fx.AssertAndThrow("No SecurityContext attached in security mode!");
                }
                return PeerSecurityHelpers.Authenticate(FindClaim(context), this.credManager.Password, message);
            }
            if (message != null)
            {
                PeerExceptionHelper.ThrowInvalidOperation_UnexpectedSecurityTokensDuringHandshake();
            }
            return true;
        }

        public void CheckIfCompatibleNodeSettings(object other)
        {
            string propertyName = null;
            PeerSecurityManager manager = other as PeerSecurityManager;
            if (manager == null)
            {
                propertyName = PeerBindingPropertyNames.Security;
            }
            else if (this.authenticationMode != manager.authenticationMode)
            {
                propertyName = PeerBindingPropertyNames.SecurityDotMode;
            }
            else
            {
                if (this.authenticationMode == PeerAuthenticationMode.None)
                {
                    return;
                }
                if (!this.tokenManager.Equals(manager.tokenManager))
                {
                    if (this.credManager != null)
                    {
                        this.credManager.CheckIfCompatible(manager.credManager);
                    }
                    else
                    {
                        propertyName = PeerBindingPropertyNames.Credentials;
                    }
                }
            }
            if (propertyName != null)
            {
                PeerExceptionHelper.ThrowInvalidOperation_PeerConflictingPeerNodeSettings(propertyName);
            }
        }

        private static void Convert(PeerSecuritySettings security, out PeerAuthenticationMode authMode, out bool signing)
        {
            authMode = PeerAuthenticationMode.None;
            signing = false;
            if ((security.Mode == SecurityMode.Transport) || (security.Mode == SecurityMode.TransportWithMessageCredential))
            {
                switch (security.Transport.CredentialType)
                {
                    case PeerTransportCredentialType.Password:
                        authMode = PeerAuthenticationMode.Password;
                        break;

                    case PeerTransportCredentialType.Certificate:
                        authMode = PeerAuthenticationMode.MutualCertificate;
                        break;
                }
            }
            if ((security.Mode == SecurityMode.Message) || (security.Mode == SecurityMode.TransportWithMessageCredential))
            {
                signing = true;
            }
        }

        public static PeerSecurityManager Create(PeerSecuritySettings security, BindingContext context, XmlDictionaryReaderQuotas readerQuotas)
        {
            PeerAuthenticationMode none = PeerAuthenticationMode.None;
            bool signing = false;
            Convert(security, out none, out signing);
            return Create(none, signing, context, readerQuotas);
        }

        public static PeerSecurityManager Create(PeerAuthenticationMode authenticationMode, bool signMessages, BindingContext context, XmlDictionaryReaderQuotas readerQuotas)
        {
            if ((authenticationMode == PeerAuthenticationMode.None) && !signMessages)
            {
                return CreateDummy();
            }
            if (authenticationMode == PeerAuthenticationMode.Password)
            {
                try
                {
                    using (new HMACSHA256())
                    {
                        using (new SHA256Managed())
                        {
                        }
                    }
                }
                catch (InvalidOperationException exception)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    PeerExceptionHelper.ThrowInvalidOperation_InsufficientCryptoSupport(exception);
                }
            }
            ChannelProtectionRequirements reqs = context.BindingParameters.Find<ChannelProtectionRequirements>();
            PeerSecurityCredentialsManager credman = GetCredentialsManager(authenticationMode, signMessages, context);
            if (credman.Credential != null)
            {
                ValidateCredentialSettings(authenticationMode, signMessages, credman.Credential);
            }
            PeerSecurityManager manager2 = Create(authenticationMode, signMessages, credman, reqs, readerQuotas);
            credman.Parent = manager2;
            manager2.ApplyAuditBehaviorSettings(context);
            return manager2;
        }

        public static PeerSecurityManager Create(PeerAuthenticationMode authenticationMode, bool messageAuthentication, PeerSecurityCredentialsManager credman, ChannelProtectionRequirements reqs, XmlDictionaryReaderQuotas readerQuotas)
        {
            PeerSecurityManager manager = null;
            X509CertificateValidator none = null;
            X509CertificateValidator validator2 = null;
            PeerCredential credential = credman.Credential;
            if ((credential == null) && (credman == null))
            {
                if ((authenticationMode != PeerAuthenticationMode.None) || messageAuthentication)
                {
                    PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.Credentials);
                }
                return CreateDummy();
            }
            manager = new PeerSecurityManager(authenticationMode, messageAuthentication) {
                credManager = credman,
                password = credman.Password,
                readerQuotas = readerQuotas
            };
            if (reqs != null)
            {
                manager.protection = new ChannelProtectionRequirements(reqs);
            }
            manager.tokenManager = credman.CreateSecurityTokenManager();
            if (credential != null)
            {
                switch (authenticationMode)
                {
                    case PeerAuthenticationMode.Password:
                        manager.password = credential.MeshPassword;
                        if (string.IsNullOrEmpty(manager.credManager.Password))
                        {
                            PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.Password);
                        }
                        none = X509CertificateValidator.None;
                        break;

                    case PeerAuthenticationMode.MutualCertificate:
                        if (manager.credManager.Certificate == null)
                        {
                            PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.Certificate);
                        }
                        if (!credential.PeerAuthentication.TryGetCertificateValidator(out none))
                        {
                            PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.PeerAuthentication);
                        }
                        break;
                }
                if (messageAuthentication)
                {
                    if (credential.MessageSenderAuthentication != null)
                    {
                        if (!credential.MessageSenderAuthentication.TryGetCertificateValidator(out validator2))
                        {
                            PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.MessageSenderAuthentication);
                        }
                        return manager;
                    }
                    PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.MessageSenderAuthentication);
                }
            }
            return manager;
        }

        internal static PeerSecurityManager CreateDummy()
        {
            return new PeerSecurityManager(PeerAuthenticationMode.None, false);
        }

        public SecurityProtocol CreateSecurityProtocol<TChannel>(EndpointAddress target, TimeSpan timespan)
        {
            TimeoutHelper helper = new TimeoutHelper(timespan);
            SecurityProtocol protocol = this.GetProtocolFactory<TChannel>().CreateSecurityProtocol(target, null, null, false, helper.RemainingTime());
            if (protocol != null)
            {
                protocol.Open(helper.RemainingTime());
            }
            return protocol;
        }

        private void CreateSecurityProtocolFactory()
        {
            lock (this.ThisLock)
            {
                if (this.securityProtocolFactory == null)
                {
                    SecurityProtocolFactory factory;
                    SecurityProtocolFactory factory2;
                    TimeoutHelper helper = new TimeoutHelper(ServiceDefaults.SendTimeout);
                    if (!this.enableSigning)
                    {
                        factory2 = new PeerDoNothingSecurityProtocolFactory();
                        factory = new PeerDoNothingSecurityProtocolFactory();
                    }
                    else
                    {
                        ChannelProtectionRequirements requirements;
                        SecurityTokenResolver resolver;
                        if (this.credManager.Certificate != null)
                        {
                            SecurityBindingElement element = SecurityBindingElement.CreateCertificateSignatureBindingElement();
                            element.ReaderQuotas = this.readerQuotas;
                            BindingParameterCollection parameters = new BindingParameterCollection();
                            if (this.protection == null)
                            {
                                requirements = new ChannelProtectionRequirements();
                            }
                            else
                            {
                                requirements = new ChannelProtectionRequirements(this.protection);
                            }
                            this.ApplySigningRequirements(requirements.IncomingSignatureParts);
                            this.ApplySigningRequirements(requirements.OutgoingSignatureParts);
                            parameters.Add(requirements);
                            parameters.Add(this.auditBehavior);
                            parameters.Add(this.credManager);
                            BindingContext context = new BindingContext(new CustomBinding(new BindingElement[] { element }), parameters);
                            factory2 = element.CreateSecurityProtocolFactory<IOutputChannel>(context, this.credManager, false, null);
                        }
                        else
                        {
                            factory2 = new PeerDoNothingSecurityProtocolFactory();
                        }
                        if (this.tokenManager.CreateSecurityTokenAuthenticator(PeerSecurityCredentialsManager.PeerClientSecurityTokenManager.CreateRequirement(SecurityTokenTypes.X509Certificate, true), out resolver) is X509SecurityTokenAuthenticator)
                        {
                            SecurityBindingElement element2 = SecurityBindingElement.CreateCertificateSignatureBindingElement();
                            element2.ReaderQuotas = this.readerQuotas;
                            BindingParameterCollection parameters2 = new BindingParameterCollection();
                            if (this.protection == null)
                            {
                                requirements = new ChannelProtectionRequirements();
                            }
                            else
                            {
                                requirements = new ChannelProtectionRequirements(this.protection);
                            }
                            this.ApplySigningRequirements(requirements.IncomingSignatureParts);
                            this.ApplySigningRequirements(requirements.OutgoingSignatureParts);
                            parameters2.Add(requirements);
                            parameters2.Add(this.auditBehavior);
                            parameters2.Add(this.credManager);
                            BindingContext context2 = new BindingContext(new CustomBinding(new BindingElement[] { element2 }), parameters2);
                            factory = element2.CreateSecurityProtocolFactory<IOutputChannel>(context2, this.credManager, true, null);
                        }
                        else
                        {
                            factory = new PeerDoNothingSecurityProtocolFactory();
                        }
                    }
                    DuplexSecurityProtocolFactory factory3 = new DuplexSecurityProtocolFactory(factory2, factory);
                    factory3.Open(true, helper.RemainingTime());
                    this.securityProtocolFactory = factory3;
                }
            }
        }

        public static Claim FindClaim(ServiceSecurityContext context)
        {
            for (int i = 0; i < context.AuthorizationContext.ClaimSets.Count; i++)
            {
                ClaimSet set = context.AuthorizationContext.ClaimSets[i];
                IEnumerator<Claim> enumerator = set.FindClaims(ClaimTypes.Rsa, null).GetEnumerator();
                if (enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
            }
            return null;
        }

        public byte[] GetAuthenticator()
        {
            if (this.authenticationMode != PeerAuthenticationMode.Password)
            {
                return null;
            }
            if (this.authenticatorHash == null)
            {
                lock (this.ThisLock)
                {
                    if (this.authenticatorHash == null)
                    {
                        this.authenticatorHash = PeerSecurityHelpers.ComputeHash(this.credManager.Certificate, this.credManager.Password);
                    }
                }
            }
            return this.authenticatorHash;
        }

        internal SelfSignedCertificate GetCertificate()
        {
            if (this.ssc == null)
            {
                lock (this.ThisLock)
                {
                    if (this.ssc == null)
                    {
                        this.ssc = SelfSignedCertificate.Create("CN=" + Guid.NewGuid().ToString(), this.Password);
                    }
                }
            }
            return this.ssc;
        }

        private static PeerSecurityCredentialsManager GetCredentialsManager(PeerAuthenticationMode mode, bool signing, BindingContext context)
        {
            if ((mode == PeerAuthenticationMode.None) && !signing)
            {
                return null;
            }
            ClientCredentials credentials = context.BindingParameters.Find<ClientCredentials>();
            if (credentials != null)
            {
                return new PeerSecurityCredentialsManager(credentials.Peer, mode, signing);
            }
            ServiceCredentials credentials2 = context.BindingParameters.Find<ServiceCredentials>();
            if (credentials2 != null)
            {
                return new PeerSecurityCredentialsManager(credentials2.Peer, mode, signing);
            }
            SecurityCredentialsManager manager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (manager == null)
            {
                PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.Credentials);
            }
            return new PeerSecurityCredentialsManager(manager.CreateSecurityTokenManager(), mode, signing);
        }

        public PeerHashToken GetExpectedTokenForClaim(Claim claim)
        {
            return new PeerHashToken(claim, this.password);
        }

        public SecurityProtocolFactory GetProtocolFactory<TChannel>()
        {
            if (this.securityProtocolFactory == null)
            {
                this.CreateSecurityProtocolFactory();
            }
            if (typeof(TChannel) == typeof(IOutputChannel))
            {
                if (this.enableSigning && (this.securityProtocolFactory.ForwardProtocolFactory is PeerDoNothingSecurityProtocolFactory))
                {
                    PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.MessageSenderAuthentication);
                }
                return this.securityProtocolFactory.ForwardProtocolFactory;
            }
            if (typeof(TChannel) == typeof(IInputChannel))
            {
                if (this.enableSigning && (this.securityProtocolFactory.ReverseProtocolFactory is PeerDoNothingSecurityProtocolFactory))
                {
                    PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.MessageSenderAuthentication);
                }
                return this.securityProtocolFactory.ReverseProtocolFactory;
            }
            if (this.enableSigning && ((this.securityProtocolFactory.ReverseProtocolFactory is PeerDoNothingSecurityProtocolFactory) || (this.securityProtocolFactory.ForwardProtocolFactory is PeerDoNothingSecurityProtocolFactory)))
            {
                PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.MessageSenderAuthentication);
            }
            return this.securityProtocolFactory;
        }

        public BindingElement GetSecurityBindingElement()
        {
            SslStreamSecurityBindingElement element = null;
            if (this.AuthenticationMode != PeerAuthenticationMode.None)
            {
                element = new SslStreamSecurityBindingElement {
                    IdentityVerifier = new PeerIdentityVerifier(),
                    RequireClientCertificate = true
                };
            }
            return element;
        }

        public PeerHashToken GetSelfToken()
        {
            if (this.authenticationMode != PeerAuthenticationMode.Password)
            {
                throw Fx.AssertAndThrow("unexpected call to GetSelfToken");
            }
            return new PeerHashToken(this.credManager.Certificate, this.credManager.Password);
        }

        public bool HasCompatibleMessageSecurity(PeerSecurityManager that)
        {
            return (this.MessageAuthentication == that.MessageAuthentication);
        }

        public void OnNeighborOpened(object sender, EventArgs args)
        {
            IPeerNeighbor neighbor = sender as IPeerNeighbor;
            EventHandler onNeighborAuthenticated = this.OnNeighborAuthenticated;
            if (onNeighborAuthenticated == null)
            {
                neighbor.Abort(PeerCloseReason.LeavingMesh, PeerCloseInitiator.LocalNode);
            }
            else if (this.authenticationMode == PeerAuthenticationMode.Password)
            {
                if (neighbor.Extensions.Find<PeerChannelAuthenticatorExtension>() != null)
                {
                    throw Fx.AssertAndThrow("extension already exists!");
                }
                PeerChannelAuthenticatorExtension item = new PeerChannelAuthenticatorExtension(this, onNeighborAuthenticated, args, this.MeshId);
                neighbor.Extensions.Add(item);
                if (neighbor.IsInitiator)
                {
                    item.InitiateHandShake();
                }
            }
            else
            {
                neighbor.TrySetState(PeerNeighborState.Authenticated);
                onNeighborAuthenticated(sender, args);
            }
        }

        public void Open()
        {
            this.CreateSecurityProtocolFactory();
        }

        public Message ProcessRequest(IPeerNeighbor neighbor, Message request)
        {
            if ((this.authenticationMode != PeerAuthenticationMode.Password) || (request == null))
            {
                this.Abort(neighbor);
                return null;
            }
            PeerChannelAuthenticatorExtension extension = neighbor.Extensions.Find<PeerChannelAuthenticatorExtension>();
            Claim claim = FindClaim(ServiceSecurityContext.Current);
            if ((extension == null) || (claim == null))
            {
                throw Fx.AssertAndThrow("No suitable claim found in the context to do security negotiation!");
            }
            return extension.ProcessRst(request, claim);
        }

        private static void ValidateCredentialSettings(PeerAuthenticationMode authenticationMode, bool signMessages, PeerCredential credential)
        {
            if ((authenticationMode != PeerAuthenticationMode.None) || signMessages)
            {
                X509CertificateValidator validator;
                switch (authenticationMode)
                {
                    case PeerAuthenticationMode.Password:
                        if (string.IsNullOrEmpty(credential.MeshPassword))
                        {
                            PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.Password);
                        }
                        break;

                    case PeerAuthenticationMode.MutualCertificate:
                        if (credential.Certificate == null)
                        {
                            PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.Certificate);
                        }
                        if (!credential.PeerAuthentication.TryGetCertificateValidator(out validator))
                        {
                            PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.PeerAuthentication);
                        }
                        break;
                }
                if (signMessages && !credential.MessageSenderAuthentication.TryGetCertificateValidator(out validator))
                {
                    PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.MessageSenderAuthentication);
                }
            }
        }

        public PeerAuthenticationMode AuthenticationMode
        {
            get
            {
                return this.authenticationMode;
            }
        }

        internal string MeshId
        {
            get
            {
                return this.meshId;
            }
            set
            {
                this.meshId = value;
            }
        }

        public bool MessageAuthentication
        {
            get
            {
                return this.enableSigning;
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }
        }

        public X509Certificate2 SelfCert
        {
            get
            {
                return this.credManager.Certificate;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }
    }
}


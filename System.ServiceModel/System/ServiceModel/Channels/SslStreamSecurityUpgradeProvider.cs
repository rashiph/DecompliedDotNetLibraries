namespace System.ServiceModel.Channels
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    internal class SslStreamSecurityUpgradeProvider : StreamSecurityUpgradeProvider, IStreamUpgradeChannelBindingProvider, IChannelBindingProvider
    {
        private SecurityTokenAuthenticator clientCertificateAuthenticator;
        private SecurityTokenManager clientSecurityTokenManager;
        private bool enableChannelBinding;
        private EndpointIdentity identity;
        private System.ServiceModel.Security.IdentityVerifier identityVerifier;
        private bool requireClientCertificate;
        private string scheme;
        private X509Certificate2 serverCertificate;
        private SecurityTokenProvider serverTokenProvider;

        private SslStreamSecurityUpgradeProvider(IDefaultCommunicationTimeouts timeouts, SecurityTokenManager clientSecurityTokenManager, bool requireClientCertificate, string scheme, System.ServiceModel.Security.IdentityVerifier identityVerifier) : base(timeouts)
        {
            this.identityVerifier = identityVerifier;
            this.scheme = scheme;
            this.clientSecurityTokenManager = clientSecurityTokenManager;
            this.requireClientCertificate = requireClientCertificate;
        }

        private SslStreamSecurityUpgradeProvider(IDefaultCommunicationTimeouts timeouts, SecurityTokenProvider serverTokenProvider, bool requireClientCertificate, SecurityTokenAuthenticator clientCertificateAuthenticator, string scheme, System.ServiceModel.Security.IdentityVerifier identityVerifier) : base(timeouts)
        {
            this.serverTokenProvider = serverTokenProvider;
            this.requireClientCertificate = requireClientCertificate;
            this.clientCertificateAuthenticator = clientCertificateAuthenticator;
            this.identityVerifier = identityVerifier;
            this.scheme = scheme;
        }

        private void CleanupServerCertificate()
        {
            if (this.serverCertificate != null)
            {
                this.serverCertificate.Reset();
                this.serverCertificate = null;
            }
        }

        public static SslStreamSecurityUpgradeProvider CreateClientProvider(SslStreamSecurityBindingElement bindingElement, BindingContext context)
        {
            SecurityCredentialsManager manager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (manager == null)
            {
                manager = ClientCredentials.CreateDefaultCredentials();
            }
            return new SslStreamSecurityUpgradeProvider(context.Binding, manager.CreateSecurityTokenManager(), bindingElement.RequireClientCertificate, context.Binding.Scheme, bindingElement.IdentityVerifier);
        }

        public static SslStreamSecurityUpgradeProvider CreateServerProvider(SslStreamSecurityBindingElement bindingElement, BindingContext context)
        {
            SecurityCredentialsManager manager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (manager == null)
            {
                manager = ServiceCredentials.CreateDefaultCredentials();
            }
            Uri listenUri = TransportSecurityHelpers.GetListenUri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress);
            SecurityTokenManager tokenManager = manager.CreateSecurityTokenManager();
            RecipientServiceModelSecurityTokenRequirement tokenRequirement = new RecipientServiceModelSecurityTokenRequirement {
                TokenType = SecurityTokenTypes.X509Certificate,
                RequireCryptographicToken = true,
                KeyUsage = SecurityKeyUsage.Exchange,
                TransportScheme = context.Binding.Scheme,
                ListenUri = listenUri
            };
            SecurityTokenProvider serverTokenProvider = tokenManager.CreateSecurityTokenProvider(tokenRequirement);
            if (serverTokenProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ClientCredentialsUnableToCreateLocalTokenProvider", new object[] { tokenRequirement })));
            }
            return new SslStreamSecurityUpgradeProvider(context.Binding, serverTokenProvider, bindingElement.RequireClientCertificate, TransportSecurityHelpers.GetCertificateTokenAuthenticator(tokenManager, context.Binding.Scheme, listenUri), context.Binding.Scheme, bindingElement.IdentityVerifier);
        }

        public override StreamUpgradeAcceptor CreateUpgradeAcceptor()
        {
            base.ThrowIfDisposedOrNotOpen();
            return new SslStreamSecurityUpgradeAcceptor(this);
        }

        public override StreamUpgradeInitiator CreateUpgradeInitiator(EndpointAddress remoteAddress, Uri via)
        {
            base.ThrowIfDisposedOrNotOpen();
            return new SslStreamSecurityUpgradeInitiator(this, remoteAddress, via);
        }

        public override T GetProperty<T>() where T: class
        {
            if (!(typeof(T) == typeof(IChannelBindingProvider)) && !(typeof(T) == typeof(IStreamUpgradeChannelBindingProvider)))
            {
                return base.GetProperty<T>();
            }
            return (T) this;
        }

        protected override void OnAbort()
        {
            if (this.clientCertificateAuthenticator != null)
            {
                System.ServiceModel.Security.SecurityUtils.AbortTokenAuthenticatorIfRequired(this.clientCertificateAuthenticator);
            }
            this.CleanupServerCertificate();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return System.ServiceModel.Security.SecurityUtils.BeginCloseTokenAuthenticatorIfRequired(this.clientCertificateAuthenticator, timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult(this, timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            if (this.clientCertificateAuthenticator != null)
            {
                System.ServiceModel.Security.SecurityUtils.CloseTokenAuthenticatorIfRequired(this.clientCertificateAuthenticator, timeout);
            }
            this.CleanupServerCertificate();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            System.ServiceModel.Security.SecurityUtils.EndCloseTokenAuthenticatorIfRequired(result);
            this.CleanupServerCertificate();
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            System.ServiceModel.Security.SecurityUtils.OpenTokenAuthenticatorIfRequired(this.ClientCertificateAuthenticator, helper.RemainingTime());
            if (this.serverTokenProvider != null)
            {
                System.ServiceModel.Security.SecurityUtils.OpenTokenProviderIfRequired(this.serverTokenProvider, helper.RemainingTime());
                SecurityToken token = this.serverTokenProvider.GetToken(timeout);
                this.SetupServerCertificate(token);
                System.ServiceModel.Security.SecurityUtils.CloseTokenProviderIfRequired(this.serverTokenProvider, helper.RemainingTime());
                this.serverTokenProvider = null;
            }
        }

        private void SetupServerCertificate(SecurityToken token)
        {
            X509SecurityToken token2 = token as X509SecurityToken;
            if (token2 == null)
            {
                System.ServiceModel.Security.SecurityUtils.AbortTokenProviderIfRequired(this.serverTokenProvider);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidTokenProvided", new object[] { this.serverTokenProvider.GetType(), typeof(X509SecurityToken) })));
            }
            this.serverCertificate = new X509Certificate2(token2.Certificate);
        }

        void IChannelBindingProvider.EnableChannelBindingSupport()
        {
            this.enableChannelBinding = true;
        }

        ChannelBinding IStreamUpgradeChannelBindingProvider.GetChannelBinding(StreamUpgradeAcceptor upgradeAcceptor, ChannelBindingKind kind)
        {
            if (upgradeAcceptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("upgradeAcceptor");
            }
            SslStreamSecurityUpgradeAcceptor acceptor = upgradeAcceptor as SslStreamSecurityUpgradeAcceptor;
            if (acceptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("upgradeAcceptor", System.ServiceModel.SR.GetString("UnsupportedUpgradeAcceptor", new object[] { upgradeAcceptor.GetType() }));
            }
            if (kind != ChannelBindingKind.Endpoint)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("kind", System.ServiceModel.SR.GetString("StreamUpgradeUnsupportedChannelBindingKind", new object[] { base.GetType(), kind }));
            }
            return acceptor.ChannelBinding;
        }

        ChannelBinding IStreamUpgradeChannelBindingProvider.GetChannelBinding(StreamUpgradeInitiator upgradeInitiator, ChannelBindingKind kind)
        {
            if (upgradeInitiator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("upgradeInitiator");
            }
            SslStreamSecurityUpgradeInitiator initiator = upgradeInitiator as SslStreamSecurityUpgradeInitiator;
            if (initiator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("upgradeInitiator", System.ServiceModel.SR.GetString("UnsupportedUpgradeInitiator", new object[] { upgradeInitiator.GetType() }));
            }
            if (kind != ChannelBindingKind.Endpoint)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("kind", System.ServiceModel.SR.GetString("StreamUpgradeUnsupportedChannelBindingKind", new object[] { base.GetType(), kind }));
            }
            return initiator.ChannelBinding;
        }

        public SecurityTokenAuthenticator ClientCertificateAuthenticator
        {
            get
            {
                if (this.clientCertificateAuthenticator == null)
                {
                    this.clientCertificateAuthenticator = new X509SecurityTokenAuthenticator(X509ClientCertificateAuthentication.DefaultCertificateValidator);
                }
                return this.clientCertificateAuthenticator;
            }
        }

        public SecurityTokenManager ClientSecurityTokenManager
        {
            get
            {
                return this.clientSecurityTokenManager;
            }
        }

        public override EndpointIdentity Identity
        {
            get
            {
                if ((this.identity == null) && (this.serverCertificate != null))
                {
                    this.identity = System.ServiceModel.Security.SecurityUtils.GetServiceCertificateIdentity(this.serverCertificate);
                }
                return this.identity;
            }
        }

        public System.ServiceModel.Security.IdentityVerifier IdentityVerifier
        {
            get
            {
                return this.identityVerifier;
            }
        }

        public bool RequireClientCertificate
        {
            get
            {
                return this.requireClientCertificate;
            }
        }

        public string Scheme
        {
            get
            {
                return this.scheme;
            }
        }

        public X509Certificate2 ServerCertificate
        {
            get
            {
                return this.serverCertificate;
            }
        }

        bool IChannelBindingProvider.IsChannelBindingSupportEnabled
        {
            get
            {
                return this.enableChannelBinding;
            }
        }

        private class OpenAsyncResult : AsyncResult
        {
            private AsyncCallback onCloseTokenProvider;
            private AsyncCallback onGetToken;
            private AsyncCallback onOpenTokenAuthenticator;
            private AsyncCallback onOpenTokenProvider;
            private SslStreamSecurityUpgradeProvider parent;
            private TimeoutHelper timeoutHelper;

            public OpenAsyncResult(SslStreamSecurityUpgradeProvider parent, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.parent = parent;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.onOpenTokenAuthenticator = Fx.ThunkCallback(new AsyncCallback(this.OnOpenTokenAuthenticator));
                IAsyncResult result = System.ServiceModel.Security.SecurityUtils.BeginOpenTokenAuthenticatorIfRequired(parent.ClientCertificateAuthenticator, this.timeoutHelper.RemainingTime(), this.onOpenTokenAuthenticator, this);
                if (result.CompletedSynchronously && this.HandleOpenAuthenticatorComplete(result))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SslStreamSecurityUpgradeProvider.OpenAsyncResult>(result);
            }

            private bool HandleCloseTokenProviderComplete(IAsyncResult result)
            {
                System.ServiceModel.Security.SecurityUtils.EndCloseTokenProviderIfRequired(result);
                this.parent.serverTokenProvider = null;
                return true;
            }

            private bool HandleGetTokenComplete(IAsyncResult result)
            {
                SecurityToken token = this.parent.serverTokenProvider.EndGetToken(result);
                this.parent.SetupServerCertificate(token);
                this.onCloseTokenProvider = Fx.ThunkCallback(new AsyncCallback(this.OnCloseTokenProvider));
                IAsyncResult result2 = System.ServiceModel.Security.SecurityUtils.BeginCloseTokenProviderIfRequired(this.parent.serverTokenProvider, this.timeoutHelper.RemainingTime(), this.onCloseTokenProvider, this);
                if (!result2.CompletedSynchronously)
                {
                    return false;
                }
                return this.HandleCloseTokenProviderComplete(result2);
            }

            private bool HandleOpenAuthenticatorComplete(IAsyncResult result)
            {
                System.ServiceModel.Security.SecurityUtils.EndOpenTokenAuthenticatorIfRequired(result);
                if (this.parent.serverTokenProvider == null)
                {
                    return true;
                }
                this.onOpenTokenProvider = Fx.ThunkCallback(new AsyncCallback(this.OnOpenTokenProvider));
                IAsyncResult result2 = System.ServiceModel.Security.SecurityUtils.BeginOpenTokenProviderIfRequired(this.parent.serverTokenProvider, this.timeoutHelper.RemainingTime(), this.onOpenTokenProvider, this);
                if (!result2.CompletedSynchronously)
                {
                    return false;
                }
                return this.HandleOpenTokenProviderComplete(result2);
            }

            private bool HandleOpenTokenProviderComplete(IAsyncResult result)
            {
                System.ServiceModel.Security.SecurityUtils.EndOpenTokenProviderIfRequired(result);
                this.onGetToken = Fx.ThunkCallback(new AsyncCallback(this.OnGetToken));
                IAsyncResult result2 = this.parent.serverTokenProvider.BeginGetToken(this.timeoutHelper.RemainingTime(), this.onGetToken, this);
                if (!result2.CompletedSynchronously)
                {
                    return false;
                }
                return this.HandleGetTokenComplete(result2);
            }

            private void OnCloseTokenProvider(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Exception exception = null;
                    bool flag = false;
                    try
                    {
                        flag = this.HandleCloseTokenProviderComplete(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    if (flag)
                    {
                        base.Complete(false, exception);
                    }
                }
            }

            private void OnGetToken(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Exception exception = null;
                    bool flag = false;
                    try
                    {
                        flag = this.HandleGetTokenComplete(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    if (flag)
                    {
                        base.Complete(false, exception);
                    }
                }
            }

            private void OnOpenTokenAuthenticator(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Exception exception = null;
                    bool flag = false;
                    try
                    {
                        flag = this.HandleOpenAuthenticatorComplete(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    if (flag)
                    {
                        base.Complete(false, exception);
                    }
                }
            }

            private void OnOpenTokenProvider(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Exception exception = null;
                    bool flag = false;
                    try
                    {
                        flag = this.HandleOpenTokenProviderComplete(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    if (flag)
                    {
                        base.Complete(false, exception);
                    }
                }
            }
        }
    }
}


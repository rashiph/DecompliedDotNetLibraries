namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.Xml;

    internal class TlsnegoTokenProvider : SspiNegotiationTokenProvider
    {
        private SecurityTokenProvider clientTokenProvider;
        private SecurityTokenAuthenticator serverTokenAuthenticator;

        protected override IAsyncResult BeginCreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.EnsureEndpointAddressDoesNotRequireEncryption(target);
            if (this.ClientTokenProvider == null)
            {
                return new CompletedAsyncResult<SspiNegotiationTokenProviderState>(this.CreateTlsSspiState(null), callback, state);
            }
            return new CreateSspiStateAsyncResult(target, via, this, timeout, callback, state);
        }

        protected override SspiNegotiationTokenProviderState CreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout)
        {
            X509SecurityToken token;
            base.EnsureEndpointAddressDoesNotRequireEncryption(target);
            if (this.ClientTokenProvider == null)
            {
                token = null;
            }
            else
            {
                token = ValidateToken(this.ClientTokenProvider.GetToken(timeout));
            }
            return this.CreateTlsSspiState(token);
        }

        protected override bool CreateNegotiationStateCompletesSynchronously(EndpointAddress target, Uri via)
        {
            return (this.ClientTokenProvider == null);
        }

        private SspiNegotiationTokenProviderState CreateTlsSspiState(X509SecurityToken token)
        {
            X509Certificate2 certificate;
            if (token == null)
            {
                certificate = null;
            }
            else
            {
                certificate = token.Certificate;
            }
            return new SspiNegotiationTokenProviderState(new TlsSspiNegotiation(string.Empty, SchProtocols.TlsClient | SchProtocols.Ssl3Client, certificate));
        }

        protected override SspiNegotiationTokenProviderState EndCreateNegotiationState(IAsyncResult result)
        {
            if (result is CompletedAsyncResult<SspiNegotiationTokenProviderState>)
            {
                return CompletedAsyncResult<SspiNegotiationTokenProviderState>.End(result);
            }
            return CreateSspiStateAsyncResult.End(result);
        }

        public override void OnAbort()
        {
            if (this.clientTokenProvider != null)
            {
                System.ServiceModel.Security.SecurityUtils.AbortTokenProviderIfRequired(this.ClientTokenProvider);
                this.clientTokenProvider = null;
            }
            if (this.serverTokenAuthenticator != null)
            {
                System.ServiceModel.Security.SecurityUtils.AbortTokenAuthenticatorIfRequired(this.ServerTokenAuthenticator);
                this.serverTokenAuthenticator = null;
            }
            base.OnAbort();
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.clientTokenProvider != null)
            {
                System.ServiceModel.Security.SecurityUtils.CloseTokenProviderIfRequired(this.ClientTokenProvider, helper.RemainingTime());
                this.clientTokenProvider = null;
            }
            if (this.serverTokenAuthenticator != null)
            {
                System.ServiceModel.Security.SecurityUtils.CloseTokenAuthenticatorIfRequired(this.ServerTokenAuthenticator, helper.RemainingTime());
                this.serverTokenAuthenticator = null;
            }
            base.OnClose(helper.RemainingTime());
        }

        public override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.ClientTokenProvider != null)
            {
                System.ServiceModel.Security.SecurityUtils.OpenTokenProviderIfRequired(this.ClientTokenProvider, helper.RemainingTime());
            }
            if (this.ServerTokenAuthenticator != null)
            {
                System.ServiceModel.Security.SecurityUtils.OpenTokenAuthenticatorIfRequired(this.ServerTokenAuthenticator, helper.RemainingTime());
            }
            base.OnOpen(helper.RemainingTime());
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateSspiNegotiation(ISspiNegotiation sspiNegotiation)
        {
            TlsSspiNegotiation negotiation = (TlsSspiNegotiation) sspiNegotiation;
            if (!negotiation.IsValidContext)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidSspiNegotiation")));
            }
            X509Certificate2 remoteCertificate = negotiation.RemoteCertificate;
            if (remoteCertificate == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("ServerCertificateNotProvided")));
            }
            if (this.ServerTokenAuthenticator != null)
            {
                X509SecurityToken token = new X509SecurityToken(remoteCertificate, false);
                return this.ServerTokenAuthenticator.ValidateToken(token);
            }
            return System.ServiceModel.Security.EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
        }

        private static X509SecurityToken ValidateToken(SecurityToken token)
        {
            X509SecurityToken token2 = token as X509SecurityToken;
            if ((token2 == null) && (token != null))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TokenProviderReturnedBadToken", new object[] { token.GetType().ToString() })));
            }
            return token2;
        }

        public SecurityTokenProvider ClientTokenProvider
        {
            get
            {
                return this.clientTokenProvider;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.clientTokenProvider = value;
            }
        }

        public override XmlDictionaryString NegotiationValueType
        {
            get
            {
                if (base.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
                {
                    return System.ServiceModel.XD.TrustApr2004Dictionary.TlsnegoValueTypeUri;
                }
                if (base.StandardsManager.MessageSecurityVersion.TrustVersion != TrustVersion.WSTrust13)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
                }
                return DXD.TrustDec2005Dictionary.TlsnegoValueTypeUri;
            }
        }

        public SecurityTokenAuthenticator ServerTokenAuthenticator
        {
            get
            {
                return this.serverTokenAuthenticator;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.serverTokenAuthenticator = value;
            }
        }

        private class CreateSspiStateAsyncResult : AsyncResult
        {
            private static readonly AsyncCallback getTokensCallback = Fx.ThunkCallback(new AsyncCallback(TlsnegoTokenProvider.CreateSspiStateAsyncResult.GetTokensCallback));
            private SspiNegotiationTokenProviderState sspiState;
            private TlsnegoTokenProvider tlsTokenProvider;

            public CreateSspiStateAsyncResult(EndpointAddress target, Uri via, TlsnegoTokenProvider tlsTokenProvider, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.tlsTokenProvider = tlsTokenProvider;
                IAsyncResult result = this.tlsTokenProvider.ClientTokenProvider.BeginGetToken(timeout, getTokensCallback, this);
                if (result.CompletedSynchronously)
                {
                    X509SecurityToken token = TlsnegoTokenProvider.ValidateToken(this.tlsTokenProvider.ClientTokenProvider.EndGetToken(result));
                    this.sspiState = this.tlsTokenProvider.CreateTlsSspiState(token);
                    base.Complete(true);
                }
            }

            public static SspiNegotiationTokenProviderState End(IAsyncResult result)
            {
                return AsyncResult.End<TlsnegoTokenProvider.CreateSspiStateAsyncResult>(result).sspiState;
            }

            private static void GetTokensCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    TlsnegoTokenProvider.CreateSspiStateAsyncResult asyncState = (TlsnegoTokenProvider.CreateSspiStateAsyncResult) result.AsyncState;
                    try
                    {
                        X509SecurityToken token = TlsnegoTokenProvider.ValidateToken(asyncState.tlsTokenProvider.ClientTokenProvider.EndGetToken(result));
                        asyncState.sspiState = asyncState.tlsTokenProvider.CreateTlsSspiState(token);
                        asyncState.Complete(false);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        asyncState.Complete(false, exception);
                    }
                }
            }
        }
    }
}


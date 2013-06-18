namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Net.Security;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Authentication;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    internal class SslStreamSecurityUpgradeInitiator : StreamSecurityUpgradeInitiatorBase
    {
        private System.Security.Authentication.ExtendedProtection.ChannelBinding channelBindingToken;
        private SecurityTokenProvider clientCertificateProvider;
        private static LocalCertificateSelectionCallback clientCertificateSelectionCallback;
        private X509SecurityToken clientToken;
        private SslStreamSecurityUpgradeProvider parent;
        private SecurityTokenAuthenticator serverCertificateAuthenticator;
        private SecurityMessageProperty serverSecurity;

        public SslStreamSecurityUpgradeInitiator(SslStreamSecurityUpgradeProvider parent, EndpointAddress remoteAddress, Uri via) : base("application/ssl-tls", remoteAddress, via)
        {
            SecurityTokenResolver resolver;
            this.parent = parent;
            InitiatorServiceModelSecurityTokenRequirement tokenRequirement = new InitiatorServiceModelSecurityTokenRequirement {
                TokenType = SecurityTokenTypes.X509Certificate,
                RequireCryptographicToken = true,
                KeyUsage = SecurityKeyUsage.Exchange,
                TargetAddress = remoteAddress,
                Via = via,
                TransportScheme = this.parent.Scheme
            };
            this.serverCertificateAuthenticator = parent.ClientSecurityTokenManager.CreateSecurityTokenAuthenticator(tokenRequirement, out resolver);
            if (parent.RequireClientCertificate)
            {
                InitiatorServiceModelSecurityTokenRequirement requirement2 = new InitiatorServiceModelSecurityTokenRequirement {
                    TokenType = SecurityTokenTypes.X509Certificate,
                    RequireCryptographicToken = true,
                    KeyUsage = SecurityKeyUsage.Signature,
                    TargetAddress = remoteAddress,
                    Via = via,
                    TransportScheme = this.parent.Scheme
                };
                this.clientCertificateProvider = parent.ClientSecurityTokenManager.CreateSecurityTokenProvider(requirement2);
                if (this.clientCertificateProvider == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ClientCredentialsUnableToCreateLocalTokenProvider", new object[] { requirement2 })));
                }
            }
        }

        private IAsyncResult BaseBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return base.BeginClose(timeout, callback, state);
        }

        private IAsyncResult BaseBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return base.BeginOpen(timeout, callback, state);
        }

        private void BaseEndClose(IAsyncResult result)
        {
            base.EndClose(result);
        }

        private void BaseEndOpen(IAsyncResult result)
        {
            base.EndOpen(result);
        }

        internal override IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(this, timeout, callback, state);
        }

        internal override IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult(this, timeout, callback, state);
        }

        internal override void Close(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.Close(helper.RemainingTime());
            if (this.clientCertificateProvider != null)
            {
                System.ServiceModel.Security.SecurityUtils.CloseTokenProviderIfRequired(this.clientCertificateProvider, helper.RemainingTime());
            }
        }

        internal override void EndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        internal override void EndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        protected override IAsyncResult OnBeginInitiateUpgrade(Stream stream, AsyncCallback callback, object state)
        {
            InitiateUpgradeAsyncResult result = new InitiateUpgradeAsyncResult(this, callback, state);
            result.Begin(stream);
            return result;
        }

        protected override Stream OnEndInitiateUpgrade(IAsyncResult result, out SecurityMessageProperty remoteSecurity)
        {
            return InitiateUpgradeAsyncResult.End(result, out remoteSecurity, out this.channelBindingToken);
        }

        protected override Stream OnInitiateUpgrade(Stream stream, out SecurityMessageProperty remoteSecurity)
        {
            X509CertificateCollection clientCertificates = null;
            LocalCertificateSelectionCallback userCertificateSelectionCallback = null;
            if (this.clientToken != null)
            {
                clientCertificates = new X509CertificateCollection();
                clientCertificates.Add(this.clientToken.Certificate);
                userCertificateSelectionCallback = ClientCertificateSelectionCallback;
            }
            SslStream stream2 = new SslStream(stream, false, new RemoteCertificateValidationCallback(this.ValidateRemoteCertificate), userCertificateSelectionCallback);
            try
            {
                stream2.AuthenticateAsClient(string.Empty, clientCertificates, SslProtocols.Default, false);
            }
            catch (SecurityTokenValidationException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(exception.Message, exception));
            }
            catch (AuthenticationException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(exception2.Message, exception2));
            }
            catch (IOException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NegotiationFailedIO", new object[] { exception3.Message }), exception3));
            }
            if (System.ServiceModel.Security.SecurityUtils.ShouldValidateSslCipherStrength())
            {
                System.ServiceModel.Security.SecurityUtils.ValidateSslCipherStrength(stream2.CipherStrength);
            }
            remoteSecurity = this.serverSecurity;
            if (this.IsChannelBindingSupportEnabled)
            {
                this.channelBindingToken = ChannelBindingUtility.GetToken(stream2);
            }
            return stream2;
        }

        internal override void Open(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.Open(helper.RemainingTime());
            if (this.clientCertificateProvider != null)
            {
                System.ServiceModel.Security.SecurityUtils.OpenTokenProviderIfRequired(this.clientCertificateProvider, helper.RemainingTime());
                this.clientToken = (X509SecurityToken) this.clientCertificateProvider.GetToken(helper.RemainingTime());
            }
        }

        private static X509Certificate SelectClientCertificate(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return localCertificates[0];
        }

        private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            X509Certificate2 certificate2 = new X509Certificate2(certificate);
            SecurityToken token = new X509SecurityToken(certificate2, false);
            ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies = this.serverCertificateAuthenticator.ValidateToken(token);
            this.serverSecurity = new SecurityMessageProperty();
            this.serverSecurity.TransportToken = new SecurityTokenSpecification(token, tokenPolicies);
            this.serverSecurity.ServiceSecurityContext = new ServiceSecurityContext(tokenPolicies);
            AuthorizationContext authorizationContext = this.serverSecurity.ServiceSecurityContext.AuthorizationContext;
            this.parent.IdentityVerifier.EnsureOutgoingIdentity(base.RemoteAddress, base.Via, authorizationContext);
            return true;
        }

        internal System.Security.Authentication.ExtendedProtection.ChannelBinding ChannelBinding
        {
            get
            {
                return this.channelBindingToken;
            }
        }

        private static LocalCertificateSelectionCallback ClientCertificateSelectionCallback
        {
            get
            {
                if (clientCertificateSelectionCallback == null)
                {
                    clientCertificateSelectionCallback = new LocalCertificateSelectionCallback(SslStreamSecurityUpgradeInitiator.SelectClientCertificate);
                }
                return clientCertificateSelectionCallback;
            }
        }

        internal bool IsChannelBindingSupportEnabled
        {
            get
            {
                return ((IChannelBindingProvider) this.parent).IsChannelBindingSupportEnabled;
            }
        }

        private class CloseAsyncResult : AsyncResult
        {
            private AsyncCallback onBaseClose;
            private AsyncCallback onCloseTokenProvider;
            private SslStreamSecurityUpgradeInitiator parent;
            private TimeoutHelper timeoutHelper;

            public CloseAsyncResult(SslStreamSecurityUpgradeInitiator parent, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.parent = parent;
                TimeoutHelper helper = new TimeoutHelper(timeout);
                this.onBaseClose = Fx.ThunkCallback(new AsyncCallback(this.OnBaseClose));
                if (parent.clientCertificateProvider != null)
                {
                    this.onCloseTokenProvider = Fx.ThunkCallback(new AsyncCallback(this.OnCloseTokenProvider));
                }
                IAsyncResult result = parent.BaseBeginClose(helper.RemainingTime(), this.onBaseClose, this);
                if (result.CompletedSynchronously && this.HandleBaseCloseComplete(result))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SslStreamSecurityUpgradeInitiator.CloseAsyncResult>(result);
            }

            private bool HandleBaseCloseComplete(IAsyncResult result)
            {
                this.parent.BaseEndClose(result);
                if (this.parent.clientCertificateProvider != null)
                {
                    IAsyncResult result2 = System.ServiceModel.Security.SecurityUtils.BeginCloseTokenProviderIfRequired(this.parent.clientCertificateProvider, this.timeoutHelper.RemainingTime(), this.onCloseTokenProvider, this);
                    if (!result2.CompletedSynchronously)
                    {
                        return false;
                    }
                    System.ServiceModel.Security.SecurityUtils.EndCloseTokenProviderIfRequired(result2);
                }
                return true;
            }

            private void OnBaseClose(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Exception exception = null;
                    bool flag = false;
                    try
                    {
                        flag = this.HandleBaseCloseComplete(result);
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

            private void OnCloseTokenProvider(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Exception exception = null;
                    try
                    {
                        System.ServiceModel.Security.SecurityUtils.EndCloseTokenProviderIfRequired(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    base.Complete(false, exception);
                }
            }
        }

        private class InitiateUpgradeAsyncResult : StreamSecurityUpgradeInitiatorAsyncResult
        {
            private ChannelBinding channelBindingToken;
            private X509CertificateCollection clientCertificates;
            private SslStreamSecurityUpgradeInitiator initiator;
            private LocalCertificateSelectionCallback selectionCallback;
            private SslStream sslStream;

            public InitiateUpgradeAsyncResult(SslStreamSecurityUpgradeInitiator initiator, AsyncCallback callback, object state) : base(callback, state)
            {
                this.initiator = initiator;
                if (initiator.clientToken != null)
                {
                    this.clientCertificates = new X509CertificateCollection();
                    this.clientCertificates.Add(initiator.clientToken.Certificate);
                    this.selectionCallback = SslStreamSecurityUpgradeInitiator.ClientCertificateSelectionCallback;
                }
            }

            public static Stream End(IAsyncResult result, out SecurityMessageProperty remoteSecurity, out ChannelBinding channelBinding)
            {
                Stream stream = StreamSecurityUpgradeInitiatorAsyncResult.End(result, out remoteSecurity);
                channelBinding = ((SslStreamSecurityUpgradeInitiator.InitiateUpgradeAsyncResult) result).channelBindingToken;
                return stream;
            }

            protected override IAsyncResult OnBeginAuthenticateAsClient(Stream stream, AsyncCallback callback)
            {
                IAsyncResult result;
                this.sslStream = new SslStream(stream, false, new RemoteCertificateValidationCallback(this.initiator.ValidateRemoteCertificate), this.selectionCallback);
                try
                {
                    result = this.sslStream.BeginAuthenticateAsClient(string.Empty, this.clientCertificates, SslProtocols.Default, false, callback, this);
                }
                catch (SecurityTokenValidationException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(exception.Message, exception));
                }
                return result;
            }

            protected override Stream OnCompleteAuthenticateAsClient(IAsyncResult result)
            {
                try
                {
                    this.sslStream.EndAuthenticateAsClient(result);
                }
                catch (SecurityTokenValidationException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(exception.Message, exception));
                }
                if (System.ServiceModel.Security.SecurityUtils.ShouldValidateSslCipherStrength())
                {
                    System.ServiceModel.Security.SecurityUtils.ValidateSslCipherStrength(this.sslStream.CipherStrength);
                }
                if (this.initiator.IsChannelBindingSupportEnabled)
                {
                    this.channelBindingToken = ChannelBindingUtility.GetToken(this.sslStream);
                }
                return this.sslStream;
            }

            protected override SecurityMessageProperty ValidateCreateSecurity()
            {
                return this.initiator.serverSecurity;
            }
        }

        private class OpenAsyncResult : AsyncResult
        {
            private AsyncCallback onBaseOpen;
            private AsyncCallback onGetClientToken;
            private AsyncCallback onOpenTokenProvider;
            private SslStreamSecurityUpgradeInitiator parent;
            private TimeoutHelper timeoutHelper;

            public OpenAsyncResult(SslStreamSecurityUpgradeInitiator parent, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.parent = parent;
                TimeoutHelper helper = new TimeoutHelper(timeout);
                this.onBaseOpen = Fx.ThunkCallback(new AsyncCallback(this.OnBaseOpen));
                if (parent.clientCertificateProvider != null)
                {
                    this.onOpenTokenProvider = Fx.ThunkCallback(new AsyncCallback(this.OnOpenTokenProvider));
                    this.onGetClientToken = Fx.ThunkCallback(new AsyncCallback(this.OnGetClientToken));
                }
                IAsyncResult result = parent.BaseBeginOpen(helper.RemainingTime(), this.onBaseOpen, this);
                if (result.CompletedSynchronously && this.HandleBaseOpenComplete(result))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SslStreamSecurityUpgradeInitiator.OpenAsyncResult>(result);
            }

            private bool HandleBaseOpenComplete(IAsyncResult result)
            {
                this.parent.BaseEndOpen(result);
                if (this.parent.clientCertificateProvider == null)
                {
                    return true;
                }
                IAsyncResult result2 = System.ServiceModel.Security.SecurityUtils.BeginOpenTokenProviderIfRequired(this.parent.clientCertificateProvider, this.timeoutHelper.RemainingTime(), this.onOpenTokenProvider, this);
                if (!result2.CompletedSynchronously)
                {
                    return false;
                }
                return this.HandleOpenTokenProviderComplete(result2);
            }

            private bool HandleGetTokenComplete(IAsyncResult result)
            {
                this.parent.clientToken = (X509SecurityToken) this.parent.clientCertificateProvider.EndGetToken(result);
                return true;
            }

            private bool HandleOpenTokenProviderComplete(IAsyncResult result)
            {
                System.ServiceModel.Security.SecurityUtils.EndOpenTokenProviderIfRequired(result);
                IAsyncResult result2 = this.parent.clientCertificateProvider.BeginGetToken(this.timeoutHelper.RemainingTime(), this.onGetClientToken, this);
                if (!result2.CompletedSynchronously)
                {
                    return false;
                }
                return this.HandleGetTokenComplete(result2);
            }

            private void OnBaseOpen(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Exception exception = null;
                    bool flag = false;
                    try
                    {
                        flag = this.HandleBaseOpenComplete(result);
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

            private void OnGetClientToken(IAsyncResult result)
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


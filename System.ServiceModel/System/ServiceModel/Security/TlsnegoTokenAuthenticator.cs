namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class TlsnegoTokenAuthenticator : SspiNegotiationTokenAuthenticator
    {
        private SecurityTokenAuthenticator clientTokenAuthenticator;
        private bool mapCertificateToWindowsAccount;
        private X509SecurityToken serverToken;
        private SecurityTokenProvider serverTokenProvider;

        protected override SspiNegotiationTokenAuthenticatorState CreateSspiState(byte[] incomingBlob, string incomingValueTypeUri)
        {
            TlsSspiNegotiation sspiNegotiation = new TlsSspiNegotiation(SchProtocols.TlsServer | SchProtocols.Ssl3Server, this.serverToken.Certificate, this.ClientTokenAuthenticator != null);
            if ((base.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005) && (this.NegotiationValueType.Value != incomingValueTypeUri))
            {
                sspiNegotiation.IncomingValueTypeUri = incomingValueTypeUri;
            }
            return new SspiNegotiationTokenAuthenticatorState(sspiNegotiation);
        }

        protected override BinaryNegotiation GetOutgoingBinaryNegotiation(ISspiNegotiation sspiNegotiation, byte[] outgoingBlob)
        {
            TlsSspiNegotiation negotiation = sspiNegotiation as TlsSspiNegotiation;
            if (((base.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005) && (negotiation != null)) && (negotiation.IncomingValueTypeUri != null))
            {
                return new BinaryNegotiation(negotiation.IncomingValueTypeUri, outgoingBlob);
            }
            return base.GetOutgoingBinaryNegotiation(sspiNegotiation, outgoingBlob);
        }

        public override void OnAbort()
        {
            if (this.serverTokenProvider != null)
            {
                System.ServiceModel.Security.SecurityUtils.AbortTokenProviderIfRequired(this.serverTokenProvider);
                this.serverTokenProvider = null;
            }
            if (this.clientTokenAuthenticator != null)
            {
                System.ServiceModel.Security.SecurityUtils.AbortTokenAuthenticatorIfRequired(this.clientTokenAuthenticator);
                this.clientTokenAuthenticator = null;
            }
            if (this.serverToken != null)
            {
                this.serverToken = null;
            }
            base.OnAbort();
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.serverTokenProvider != null)
            {
                System.ServiceModel.Security.SecurityUtils.CloseTokenProviderIfRequired(this.serverTokenProvider, helper.RemainingTime());
                this.serverTokenProvider = null;
            }
            if (this.clientTokenAuthenticator != null)
            {
                System.ServiceModel.Security.SecurityUtils.CloseTokenAuthenticatorIfRequired(this.clientTokenAuthenticator, helper.RemainingTime());
                this.clientTokenAuthenticator = null;
            }
            if (this.serverToken != null)
            {
                this.serverToken = null;
            }
            base.OnClose(helper.RemainingTime());
        }

        public override void OnOpen(TimeSpan timeout)
        {
            if (this.serverTokenProvider == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NoServerX509TokenProvider")));
            }
            TimeoutHelper helper = new TimeoutHelper(timeout);
            System.ServiceModel.Security.SecurityUtils.OpenTokenProviderIfRequired(this.serverTokenProvider, helper.RemainingTime());
            if (this.clientTokenAuthenticator != null)
            {
                System.ServiceModel.Security.SecurityUtils.OpenTokenAuthenticatorIfRequired(this.clientTokenAuthenticator, helper.RemainingTime());
            }
            SecurityToken token = this.serverTokenProvider.GetToken(helper.RemainingTime());
            this.serverToken = this.ValidateX509Token(token);
            base.OnOpen(helper.RemainingTime());
        }

        protected override void ValidateIncomingBinaryNegotiation(BinaryNegotiation incomingNego)
        {
            if (((incomingNego != null) && (incomingNego.ValueTypeUri != this.NegotiationValueType.Value)) && (base.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005))
            {
                incomingNego.Validate(DXD.TrustDec2005Dictionary.TlsnegoValueTypeUri);
            }
            else
            {
                base.ValidateIncomingBinaryNegotiation(incomingNego);
            }
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateSspiNegotiation(ISspiNegotiation sspiNegotiation)
        {
            X509SecurityToken token;
            WindowsIdentity identity;
            TlsSspiNegotiation negotiation = (TlsSspiNegotiation) sspiNegotiation;
            if (!negotiation.IsValidContext)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidSspiNegotiation")));
            }
            if (this.ClientTokenAuthenticator == null)
            {
                return System.ServiceModel.Security.EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }
            X509Certificate2 remoteCertificate = negotiation.RemoteCertificate;
            if (remoteCertificate == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityTokenValidationException(System.ServiceModel.SR.GetString("ClientCertificateNotProvided")));
            }
            if (this.ClientTokenAuthenticator == null)
            {
                return System.ServiceModel.Security.EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }
            if (!this.MapCertificateToWindowsAccount || !negotiation.TryGetContextIdentity(out identity))
            {
                token = new X509SecurityToken(remoteCertificate);
            }
            else
            {
                token = new X509WindowsSecurityToken(remoteCertificate, identity, identity.AuthenticationType, true);
                identity.Dispose();
            }
            ReadOnlyCollection<IAuthorizationPolicy> onlys = this.ClientTokenAuthenticator.ValidateToken(token);
            token.Dispose();
            return onlys;
        }

        private X509SecurityToken ValidateX509Token(SecurityToken token)
        {
            X509SecurityToken token2 = token as X509SecurityToken;
            if (token2 == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TokenProviderReturnedBadToken", new object[] { (token == null) ? "<null>" : token.GetType().ToString() })));
            }
            System.ServiceModel.Security.SecurityUtils.EnsureCertificateCanDoKeyExchange(token2.Certificate);
            return token2;
        }

        public SecurityTokenAuthenticator ClientTokenAuthenticator
        {
            get
            {
                return this.clientTokenAuthenticator;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.clientTokenAuthenticator = value;
            }
        }

        public bool MapCertificateToWindowsAccount
        {
            get
            {
                return this.mapCertificateToWindowsAccount;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.mapCertificateToWindowsAccount = value;
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

        public SecurityTokenProvider ServerTokenProvider
        {
            get
            {
                return this.serverTokenProvider;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.serverTokenProvider = value;
            }
        }
    }
}


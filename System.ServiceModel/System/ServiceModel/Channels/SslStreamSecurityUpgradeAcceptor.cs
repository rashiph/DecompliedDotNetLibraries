namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.Security.Authentication;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;

    internal class SslStreamSecurityUpgradeAcceptor : StreamSecurityUpgradeAcceptorBase
    {
        private System.Security.Authentication.ExtendedProtection.ChannelBinding channelBindingToken;
        private X509Certificate2 clientCertificate;
        private SecurityMessageProperty clientSecurity;
        private SslStreamSecurityUpgradeProvider parent;

        public SslStreamSecurityUpgradeAcceptor(SslStreamSecurityUpgradeProvider parent) : base("application/ssl-tls")
        {
            this.parent = parent;
            this.clientSecurity = new SecurityMessageProperty();
        }

        public override SecurityMessageProperty GetRemoteSecurity()
        {
            if (this.clientSecurity.TransportToken != null)
            {
                return this.clientSecurity;
            }
            if (this.clientCertificate != null)
            {
                SecurityToken token = new X509SecurityToken(this.clientCertificate);
                ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies = System.ServiceModel.Security.SecurityUtils.NonValidatingX509Authenticator.ValidateToken(token);
                this.clientSecurity = new SecurityMessageProperty();
                this.clientSecurity.TransportToken = new SecurityTokenSpecification(token, tokenPolicies);
                this.clientSecurity.ServiceSecurityContext = new ServiceSecurityContext(tokenPolicies);
                return this.clientSecurity;
            }
            return base.GetRemoteSecurity();
        }

        protected override Stream OnAcceptUpgrade(Stream stream, out SecurityMessageProperty remoteSecurity)
        {
            SslStream stream2 = new SslStream(stream, false, new RemoteCertificateValidationCallback(this.ValidateRemoteCertificate));
            try
            {
                stream2.AuthenticateAsServer(this.parent.ServerCertificate, this.parent.RequireClientCertificate, SslProtocols.Default, false);
            }
            catch (AuthenticationException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(exception.Message, exception));
            }
            catch (IOException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NegotiationFailedIO", new object[] { exception2.Message }), exception2));
            }
            if (System.ServiceModel.Security.SecurityUtils.ShouldValidateSslCipherStrength())
            {
                System.ServiceModel.Security.SecurityUtils.ValidateSslCipherStrength(stream2.CipherStrength);
            }
            remoteSecurity = this.clientSecurity;
            if (this.IsChannelBindingSupportEnabled)
            {
                this.channelBindingToken = ChannelBindingUtility.GetToken(stream2);
            }
            return stream2;
        }

        protected override IAsyncResult OnBeginAcceptUpgrade(Stream stream, AsyncCallback callback, object state)
        {
            AcceptUpgradeAsyncResult result = new AcceptUpgradeAsyncResult(this, callback, state);
            result.Begin(stream);
            return result;
        }

        protected override Stream OnEndAcceptUpgrade(IAsyncResult result, out SecurityMessageProperty remoteSecurity)
        {
            return AcceptUpgradeAsyncResult.End(result, out remoteSecurity, out this.channelBindingToken);
        }

        private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (this.parent.RequireClientCertificate)
            {
                if (certificate == null)
                {
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, 0x4002d, System.ServiceModel.SR.GetString("TraceCodeSslClientCertMissing"), this);
                    }
                    return false;
                }
                X509Certificate2 certificate2 = new X509Certificate2(certificate);
                this.clientCertificate = certificate2;
                try
                {
                    SecurityToken token = new X509SecurityToken(certificate2, false);
                    ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies = this.parent.ClientCertificateAuthenticator.ValidateToken(token);
                    this.clientSecurity = new SecurityMessageProperty();
                    this.clientSecurity.TransportToken = new SecurityTokenSpecification(token, tokenPolicies);
                    this.clientSecurity.ServiceSecurityContext = new ServiceSecurityContext(tokenPolicies);
                }
                catch (SecurityTokenException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                    return false;
                }
            }
            return true;
        }

        internal System.Security.Authentication.ExtendedProtection.ChannelBinding ChannelBinding
        {
            get
            {
                return this.channelBindingToken;
            }
        }

        internal bool IsChannelBindingSupportEnabled
        {
            get
            {
                return ((IChannelBindingProvider) this.parent).IsChannelBindingSupportEnabled;
            }
        }

        private class AcceptUpgradeAsyncResult : StreamSecurityUpgradeAcceptorAsyncResult
        {
            private SslStreamSecurityUpgradeAcceptor acceptor;
            private ChannelBinding channelBindingToken;
            private SslStream sslStream;

            public AcceptUpgradeAsyncResult(SslStreamSecurityUpgradeAcceptor acceptor, AsyncCallback callback, object state) : base(callback, state)
            {
                this.acceptor = acceptor;
            }

            public static Stream End(IAsyncResult result, out SecurityMessageProperty remoteSecurity, out ChannelBinding channelBinding)
            {
                Stream stream = StreamSecurityUpgradeAcceptorAsyncResult.End(result, out remoteSecurity);
                channelBinding = ((SslStreamSecurityUpgradeAcceptor.AcceptUpgradeAsyncResult) result).channelBindingToken;
                return stream;
            }

            protected override IAsyncResult OnBegin(Stream stream, AsyncCallback callback)
            {
                this.sslStream = new SslStream(stream, false, new RemoteCertificateValidationCallback(this.acceptor.ValidateRemoteCertificate));
                return this.sslStream.BeginAuthenticateAsServer(this.acceptor.parent.ServerCertificate, this.acceptor.parent.RequireClientCertificate, SslProtocols.Default, false, callback, this);
            }

            protected override Stream OnCompleteAuthenticateAsServer(IAsyncResult result)
            {
                this.sslStream.EndAuthenticateAsServer(result);
                if (System.ServiceModel.Security.SecurityUtils.ShouldValidateSslCipherStrength())
                {
                    System.ServiceModel.Security.SecurityUtils.ValidateSslCipherStrength(this.sslStream.CipherStrength);
                }
                if (this.acceptor.IsChannelBindingSupportEnabled)
                {
                    this.channelBindingToken = ChannelBindingUtility.GetToken(this.sslStream);
                }
                return this.sslStream;
            }

            protected override SecurityMessageProperty ValidateCreateSecurity()
            {
                return this.acceptor.clientSecurity;
            }
        }
    }
}


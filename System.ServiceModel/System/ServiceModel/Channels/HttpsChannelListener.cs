namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Runtime;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;

    internal class HttpsChannelListener : HttpChannelListener
    {
        private SecurityTokenAuthenticator certificateAuthenticator;
        private const HttpStatusCode CertificateErrorStatusCode = HttpStatusCode.Forbidden;
        private IChannelBindingProvider channelBindingProvider;
        private bool requireClientCertificate;
        private readonly bool useCustomClientCertificateVerification;
        private bool useHostedClientCertificateMapping;

        public HttpsChannelListener(HttpsTransportBindingElement httpsBindingElement, BindingContext context) : base(httpsBindingElement, context)
        {
            this.requireClientCertificate = httpsBindingElement.RequireClientCertificate;
            SecurityCredentialsManager manager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (manager == null)
            {
                manager = ServiceCredentials.CreateDefaultCredentials();
            }
            SecurityTokenManager tokenManager = manager.CreateSecurityTokenManager();
            this.certificateAuthenticator = TransportSecurityHelpers.GetCertificateTokenAuthenticator(tokenManager, context.Binding.Scheme, TransportSecurityHelpers.GetListenUri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress));
            ServiceCredentials credentials = manager as ServiceCredentials;
            if ((credentials != null) && (credentials.ClientCertificate.Authentication.CertificateValidationMode == X509CertificateValidationMode.Custom))
            {
                this.useCustomClientCertificateVerification = true;
            }
            else
            {
                this.useCustomClientCertificateVerification = false;
                X509SecurityTokenAuthenticator certificateAuthenticator = this.certificateAuthenticator as X509SecurityTokenAuthenticator;
                if (certificateAuthenticator != null)
                {
                    this.certificateAuthenticator = new X509SecurityTokenAuthenticator(X509CertificateValidator.None, certificateAuthenticator.MapCertificateToWindowsAccount, base.ExtractGroupsForWindowsAccounts, false);
                }
            }
            if (this.RequireClientCertificate && (base.AuthenticationScheme != AuthenticationSchemes.Anonymous))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new InvalidOperationException(System.ServiceModel.SR.GetString("HttpAuthSchemeAndClientCert", new object[] { base.AuthenticationScheme })), TraceEventType.Error);
            }
            this.channelBindingProvider = new ChannelBindingProviderHelper();
        }

        internal override void ApplyHostedContext(string virtualPath, bool isMetadataListener)
        {
            base.ApplyHostedContext(virtualPath, isMetadataListener);
            bool? requireClientCertificate = null;
            if (!isMetadataListener)
            {
                requireClientCertificate = new bool?(this.RequireClientCertificate);
            }
            this.useHostedClientCertificateMapping = AspNetEnvironment.Current.ValidateHttpsSettings(virtualPath, ref requireClientCertificate);
            if (isMetadataListener && requireClientCertificate.HasValue)
            {
                this.requireClientCertificate = requireClientCertificate.Value;
            }
        }

        private SecurityMessageProperty CreateSecurityProperty(X509Certificate2 certificate, WindowsIdentity identity)
        {
            SecurityToken token;
            if (identity == null)
            {
                token = new X509SecurityToken(certificate, false);
            }
            else
            {
                string str;
                switch (base.AuthenticationScheme)
                {
                    case AuthenticationSchemes.Digest:
                        str = "Basic";
                        break;

                    case AuthenticationSchemes.Negotiate:
                        str = "Negotiate";
                        break;

                    case AuthenticationSchemes.Ntlm:
                        str = "NTLM";
                        break;

                    case AuthenticationSchemes.IntegratedWindowsAuthentication:
                        str = "Negotiate";
                        break;

                    case AuthenticationSchemes.Basic:
                        str = "Basic";
                        break;

                    default:
                        str = "";
                        break;
                }
                token = new X509WindowsSecurityToken(certificate, identity, str, false);
            }
            ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies = this.certificateAuthenticator.ValidateToken(token);
            return new SecurityMessageProperty { TransportToken = new SecurityTokenSpecification(token, tokenPolicies), ServiceSecurityContext = new ServiceSecurityContext(tokenPolicies) };
        }

        internal override ITransportManagerRegistration CreateTransportManagerRegistration(Uri listenUri)
        {
            return new SharedHttpsTransportManager(listenUri, this);
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IChannelBindingProvider))
            {
                return (T) this.channelBindingProvider;
            }
            return base.GetProperty<T>();
        }

        public override SecurityMessageProperty ProcessAuthentication(HttpListenerContext listenerContext)
        {
            if (this.requireClientCertificate)
            {
                SecurityMessageProperty property;
                X509Certificate2 certificate = null;
                try
                {
                    X509Certificate clientCertificate = listenerContext.Request.GetClientCertificate();
                    bool useCustomClientCertificateVerification = this.useCustomClientCertificateVerification;
                    certificate = new X509Certificate2(clientCertificate);
                    property = this.CreateSecurityProperty(certificate, null);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (AuditLevel.Failure == (base.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Failure))
                    {
                        base.WriteAuditEvent(AuditLevel.Failure, (certificate != null) ? System.ServiceModel.Security.SecurityUtils.GetCertificateId(certificate) : string.Empty, exception);
                    }
                    throw;
                }
                if (AuditLevel.Success == (base.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Success))
                {
                    base.WriteAuditEvent(AuditLevel.Success, (certificate != null) ? System.ServiceModel.Security.SecurityUtils.GetCertificateId(certificate) : string.Empty, null);
                }
                return property;
            }
            if (base.AuthenticationScheme == AuthenticationSchemes.Anonymous)
            {
                return new SecurityMessageProperty();
            }
            return base.ProcessAuthentication(listenerContext);
        }

        public override SecurityMessageProperty ProcessAuthentication(HttpChannelListener.IHttpAuthenticationContext authenticationContext)
        {
            if (this.requireClientCertificate)
            {
                SecurityMessageProperty property;
                X509Certificate2 clientCertificate = null;
                try
                {
                    bool flag;
                    clientCertificate = authenticationContext.GetClientCertificate(out flag);
                    bool useCustomClientCertificateVerification = this.useCustomClientCertificateVerification;
                    WindowsIdentity wid = null;
                    if (this.useHostedClientCertificateMapping)
                    {
                        wid = authenticationContext.LogonUserIdentity;
                        if ((wid == null) || !wid.IsAuthenticated)
                        {
                            wid = WindowsIdentity.GetAnonymous();
                        }
                        else
                        {
                            wid = System.ServiceModel.Security.SecurityUtils.CloneWindowsIdentityIfNecessary(wid, "SSL/PCT");
                        }
                    }
                    property = this.CreateSecurityProperty(clientCertificate, wid);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (AuditLevel.Failure == (base.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Failure))
                    {
                        base.WriteAuditEvent(AuditLevel.Failure, (clientCertificate != null) ? System.ServiceModel.Security.SecurityUtils.GetCertificateId(clientCertificate) : string.Empty, exception);
                    }
                    throw;
                }
                if (AuditLevel.Success == (base.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Success))
                {
                    base.WriteAuditEvent(AuditLevel.Success, (clientCertificate != null) ? System.ServiceModel.Security.SecurityUtils.GetCertificateId(clientCertificate) : string.Empty, null);
                }
                return property;
            }
            if (base.AuthenticationScheme == AuthenticationSchemes.Anonymous)
            {
                return new SecurityMessageProperty();
            }
            return base.ProcessAuthentication(authenticationContext);
        }

        public override HttpStatusCode ValidateAuthentication(HttpListenerContext listenerContext)
        {
            HttpStatusCode forbidden = base.ValidateAuthentication(listenerContext);
            if ((forbidden == HttpStatusCode.OK) && this.RequireClientCertificate)
            {
                HttpListenerRequest request = listenerContext.Request;
                X509Certificate2 clientCertificate = request.GetClientCertificate();
                if (clientCertificate == null)
                {
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Warning, 0x40010, System.ServiceModel.SR.GetString("TraceCodeHttpsClientCertificateNotPresent"), new HttpListenerRequestTraceRecord(listenerContext.Request), this, null);
                    }
                    forbidden = HttpStatusCode.Forbidden;
                }
                else if ((request.ClientCertificateError != 0) && !this.useCustomClientCertificateVerification)
                {
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Warning, 0x4000f, System.ServiceModel.SR.GetString("TraceCodeHttpsClientCertificateInvalid"), new HttpListenerRequestTraceRecord(listenerContext.Request), this, null);
                    }
                    forbidden = HttpStatusCode.Forbidden;
                }
                if ((forbidden != HttpStatusCode.OK) && (AuditLevel.Failure == (base.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Failure)))
                {
                    string message = System.ServiceModel.SR.GetString("HttpAuthenticationFailed", new object[] { base.AuthenticationScheme, forbidden });
                    Exception exception = DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(message));
                    base.WriteAuditEvent(AuditLevel.Failure, (clientCertificate != null) ? System.ServiceModel.Security.SecurityUtils.GetCertificateId(clientCertificate) : string.Empty, exception);
                }
            }
            return forbidden;
        }

        public override HttpStatusCode ValidateAuthentication(HttpChannelListener.IHttpAuthenticationContext authenticationContext)
        {
            HttpStatusCode forbidden = base.ValidateAuthentication(authenticationContext);
            if ((forbidden == HttpStatusCode.OK) && this.RequireClientCertificate)
            {
                bool flag;
                X509Certificate2 clientCertificate = authenticationContext.GetClientCertificate(out flag);
                if (clientCertificate == null)
                {
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, 0x40010, System.ServiceModel.SR.GetString("TraceCodeHttpsClientCertificateNotPresent"), authenticationContext.CreateTraceRecord(), this, null);
                    }
                    forbidden = HttpStatusCode.Forbidden;
                }
                else if (!flag && !this.useCustomClientCertificateVerification)
                {
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, 0x4000f, System.ServiceModel.SR.GetString("TraceCodeHttpsClientCertificateInvalid"), authenticationContext.CreateTraceRecord(), this, null);
                    }
                    forbidden = HttpStatusCode.Forbidden;
                }
                if ((forbidden != HttpStatusCode.OK) && (AuditLevel.Failure == (base.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Failure)))
                {
                    string message = System.ServiceModel.SR.GetString("HttpAuthenticationFailed", new object[] { base.AuthenticationScheme, forbidden });
                    Exception exception = DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(message));
                    base.WriteAuditEvent(AuditLevel.Failure, (clientCertificate != null) ? System.ServiceModel.Security.SecurityUtils.GetCertificateId(clientCertificate) : string.Empty, exception);
                }
            }
            return forbidden;
        }

        public override bool IsChannelBindingSupportEnabled
        {
            get
            {
                return this.channelBindingProvider.IsChannelBindingSupportEnabled;
            }
        }

        public bool RequireClientCertificate
        {
            get
            {
                return this.requireClientCertificate;
            }
        }

        public override string Scheme
        {
            get
            {
                return Uri.UriSchemeHttps;
            }
        }

        internal override UriPrefixTable<ITransportManagerRegistration> TransportManagerTable
        {
            get
            {
                return SharedHttpsTransportManager.StaticTransportManagerTable;
            }
        }
    }
}


namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    internal abstract class MsmqChannelListenerBase<TChannel> : MsmqChannelListenerBase, IChannelListener<TChannel>, IChannelListener, ICommunicationObject where TChannel: class, IChannel
    {
        private SecurityTokenAuthenticator x509SecurityTokenAuthenticator;

        protected MsmqChannelListenerBase(MsmqBindingElementBase bindingElement, BindingContext context, MsmqReceiveParameters receiveParameters, MessageEncoderFactory messageEncoderFactory) : base(bindingElement, context, receiveParameters, messageEncoderFactory)
        {
        }

        public abstract TChannel AcceptChannel();
        public abstract TChannel AcceptChannel(TimeSpan timeout);
        public abstract IAsyncResult BeginAcceptChannel(AsyncCallback callback, object state);
        public abstract IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state);
        internal override ITransportManagerRegistration CreateTransportManagerRegistration(Uri listenUri)
        {
            return null;
        }

        public abstract TChannel EndAcceptChannel(IAsyncResult result);
        protected override void OnAbort()
        {
            this.OnCloseCore(true);
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnCloseCore(false);
            return base.OnBeginClose(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            IAsyncResult result = base.OnBeginOpen(helper.RemainingTime(), callback, state);
            this.OnOpenCore(helper.RemainingTime());
            return result;
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.OnCloseCore(false);
            base.OnClose(timeout);
        }

        protected virtual void OnCloseCore(bool isAborting)
        {
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnOpen(helper.RemainingTime());
            this.OnOpenCore(helper.RemainingTime());
        }

        protected virtual void OnOpenCore(TimeSpan timeout)
        {
            if (MsmqAuthenticationMode.Certificate == base.ReceiveParameters.TransportSecurity.MsmqAuthenticationMode)
            {
                System.ServiceModel.Security.SecurityUtils.OpenTokenAuthenticatorIfRequired(this.x509SecurityTokenAuthenticator, timeout);
            }
        }

        internal override IList<TransportManager> SelectTransportManagers()
        {
            lock (this.TransportManagerTable)
            {
                ITransportManagerRegistration registration;
                if (this.TransportManagerTable.TryLookupUri(this.Uri, HostNameComparisonMode.Exact, out registration))
                {
                    IList<TransportManager> list = registration.Select(this);
                    if (list != null)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            list[i].Open(this);
                        }
                    }
                }
            }
            return null;
        }

        protected void SetSecurityTokenAuthenticator(string scheme, BindingContext context)
        {
            if (base.ReceiveParameters.TransportSecurity.MsmqAuthenticationMode == MsmqAuthenticationMode.Certificate)
            {
                SecurityTokenResolver resolver;
                SecurityCredentialsManager manager = context.BindingParameters.Find<SecurityCredentialsManager>();
                if (manager == null)
                {
                    manager = ServiceCredentials.CreateDefaultCredentials();
                }
                SecurityTokenManager manager2 = manager.CreateSecurityTokenManager();
                RecipientServiceModelSecurityTokenRequirement tokenRequirement = new RecipientServiceModelSecurityTokenRequirement {
                    TokenType = SecurityTokenTypes.X509Certificate,
                    TransportScheme = scheme,
                    ListenUri = this.Uri,
                    KeyUsage = SecurityKeyUsage.Signature
                };
                this.x509SecurityTokenAuthenticator = manager2.CreateSecurityTokenAuthenticator(tokenRequirement, out resolver);
            }
        }

        internal SecurityMessageProperty ValidateSecurity(MsmqInputMessage msmqMessage)
        {
            SecurityMessageProperty property = null;
            X509Certificate2 certificate = null;
            WindowsSidIdentity primaryIdentity = null;
            try
            {
                if (MsmqAuthenticationMode.Certificate == base.ReceiveParameters.TransportSecurity.MsmqAuthenticationMode)
                {
                    try
                    {
                        certificate = new X509Certificate2(msmqMessage.SenderCertificate.GetBufferCopy(msmqMessage.SenderCertificateLength.Value));
                        X509SecurityToken token = new X509SecurityToken(certificate, false);
                        ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies = this.x509SecurityTokenAuthenticator.ValidateToken(token);
                        property = new SecurityMessageProperty {
                            TransportToken = new SecurityTokenSpecification(token, tokenPolicies),
                            ServiceSecurityContext = new ServiceSecurityContext(tokenPolicies)
                        };
                        goto Label_01C4;
                    }
                    catch (SecurityTokenValidationException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("MsmqBadCertificate"), exception));
                    }
                    catch (CryptographicException exception2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("MsmqBadCertificate"), exception2));
                    }
                }
                if (MsmqAuthenticationMode.WindowsDomain == base.ReceiveParameters.TransportSecurity.MsmqAuthenticationMode)
                {
                    byte[] bufferCopy = msmqMessage.SenderId.GetBufferCopy(msmqMessage.SenderIdLength.Value);
                    if (bufferCopy.Length == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("MsmqNoSid")));
                    }
                    SecurityIdentifier resource = new SecurityIdentifier(bufferCopy, 0);
                    List<Claim> claims = new List<Claim>(2) {
                        new Claim(ClaimTypes.Sid, resource, Rights.Identity),
                        Claim.CreateWindowsSidClaim(resource)
                    };
                    ClaimSet issuance = new DefaultClaimSet(ClaimSet.System, claims);
                    List<IAuthorizationPolicy> list2 = new List<IAuthorizationPolicy>(1);
                    primaryIdentity = new WindowsSidIdentity(resource);
                    list2.Add(new UnconditionalPolicy(primaryIdentity, issuance));
                    ReadOnlyCollection<IAuthorizationPolicy> onlys2 = list2.AsReadOnly();
                    property = new SecurityMessageProperty {
                        TransportToken = new SecurityTokenSpecification(null, onlys2),
                        ServiceSecurityContext = new ServiceSecurityContext(onlys2)
                    };
                }
            }
            catch (Exception exception3)
            {
                if (Fx.IsFatal(exception3))
                {
                    throw;
                }
                if (AuditLevel.Failure == (base.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Failure))
                {
                    this.WriteAuditEvent(AuditLevel.Failure, certificate, primaryIdentity, null);
                }
                throw;
            }
        Label_01C4:
            if ((property != null) && (AuditLevel.Success == (base.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Success)))
            {
                this.WriteAuditEvent(AuditLevel.Success, certificate, primaryIdentity, null);
            }
            return property;
        }

        private void WriteAuditEvent(AuditLevel auditLevel, X509Certificate2 certificate, WindowsSidIdentity wsid, Exception exception)
        {
            try
            {
                string clientIdentity = string.Empty;
                if (certificate != null)
                {
                    clientIdentity = System.ServiceModel.Security.SecurityUtils.GetCertificateId(certificate);
                }
                else if (wsid != null)
                {
                    clientIdentity = System.ServiceModel.Security.SecurityUtils.GetIdentityName(wsid);
                }
                if (auditLevel == AuditLevel.Success)
                {
                    SecurityAuditHelper.WriteTransportAuthenticationSuccessEvent(base.AuditBehavior.AuditLogLocation, base.AuditBehavior.SuppressAuditFailure, null, this.Uri, clientIdentity);
                }
                else
                {
                    SecurityAuditHelper.WriteTransportAuthenticationFailureEvent(base.AuditBehavior.AuditLogLocation, base.AuditBehavior.SuppressAuditFailure, null, this.Uri, clientIdentity, exception);
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2) || (auditLevel == AuditLevel.Success))
                {
                    throw;
                }
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Error);
            }
        }

        public override string Scheme
        {
            get
            {
                return "net.msmq";
            }
        }

        internal override UriPrefixTable<ITransportManagerRegistration> TransportManagerTable
        {
            get
            {
                return Msmq.StaticTransportManagerTable;
            }
        }
    }
}


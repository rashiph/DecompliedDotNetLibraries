namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;

    public class ServiceCredentialsSecurityTokenManager : SecurityTokenManager, IEndpointIdentityProvider
    {
        private System.ServiceModel.Description.ServiceCredentials parent;

        public ServiceCredentialsSecurityTokenManager(System.ServiceModel.Description.ServiceCredentials parent)
        {
            if (parent == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parent");
            }
            this.parent = parent;
        }

        private X509SecurityTokenAuthenticator CreateClientX509TokenAuthenticator()
        {
            X509ClientCertificateAuthentication authentication = this.parent.ClientCertificate.Authentication;
            return new X509SecurityTokenAuthenticator(authentication.GetCertificateValidator(), authentication.MapClientCertificateToWindowsAccount, authentication.IncludeWindowsGroups);
        }

        private SecurityTokenProvider CreateLocalSecurityTokenProvider(RecipientServiceModelSecurityTokenRequirement recipientRequirement)
        {
            AuthenticationSchemes schemes;
            string tokenType = recipientRequirement.TokenType;
            SecurityTokenProvider provider = null;
            if (tokenType == SecurityTokenTypes.X509Certificate)
            {
                return this.CreateServerX509TokenProvider();
            }
            if (!(tokenType == ServiceModelSecurityTokenTypes.SspiCredential))
            {
                return provider;
            }
            if (recipientRequirement.TryGetProperty<AuthenticationSchemes>(ServiceModelSecurityTokenRequirement.HttpAuthenticationSchemeProperty, out schemes) && (schemes == AuthenticationSchemes.Basic))
            {
                return new SspiSecurityTokenProvider(null, this.parent.UserNameAuthentication.IncludeWindowsGroups, false);
            }
            return new SspiSecurityTokenProvider(null, this.parent.WindowsAuthentication.IncludeWindowsGroups, this.parent.WindowsAuthentication.AllowAnonymousLogons);
        }

        private SamlSecurityTokenAuthenticator CreateSamlTokenAuthenticator(RecipientServiceModelSecurityTokenRequirement recipientRequirement, out SecurityTokenResolver outOfBandTokenResolver)
        {
            SamlSecurityTokenAuthenticator authenticator;
            if (recipientRequirement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("recipientRequirement");
            }
            Collection<SecurityToken> collection = new Collection<SecurityToken>();
            if (this.parent.ServiceCertificate.Certificate != null)
            {
                collection.Add(new X509SecurityToken(this.parent.ServiceCertificate.Certificate));
            }
            List<SecurityTokenAuthenticator> supportingAuthenticators = new List<SecurityTokenAuthenticator>();
            if ((this.parent.IssuedTokenAuthentication.KnownCertificates != null) && (this.parent.IssuedTokenAuthentication.KnownCertificates.Count > 0))
            {
                for (int i = 0; i < this.parent.IssuedTokenAuthentication.KnownCertificates.Count; i++)
                {
                    collection.Add(new X509SecurityToken(this.parent.IssuedTokenAuthentication.KnownCertificates[i]));
                }
            }
            X509CertificateValidator certificateValidator = this.parent.IssuedTokenAuthentication.GetCertificateValidator();
            supportingAuthenticators.Add(new X509SecurityTokenAuthenticator(certificateValidator));
            if (this.parent.IssuedTokenAuthentication.AllowUntrustedRsaIssuers)
            {
                supportingAuthenticators.Add(new RsaSecurityTokenAuthenticator());
            }
            outOfBandTokenResolver = (collection.Count > 0) ? SecurityTokenResolver.CreateDefaultSecurityTokenResolver(new ReadOnlyCollection<SecurityToken>(collection), false) : null;
            if ((recipientRequirement.SecurityBindingElement == null) || (recipientRequirement.SecurityBindingElement.LocalServiceSettings == null))
            {
                authenticator = new SamlSecurityTokenAuthenticator(supportingAuthenticators);
            }
            else
            {
                authenticator = new SamlSecurityTokenAuthenticator(supportingAuthenticators, recipientRequirement.SecurityBindingElement.LocalServiceSettings.MaxClockSkew);
            }
            authenticator.AudienceUriMode = this.parent.IssuedTokenAuthentication.AudienceUriMode;
            IList<string> allowedAudienceUris = authenticator.AllowedAudienceUris;
            if (this.parent.IssuedTokenAuthentication.AllowedAudienceUris != null)
            {
                for (int j = 0; j < this.parent.IssuedTokenAuthentication.AllowedAudienceUris.Count; j++)
                {
                    allowedAudienceUris.Add(this.parent.IssuedTokenAuthentication.AllowedAudienceUris[j]);
                }
            }
            if (recipientRequirement.ListenUri != null)
            {
                allowedAudienceUris.Add(recipientRequirement.ListenUri.AbsoluteUri);
            }
            return authenticator;
        }

        protected SecurityTokenAuthenticator CreateSecureConversationTokenAuthenticator(RecipientServiceModelSecurityTokenRequirement recipientRequirement, bool preserveBootstrapTokens, out SecurityTokenResolver sctResolver)
        {
            SecurityBindingElement securityBindingElement = recipientRequirement.SecurityBindingElement;
            if (securityBindingElement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("TokenAuthenticatorRequiresSecurityBindingElement", new object[] { recipientRequirement }));
            }
            bool flag = !recipientRequirement.SupportSecurityContextCancellation;
            LocalServiceSecuritySettings localServiceSettings = securityBindingElement.LocalServiceSettings;
            IMessageFilterTable<EndpointAddress> propertyOrDefault = recipientRequirement.GetPropertyOrDefault<IMessageFilterTable<EndpointAddress>>(ServiceModelSecurityTokenRequirement.EndpointFilterTableProperty, null);
            if (!flag)
            {
                sctResolver = new SecurityContextSecurityTokenResolver(0x7fffffff, false);
                return new SecuritySessionSecurityTokenAuthenticator { BootstrapSecurityBindingElement = System.ServiceModel.Security.SecurityUtils.GetIssuerSecurityBindingElement(recipientRequirement), IssuedSecurityTokenParameters = recipientRequirement.GetProperty<SecurityTokenParameters>(ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty), IssuedTokenCache = (ISecurityContextSecurityTokenCache) sctResolver, IssuerBindingContext = recipientRequirement.GetProperty<BindingContext>(ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty), KeyEntropyMode = securityBindingElement.KeyEntropyMode, ListenUri = recipientRequirement.ListenUri, SecurityAlgorithmSuite = recipientRequirement.SecurityAlgorithmSuite, SessionTokenLifetime = TimeSpan.MaxValue, KeyRenewalInterval = securityBindingElement.LocalServiceSettings.SessionKeyRenewalInterval, StandardsManager = System.ServiceModel.Security.SecurityUtils.CreateSecurityStandardsManager(recipientRequirement, this), EndpointFilterTable = propertyOrDefault, MaximumConcurrentNegotiations = localServiceSettings.MaxStatefulNegotiations, NegotiationTimeout = localServiceSettings.NegotiationTimeout, PreserveBootstrapTokens = preserveBootstrapTokens };
            }
            sctResolver = new SecurityContextSecurityTokenResolver(localServiceSettings.MaxCachedCookies, true, localServiceSettings.MaxClockSkew);
            return new AcceleratedTokenAuthenticator { 
                BootstrapSecurityBindingElement = System.ServiceModel.Security.SecurityUtils.GetIssuerSecurityBindingElement(recipientRequirement), KeyEntropyMode = securityBindingElement.KeyEntropyMode, EncryptStateInServiceToken = true, IssuedSecurityTokenParameters = recipientRequirement.GetProperty<SecurityTokenParameters>(ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty), IssuedTokenCache = (ISecurityContextSecurityTokenCache) sctResolver, IssuerBindingContext = recipientRequirement.GetProperty<BindingContext>(ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty), ListenUri = recipientRequirement.ListenUri, SecurityAlgorithmSuite = recipientRequirement.SecurityAlgorithmSuite, StandardsManager = System.ServiceModel.Security.SecurityUtils.CreateSecurityStandardsManager(recipientRequirement, this), SecurityStateEncoder = this.parent.SecureConversationAuthentication.SecurityStateEncoder, KnownTypes = this.parent.SecureConversationAuthentication.SecurityContextClaimTypes, PreserveBootstrapTokens = preserveBootstrapTokens, MaximumCachedNegotiationState = localServiceSettings.MaxStatefulNegotiations, NegotiationTimeout = localServiceSettings.NegotiationTimeout, ServiceTokenLifetime = localServiceSettings.IssuedCookieLifetime, MaximumConcurrentNegotiations = localServiceSettings.MaxStatefulNegotiations, 
                AuditLogLocation = recipientRequirement.AuditLogLocation, SuppressAuditFailure = recipientRequirement.SuppressAuditFailure, MessageAuthenticationAuditLevel = recipientRequirement.MessageAuthenticationAuditLevel, EndpointFilterTable = propertyOrDefault
             };
        }

        public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver)
        {
            if (tokenRequirement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenRequirement");
            }
            string tokenType = tokenRequirement.TokenType;
            outOfBandTokenResolver = null;
            SecurityTokenAuthenticator authenticator = null;
            if (((tokenRequirement is InitiatorServiceModelSecurityTokenRequirement) && (tokenType == SecurityTokenTypes.X509Certificate)) && (tokenRequirement.KeyUsage == SecurityKeyUsage.Exchange))
            {
                return new X509SecurityTokenAuthenticator(X509CertificateValidator.None, false);
            }
            RecipientServiceModelSecurityTokenRequirement recipientRequirement = tokenRequirement as RecipientServiceModelSecurityTokenRequirement;
            if (recipientRequirement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SecurityTokenManagerCannotCreateAuthenticatorForRequirement", new object[] { tokenRequirement })));
            }
            if (tokenType == SecurityTokenTypes.X509Certificate)
            {
                authenticator = this.CreateClientX509TokenAuthenticator();
            }
            else if (tokenType == SecurityTokenTypes.Kerberos)
            {
                authenticator = new KerberosSecurityTokenAuthenticatorWrapper(new KerberosSecurityTokenAuthenticator(this.parent.WindowsAuthentication.IncludeWindowsGroups));
            }
            else if (tokenType == SecurityTokenTypes.UserName)
            {
                if (this.parent.UserNameAuthentication.UserNamePasswordValidationMode == UserNamePasswordValidationMode.Windows)
                {
                    if (this.parent.UserNameAuthentication.CacheLogonTokens)
                    {
                        authenticator = new WindowsUserNameCachingSecurityTokenAuthenticator(this.parent.UserNameAuthentication.IncludeWindowsGroups, this.parent.UserNameAuthentication.MaxCachedLogonTokens, this.parent.UserNameAuthentication.CachedLogonTokenLifetime);
                    }
                    else
                    {
                        authenticator = new WindowsUserNameSecurityTokenAuthenticator(this.parent.UserNameAuthentication.IncludeWindowsGroups);
                    }
                }
                else
                {
                    authenticator = new CustomUserNameSecurityTokenAuthenticator(this.parent.UserNameAuthentication.GetUserNamePasswordValidator());
                }
            }
            else if (tokenType == SecurityTokenTypes.Rsa)
            {
                authenticator = new RsaSecurityTokenAuthenticator();
            }
            else if (tokenType == ServiceModelSecurityTokenTypes.AnonymousSslnego)
            {
                authenticator = this.CreateTlsnegoSecurityTokenAuthenticator(recipientRequirement, false, out outOfBandTokenResolver);
            }
            else if (tokenType == ServiceModelSecurityTokenTypes.MutualSslnego)
            {
                authenticator = this.CreateTlsnegoSecurityTokenAuthenticator(recipientRequirement, true, out outOfBandTokenResolver);
            }
            else if (tokenType == ServiceModelSecurityTokenTypes.Spnego)
            {
                authenticator = this.CreateSpnegoSecurityTokenAuthenticator(recipientRequirement, out outOfBandTokenResolver);
            }
            else if (tokenType == ServiceModelSecurityTokenTypes.SecureConversation)
            {
                authenticator = this.CreateSecureConversationTokenAuthenticator(recipientRequirement, false, out outOfBandTokenResolver);
            }
            else if (((tokenType == SecurityTokenTypes.Saml) || (tokenType == "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1")) || ((tokenType == "urn:oasis:names:tc:SAML:1.0:assertion") || ((tokenType == null) && this.IsIssuedSecurityTokenRequirement(recipientRequirement))))
            {
                authenticator = this.CreateSamlTokenAuthenticator(recipientRequirement, out outOfBandTokenResolver);
            }
            if (authenticator == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SecurityTokenManagerCannotCreateAuthenticatorForRequirement", new object[] { tokenRequirement })));
            }
            return authenticator;
        }

        public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement requirement)
        {
            if (requirement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requirement");
            }
            RecipientServiceModelSecurityTokenRequirement recipientRequirement = requirement as RecipientServiceModelSecurityTokenRequirement;
            SecurityTokenProvider provider = null;
            if (recipientRequirement != null)
            {
                provider = this.CreateLocalSecurityTokenProvider(recipientRequirement);
            }
            else if (requirement is InitiatorServiceModelSecurityTokenRequirement)
            {
                provider = this.CreateUncorrelatedDuplexSecurityTokenProvider((InitiatorServiceModelSecurityTokenRequirement) requirement);
            }
            if (provider == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SecurityTokenManagerCannotCreateProviderForRequirement", new object[] { requirement })));
            }
            return provider;
        }

        public override SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityTokenVersion version)
        {
            if (version == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
            }
            MessageSecurityTokenVersion version2 = version as MessageSecurityTokenVersion;
            if (version2 == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SecurityTokenManagerCannotCreateSerializerForVersion", new object[] { version })));
            }
            SamlSerializer samlSerializer = null;
            if (this.parent.IssuedTokenAuthentication != null)
            {
                samlSerializer = this.parent.IssuedTokenAuthentication.SamlSerializer;
            }
            else
            {
                samlSerializer = new SamlSerializer();
            }
            return new WSSecurityTokenSerializer(version2.SecurityVersion, version2.TrustVersion, version2.SecureConversationVersion, version2.EmitBspRequiredAttributes, samlSerializer, this.parent.SecureConversationAuthentication.SecurityStateEncoder, this.parent.SecureConversationAuthentication.SecurityContextClaimTypes);
        }

        private X509SecurityTokenProvider CreateServerX509TokenProvider()
        {
            if (this.parent.ServiceCertificate.Certificate == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ServiceCertificateNotProvidedOnServiceCredentials")));
            }
            System.ServiceModel.Security.SecurityUtils.EnsureCertificateCanDoKeyExchange(this.parent.ServiceCertificate.Certificate);
            return new ServiceX509SecurityTokenProvider(this.parent.ServiceCertificate.Certificate);
        }

        private SecurityTokenAuthenticator CreateSpnegoSecurityTokenAuthenticator(RecipientServiceModelSecurityTokenRequirement recipientRequirement, out SecurityTokenResolver sctResolver)
        {
            SecurityBindingElement securityBindingElement = recipientRequirement.SecurityBindingElement;
            if (securityBindingElement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("TokenAuthenticatorRequiresSecurityBindingElement", new object[] { recipientRequirement }));
            }
            bool flag = !recipientRequirement.SupportSecurityContextCancellation;
            LocalServiceSecuritySettings localServiceSettings = securityBindingElement.LocalServiceSettings;
            sctResolver = new SecurityContextSecurityTokenResolver(localServiceSettings.MaxCachedCookies, true);
            ExtendedProtectionPolicy result = null;
            recipientRequirement.TryGetProperty<ExtendedProtectionPolicy>(ServiceModelSecurityTokenRequirement.ExtendedProtectionPolicy, out result);
            SpnegoTokenAuthenticator authenticator = new SpnegoTokenAuthenticator {
                ExtendedProtectionPolicy = result,
                AllowUnauthenticatedCallers = this.parent.WindowsAuthentication.AllowAnonymousLogons,
                ExtractGroupsForWindowsAccounts = this.parent.WindowsAuthentication.IncludeWindowsGroups,
                IsClientAnonymous = false,
                EncryptStateInServiceToken = flag,
                IssuedSecurityTokenParameters = recipientRequirement.GetProperty<SecurityTokenParameters>(ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty),
                IssuedTokenCache = (ISecurityContextSecurityTokenCache) sctResolver,
                IssuerBindingContext = recipientRequirement.GetProperty<BindingContext>(ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty),
                ListenUri = recipientRequirement.ListenUri,
                SecurityAlgorithmSuite = recipientRequirement.SecurityAlgorithmSuite,
                StandardsManager = System.ServiceModel.Security.SecurityUtils.CreateSecurityStandardsManager(recipientRequirement, this),
                SecurityStateEncoder = this.parent.SecureConversationAuthentication.SecurityStateEncoder,
                KnownTypes = this.parent.SecureConversationAuthentication.SecurityContextClaimTypes
            };
            if (securityBindingElement is TransportSecurityBindingElement)
            {
                authenticator.MaxMessageSize = System.ServiceModel.Security.SecurityUtils.GetMaxNegotiationBufferSize(authenticator.IssuerBindingContext);
            }
            authenticator.MaximumCachedNegotiationState = localServiceSettings.MaxStatefulNegotiations;
            authenticator.NegotiationTimeout = localServiceSettings.NegotiationTimeout;
            authenticator.ServiceTokenLifetime = localServiceSettings.IssuedCookieLifetime;
            authenticator.MaximumConcurrentNegotiations = localServiceSettings.MaxStatefulNegotiations;
            authenticator.AuditLogLocation = recipientRequirement.AuditLogLocation;
            authenticator.SuppressAuditFailure = recipientRequirement.SuppressAuditFailure;
            authenticator.MessageAuthenticationAuditLevel = recipientRequirement.MessageAuthenticationAuditLevel;
            return authenticator;
        }

        private SecurityTokenAuthenticator CreateTlsnegoClientX509TokenAuthenticator(RecipientServiceModelSecurityTokenRequirement recipientRequirement)
        {
            SecurityTokenResolver resolver;
            RecipientServiceModelSecurityTokenRequirement tokenRequirement = new RecipientServiceModelSecurityTokenRequirement {
                TokenType = SecurityTokenTypes.X509Certificate,
                KeyUsage = SecurityKeyUsage.Signature,
                ListenUri = recipientRequirement.ListenUri,
                KeyType = SecurityKeyType.AsymmetricKey,
                SecurityBindingElement = recipientRequirement.SecurityBindingElement
            };
            return this.CreateSecurityTokenAuthenticator(tokenRequirement, out resolver);
        }

        private SecurityTokenAuthenticator CreateTlsnegoSecurityTokenAuthenticator(RecipientServiceModelSecurityTokenRequirement recipientRequirement, bool requireClientCertificate, out SecurityTokenResolver sctResolver)
        {
            SecurityBindingElement securityBindingElement = recipientRequirement.SecurityBindingElement;
            if (securityBindingElement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("TokenAuthenticatorRequiresSecurityBindingElement", new object[] { recipientRequirement }));
            }
            bool flag = !recipientRequirement.SupportSecurityContextCancellation;
            LocalServiceSecuritySettings localServiceSettings = securityBindingElement.LocalServiceSettings;
            sctResolver = new SecurityContextSecurityTokenResolver(localServiceSettings.MaxCachedCookies, true);
            TlsnegoTokenAuthenticator authenticator = new TlsnegoTokenAuthenticator {
                IsClientAnonymous = !requireClientCertificate
            };
            if (requireClientCertificate)
            {
                authenticator.ClientTokenAuthenticator = this.CreateTlsnegoClientX509TokenAuthenticator(recipientRequirement);
                authenticator.MapCertificateToWindowsAccount = this.ServiceCredentials.ClientCertificate.Authentication.MapClientCertificateToWindowsAccount;
            }
            authenticator.EncryptStateInServiceToken = flag;
            authenticator.IssuedSecurityTokenParameters = recipientRequirement.GetProperty<SecurityTokenParameters>(ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty);
            authenticator.IssuedTokenCache = (ISecurityContextSecurityTokenCache) sctResolver;
            authenticator.IssuerBindingContext = recipientRequirement.GetProperty<BindingContext>(ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty);
            authenticator.ListenUri = recipientRequirement.ListenUri;
            authenticator.SecurityAlgorithmSuite = recipientRequirement.SecurityAlgorithmSuite;
            authenticator.StandardsManager = System.ServiceModel.Security.SecurityUtils.CreateSecurityStandardsManager(recipientRequirement, this);
            authenticator.SecurityStateEncoder = this.parent.SecureConversationAuthentication.SecurityStateEncoder;
            authenticator.KnownTypes = this.parent.SecureConversationAuthentication.SecurityContextClaimTypes;
            authenticator.ServerTokenProvider = this.CreateTlsnegoServerX509TokenProvider(recipientRequirement);
            authenticator.MaximumCachedNegotiationState = localServiceSettings.MaxStatefulNegotiations;
            authenticator.NegotiationTimeout = localServiceSettings.NegotiationTimeout;
            authenticator.ServiceTokenLifetime = localServiceSettings.IssuedCookieLifetime;
            authenticator.MaximumConcurrentNegotiations = localServiceSettings.MaxStatefulNegotiations;
            if (securityBindingElement is TransportSecurityBindingElement)
            {
                authenticator.MaxMessageSize = System.ServiceModel.Security.SecurityUtils.GetMaxNegotiationBufferSize(authenticator.IssuerBindingContext);
            }
            authenticator.AuditLogLocation = recipientRequirement.AuditLogLocation;
            authenticator.SuppressAuditFailure = recipientRequirement.SuppressAuditFailure;
            authenticator.MessageAuthenticationAuditLevel = recipientRequirement.MessageAuthenticationAuditLevel;
            return authenticator;
        }

        private SecurityTokenProvider CreateTlsnegoServerX509TokenProvider(RecipientServiceModelSecurityTokenRequirement recipientRequirement)
        {
            RecipientServiceModelSecurityTokenRequirement tokenRequirement = new RecipientServiceModelSecurityTokenRequirement {
                TokenType = SecurityTokenTypes.X509Certificate,
                KeyUsage = SecurityKeyUsage.Exchange,
                ListenUri = recipientRequirement.ListenUri,
                KeyType = SecurityKeyType.AsymmetricKey,
                SecurityBindingElement = recipientRequirement.SecurityBindingElement
            };
            return this.CreateSecurityTokenProvider(tokenRequirement);
        }

        private SecurityTokenProvider CreateUncorrelatedDuplexSecurityTokenProvider(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            string tokenType = initiatorRequirement.TokenType;
            SecurityTokenProvider provider = null;
            if (!(tokenType == SecurityTokenTypes.X509Certificate))
            {
                return provider;
            }
            if (initiatorRequirement.KeyUsage == SecurityKeyUsage.Exchange)
            {
                if (this.parent.ClientCertificate.Certificate == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ClientCertificateNotProvidedOnServiceCredentials")));
                }
                return new X509SecurityTokenProvider(this.parent.ClientCertificate.Certificate);
            }
            return this.CreateServerX509TokenProvider();
        }

        public virtual EndpointIdentity GetIdentityOfSelf(SecurityTokenRequirement tokenRequirement)
        {
            if (tokenRequirement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenRequirement");
            }
            if (tokenRequirement is RecipientServiceModelSecurityTokenRequirement)
            {
                string tokenType = tokenRequirement.TokenType;
                if (((tokenType == SecurityTokenTypes.X509Certificate) || (tokenType == ServiceModelSecurityTokenTypes.AnonymousSslnego)) || (tokenType == ServiceModelSecurityTokenTypes.MutualSslnego))
                {
                    if (this.parent.ServiceCertificate.Certificate != null)
                    {
                        return EndpointIdentity.CreateX509CertificateIdentity(this.parent.ServiceCertificate.Certificate);
                    }
                }
                else
                {
                    if ((tokenType == SecurityTokenTypes.Kerberos) || (tokenType == ServiceModelSecurityTokenTypes.Spnego))
                    {
                        return System.ServiceModel.Security.SecurityUtils.CreateWindowsIdentity();
                    }
                    if (tokenType == ServiceModelSecurityTokenTypes.SecureConversation)
                    {
                        SecurityBindingElement secureConversationSecurityBindingElement = ((RecipientServiceModelSecurityTokenRequirement) tokenRequirement).SecureConversationSecurityBindingElement;
                        if (secureConversationSecurityBindingElement != null)
                        {
                            if ((secureConversationSecurityBindingElement == null) || (secureConversationSecurityBindingElement is TransportSecurityBindingElement))
                            {
                                return null;
                            }
                            SecurityTokenParameters parameters = (secureConversationSecurityBindingElement is SymmetricSecurityBindingElement) ? ((SymmetricSecurityBindingElement) secureConversationSecurityBindingElement).ProtectionTokenParameters : ((AsymmetricSecurityBindingElement) secureConversationSecurityBindingElement).RecipientTokenParameters;
                            SecurityTokenRequirement requirement = new RecipientServiceModelSecurityTokenRequirement();
                            parameters.InitializeSecurityTokenRequirement(requirement);
                            return this.GetIdentityOfSelf(requirement);
                        }
                    }
                }
            }
            return null;
        }

        protected bool IsIssuedSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            return ((requirement != null) && requirement.Properties.ContainsKey(ServiceModelSecurityTokenRequirement.IssuerAddressProperty));
        }

        public System.ServiceModel.Description.ServiceCredentials ServiceCredentials
        {
            get
            {
                return this.parent;
            }
        }

        internal class KerberosSecurityTokenAuthenticatorWrapper : CommunicationObjectSecurityTokenAuthenticator
        {
            private System.IdentityModel.SafeFreeCredentials credentialsHandle;
            private KerberosSecurityTokenAuthenticator innerAuthenticator;

            public KerberosSecurityTokenAuthenticatorWrapper(KerberosSecurityTokenAuthenticator innerAuthenticator)
            {
                this.innerAuthenticator = innerAuthenticator;
            }

            protected override bool CanValidateTokenCore(SecurityToken token)
            {
                return this.innerAuthenticator.CanValidateToken(token);
            }

            private void FreeCredentialsHandle()
            {
                if (this.credentialsHandle != null)
                {
                    this.credentialsHandle.Close();
                    this.credentialsHandle = null;
                }
            }

            public override void OnAbort()
            {
                base.OnAbort();
                this.FreeCredentialsHandle();
            }

            public override void OnClose(TimeSpan timeout)
            {
                base.OnClose(timeout);
                this.FreeCredentialsHandle();
            }

            public override void OnOpening()
            {
                base.OnOpening();
                if (this.credentialsHandle == null)
                {
                    this.credentialsHandle = System.ServiceModel.Security.SecurityUtils.GetCredentialsHandle("Kerberos", null, true, new string[0]);
                }
            }

            internal ReadOnlyCollection<IAuthorizationPolicy> ValidateToken(SecurityToken token, ChannelBinding channelBinding, ExtendedProtectionPolicy protectionPolicy)
            {
                KerberosReceiverSecurityToken token2 = (KerberosReceiverSecurityToken) token;
                token2.Initialize(this.credentialsHandle, channelBinding, protectionPolicy);
                return this.innerAuthenticator.ValidateToken(token2);
            }

            protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
            {
                return this.ValidateToken(token, null, null);
            }
        }
    }
}


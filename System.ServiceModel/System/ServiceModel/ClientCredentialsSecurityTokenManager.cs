namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    public class ClientCredentialsSecurityTokenManager : SecurityTokenManager
    {
        private System.ServiceModel.Description.ClientCredentials parent;

        public ClientCredentialsSecurityTokenManager(System.ServiceModel.Description.ClientCredentials clientCredentials)
        {
            if (clientCredentials == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("clientCredentials");
            }
            this.parent = clientCredentials;
        }

        private bool CardSpaceTryCreateSecurityTokenProviderStub(SecurityTokenRequirement tokenRequirement, ClientCredentialsSecurityTokenManager clientCredentialsTokenManager, out SecurityTokenProvider provider)
        {
            return InfoCardHelper.TryCreateSecurityTokenProvider(tokenRequirement, clientCredentialsTokenManager, out provider);
        }

        private void CopyIssuerChannelBehaviorsAndAddSecurityCredentials(IssuedSecurityTokenProvider federationTokenProvider, KeyedByTypeCollection<IEndpointBehavior> issuerChannelBehaviors, EndpointAddress issuerAddress)
        {
            if (issuerChannelBehaviors != null)
            {
                foreach (IEndpointBehavior behavior in issuerChannelBehaviors)
                {
                    if (behavior is SecurityCredentialsManager)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("IssuerChannelBehaviorsCannotContainSecurityCredentialsManager", new object[] { issuerAddress, typeof(SecurityCredentialsManager) })));
                    }
                    federationTokenProvider.IssuerChannelBehaviors.Add(behavior);
                }
            }
            federationTokenProvider.IssuerChannelBehaviors.Add(this.parent);
        }

        private IssuedSecurityTokenProvider CreateIssuedSecurityTokenProvider(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            KeyedByTypeCollection<IEndpointBehavior> localIssuerChannelBehaviors;
            MessageSecurityVersion version;
            SecurityTokenSerializer serializer;
            ChannelParameterCollection parameters2;
            if (initiatorRequirement.TargetAddress == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("TokenRequirementDoesNotSpecifyTargetAddress", new object[] { initiatorRequirement }));
            }
            SecurityBindingElement securityBindingElement = initiatorRequirement.SecurityBindingElement;
            if (securityBindingElement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("TokenProviderRequiresSecurityBindingElement", new object[] { initiatorRequirement }));
            }
            EndpointAddress issuerAddress = initiatorRequirement.IssuerAddress;
            Binding issuerBinding = initiatorRequirement.IssuerBinding;
            bool flag = (issuerAddress == null) || issuerAddress.Equals(EndpointAddress.AnonymousAddress);
            if (flag)
            {
                issuerAddress = this.parent.IssuedToken.LocalIssuerAddress;
                issuerBinding = this.parent.IssuedToken.LocalIssuerBinding;
            }
            if (issuerAddress == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("StsAddressNotSet", new object[] { initiatorRequirement.TargetAddress })));
            }
            if (issuerBinding == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("StsBindingNotSet", new object[] { issuerAddress })));
            }
            Uri uri = issuerAddress.Uri;
            if (!this.parent.IssuedToken.IssuerChannelBehaviors.TryGetValue(issuerAddress.Uri, out localIssuerChannelBehaviors) && flag)
            {
                localIssuerChannelBehaviors = this.parent.IssuedToken.LocalIssuerChannelBehaviors;
            }
            IssuedSecurityTokenProvider federationTokenProvider = new IssuedSecurityTokenProvider(this.GetCredentialsHandle(initiatorRequirement)) {
                TargetAddress = initiatorRequirement.TargetAddress
            };
            this.CopyIssuerChannelBehaviorsAndAddSecurityCredentials(federationTokenProvider, localIssuerChannelBehaviors, issuerAddress);
            federationTokenProvider.CacheIssuedTokens = this.parent.IssuedToken.CacheIssuedTokens;
            federationTokenProvider.IdentityVerifier = securityBindingElement.LocalClientSettings.IdentityVerifier;
            federationTokenProvider.IssuerAddress = issuerAddress;
            federationTokenProvider.IssuerBinding = issuerBinding;
            federationTokenProvider.KeyEntropyMode = this.GetIssuerBindingKeyEntropyModeOrDefault(issuerBinding);
            federationTokenProvider.MaxIssuedTokenCachingTime = this.parent.IssuedToken.MaxIssuedTokenCachingTime;
            federationTokenProvider.SecurityAlgorithmSuite = initiatorRequirement.SecurityAlgorithmSuite;
            IssuedSecurityTokenParameters property = initiatorRequirement.GetProperty<IssuedSecurityTokenParameters>(ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty);
            this.GetIssuerBindingSecurityVersion(issuerBinding, property.DefaultMessageSecurityVersion, initiatorRequirement.SecurityBindingElement, out version, out serializer);
            federationTokenProvider.MessageSecurityVersion = version;
            federationTokenProvider.SecurityTokenSerializer = serializer;
            federationTokenProvider.IssuedTokenRenewalThresholdPercentage = this.parent.IssuedToken.IssuedTokenRenewalThresholdPercentage;
            IEnumerable<XmlElement> enumerable = property.CreateRequestParameters(version, serializer);
            if (enumerable != null)
            {
                foreach (XmlElement element2 in enumerable)
                {
                    federationTokenProvider.TokenRequestParameters.Add(element2);
                }
            }
            if (initiatorRequirement.TryGetProperty<ChannelParameterCollection>(ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty, out parameters2))
            {
                federationTokenProvider.ChannelParameters = parameters2;
            }
            return federationTokenProvider;
        }

        private SecurityTokenProvider CreateSecureConversationSecurityTokenProvider(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            Uri uri2;
            int num2;
            EndpointAddress targetAddress = initiatorRequirement.TargetAddress;
            if (targetAddress == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("TokenRequirementDoesNotSpecifyTargetAddress", new object[] { initiatorRequirement }));
            }
            SecurityBindingElement securityBindingElement = initiatorRequirement.SecurityBindingElement;
            if (securityBindingElement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("TokenProviderRequiresSecurityBindingElement", new object[] { initiatorRequirement }));
            }
            LocalClientSecuritySettings localClientSettings = securityBindingElement.LocalClientSettings;
            BindingContext property = initiatorRequirement.GetProperty<BindingContext>(ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty);
            ChannelParameterCollection propertyOrDefault = initiatorRequirement.GetPropertyOrDefault<ChannelParameterCollection>(ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty, null);
            if (initiatorRequirement.SupportSecurityContextCancellation)
            {
                Uri uri;
                int num;
                EndpointAddress address2;
                SecuritySessionSecurityTokenProvider provider = new SecuritySessionSecurityTokenProvider(this.GetCredentialsHandle(initiatorRequirement)) {
                    BootstrapSecurityBindingElement = System.ServiceModel.Security.SecurityUtils.GetIssuerSecurityBindingElement(initiatorRequirement),
                    IssuedSecurityTokenParameters = initiatorRequirement.GetProperty<SecurityTokenParameters>(ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty),
                    IssuerBindingContext = property,
                    KeyEntropyMode = securityBindingElement.KeyEntropyMode,
                    SecurityAlgorithmSuite = initiatorRequirement.SecurityAlgorithmSuite,
                    StandardsManager = System.ServiceModel.Security.SecurityUtils.CreateSecurityStandardsManager(initiatorRequirement, this),
                    TargetAddress = targetAddress,
                    Via = initiatorRequirement.GetPropertyOrDefault<Uri>(ServiceModelSecurityTokenRequirement.ViaProperty, null)
                };
                if (initiatorRequirement.TryGetProperty<Uri>(ServiceModelSecurityTokenRequirement.PrivacyNoticeUriProperty, out uri))
                {
                    provider.PrivacyNoticeUri = uri;
                }
                if (initiatorRequirement.TryGetProperty<int>(ServiceModelSecurityTokenRequirement.PrivacyNoticeVersionProperty, out num))
                {
                    provider.PrivacyNoticeVersion = num;
                }
                if (initiatorRequirement.TryGetProperty<EndpointAddress>(ServiceModelSecurityTokenRequirement.DuplexClientLocalAddressProperty, out address2))
                {
                    provider.LocalAddress = address2;
                }
                provider.ChannelParameters = propertyOrDefault;
                return provider;
            }
            AcceleratedTokenProvider provider2 = new AcceleratedTokenProvider(this.GetCredentialsHandle(initiatorRequirement)) {
                IssuerAddress = initiatorRequirement.IssuerAddress,
                BootstrapSecurityBindingElement = System.ServiceModel.Security.SecurityUtils.GetIssuerSecurityBindingElement(initiatorRequirement),
                CacheServiceTokens = localClientSettings.CacheCookies,
                IssuerBindingContext = property,
                KeyEntropyMode = securityBindingElement.KeyEntropyMode,
                MaxServiceTokenCachingTime = localClientSettings.MaxCookieCachingTime,
                SecurityAlgorithmSuite = initiatorRequirement.SecurityAlgorithmSuite,
                ServiceTokenValidityThresholdPercentage = localClientSettings.CookieRenewalThresholdPercentage,
                StandardsManager = System.ServiceModel.Security.SecurityUtils.CreateSecurityStandardsManager(initiatorRequirement, this),
                TargetAddress = targetAddress,
                Via = initiatorRequirement.GetPropertyOrDefault<Uri>(ServiceModelSecurityTokenRequirement.ViaProperty, null)
            };
            if (initiatorRequirement.TryGetProperty<Uri>(ServiceModelSecurityTokenRequirement.PrivacyNoticeUriProperty, out uri2))
            {
                provider2.PrivacyNoticeUri = uri2;
            }
            provider2.ChannelParameters = propertyOrDefault;
            if (initiatorRequirement.TryGetProperty<int>(ServiceModelSecurityTokenRequirement.PrivacyNoticeVersionProperty, out num2))
            {
                provider2.PrivacyNoticeVersion = num2;
            }
            return provider2;
        }

        public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver)
        {
            if (tokenRequirement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenRequirement");
            }
            outOfBandTokenResolver = null;
            SecurityTokenAuthenticator authenticator = null;
            InitiatorServiceModelSecurityTokenRequirement requirement = tokenRequirement as InitiatorServiceModelSecurityTokenRequirement;
            if (requirement != null)
            {
                string tokenType = requirement.TokenType;
                if (this.IsIssuedSecurityTokenRequirement(requirement))
                {
                    return new GenericXmlSecurityTokenAuthenticator();
                }
                if (tokenType == SecurityTokenTypes.X509Certificate)
                {
                    if (requirement.IsOutOfBandToken)
                    {
                        authenticator = new X509SecurityTokenAuthenticator(X509CertificateValidator.None);
                    }
                    else
                    {
                        authenticator = this.CreateServerX509TokenAuthenticator();
                    }
                }
                else if (tokenType == SecurityTokenTypes.Rsa)
                {
                    authenticator = new RsaSecurityTokenAuthenticator();
                }
                else if (tokenType == SecurityTokenTypes.Kerberos)
                {
                    authenticator = new KerberosRequestorSecurityTokenAuthenticator();
                }
                else if (((tokenType == ServiceModelSecurityTokenTypes.SecureConversation) || (tokenType == ServiceModelSecurityTokenTypes.MutualSslnego)) || ((tokenType == ServiceModelSecurityTokenTypes.AnonymousSslnego) || (tokenType == ServiceModelSecurityTokenTypes.Spnego)))
                {
                    authenticator = new GenericXmlSecurityTokenAuthenticator();
                }
            }
            else if ((tokenRequirement is RecipientServiceModelSecurityTokenRequirement) && (tokenRequirement.TokenType == SecurityTokenTypes.X509Certificate))
            {
                authenticator = this.CreateServerX509TokenAuthenticator();
            }
            if (authenticator == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SecurityTokenManagerCannotCreateAuthenticatorForRequirement", new object[] { tokenRequirement })));
            }
            return authenticator;
        }

        public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement)
        {
            return this.CreateSecurityTokenProvider(tokenRequirement, false);
        }

        internal SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement, bool disableInfoCard)
        {
            if (tokenRequirement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenRequirement");
            }
            SecurityTokenProvider provider = null;
            if (disableInfoCard || !this.CardSpaceTryCreateSecurityTokenProviderStub(tokenRequirement, this, out provider))
            {
                if (((tokenRequirement is RecipientServiceModelSecurityTokenRequirement) && (tokenRequirement.TokenType == SecurityTokenTypes.X509Certificate)) && (tokenRequirement.KeyUsage == SecurityKeyUsage.Exchange))
                {
                    if (this.parent.ClientCertificate.Certificate == null)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ClientCertificateNotProvidedOnClientCredentials")));
                    }
                    provider = new X509SecurityTokenProvider(this.parent.ClientCertificate.Certificate);
                }
                else if (tokenRequirement is InitiatorServiceModelSecurityTokenRequirement)
                {
                    InitiatorServiceModelSecurityTokenRequirement requirement = tokenRequirement as InitiatorServiceModelSecurityTokenRequirement;
                    string tokenType = requirement.TokenType;
                    if (this.IsIssuedSecurityTokenRequirement(requirement))
                    {
                        provider = this.CreateIssuedSecurityTokenProvider(requirement);
                    }
                    else if (tokenType == SecurityTokenTypes.X509Certificate)
                    {
                        if (requirement.Properties.ContainsKey(SecurityTokenRequirement.KeyUsageProperty) && (requirement.KeyUsage == SecurityKeyUsage.Exchange))
                        {
                            provider = this.CreateServerX509TokenProvider(requirement.TargetAddress);
                        }
                        else
                        {
                            if (this.parent.ClientCertificate.Certificate == null)
                            {
                                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ClientCertificateNotProvidedOnClientCredentials")));
                            }
                            provider = new X509SecurityTokenProvider(this.parent.ClientCertificate.Certificate);
                        }
                    }
                    else if (tokenType == SecurityTokenTypes.Kerberos)
                    {
                        provider = new KerberosSecurityTokenProviderWrapper(new KerberosSecurityTokenProvider(this.GetServicePrincipalName(requirement), this.parent.Windows.AllowedImpersonationLevel, System.ServiceModel.Security.SecurityUtils.GetNetworkCredentialOrDefault(this.parent.Windows.ClientCredential)), this.GetCredentialsHandle(requirement));
                    }
                    else if (tokenType == SecurityTokenTypes.UserName)
                    {
                        if (this.parent.UserName.UserName == null)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UserNamePasswordNotProvidedOnClientCredentials")));
                        }
                        provider = new UserNameSecurityTokenProvider(this.parent.UserName.UserName, this.parent.UserName.Password);
                    }
                    else if (tokenType == ServiceModelSecurityTokenTypes.SspiCredential)
                    {
                        if (this.IsDigestAuthenticationScheme(requirement))
                        {
                            provider = new SspiSecurityTokenProvider(System.ServiceModel.Security.SecurityUtils.GetNetworkCredentialOrDefault(this.parent.HttpDigest.ClientCredential), true, this.parent.HttpDigest.AllowedImpersonationLevel);
                        }
                        else
                        {
                            provider = new SspiSecurityTokenProvider(System.ServiceModel.Security.SecurityUtils.GetNetworkCredentialOrDefault(this.parent.Windows.ClientCredential), this.parent.Windows.AllowNtlm, this.parent.Windows.AllowedImpersonationLevel);
                        }
                    }
                    else if (tokenType == ServiceModelSecurityTokenTypes.Spnego)
                    {
                        provider = this.CreateSpnegoTokenProvider(requirement);
                    }
                    else if (tokenType == ServiceModelSecurityTokenTypes.MutualSslnego)
                    {
                        provider = this.CreateTlsnegoTokenProvider(requirement, true);
                    }
                    else if (tokenType == ServiceModelSecurityTokenTypes.AnonymousSslnego)
                    {
                        provider = this.CreateTlsnegoTokenProvider(requirement, false);
                    }
                    else if (tokenType == ServiceModelSecurityTokenTypes.SecureConversation)
                    {
                        provider = this.CreateSecureConversationSecurityTokenProvider(requirement);
                    }
                }
            }
            if (provider == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SecurityTokenManagerCannotCreateProviderForRequirement", new object[] { tokenRequirement })));
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
            return new WSSecurityTokenSerializer(version2.SecurityVersion, version2.TrustVersion, version2.SecureConversationVersion, version2.EmitBspRequiredAttributes, null, null, null);
        }

        protected SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityVersion version)
        {
            if (version == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            }
            return this.CreateSecurityTokenSerializer(MessageSecurityTokenVersion.GetSecurityTokenVersion(version, true));
        }

        private X509SecurityTokenAuthenticator CreateServerX509TokenAuthenticator()
        {
            return new X509SecurityTokenAuthenticator(this.parent.ServiceCertificate.Authentication.GetCertificateValidator(), false);
        }

        private SecurityTokenProvider CreateServerX509TokenProvider(EndpointAddress targetAddress)
        {
            X509Certificate2 defaultCertificate = null;
            if (targetAddress != null)
            {
                this.parent.ServiceCertificate.ScopedCertificates.TryGetValue(targetAddress.Uri, out defaultCertificate);
            }
            if (defaultCertificate == null)
            {
                defaultCertificate = this.parent.ServiceCertificate.DefaultCertificate;
            }
            if (((defaultCertificate == null) && (targetAddress.Identity != null)) && (targetAddress.Identity.GetType() == typeof(X509CertificateEndpointIdentity)))
            {
                defaultCertificate = ((X509CertificateEndpointIdentity) targetAddress.Identity).Certificates[0];
            }
            if (defaultCertificate == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ServiceCertificateNotProvidedOnClientCredentials", new object[] { targetAddress.Uri })));
            }
            return new X509SecurityTokenProvider(defaultCertificate);
        }

        private SecurityTokenProvider CreateSpnegoTokenProvider(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            EndpointAddress targetAddress = initiatorRequirement.TargetAddress;
            if (targetAddress == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("TokenRequirementDoesNotSpecifyTargetAddress", new object[] { initiatorRequirement }));
            }
            SecurityBindingElement securityBindingElement = initiatorRequirement.SecurityBindingElement;
            if (securityBindingElement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("TokenProviderRequiresSecurityBindingElement", new object[] { initiatorRequirement }));
            }
            SspiIssuanceChannelParameter sspiIssuanceChannelParameter = this.GetSspiIssuanceChannelParameter(initiatorRequirement);
            bool flag = (sspiIssuanceChannelParameter == null) || sspiIssuanceChannelParameter.GetTokenOnOpen;
            LocalClientSecuritySettings localClientSettings = securityBindingElement.LocalClientSettings;
            BindingContext property = initiatorRequirement.GetProperty<BindingContext>(ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty);
            SpnegoTokenProvider provider = new SpnegoTokenProvider((sspiIssuanceChannelParameter != null) ? sspiIssuanceChannelParameter.CredentialsHandle : null, securityBindingElement);
            SspiSecurityToken spnegoClientCredential = this.GetSpnegoClientCredential(initiatorRequirement);
            provider.ClientCredential = spnegoClientCredential.NetworkCredential;
            provider.IssuerAddress = initiatorRequirement.IssuerAddress;
            provider.AllowedImpersonationLevel = this.parent.Windows.AllowedImpersonationLevel;
            provider.AllowNtlm = spnegoClientCredential.AllowNtlm;
            provider.IdentityVerifier = localClientSettings.IdentityVerifier;
            provider.SecurityAlgorithmSuite = initiatorRequirement.SecurityAlgorithmSuite;
            provider.AuthenticateServer = !initiatorRequirement.Properties.ContainsKey(ServiceModelSecurityTokenRequirement.SupportingTokenAttachmentModeProperty);
            provider.NegotiateTokenOnOpen = flag;
            provider.CacheServiceTokens = flag || localClientSettings.CacheCookies;
            provider.IssuerBindingContext = property;
            provider.MaxServiceTokenCachingTime = localClientSettings.MaxCookieCachingTime;
            provider.ServiceTokenValidityThresholdPercentage = localClientSettings.CookieRenewalThresholdPercentage;
            provider.StandardsManager = System.ServiceModel.Security.SecurityUtils.CreateSecurityStandardsManager(initiatorRequirement, this);
            provider.TargetAddress = targetAddress;
            provider.Via = initiatorRequirement.GetPropertyOrDefault<Uri>(ServiceModelSecurityTokenRequirement.ViaProperty, null);
            provider.ApplicationProtectionRequirements = (property != null) ? property.BindingParameters.Find<ChannelProtectionRequirements>() : null;
            provider.InteractiveNegoExLogonEnabled = this.ClientCredentials.SupportInteractive;
            return provider;
        }

        private SecurityTokenProvider CreateTlsnegoClientX509TokenProvider(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            ChannelParameterCollection parameters;
            InitiatorServiceModelSecurityTokenRequirement tokenRequirement = new InitiatorServiceModelSecurityTokenRequirement {
                TokenType = SecurityTokenTypes.X509Certificate,
                TargetAddress = initiatorRequirement.TargetAddress,
                SecurityBindingElement = initiatorRequirement.SecurityBindingElement,
                SecurityAlgorithmSuite = initiatorRequirement.SecurityAlgorithmSuite,
                RequireCryptographicToken = true,
                MessageSecurityVersion = initiatorRequirement.MessageSecurityVersion,
                KeyUsage = SecurityKeyUsage.Signature,
                KeyType = SecurityKeyType.AsymmetricKey
            };
            tokenRequirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = MessageDirection.Output;
            if (initiatorRequirement.TryGetProperty<ChannelParameterCollection>(ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty, out parameters))
            {
                tokenRequirement.Properties[ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty] = parameters;
            }
            return this.CreateSecurityTokenProvider(tokenRequirement);
        }

        private SecurityTokenAuthenticator CreateTlsnegoServerX509TokenAuthenticator(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            ChannelParameterCollection parameters;
            SecurityTokenResolver resolver;
            InitiatorServiceModelSecurityTokenRequirement tokenRequirement = new InitiatorServiceModelSecurityTokenRequirement {
                TokenType = SecurityTokenTypes.X509Certificate,
                RequireCryptographicToken = true,
                SecurityBindingElement = initiatorRequirement.SecurityBindingElement,
                MessageSecurityVersion = initiatorRequirement.MessageSecurityVersion,
                KeyUsage = SecurityKeyUsage.Exchange,
                KeyType = SecurityKeyType.AsymmetricKey
            };
            tokenRequirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = MessageDirection.Input;
            if (initiatorRequirement.TryGetProperty<ChannelParameterCollection>(ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty, out parameters))
            {
                tokenRequirement.Properties[ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty] = parameters;
            }
            return this.CreateSecurityTokenAuthenticator(tokenRequirement, out resolver);
        }

        private SecurityTokenProvider CreateTlsnegoTokenProvider(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement, bool requireClientCertificate)
        {
            if (initiatorRequirement.TargetAddress == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("TokenRequirementDoesNotSpecifyTargetAddress", new object[] { initiatorRequirement }));
            }
            SecurityBindingElement securityBindingElement = initiatorRequirement.SecurityBindingElement;
            if (securityBindingElement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("TokenProviderRequiresSecurityBindingElement", new object[] { initiatorRequirement }));
            }
            SspiIssuanceChannelParameter sspiIssuanceChannelParameter = this.GetSspiIssuanceChannelParameter(initiatorRequirement);
            bool flag = (sspiIssuanceChannelParameter != null) && sspiIssuanceChannelParameter.GetTokenOnOpen;
            LocalClientSecuritySettings localClientSettings = securityBindingElement.LocalClientSettings;
            BindingContext property = initiatorRequirement.GetProperty<BindingContext>(ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty);
            TlsnegoTokenProvider provider = new TlsnegoTokenProvider {
                IssuerAddress = initiatorRequirement.IssuerAddress,
                NegotiateTokenOnOpen = flag,
                CacheServiceTokens = flag || localClientSettings.CacheCookies
            };
            if (requireClientCertificate)
            {
                provider.ClientTokenProvider = this.CreateTlsnegoClientX509TokenProvider(initiatorRequirement);
            }
            provider.IssuerBindingContext = property;
            provider.ApplicationProtectionRequirements = (property != null) ? property.BindingParameters.Find<ChannelProtectionRequirements>() : null;
            provider.MaxServiceTokenCachingTime = localClientSettings.MaxCookieCachingTime;
            provider.SecurityAlgorithmSuite = initiatorRequirement.SecurityAlgorithmSuite;
            provider.ServerTokenAuthenticator = this.CreateTlsnegoServerX509TokenAuthenticator(initiatorRequirement);
            provider.ServiceTokenValidityThresholdPercentage = localClientSettings.CookieRenewalThresholdPercentage;
            provider.StandardsManager = System.ServiceModel.Security.SecurityUtils.CreateSecurityStandardsManager(initiatorRequirement, this);
            provider.TargetAddress = initiatorRequirement.TargetAddress;
            provider.Via = initiatorRequirement.GetPropertyOrDefault<Uri>(ServiceModelSecurityTokenRequirement.ViaProperty, null);
            return provider;
        }

        private System.IdentityModel.SafeFreeCredentials GetCredentialsHandle(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            SspiIssuanceChannelParameter sspiIssuanceChannelParameter = this.GetSspiIssuanceChannelParameter(initiatorRequirement);
            if (sspiIssuanceChannelParameter == null)
            {
                return null;
            }
            return sspiIssuanceChannelParameter.CredentialsHandle;
        }

        private SecurityKeyEntropyMode GetIssuerBindingKeyEntropyModeOrDefault(Binding issuerBinding)
        {
            SecurityBindingElement element = issuerBinding.CreateBindingElements().Find<SecurityBindingElement>();
            if (element != null)
            {
                return element.KeyEntropyMode;
            }
            return this.parent.IssuedToken.DefaultKeyEntropyMode;
        }

        private void GetIssuerBindingSecurityVersion(Binding issuerBinding, MessageSecurityVersion issuedTokenParametersDefaultMessageSecurityVersion, SecurityBindingElement outerSecurityBindingElement, out MessageSecurityVersion messageSecurityVersion, out SecurityTokenSerializer tokenSerializer)
        {
            messageSecurityVersion = null;
            if (issuerBinding != null)
            {
                SecurityBindingElement element = issuerBinding.CreateBindingElements().Find<SecurityBindingElement>();
                if (element != null)
                {
                    messageSecurityVersion = element.MessageSecurityVersion;
                }
            }
            if (messageSecurityVersion == null)
            {
                if (issuedTokenParametersDefaultMessageSecurityVersion != null)
                {
                    messageSecurityVersion = issuedTokenParametersDefaultMessageSecurityVersion;
                }
                else if (outerSecurityBindingElement != null)
                {
                    messageSecurityVersion = outerSecurityBindingElement.MessageSecurityVersion;
                }
            }
            if (messageSecurityVersion == null)
            {
                messageSecurityVersion = MessageSecurityVersion.Default;
            }
            tokenSerializer = this.CreateSecurityTokenSerializer(messageSecurityVersion.SecurityTokenVersion);
        }

        private string GetServicePrincipalName(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            IdentityVerifier identityVerifier;
            EndpointIdentity identity;
            EndpointAddress targetAddress = initiatorRequirement.TargetAddress;
            if (targetAddress == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("TokenRequirementDoesNotSpecifyTargetAddress", new object[] { initiatorRequirement }));
            }
            SecurityBindingElement securityBindingElement = initiatorRequirement.SecurityBindingElement;
            if (securityBindingElement != null)
            {
                identityVerifier = securityBindingElement.LocalClientSettings.IdentityVerifier;
            }
            else
            {
                identityVerifier = IdentityVerifier.CreateDefault();
            }
            identityVerifier.TryGetIdentity(targetAddress, out identity);
            return System.ServiceModel.Security.SecurityUtils.GetSpnFromIdentity(identity, targetAddress);
        }

        private SspiSecurityToken GetSpnegoClientCredential(InitiatorServiceModelSecurityTokenRequirement initiatorRequirement)
        {
            ChannelParameterCollection parameters;
            InitiatorServiceModelSecurityTokenRequirement tokenRequirement = new InitiatorServiceModelSecurityTokenRequirement {
                TargetAddress = initiatorRequirement.TargetAddress,
                TokenType = ServiceModelSecurityTokenTypes.SspiCredential,
                Via = initiatorRequirement.Via,
                RequireCryptographicToken = false,
                SecurityBindingElement = initiatorRequirement.SecurityBindingElement,
                MessageSecurityVersion = initiatorRequirement.MessageSecurityVersion
            };
            if (initiatorRequirement.TryGetProperty<ChannelParameterCollection>(ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty, out parameters))
            {
                tokenRequirement.Properties[ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty] = parameters;
            }
            SecurityTokenProvider tokenProvider = this.CreateSecurityTokenProvider(tokenRequirement);
            System.ServiceModel.Security.SecurityUtils.OpenTokenProviderIfRequired(tokenProvider, TimeSpan.Zero);
            SspiSecurityToken token = (SspiSecurityToken) tokenProvider.GetToken(TimeSpan.Zero);
            System.ServiceModel.Security.SecurityUtils.AbortTokenProviderIfRequired(tokenProvider);
            return token;
        }

        private SspiIssuanceChannelParameter GetSspiIssuanceChannelParameter(SecurityTokenRequirement initiatorRequirement)
        {
            ChannelParameterCollection parameters;
            if (initiatorRequirement.TryGetProperty<ChannelParameterCollection>(ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty, out parameters) && (parameters != null))
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (parameters[i] is SspiIssuanceChannelParameter)
                    {
                        return (SspiIssuanceChannelParameter) parameters[i];
                    }
                }
            }
            return null;
        }

        private bool IsDigestAuthenticationScheme(SecurityTokenRequirement requirement)
        {
            if (requirement.Properties.ContainsKey(ServiceModelSecurityTokenRequirement.HttpAuthenticationSchemeProperty))
            {
                AuthenticationSchemes schemes = (AuthenticationSchemes) requirement.Properties[ServiceModelSecurityTokenRequirement.HttpAuthenticationSchemeProperty];
                return (schemes == AuthenticationSchemes.Digest);
            }
            return false;
        }

        protected internal bool IsIssuedSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            if ((requirement == null) || !requirement.Properties.ContainsKey(ServiceModelSecurityTokenRequirement.IssuerAddressProperty))
            {
                return false;
            }
            return ((!(requirement.TokenType == ServiceModelSecurityTokenTypes.AnonymousSslnego) && !(requirement.TokenType == ServiceModelSecurityTokenTypes.MutualSslnego)) && (!(requirement.TokenType == ServiceModelSecurityTokenTypes.SecureConversation) && !(requirement.TokenType == ServiceModelSecurityTokenTypes.Spnego)));
        }

        public System.ServiceModel.Description.ClientCredentials ClientCredentials
        {
            get
            {
                return this.parent;
            }
        }

        internal class KerberosSecurityTokenProviderWrapper : CommunicationObjectSecurityTokenProvider
        {
            private System.IdentityModel.SafeFreeCredentials credentialsHandle;
            private KerberosSecurityTokenProvider innerProvider;
            private bool ownCredentialsHandle;

            public KerberosSecurityTokenProviderWrapper(KerberosSecurityTokenProvider innerProvider, System.IdentityModel.SafeFreeCredentials credentialsHandle)
            {
                this.innerProvider = innerProvider;
                this.credentialsHandle = credentialsHandle;
            }

            private void FreeCredentialsHandle()
            {
                if (this.credentialsHandle != null)
                {
                    if (this.ownCredentialsHandle)
                    {
                        this.credentialsHandle.Close();
                    }
                    this.credentialsHandle = null;
                }
            }

            internal SecurityToken GetToken(TimeSpan timeout, ChannelBinding channelbinding)
            {
                return new KerberosRequestorSecurityToken(this.innerProvider.ServicePrincipalName, this.innerProvider.TokenImpersonationLevel, this.innerProvider.NetworkCredential, System.ServiceModel.Security.SecurityUniqueId.Create().Value, this.credentialsHandle, channelbinding);
            }

            protected override SecurityToken GetTokenCore(TimeSpan timeout)
            {
                return this.GetToken(timeout, null);
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
                    this.credentialsHandle = System.ServiceModel.Security.SecurityUtils.GetCredentialsHandle("Kerberos", this.innerProvider.NetworkCredential, false, new string[0]);
                    this.ownCredentialsHandle = true;
                }
            }
        }
    }
}


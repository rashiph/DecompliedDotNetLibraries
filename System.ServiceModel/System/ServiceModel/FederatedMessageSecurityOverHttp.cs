namespace System.ServiceModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IdentityModel.Tokens;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    public sealed class FederatedMessageSecurityOverHttp
    {
        private SecurityAlgorithmSuite algorithmSuite = SecurityAlgorithmSuite.Default;
        private Collection<ClaimTypeRequirement> claimTypeRequirements = new Collection<ClaimTypeRequirement>();
        internal const bool DefaultEstablishSecurityContext = true;
        internal const SecurityKeyType DefaultIssuedKeyType = SecurityKeyType.SymmetricKey;
        internal const bool DefaultNegotiateServiceCredential = true;
        private bool establishSecurityContext = true;
        private SecurityKeyType issuedKeyType = SecurityKeyType.SymmetricKey;
        private string issuedTokenType;
        private EndpointAddress issuerAddress;
        private Binding issuerBinding;
        private EndpointAddress issuerMetadataAddress;
        private bool negotiateServiceCredential = true;
        private Collection<XmlElement> tokenRequestParameters = new Collection<XmlElement>();

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal SecurityBindingElement CreateSecurityBindingElement(bool isSecureTransportMode, bool isReliableSession, MessageSecurityVersion version)
        {
            SecurityBindingElement element;
            SecurityBindingElement element3;
            if ((this.IssuedKeyType == SecurityKeyType.BearerKey) && (version.TrustVersion == TrustVersion.WSTrustFeb2005))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BearerKeyIncompatibleWithWSFederationHttpBinding")));
            }
            if (isReliableSession && !this.EstablishSecurityContext)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecureConversationRequiredByReliableSession")));
            }
            bool emitBspRequiredAttributes = true;
            IssuedSecurityTokenParameters issuedTokenParameters = new IssuedSecurityTokenParameters(this.IssuedTokenType, this.IssuerAddress, this.IssuerBinding) {
                IssuerMetadataAddress = this.issuerMetadataAddress,
                KeyType = this.IssuedKeyType
            };
            if (this.IssuedKeyType == SecurityKeyType.SymmetricKey)
            {
                issuedTokenParameters.KeySize = this.AlgorithmSuite.DefaultSymmetricKeyLength;
            }
            else
            {
                issuedTokenParameters.KeySize = 0;
            }
            foreach (ClaimTypeRequirement requirement in this.claimTypeRequirements)
            {
                issuedTokenParameters.ClaimTypeRequirements.Add(requirement);
            }
            foreach (XmlElement element2 in this.TokenRequestParameters)
            {
                issuedTokenParameters.AdditionalRequestParameters.Add(element2);
            }
            WSSecurityTokenSerializer tokenSerializer = new WSSecurityTokenSerializer(version.SecurityVersion, version.TrustVersion, version.SecureConversationVersion, emitBspRequiredAttributes, null, null, null);
            SecurityStandardsManager standardsManager = new SecurityStandardsManager(version, tokenSerializer);
            issuedTokenParameters.AddAlgorithmParameters(this.AlgorithmSuite, standardsManager, this.issuedKeyType);
            if (isSecureTransportMode)
            {
                element3 = SecurityBindingElement.CreateIssuedTokenOverTransportBindingElement(issuedTokenParameters);
            }
            else if (this.negotiateServiceCredential)
            {
                element3 = SecurityBindingElement.CreateIssuedTokenForSslBindingElement(issuedTokenParameters, version.SecurityPolicyVersion != SecurityPolicyVersion.WSSecurityPolicy11);
            }
            else
            {
                element3 = SecurityBindingElement.CreateIssuedTokenForCertificateBindingElement(issuedTokenParameters);
            }
            element3.MessageSecurityVersion = version;
            element3.DefaultAlgorithmSuite = this.AlgorithmSuite;
            if (this.EstablishSecurityContext)
            {
                element = SecurityBindingElement.CreateSecureConversationBindingElement(element3, true);
            }
            else
            {
                element = element3;
            }
            element.MessageSecurityVersion = version;
            element.DefaultAlgorithmSuite = this.AlgorithmSuite;
            element.IncludeTimestamp = true;
            if (!isReliableSession)
            {
                element.LocalServiceSettings.ReconnectTransportOnFailure = false;
                element.LocalClientSettings.ReconnectTransportOnFailure = false;
            }
            else
            {
                element.LocalServiceSettings.ReconnectTransportOnFailure = true;
                element.LocalClientSettings.ReconnectTransportOnFailure = true;
            }
            if (this.establishSecurityContext)
            {
                element3.LocalServiceSettings.IssuedCookieLifetime = NegotiationTokenAuthenticator<SspiNegotiationTokenAuthenticatorState>.defaultServerIssuedTransitionTokenLifetime;
            }
            return element;
        }

        internal bool InternalShouldSerialize()
        {
            if (((!this.ShouldSerializeAlgorithmSuite() && !this.ShouldSerializeClaimTypeRequirements()) && (!this.ShouldSerializeNegotiateServiceCredential() && !this.ShouldSerializeEstablishSecurityContext())) && !this.ShouldSerializeIssuedKeyType())
            {
                return this.ShouldSerializeTokenRequestParameters();
            }
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeAlgorithmSuite()
        {
            return (this.AlgorithmSuite != SecurityAlgorithmSuite.Default);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeClaimTypeRequirements()
        {
            return (this.ClaimTypeRequirements.Count > 0);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeEstablishSecurityContext()
        {
            return !this.EstablishSecurityContext;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeIssuedKeyType()
        {
            return (this.IssuedKeyType != SecurityKeyType.SymmetricKey);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeNegotiateServiceCredential()
        {
            return !this.NegotiateServiceCredential;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTokenRequestParameters()
        {
            return (this.TokenRequestParameters.Count > 0);
        }

        internal static bool TryCreate(SecurityBindingElement sbe, bool isSecureTransportMode, bool isReliableSession, MessageSecurityVersion version, out FederatedMessageSecurityOverHttp messageSecurity)
        {
            bool flag;
            bool flag2;
            bool flag3;
            IssuedSecurityTokenParameters parameters;
            Collection<XmlElement> collection;
            messageSecurity = null;
            if (sbe.IncludeTimestamp)
            {
                SecurityBindingElement element;
                if (sbe.SecurityHeaderLayout != SecurityHeaderLayout.Strict)
                {
                    return false;
                }
                flag = true;
                flag2 = SecurityBindingElement.IsSecureConversationBinding(sbe, true, out element);
                element = flag2 ? element : sbe;
                if (isSecureTransportMode && !(element is TransportSecurityBindingElement))
                {
                    return false;
                }
                flag3 = true;
                if (isSecureTransportMode)
                {
                    if (!SecurityBindingElement.IsIssuedTokenOverTransportBinding(element, out parameters))
                    {
                        return false;
                    }
                    goto Label_0078;
                }
                if (SecurityBindingElement.IsIssuedTokenForSslBinding(element, version.SecurityPolicyVersion != SecurityPolicyVersion.WSSecurityPolicy11, out parameters))
                {
                    flag3 = true;
                    goto Label_0078;
                }
                if (SecurityBindingElement.IsIssuedTokenForCertificateBinding(element, out parameters))
                {
                    flag3 = false;
                    goto Label_0078;
                }
            }
            return false;
        Label_0078:
            if ((parameters.KeyType == SecurityKeyType.BearerKey) && (version.TrustVersion == TrustVersion.WSTrustFeb2005))
            {
                return false;
            }
            WSSecurityTokenSerializer tokenSerializer = new WSSecurityTokenSerializer(version.SecurityVersion, version.TrustVersion, version.SecureConversationVersion, flag, null, null, null);
            SecurityStandardsManager standardsManager = new SecurityStandardsManager(version, tokenSerializer);
            if (!parameters.DoAlgorithmsMatch(sbe.DefaultAlgorithmSuite, standardsManager, out collection))
            {
                return false;
            }
            messageSecurity = new FederatedMessageSecurityOverHttp();
            messageSecurity.AlgorithmSuite = sbe.DefaultAlgorithmSuite;
            messageSecurity.NegotiateServiceCredential = flag3;
            messageSecurity.EstablishSecurityContext = flag2;
            messageSecurity.IssuedTokenType = parameters.TokenType;
            messageSecurity.IssuerAddress = parameters.IssuerAddress;
            messageSecurity.IssuerBinding = parameters.IssuerBinding;
            messageSecurity.IssuerMetadataAddress = parameters.IssuerMetadataAddress;
            messageSecurity.IssuedKeyType = parameters.KeyType;
            foreach (ClaimTypeRequirement requirement in parameters.ClaimTypeRequirements)
            {
                messageSecurity.ClaimTypeRequirements.Add(requirement);
            }
            foreach (XmlElement element2 in collection)
            {
                messageSecurity.TokenRequestParameters.Add(element2);
            }
            if ((parameters.AlternativeIssuerEndpoints != null) && (parameters.AlternativeIssuerEndpoints.Count > 0))
            {
                return false;
            }
            return true;
        }

        public SecurityAlgorithmSuite AlgorithmSuite
        {
            get
            {
                return this.algorithmSuite;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.algorithmSuite = value;
            }
        }

        public Collection<ClaimTypeRequirement> ClaimTypeRequirements
        {
            get
            {
                return this.claimTypeRequirements;
            }
        }

        public bool EstablishSecurityContext
        {
            get
            {
                return this.establishSecurityContext;
            }
            set
            {
                this.establishSecurityContext = value;
            }
        }

        public SecurityKeyType IssuedKeyType
        {
            get
            {
                return this.issuedKeyType;
            }
            set
            {
                if (!SecurityKeyTypeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.issuedKeyType = value;
            }
        }

        [DefaultValue((string) null)]
        public string IssuedTokenType
        {
            get
            {
                return this.issuedTokenType;
            }
            set
            {
                this.issuedTokenType = value;
            }
        }

        [DefaultValue((string) null)]
        public EndpointAddress IssuerAddress
        {
            get
            {
                return this.issuerAddress;
            }
            set
            {
                this.issuerAddress = value;
            }
        }

        [DefaultValue((string) null)]
        public Binding IssuerBinding
        {
            get
            {
                return this.issuerBinding;
            }
            set
            {
                this.issuerBinding = value;
            }
        }

        [DefaultValue((string) null)]
        public EndpointAddress IssuerMetadataAddress
        {
            get
            {
                return this.issuerMetadataAddress;
            }
            set
            {
                this.issuerMetadataAddress = value;
            }
        }

        public bool NegotiateServiceCredential
        {
            get
            {
                return this.negotiateServiceCredential;
            }
            set
            {
                this.negotiateServiceCredential = value;
            }
        }

        public Collection<XmlElement> TokenRequestParameters
        {
            get
            {
                return this.tokenRequestParameters;
            }
        }
    }
}


namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal abstract class TrustDriver
    {
        protected TrustDriver()
        {
        }

        public abstract XmlElement CreateCanonicalizationAlgorithmElement(string canonicalicationAlgorithm);
        public abstract XmlElement CreateComputedKeyAlgorithmElement(string computedKeyAlgorithm);
        public abstract XmlElement CreateEncryptionAlgorithmElement(string encryptionAlgorithm);
        public abstract XmlElement CreateEncryptWithElement(string encryptionAlgorithm);
        public abstract IChannelFactory<IRequestChannel> CreateFederationProxy(EndpointAddress address, Binding binding, KeyedByTypeCollection<IEndpointBehavior> channelBehaviors);
        public abstract XmlElement CreateKeySizeElement(int keySize);
        public abstract XmlElement CreateKeyTypeElement(SecurityKeyType keyType);
        public abstract RequestSecurityToken CreateRequestSecurityToken(XmlReader reader);
        public abstract RequestSecurityTokenResponse CreateRequestSecurityTokenResponse(XmlReader reader);
        public abstract RequestSecurityTokenResponseCollection CreateRequestSecurityTokenResponseCollection(XmlReader xmlReader);
        public abstract XmlElement CreateRequiredClaimsElement(IEnumerable<XmlElement> claimsList);
        public abstract XmlElement CreateSignWithElement(string signatureAlgorithm);
        public abstract XmlElement CreateTokenTypeElement(string tokenTypeUri);
        public abstract XmlElement CreateUseKeyElement(SecurityKeyIdentifier keyIdentifier, SecurityStandardsManager standardsManager);
        public abstract T GetAppliesTo<T>(RequestSecurityToken rst, XmlObjectSerializer serializer);
        public abstract T GetAppliesTo<T>(RequestSecurityTokenResponse rstr, XmlObjectSerializer serializer);
        public abstract void GetAppliesToQName(RequestSecurityToken rst, out string localName, out string namespaceUri);
        public abstract void GetAppliesToQName(RequestSecurityTokenResponse rstr, out string localName, out string namespaceUri);
        public abstract byte[] GetAuthenticator(RequestSecurityTokenResponse rstr);
        public abstract BinaryNegotiation GetBinaryNegotiation(RequestSecurityToken rst);
        public abstract BinaryNegotiation GetBinaryNegotiation(RequestSecurityTokenResponse rstr);
        public abstract SecurityToken GetEntropy(RequestSecurityToken rst, SecurityTokenResolver resolver);
        public abstract SecurityToken GetEntropy(RequestSecurityTokenResponse rstr, SecurityTokenResolver resolver);
        public abstract GenericXmlSecurityToken GetIssuedToken(RequestSecurityTokenResponse rstr, string expectedTokenType, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, RSA clientKey);
        public abstract GenericXmlSecurityToken GetIssuedToken(RequestSecurityTokenResponse rstr, SecurityTokenResolver resolver, IList<SecurityTokenAuthenticator> allowedAuthenticators, SecurityKeyEntropyMode keyEntropyMode, byte[] requestorEntropy, string expectedTokenType, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, int defaultKeySize, bool isBearerKeyType);
        public abstract bool IsAppliesTo(string localName, string namespaceUri);
        public abstract bool IsAtRequestSecurityTokenResponse(XmlReader reader);
        public abstract bool IsAtRequestSecurityTokenResponseCollection(XmlReader reader);
        internal virtual bool IsCanonicalizationAlgorithmElement(XmlElement element, out string canonicalizationAlgorithm)
        {
            canonicalizationAlgorithm = null;
            return false;
        }

        internal virtual bool IsEncryptionAlgorithmElement(XmlElement element, out string encryptionAlgorithm)
        {
            encryptionAlgorithm = null;
            return false;
        }

        internal virtual bool IsEncryptWithElement(XmlElement element, out string encryptWithAlgorithm)
        {
            encryptWithAlgorithm = null;
            return false;
        }

        internal virtual bool IsKeyWrapAlgorithmElement(XmlElement element, out string keyWrapAlgorithm)
        {
            keyWrapAlgorithm = null;
            return false;
        }

        public abstract bool IsRequestedProofTokenElement(string name, string nameSpace);
        public abstract bool IsRequestedSecurityTokenElement(string name, string nameSpace);
        internal virtual bool IsSignWithElement(XmlElement element, out string signatureAlgorithm)
        {
            signatureAlgorithm = null;
            return false;
        }

        public abstract void OnRSTRorRSTRCMissingException();
        public abstract Collection<XmlElement> ProcessUnknownRequestParameters(Collection<XmlElement> unknownRequestParameters, Collection<XmlElement> originalRequestParameters);
        public abstract bool TryParseKeySizeElement(XmlElement element, out int keySize);
        public abstract bool TryParseKeyTypeElement(XmlElement element, out SecurityKeyType keyType);
        public abstract bool TryParseRequiredClaimsElement(XmlElement element, out Collection<XmlElement> requiredClaims);
        public abstract bool TryParseTokenTypeElement(XmlElement element, out string tokenType);
        public abstract void WriteRequestSecurityToken(RequestSecurityToken rst, XmlWriter w);
        public abstract void WriteRequestSecurityTokenResponse(RequestSecurityTokenResponse rstr, XmlWriter w);
        public abstract void WriteRequestSecurityTokenResponseCollection(RequestSecurityTokenResponseCollection rstrCollection, XmlWriter writer);

        public abstract string ComputedKeyAlgorithm { get; }

        public virtual bool IsIssuedTokensSupported
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsSessionSupported
        {
            get
            {
                return false;
            }
        }

        public virtual string IssuedTokensHeaderName
        {
            get
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TrustDriverVersionDoesNotSupportIssuedTokens")));
            }
        }

        public virtual string IssuedTokensHeaderNamespace
        {
            get
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TrustDriverVersionDoesNotSupportIssuedTokens")));
            }
        }

        public abstract XmlDictionaryString Namespace { get; }

        public abstract XmlDictionaryString RequestSecurityTokenAction { get; }

        public abstract XmlDictionaryString RequestSecurityTokenResponseAction { get; }

        public abstract XmlDictionaryString RequestSecurityTokenResponseFinalAction { get; }

        public virtual string RequestTypeClose
        {
            get
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TrustDriverVersionDoesNotSupportSession")));
            }
        }

        public abstract string RequestTypeIssue { get; }

        public virtual string RequestTypeRenew
        {
            get
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TrustDriverVersionDoesNotSupportSession")));
            }
        }

        public abstract SecurityStandardsManager StandardsManager { get; }
    }
}


namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class TrustDictionary
    {
        public XmlDictionaryString AppliesTo;
        public XmlDictionaryString Authenticator;
        public XmlDictionaryString BinaryExchange;
        public XmlDictionaryString BinarySecret;
        public XmlDictionaryString BinarySecretClauseType;
        public XmlDictionaryString CanonicalizationAlgorithm;
        public XmlDictionaryString Claims;
        public XmlDictionaryString CloseTarget;
        public XmlDictionaryString CombinedHash;
        public XmlDictionaryString CombinedHashLabel;
        public XmlDictionaryString ComputedKey;
        public XmlDictionaryString ComputedKeyAlgorithm;
        public XmlDictionaryString Context;
        public XmlDictionaryString EncryptionAlgorithm;
        public XmlDictionaryString EncryptWith;
        public XmlDictionaryString Entropy;
        public XmlDictionaryString FailedAuthenticationFaultCode;
        public XmlDictionaryString InvalidRequestFaultCode;
        public XmlDictionaryString IssuedTokensHeader;
        public XmlDictionaryString KeySize;
        public XmlDictionaryString KeyType;
        public XmlDictionaryString Lifetime;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString NonceBinarySecret;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString Psha1ComputedKeyUri;
        public XmlDictionaryString PublicKeyType;
        public XmlDictionaryString RenewTarget;
        public XmlDictionaryString RequestedAttachedReference;
        public XmlDictionaryString RequestedProofToken;
        public XmlDictionaryString RequestedSecurityToken;
        public XmlDictionaryString RequestedTokenClosed;
        public XmlDictionaryString RequestedTokenReference;
        public XmlDictionaryString RequestedUnattachedReference;
        public XmlDictionaryString RequestFailedFaultCode;
        public XmlDictionaryString RequestSecurityToken;
        public XmlDictionaryString RequestSecurityTokenIssuance;
        public XmlDictionaryString RequestSecurityTokenIssuanceResponse;
        public XmlDictionaryString RequestSecurityTokenResponse;
        public XmlDictionaryString RequestSecurityTokenResponseCollection;
        public XmlDictionaryString RequestType;
        public XmlDictionaryString RequestTypeClose;
        public XmlDictionaryString RequestTypeIssue;
        public XmlDictionaryString RequestTypeRenew;
        public XmlDictionaryString SignWith;
        public XmlDictionaryString SpnegoValueTypeUri;
        public XmlDictionaryString SymmetricKeyBinarySecret;
        public XmlDictionaryString SymmetricKeyType;
        public XmlDictionaryString TlsnegoValueTypeUri;
        public XmlDictionaryString TokenType;
        public XmlDictionaryString Type;
        public XmlDictionaryString UseKey;

        public TrustDictionary()
        {
        }

        public TrustDictionary(ServiceModelDictionary dictionary)
        {
        }
    }
}


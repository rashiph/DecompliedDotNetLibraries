namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class SecureConversationDictionary
    {
        public XmlDictionaryString AlgorithmAttribute;
        public XmlDictionaryString BadContextTokenFaultCode;
        public XmlDictionaryString Cookie;
        public XmlDictionaryString DerivedKeyToken;
        public XmlDictionaryString DerivedKeyTokenType;
        public XmlDictionaryString Generation;
        public XmlDictionaryString Identifier;
        public XmlDictionaryString Label;
        public XmlDictionaryString Length;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString Nonce;
        public XmlDictionaryString Offset;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString Properties;
        public XmlDictionaryString RenewNeededFaultCode;
        public XmlDictionaryString RequestSecurityContextIssuance;
        public XmlDictionaryString RequestSecurityContextIssuanceResponse;
        public XmlDictionaryString SecurityContextToken;
        public XmlDictionaryString SecurityContextTokenReferenceValueType;
        public XmlDictionaryString SecurityContextTokenType;

        public SecureConversationDictionary()
        {
        }

        public SecureConversationDictionary(ServiceModelDictionary dictionary)
        {
        }
    }
}


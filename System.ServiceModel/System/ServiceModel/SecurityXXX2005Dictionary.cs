namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class SecurityXXX2005Dictionary
    {
        public XmlDictionaryString EncryptedHeader;
        public XmlDictionaryString EncryptedKeyHashValueType;
        public XmlDictionaryString EncryptedKeyTokenType;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString Saml11AssertionValueType;
        public XmlDictionaryString Saml20TokenType;
        public XmlDictionaryString SamlTokenType;
        public XmlDictionaryString SignatureConfirmation;
        public XmlDictionaryString ThumbprintSha1ValueType;
        public XmlDictionaryString TokenTypeAttribute;
        public XmlDictionaryString ValueAttribute;

        public SecurityXXX2005Dictionary(ServiceModelDictionary dictionary)
        {
            this.EncryptedHeader = dictionary.CreateString("EncryptedHeader", 60);
            this.Namespace = dictionary.CreateString("http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd", 0x3d);
            this.Prefix = dictionary.CreateString("k", 0xb9);
            this.SignatureConfirmation = dictionary.CreateString("SignatureConfirmation", 0xba);
            this.ValueAttribute = dictionary.CreateString("Value", 0x4d);
            this.TokenTypeAttribute = dictionary.CreateString("TokenType", 0xbb);
            this.ThumbprintSha1ValueType = dictionary.CreateString("http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#ThumbprintSHA1", 0xbc);
            this.EncryptedKeyTokenType = dictionary.CreateString("http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKey", 0xbd);
            this.EncryptedKeyHashValueType = dictionary.CreateString("http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKeySHA1", 190);
            this.SamlTokenType = dictionary.CreateString("http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1", 0xbf);
            this.Saml20TokenType = dictionary.CreateString("http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0", 0xc0);
            this.Saml11AssertionValueType = dictionary.CreateString("http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID", 0xc1);
        }
    }
}


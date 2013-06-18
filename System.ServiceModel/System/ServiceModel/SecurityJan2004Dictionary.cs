namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class SecurityJan2004Dictionary
    {
        public XmlDictionaryString BinarySecurityToken;
        public XmlDictionaryString EncodingType;
        public XmlDictionaryString EncodingTypeValueBase64Binary;
        public XmlDictionaryString EncodingTypeValueHexBinary;
        public XmlDictionaryString EncodingTypeValueText;
        public XmlDictionaryString FailedAuthenticationFaultCode;
        public XmlDictionaryString InvalidSecurityFaultCode;
        public XmlDictionaryString InvalidSecurityTokenFaultCode;
        public XmlDictionaryString KerberosHashValueType;
        public XmlDictionaryString KerberosTokenType1510;
        public XmlDictionaryString KerberosTokenTypeGSS;
        public XmlDictionaryString KeyIdentifier;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString NonceElement;
        public XmlDictionaryString PasswordElement;
        public XmlDictionaryString PasswordTextName;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString Reference;
        public XmlDictionaryString RelAssertionValueType;
        public XmlDictionaryString SamlAssertion;
        public XmlDictionaryString SamlAssertionIdValueType;
        public XmlDictionaryString SamlUri;
        public XmlDictionaryString Security;
        public XmlDictionaryString SecurityTokenReference;
        public XmlDictionaryString TypeAttribute;
        public XmlDictionaryString URI;
        public XmlDictionaryString UserNameElement;
        public XmlDictionaryString UserNameTokenElement;
        public XmlDictionaryString ValueType;
        public XmlDictionaryString X509SKIValueType;

        public SecurityJan2004Dictionary(ServiceModelDictionary dictionary)
        {
            this.SecurityTokenReference = dictionary.CreateString("SecurityTokenReference", 30);
            this.Namespace = dictionary.CreateString("http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd", 0x24);
            this.Security = dictionary.CreateString("Security", 0x34);
            this.ValueType = dictionary.CreateString("ValueType", 0x3a);
            this.TypeAttribute = dictionary.CreateString("Type", 0x3b);
            this.Prefix = dictionary.CreateString("o", 0xa4);
            this.NonceElement = dictionary.CreateString("Nonce", 40);
            this.PasswordElement = dictionary.CreateString("Password", 0xa5);
            this.PasswordTextName = dictionary.CreateString("PasswordText", 0xa6);
            this.UserNameElement = dictionary.CreateString("Username", 0xa7);
            this.UserNameTokenElement = dictionary.CreateString("UsernameToken", 0xa8);
            this.BinarySecurityToken = dictionary.CreateString("BinarySecurityToken", 0xa9);
            this.EncodingType = dictionary.CreateString("EncodingType", 170);
            this.Reference = dictionary.CreateString("Reference", 12);
            this.URI = dictionary.CreateString("URI", 11);
            this.KeyIdentifier = dictionary.CreateString("KeyIdentifier", 0xab);
            this.EncodingTypeValueBase64Binary = dictionary.CreateString("http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary", 0xac);
            this.EncodingTypeValueHexBinary = dictionary.CreateString("http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#HexBinary", 0xad);
            this.EncodingTypeValueText = dictionary.CreateString("http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Text", 0xae);
            this.X509SKIValueType = dictionary.CreateString("http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509SubjectKeyIdentifier", 0xaf);
            this.KerberosTokenTypeGSS = dictionary.CreateString("http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#GSS_Kerberosv5_AP_REQ", 0xb0);
            this.KerberosTokenType1510 = dictionary.CreateString("http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#GSS_Kerberosv5_AP_REQ1510", 0xb1);
            this.SamlAssertionIdValueType = dictionary.CreateString("http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.0#SAMLAssertionID", 0xb2);
            this.SamlAssertion = dictionary.CreateString("Assertion", 0xb3);
            this.SamlUri = dictionary.CreateString("urn:oasis:names:tc:SAML:1.0:assertion", 180);
            this.RelAssertionValueType = dictionary.CreateString("http://docs.oasis-open.org/wss/oasis-wss-rel-token-profile-1.0.pdf#license", 0xb5);
            this.FailedAuthenticationFaultCode = dictionary.CreateString("FailedAuthentication", 0xb6);
            this.InvalidSecurityTokenFaultCode = dictionary.CreateString("InvalidSecurityToken", 0xb7);
            this.InvalidSecurityFaultCode = dictionary.CreateString("InvalidSecurity", 0xb8);
            this.KerberosHashValueType = dictionary.CreateString("http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#Kerberosv5APREQSHA1", 0x1ab);
        }
    }
}


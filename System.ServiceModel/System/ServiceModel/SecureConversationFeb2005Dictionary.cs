namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class SecureConversationFeb2005Dictionary : SecureConversationDictionary
    {
        public XmlDictionaryString RequestSecurityContextClose;
        public XmlDictionaryString RequestSecurityContextCloseResponse;
        public XmlDictionaryString RequestSecurityContextRenew;
        public XmlDictionaryString RequestSecurityContextRenewResponse;

        public SecureConversationFeb2005Dictionary(ServiceModelDictionary dictionary) : base(dictionary)
        {
            base.Namespace = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/sc", 0x26);
            base.DerivedKeyToken = dictionary.CreateString("DerivedKeyToken", 0x27);
            base.Nonce = dictionary.CreateString("Nonce", 40);
            base.Length = dictionary.CreateString("Length", 0x38);
            base.SecurityContextToken = dictionary.CreateString("SecurityContextToken", 0x73);
            base.AlgorithmAttribute = dictionary.CreateString("Algorithm", 8);
            base.Generation = dictionary.CreateString("Generation", 0x74);
            base.Label = dictionary.CreateString("Label", 0x75);
            base.Offset = dictionary.CreateString("Offset", 0x76);
            base.Properties = dictionary.CreateString("Properties", 0x77);
            base.Identifier = dictionary.CreateString("Identifier", 15);
            base.Cookie = dictionary.CreateString("Cookie", 120);
            base.RenewNeededFaultCode = dictionary.CreateString("RenewNeeded", 0x7f);
            base.BadContextTokenFaultCode = dictionary.CreateString("BadContextToken", 0x80);
            base.Prefix = dictionary.CreateString("c", 0x81);
            base.DerivedKeyTokenType = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/sc/dk", 130);
            base.SecurityContextTokenType = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/sc/sct", 0x83);
            base.SecurityContextTokenReferenceValueType = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/sc/sct", 0x83);
            base.RequestSecurityContextIssuance = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT", 0x84);
            base.RequestSecurityContextIssuanceResponse = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT", 0x85);
            this.RequestSecurityContextRenew = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT/Renew", 0x86);
            this.RequestSecurityContextRenewResponse = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT/Renew", 0x87);
            this.RequestSecurityContextClose = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT/Cancel", 0x88);
            this.RequestSecurityContextCloseResponse = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT/Cancel", 0x89);
        }
    }
}


namespace System.ServiceModel
{
    using System;

    internal class SecureConversationApr2004Dictionary : SecureConversationDictionary
    {
        public SecureConversationApr2004Dictionary(ServiceModelDictionary dictionary) : base(dictionary)
        {
            base.SecurityContextToken = dictionary.CreateString("SecurityContextToken", 0x73);
            base.DerivedKeyToken = dictionary.CreateString("DerivedKeyToken", 0x27);
            base.AlgorithmAttribute = dictionary.CreateString("Algorithm", 8);
            base.Generation = dictionary.CreateString("Generation", 0x74);
            base.Label = dictionary.CreateString("Label", 0x75);
            base.Length = dictionary.CreateString("Length", 0x38);
            base.Nonce = dictionary.CreateString("Nonce", 40);
            base.Offset = dictionary.CreateString("Offset", 0x76);
            base.Properties = dictionary.CreateString("Properties", 0x77);
            base.Identifier = dictionary.CreateString("Identifier", 15);
            base.Cookie = dictionary.CreateString("Cookie", 120);
            base.Prefix = dictionary.CreateString("wsc", 0x79);
            base.Namespace = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/04/sc", 0x7a);
            base.DerivedKeyTokenType = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/04/security/sc/dk", 0x7b);
            base.SecurityContextTokenType = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/04/security/sc/sct", 0x7c);
            base.SecurityContextTokenReferenceValueType = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/04/security/sc/sct", 0x7c);
            base.RequestSecurityContextIssuance = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/SCT", 0x7d);
            base.RequestSecurityContextIssuanceResponse = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/SCT", 0x7e);
            base.RenewNeededFaultCode = dictionary.CreateString("RenewNeeded", 0x7f);
            base.BadContextTokenFaultCode = dictionary.CreateString("BadContextToken", 0x80);
        }
    }
}


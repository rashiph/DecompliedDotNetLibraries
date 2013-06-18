namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class SecureConversationDec2005Dictionary : SecureConversationDictionary
    {
        public XmlDictionaryString Instance;
        public XmlDictionaryString RequestSecurityContextClose;
        public XmlDictionaryString RequestSecurityContextCloseResponse;
        public XmlDictionaryString RequestSecurityContextRenew;
        public XmlDictionaryString RequestSecurityContextRenewResponse;

        public SecureConversationDec2005Dictionary(XmlDictionary dictionary)
        {
            base.SecurityContextToken = dictionary.Add("SecurityContextToken");
            base.AlgorithmAttribute = dictionary.Add("Algorithm");
            base.Generation = dictionary.Add("Generation");
            base.Label = dictionary.Add("Label");
            base.Offset = dictionary.Add("Offset");
            base.Properties = dictionary.Add("Properties");
            base.Identifier = dictionary.Add("Identifier");
            base.Cookie = dictionary.Add("Cookie");
            base.RenewNeededFaultCode = dictionary.Add("RenewNeeded");
            base.BadContextTokenFaultCode = dictionary.Add("BadContextToken");
            base.Prefix = dictionary.Add("sc");
            base.DerivedKeyTokenType = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/dk");
            base.SecurityContextTokenType = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/sct");
            base.SecurityContextTokenReferenceValueType = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/sct");
            base.RequestSecurityContextIssuance = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/SCT");
            base.RequestSecurityContextIssuanceResponse = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/SCT");
            this.RequestSecurityContextRenew = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/SCT/Renew");
            this.RequestSecurityContextRenewResponse = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/SCT/Renew");
            this.RequestSecurityContextClose = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/SCT/Cancel");
            this.RequestSecurityContextCloseResponse = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/SCT/Cancel");
            base.Namespace = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512");
            base.DerivedKeyToken = dictionary.Add("DerivedKeyToken");
            base.Nonce = dictionary.Add("Nonce");
            base.Length = dictionary.Add("Length");
            this.Instance = dictionary.Add("Instance");
        }
    }
}


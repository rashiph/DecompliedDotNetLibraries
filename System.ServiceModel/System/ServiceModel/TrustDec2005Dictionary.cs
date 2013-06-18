namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class TrustDec2005Dictionary : TrustDictionary
    {
        public XmlDictionaryString AsymmetricKeyBinarySecret;
        public XmlDictionaryString BearerKeyType;
        public XmlDictionaryString Dialect;
        public XmlDictionaryString DialectType;
        public XmlDictionaryString KeyWrapAlgorithm;
        public XmlDictionaryString RequestSecurityTokenCancellation;
        public XmlDictionaryString RequestSecurityTokenCancellationResponse;
        public XmlDictionaryString RequestSecurityTokenCollectionCancellationFinalResponse;
        public XmlDictionaryString RequestSecurityTokenCollectionIssuanceFinalResponse;
        public XmlDictionaryString RequestSecurityTokenCollectionRenewalFinalResponse;
        public XmlDictionaryString RequestSecurityTokenRenewal;
        public XmlDictionaryString RequestSecurityTokenRenewalResponse;
        public XmlDictionaryString SecondaryParameters;

        public TrustDec2005Dictionary(XmlDictionary dictionary)
        {
            base.CombinedHashLabel = dictionary.Add("AUTH-HASH");
            base.RequestSecurityTokenResponse = dictionary.Add("RequestSecurityTokenResponse");
            base.TokenType = dictionary.Add("TokenType");
            base.KeySize = dictionary.Add("KeySize");
            base.RequestedTokenReference = dictionary.Add("RequestedTokenReference");
            base.AppliesTo = dictionary.Add("AppliesTo");
            base.Authenticator = dictionary.Add("Authenticator");
            base.CombinedHash = dictionary.Add("CombinedHash");
            base.BinaryExchange = dictionary.Add("BinaryExchange");
            base.Lifetime = dictionary.Add("Lifetime");
            base.RequestedSecurityToken = dictionary.Add("RequestedSecurityToken");
            base.Entropy = dictionary.Add("Entropy");
            base.RequestedProofToken = dictionary.Add("RequestedProofToken");
            base.ComputedKey = dictionary.Add("ComputedKey");
            base.RequestSecurityToken = dictionary.Add("RequestSecurityToken");
            base.RequestType = dictionary.Add("RequestType");
            base.Context = dictionary.Add("Context");
            base.BinarySecret = dictionary.Add("BinarySecret");
            base.Type = dictionary.Add("Type");
            base.SpnegoValueTypeUri = dictionary.Add("http://schemas.xmlsoap.org/ws/2005/02/trust/spnego");
            base.TlsnegoValueTypeUri = dictionary.Add("http://schemas.xmlsoap.org/ws/2005/02/trust/tlsnego");
            base.Prefix = dictionary.Add("trust");
            base.RequestSecurityTokenIssuance = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue");
            base.RequestSecurityTokenIssuanceResponse = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/Issue");
            base.RequestTypeIssue = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue");
            this.AsymmetricKeyBinarySecret = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/AsymmetricKey");
            base.SymmetricKeyBinarySecret = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/SymmetricKey");
            base.NonceBinarySecret = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/Nonce");
            base.Psha1ComputedKeyUri = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/CK/PSHA1");
            base.KeyType = dictionary.Add("KeyType");
            base.SymmetricKeyType = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/SymmetricKey");
            base.PublicKeyType = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/PublicKey");
            base.Claims = dictionary.Add("Claims");
            base.InvalidRequestFaultCode = dictionary.Add("InvalidRequest");
            base.FailedAuthenticationFaultCode = dictionary.Add("FailedAuthentication");
            base.UseKey = dictionary.Add("UseKey");
            base.SignWith = dictionary.Add("SignWith");
            base.EncryptWith = dictionary.Add("EncryptWith");
            base.EncryptionAlgorithm = dictionary.Add("EncryptionAlgorithm");
            base.CanonicalizationAlgorithm = dictionary.Add("CanonicalizationAlgorithm");
            base.ComputedKeyAlgorithm = dictionary.Add("ComputedKeyAlgorithm");
            base.RequestSecurityTokenResponseCollection = dictionary.Add("RequestSecurityTokenResponseCollection");
            base.Namespace = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512");
            base.BinarySecretClauseType = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512#BinarySecret");
            this.RequestSecurityTokenCollectionIssuanceFinalResponse = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTRC/IssueFinal");
            this.RequestSecurityTokenRenewal = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Renew");
            this.RequestSecurityTokenRenewalResponse = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/Renew");
            this.RequestSecurityTokenCollectionRenewalFinalResponse = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/RenewFinal");
            this.RequestSecurityTokenCancellation = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Cancel");
            this.RequestSecurityTokenCancellationResponse = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/Cancel");
            this.RequestSecurityTokenCollectionCancellationFinalResponse = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/CancelFinal");
            base.RequestTypeRenew = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/Renew");
            base.RequestTypeClose = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/Cancel");
            base.RenewTarget = dictionary.Add("RenewTarget");
            base.CloseTarget = dictionary.Add("CancelTarget");
            base.RequestedTokenClosed = dictionary.Add("RequestedTokenCancelled");
            base.RequestedAttachedReference = dictionary.Add("RequestedAttachedReference");
            base.RequestedUnattachedReference = dictionary.Add("RequestedUnattachedReference");
            base.IssuedTokensHeader = dictionary.Add("IssuedTokens");
            this.KeyWrapAlgorithm = dictionary.Add("KeyWrapAlgorithm");
            this.BearerKeyType = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-trust/200512/Bearer");
            this.SecondaryParameters = dictionary.Add("SecondaryParameters");
            this.Dialect = dictionary.Add("Dialect");
            this.DialectType = dictionary.Add("http://schemas.xmlsoap.org/ws/2005/05/identity");
        }
    }
}

